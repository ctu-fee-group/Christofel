using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.ExecutionEvents.Options;
using Microsoft.Extensions.Options;
using Remora.Commands.Results;
using Remora.Commands.Signatures;
using Remora.Commands.Tokenization;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Christofel.CommandsLib.ExecutionEvents
{
    public class WrongParametersExecutionEvent : IPostExecutionEvent
    {
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly CommandTree _commandTree;
        private readonly WrongParametersEventOptions _options;

        public WrongParametersExecutionEvent(FeedbackService feedbackService, IDiscordRestInteractionAPI interactionApi,
            CommandTree commandTree, IOptionsSnapshot<WrongParametersEventOptions> options)
        {
            _options = options.Value;
            _commandTree = commandTree;
            _interactionApi = interactionApi;
            _feedbackService = feedbackService;
        }

        public Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult,
            CancellationToken ct = new CancellationToken())
        {
            if (!commandResult.IsSuccess && commandResult.Error is CommandNotFoundError commandNotFoundError)
            {
                var foundChildNode = FindCommandNode(commandNotFoundError.OriginalInput);

                if (foundChildNode is not null)
                {
                    string? explained = null;

                    switch (foundChildNode)
                    {
                        case CommandNode commandNode:
                            explained = ExplainCommandNode(commandNode, ct);
                            break;
                        case GroupNode groupNode:
                            explained = ExplainGroupNode(groupNode, ct);
                            break;
                    }

                    if (explained is not null)
                    {
                        return SendResponse(context, explained, ct);
                    }
                }
            }

            return Task.FromResult(Result.FromSuccess());
        }

        private async Task<Result> SendResponse(ICommandContext context, string message, CancellationToken ct)
        {
            if (context is InteractionContext interactionContext)
            {
                var response = await _interactionApi.CreateInteractionResponseAsync
                (
                    interactionContext.ID,
                    interactionContext.Token,
                    new InteractionResponse(InteractionCallbackType.DeferredChannelMessageWithSource),
                    ct
                );

                if (!response.IsSuccess)
                {
                    return response;
                }
            }

            var feedbackResult =
                await _feedbackService.SendContextualErrorAsync(message, ct: ct);

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        private string ExplainGroupNode(GroupNode groupNode,
            CancellationToken ct)
        {
            var fullName = GetFullExecutionName(groupNode);
            var childNames = string.Join('|', groupNode.Children.Select(x => x.Key));
            var dataDictionary = new Dictionary<string, string>
            {
                { "Name", fullName },
                { "Children", childNames }
            };

            return FormatString(_options.GroupExplanationFormat, dataDictionary);
        }

        private string ExplainCommandNode(CommandNode commandNode,
            CancellationToken ct)
        {
            var fullName = GetFullExecutionName(commandNode);
            var explainedParameters = commandNode.Shape.Parameters.Select(ExplainParameter);
            
            var dataDictionary = new Dictionary<string, string>
            {
                { "Name", fullName },
                { "Parameters", string.Join(" ", explainedParameters) }
            };

            return FormatString(_options.CommandExplanationFormat, dataDictionary);
        }

        private string ExplainParameter(IParameterShape parameterShape)
        {
            var format = parameterShape.IsOmissible()
                ? _options.OptionalParameterExplanationFormat
                : _options.RequiredParameterExplanationFormat;
            var dataDictionary = new Dictionary<string, string>
            {
                { "Name", parameterShape.HintName },
                { "Type", parameterShape.Parameter.ParameterType.Name }
            };

            return FormatString(format, dataDictionary);
        }

        private string GetFullExecutionName(CommandNode commandNode)
        {
            var nameParts = new List<string>();
            nameParts.Add(commandNode.Key);

            GetPartialExecutionName(commandNode.Parent, nameParts);

            nameParts.Reverse();
            return string.Join(' ', nameParts);
        }

        private string GetFullExecutionName(GroupNode groupNode)
        {
            var nameParts = new List<string>();

            GetPartialExecutionName(groupNode, nameParts);

            nameParts.Reverse();
            return string.Join(' ', nameParts);
        }

        private void GetPartialExecutionName(IParentNode? parent, List<string> parts)
        {
            while (parent is not null)
            {
                switch (parent)
                {
                    case GroupNode group:
                        parent = group.Parent;
                        parts.Add(group.Key);
                        break;
                    default:
                        parent = null;
                        break;
                }
            }
        }

        private IChildNode? FindCommandNode(string command)
        {
            var tokenizer = new TokenizingEnumerator(command);
            return FindCommandNode(_commandTree.Root, tokenizer);
        }

        private IChildNode? FindCommandNode(IParentNode node, TokenizingEnumerator tokenizingEnumerator)
        {
            foreach (var child in node.Children)
            {
                if (!IsNodeMatch(child, tokenizingEnumerator, new TreeSearchOptions()))
                {
                    continue;
                }

                switch (child)
                {
                    case CommandNode commandNode:
                        return commandNode; // Return first matching
                    case IParentNode parentNode:
                        if (!tokenizingEnumerator.MoveNext())
                        {
                            return child;
                        }

                        return FindCommandNode(parentNode, tokenizingEnumerator);
                }
            }

            return node is IChildNode childNode ? childNode : null;
        }

        /// <summary>
        /// Determines whether a node matches the current state of the tokenizer.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="tokenizer">The tokenizer.</param>
        /// <param name="searchOptions">A set of search options.</param>
        /// <returns>true if the node matches; otherwise, false.</returns>
        private bool IsNodeMatch(IChildNode node, TokenizingEnumerator tokenizer, TreeSearchOptions searchOptions)
        {
            if (!tokenizer.MoveNext())
            {
                return false;
            }

            if (tokenizer.Current.Type != TokenType.Value)
            {
                return false;
            }

            if (tokenizer.Current.Value.Equals(node.Key, searchOptions.KeyComparison))
            {
                return true;
            }

            foreach (var alias in node.Aliases)
            {
                if (tokenizer.Current.Value.Equals(alias, searchOptions.KeyComparison))
                {
                    return true;
                }
            }

            return false;
        }

        private string FormatString(string format, Dictionary<string, string> parameters)
        {
            parameters.Add("Prefix", _options.Prefix);

            foreach (var parameter in parameters)
            {
                format = format.Replace("{" + parameter.Key + "}", parameter.Value);
            }

            return format;
        }
    }
}