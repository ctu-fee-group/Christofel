using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib;
using Discord;
using Discord.Net.Interactions.Abstractions;
using Discord.Net.Interactions.CommandsInfo;
using Discord.Net.Interactions.Executors;
using Discord.Net.Interactions.HandlerCreator;
using Discord.Net.Interactions.Verifier;
using Discord.Net.Interactions.Verifier.Interfaces;
using Discord.Net.Interactions.Verifier.Verifiers;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Messages.Commands
{
    public class ReactCommandGroup : ICommandGroup
    {
        private class ReactData : IHasMessageChannel, IHasMessageId, IHasUserMessage, IHasEmote
        {
            public IMessageChannel? Channel { get; set; }
            public ulong? MessageId { get; set; }
            public IUserMessage? UserMessage { get; set; }
            public IEmote? Emote { get; set; }
        }

        private readonly ILogger<ReactCommandGroup> _logger;
        private readonly ICommandPermissionsResolver<PermissionSlashInfo> _resolver;
        private readonly BotOptions _options;
        private readonly DiscordSocketClient _client;

        public ReactCommandGroup(DiscordSocketClient client, ILogger<ReactCommandGroup> logger,
            ICommandPermissionsResolver<PermissionSlashInfo> resolver, IOptions<BotOptions> options)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
            _resolver = resolver;
        }

        // /react handler
        public async Task HandleReactAsync(SocketInteraction command, IChannel? channel, string messageId,
            string emojiString, CancellationToken token = default)
        {
            Verified<ReactData> verified = await
                new CommandVerifier<ReactData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyMessageId(messageId)
                    .VerifyUserMessage(token: token)
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

        public Task SetupCommandsAsync(IInteractionHolder holder, CancellationToken token = new CancellationToken())
        {
            IInteractionExecutor executor = new InteractionExecutorBuilder<PermissionSlashInfo>()
                .WithLogger(_logger)
                .WithPermissionCheck(_resolver)
                .WithThreadPool()
                .Build();

            DiscordInteractionHandler handler = new PlainCommandHandlerCreator()
                .CreateHandlerForCommand((CommandDelegate<IChannel?, string, string>) HandleReactAsync);

            PermissionSlashInfoBuilder commandBuilder = new PermissionSlashInfoBuilder()
                .WithGuild(_options.GuildId)
                .WithHandler(handler)
                .WithPermission("messages.react.react")
                .WithBuilder(new SlashCommandBuilder()
                .WithName("react")
                .WithDescription("React to a message")
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
                    .WithType(ApplicationCommandOptionType.Channel)));

            holder.AddInteraction(commandBuilder.Build(), executor);
            return Task.CompletedTask;
        }
    }
}