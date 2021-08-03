using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.HandlerCreator;
using Christofel.CommandsLib.Verifier;
using Christofel.CommandsLib.Verifier.Interfaces;
using Christofel.CommandsLib.Verifier.Verifiers;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Messages.Commands
{
    public class ReactCommandGroup : ICommandGroup
    {
        private class ReactData : IHasMessageChannel, IHasMessageId, IHasRestUserMessage, IHasEmote
        {
            public IMessageChannel? Channel { get; set; }
            public ulong? MessageId { get; set; }
            public IUserMessage? UserMessage { get; set; }
            public IEmote? Emote { get; set; }
        }

        private readonly ILogger<ReactCommandGroup> _logger;
        private readonly IPermissionsResolver _resolver;
        private readonly BotOptions _options;
        private readonly DiscordSocketClient _client;

        public ReactCommandGroup(DiscordSocketClient client, ILogger<ReactCommandGroup> logger,
            IPermissionsResolver resolver, IOptions<BotOptions> options)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
            _resolver = resolver;
        }

        // /react handler
        public async Task HandleReactAsync(SocketSlashCommand command, IChannel? channel, string messageId,
            string emojiString, CancellationToken token = default)
        {
            Verified<ReactData> verified = await
                new CommandVerifier<ReactData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyMessageId(messageId)
                    .VerifyRestUserMessage(token: token)
                    .VerifyEmote(emojiString)
                    .FinishVerificationAsync();

            if (verified.Success)
            {
                ReactData data = verified.Result;
                if (data.UserMessage == null || data.Emote == null)
                {
                    throw new InvalidOperationException("Validation failed");
                }

                await data.UserMessage.AddReactionAsync(data.Emote, new RequestOptions() {CancelToken = token});
                await command.RespondAsync("Reaction added", ephemeral: true,
                    options: new RequestOptions {CancelToken = token});
            }
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            ICommandExecutor executor = new CommandExecutorBuilder()
                .WithLogger(_logger)
                .WithPermissionsCheck(_resolver)
                .WithThreadPool()
                .Build();

            SlashCommandHandler handler = new PlainCommandHandlerCreator()
                .CreateHandlerForCommand((CommandDelegate<IChannel?, string, string>) HandleReactAsync);

            SlashCommandBuilder commandBuilder = new SlashCommandBuilderInfo()
                .WithName("react")
                .WithDescription("React to a message")
                .WithPermission("messages.react.react")
                .WithGuild(_options.GuildId)
                .WithHandler(handler)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("messageid")
                    .WithDescription("Message to react to")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("emojistring")
                    .WithDescription("Emoji or Emote to react with")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("channel")
                    .WithDescription("Channel with target message. Default is the current channel")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Channel));

            holder.AddCommand(commandBuilder, executor);
            return Task.CompletedTask;
        }
    }
}