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

namespace Christofel.Management.Commands
{
    public class MessageCommandsGroup : IChristofelCommandGroup
    {
        public class SlowmodeData : IHasTextChannel
        {
            public ITextChannel? TextChannel { get; set; }
        }
        
        // /slowmode for interval hours minutes seconds channel
        // /slowmode enablepermanent interval channel
        // /slowmode disable channel
        private readonly DiscordSocketClient _client;
        private readonly BotOptions _options;
        private readonly ILogger<MessageCommandsGroup> _logger;
        private readonly ICommandPermissionsResolver<PermissionSlashInfo> _resolver;

        public MessageCommandsGroup(IOptions<BotOptions> options, ICommandPermissionsResolver<PermissionSlashInfo> resolver,
            ILogger<MessageCommandsGroup> logger, DiscordSocketClient client)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
            _resolver = resolver;
        }

        public async Task HandleSlowmodeFor(SocketSlashCommand command, long interval, long? hours, long? minutes,
            long? seconds, IChannel? channel, CancellationToken token = default)
        {
            await command.FollowupChunkAsync("Not implemented yet", ephemeral: true, options: new RequestOptions(){CancelToken = token});
            throw new NotImplementedException();
        }

        public async Task HandleSlowmodeEnable(SocketSlashCommand command, long interval, IChannel? channel,
            CancellationToken token = default)
        {
            Verified<SlowmodeData> verified = await new CommandVerifier<SlowmodeData>(_client, command, _logger)
                .VerifyTextChannel(channel ?? command.Channel)
                .VerifyMinMax(interval, 1, 3600, "interval")
                .FinishVerificationAsync();

            int intInterval = unchecked((int) interval);

            if (verified.Success)
            {
                SlowmodeData data = verified.Result;
                if (data.TextChannel == null)
                {
                    throw new InvalidOperationException("Verification failed");
                }

                try
                {
                    await data.TextChannel
                        .ModifyAsync(props =>
                                props.SlowModeInterval = intInterval,
                            new RequestOptions() {CancelToken = token});
                    await command.FollowupChunkAsync("Slowmode enabled", ephemeral: true, options: new RequestOptions(){CancelToken = token});
                }
                catch (Exception)
                {
                    await command.FollowupChunkAsync("Could not change the slowmode interval, check permissions", ephemeral: true, options: new RequestOptions(){CancelToken = token});
                    throw;
                }
            }
        }

        public async Task HandleSlowmodeDisable(SocketSlashCommand command, IChannel? channel,
            CancellationToken token = default)
        {
            Verified<SlowmodeData> verified = await new CommandVerifier<SlowmodeData>(_client, command, _logger)
                .VerifyTextChannel(channel ?? command.Channel)
                .FinishVerificationAsync();

            if (verified.Success)
            {
                SlowmodeData data = verified.Result;
                if (data.TextChannel == null)
                {
                    throw new InvalidOperationException("Verification failed");
                }

                try
                {
                    await data.TextChannel
                        .ModifyAsync(props =>
                                props.SlowModeInterval = 0,
                            new RequestOptions() {CancelToken = token});
                    await command.FollowupChunkAsync("Slowmode disabled", ephemeral: true, options: new RequestOptions(){CancelToken = token});
                }
                catch (Exception)
                {
                    await command.FollowupChunkAsync("Could not change the slowmode interval, check permissions", ephemeral: true, options: new RequestOptions(){CancelToken = token});
                    throw;
                }
            }
        }

        private SlashCommandOptionBuilder GetForSlowmodeSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
                .WithName("for")
                .WithDescription("Enable slowmode for specified duration")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("interval")
                    .WithDescription("Interval in seconds for slowmode, min 1, max 3600")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("hours")
                    .WithDescription("How many hours to enable slowmode for")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("minutes")
                    .WithDescription("How many minutes to enable slowmode for")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("seconds")
                    .WithDescription("How many seconds to enable slowmode for")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("channel")
                    .WithDescription("Channel where to turn on slowmode")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Channel));
        }

        private SlashCommandOptionBuilder GetEnablePermanentSlowmodeSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
                .WithName("enablepermanent")
                .WithDescription("Enable slowmode permanently in specified channel")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("interval")
                    .WithDescription("Interval in seconds for slowmode, min 1, max 3600")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Integer))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("channel")
                    .WithDescription("Channel where to enable slowmode")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Channel));
        }

        private SlashCommandOptionBuilder GetDisableSlowmodeSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
                .WithName("disable")
                .WithDescription("Disable slowmode in channel")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("channel")
                    .WithDescription("Channel where to disable slowmode")
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Channel));
        }

        public Task SetupCommandsAsync(ICommandHolder<PermissionSlashInfo> holder, CancellationToken token = new CancellationToken())
        {
            SlashCommandHandler slowmodeHandler = new SubCommandHandlerCreator()
                .CreateHandlerForCommand(
                    ("for", (CommandDelegate<long, long?, long?, long?, IChannel?>) HandleSlowmodeFor),
                    ("enablepermanent", (CommandDelegate<long, IChannel?>) HandleSlowmodeEnable),
                    ("disable", (CommandDelegate<IChannel?>) HandleSlowmodeDisable)
                );

            PermissionSlashInfoBuilder slowmodeBuilder = new PermissionSlashInfoBuilder()
                .WithGuild(_options.GuildId)
                .WithPermission("management.messages.slowmode")
                .WithHandler(slowmodeHandler)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("slowmode")
                    .WithDescription("Controls slowmode of a channel")
                    .AddOption(GetForSlowmodeSubcommandBuilder())
                    .AddOption(GetEnablePermanentSlowmodeSubcommandBuilder())
                    .AddOption(GetDisableSlowmodeSubcommandBuilder()));

            ICommandExecutor<PermissionSlashInfo> executor = new CommandExecutorBuilder<PermissionSlashInfo>()
                .WithLogger(_logger)
                .WithPermissionCheck(_resolver)
                .WithThreadPool()
                .WithDeferMessage()
                .Build();

            holder.AddCommand(slowmodeBuilder.Build(), executor);
            return Task.CompletedTask;
        }
    }
}