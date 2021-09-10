//
//   WrongParametersExecutionEvent.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    /// <summary>
    /// Event catching missing command errors and telling the user correct syntax or what commands are in the group.
    /// </summary>
    public class WrongParametersExecutionEvent : IPostExecutionEvent
    {
        private readonly CommandTree _commandTree;
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly WrongParametersEventOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="WrongParametersExecutionEvent"/> class.
        /// </summary>
        /// <param name="feedbackService">Service used for sending feedback to the user.</param>
        /// <param name="interactionApi">Api used for sending interaction response.</param>
        /// <param name="commandTree">Command tree holding all application commands.</param>
        /// <param name="options">Options used for formatting of the usage.</param>
        public WrongParametersExecutionEvent
        (
            FeedbackService feedbackService,
            IDiscordRestInteractionAPI interactionApi,
            CommandTree commandTree,
            IOptionsSnapshot<WrongParametersEventOptions> options
        )
        {
            _options = options.Value;
            _commandTree = commandTree;
            _interactionApi = interactionApi;
            _feedbackService = feedbackService;
        }

        /// <inheritdoc />
        public Task<Result> AfterExecutionAsync
        (
            ICommandContext context,
            IResult commandResult,
            CancellationToken ct = default
        )
        {
            if (!commandResult.IsSuccess && commandResult.Error is CommandNotFoundError commandNotFoundError)
            {
                var foundChildNode = FindCommandNode(commandNotFoundError.OriginalInput);

                if (foundChildNode is not null)
                {
                    var explained = foundChildNode switch
                    {
                        CommandNode commandNode => ExplainCommandNode(commandNode),
                        GroupNode groupNode => ExplainGroupNode(groupNode),
                        _ => null,
                    };

                    if (explained is not null)
                    {
                        return SendResponse(context, explained, ct);
                    }
                }
            }

            return Task.FromResult(Result.FromSuccess());
        }

        /// <summary>
        /// Sends message to the user using embed.
        /// </summary>
        /// <param name="context">Context of the command.</param>
        /// <param name="message">Message to be sent.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
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

        /// <summary>
        /// Creates explanation string for matched group node.
        /// </summary>
        /// <param name="groupNode">Matched group node.</param>
        /// <returns>Formatted explanation of the usage of the group.</returns>
        private string ExplainGroupNode(GroupNode groupNode)
        {
            var fullName = GetFullExecutionName(groupNode);
            var childNames = string.Join('|', groupNode.Children.Select(x => x.Key));
            var dataDictionary = new Dictionary<string, string> { { "Name", fullName }, { "Children", childNames } };

            return FormatString(_options.GroupExplanationFormat, dataDictionary);
        }

        /// <summary>
        /// Creates explanation string for matched command node.
        /// </summary>
        /// <param name="commandNode">Matched command node.</param>
        /// <returns>Formatted explanation of the usage of the command.</returns>
        private string ExplainCommandNode(CommandNode commandNode)
        {
            var fullName = GetFullExecutionName(commandNode);
            var explainedParameters = commandNode.Shape.Parameters.Select(ExplainParameter);

            var dataDictionary = new Dictionary<string, string>
            {
                { "Name", fullName }, { "Parameters", string.Join(" ", explainedParameters) },
            };

            return FormatString(_options.CommandExplanationFormat, dataDictionary);
        }

        /// <summary>
        /// Creates explanation string for one parameter.
        /// </summary>
        /// <param name="parameterShape">Current parameter of matched command.</param>
        /// <returns>Formatted explanation of the parameter.</returns>
        private string ExplainParameter(IParameterShape parameterShape)
        {
            var format = parameterShape.IsOmissible()
                ? _options.OptionalParameterExplanationFormat
                : _options.RequiredParameterExplanationFormat;
            var dataDictionary = new Dictionary<string, string>
            {
                { "Name", parameterShape.HintName }, { "Type", parameterShape.Parameter.ParameterType.Name },
            };

            return FormatString(format, dataDictionary);
        }

        /// <summary>
        /// Obtains full name of the command by iterating through parents.
        /// </summary>
        /// <param name="commandNode">The node.</param>
        /// <returns>Full name of the command.</returns>
        private string GetFullExecutionName(CommandNode commandNode)
        {
            var nameParts = new List<string> { commandNode.Key };

            GetPartialExecutionName(commandNode.Parent, nameParts);

            nameParts.Reverse();
            return string.Join(' ', nameParts);
        }

        /// <summary>
        /// Obtains full name of the group by iterating through parents.
        /// </summary>
        /// <param name="groupNode">The node.</param>
        /// <returns>Full name of the group.</returns>
        private string GetFullExecutionName(GroupNode groupNode)
        {
            var nameParts = new List<string>();

            GetPartialExecutionName(groupNode, nameParts);

            nameParts.Reverse();
            return string.Join(' ', nameParts);
        }

        /// <summary>
        /// Fills list with parent names.
        /// </summary>
        /// <param name="parent">Current node.</param>
        /// <param name="parts">Parts list to be filled.</param>
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

        /// <summary>
        /// Tries to obtain command node by command name.
        /// </summary>
        /// <param name="command">Name of the command.</param>
        /// <returns>Last matched node.</returns>
        private IChildNode? FindCommandNode(string command)
        {
            var tokenizer = new TokenizingEnumerator(command);
            return FindCommandNode(_commandTree.Root, tokenizer);
        }

        /// <summary>
        /// Tries to obtain command node from parent node and token enumerator.
        /// </summary>
        /// <param name="node">Current node to search.</param>
        /// <param name="tokenizingEnumerator">Tokenizing enumerator.</param>
        /// <returns>Last matched node.</returns>
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

            return node is IChildNode childNode
                ? childNode
                : null;
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

        /// <summary>
        /// Replaces parameters inside of the string.
        /// </summary>
        /// <remarks>
        /// Replaces parameters enclosed in curly braces {} by the value that is in parameters Dictionary.
        /// </remarks>
        /// <param name="format">The format that should be printed.</param>
        /// <param name="parameters">The parameters that will be replaces.</param>
        /// <returns>Formatted string.</returns>
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