using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.HandlerCreator;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.Verifier;
using Christofel.CommandsLib.Verifier.Interfaces;
using Christofel.CommandsLib.Verifier.Verifiers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Management.Commands
{
    public class MessageCommandsGroup : ICommandGroup
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
        private readonly IPermissionsResolver _resolver;

        public MessageCommandsGroup(IOptions<BotOptions> options, IPermissionsResolver resolver,
            ILogger<MessageCommandsGroup> logger, DiscordSocketClient client)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
            _resolver = resolver;
        }

        public async Task HandleSlowmodeFor(SocketSlashCommand command, int interval, int? hours, int? minutes,
            int? seconds, IChannel? channel, CancellationToken token = default)
        {
            await command.RespondChunkAsync("Not implemented yet", ephemeral: true, options: new RequestOptions(){CancelToken = token});
            throw new NotImplementedException();
        }

        public async Task HandleSlowmodeEnable(SocketSlashCommand command, int interval, IChannel? channel,
            CancellationToken token = default)
        {
            Verified<SlowmodeData> verified = await new CommandVerifier<SlowmodeData>(_client, command, _logger)
                .VerifyTextChannel(channel ?? command.Channel)
                .VerifyMinMax(interval, 1, 3600, "interval")
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
                                props.SlowModeInterval = interval,
                            new RequestOptions() {CancelToken = token});
                    await command.RespondChunkAsync("Slowmode enabled", ephemeral: true, options: new RequestOptions(){CancelToken = token});
                }
                catch (Exception)
                {
                    await command.RespondChunkAsync("Could not change the slowmode interval, check permissions", ephemeral: true, options: new RequestOptions(){CancelToken = token});
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
                    await command.RespondChunkAsync("Slowmode disabled", ephemeral: true, options: new RequestOptions(){CancelToken = token});
                }
                catch (Exception)
                {
                    await command.RespondChunkAsync("Could not change the slowmode interval, check permissions", ephemeral: true, options: new RequestOptions(){CancelToken = token});
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

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            SlashCommandHandler slowmodeHandler = new SubCommandHandlerCreator()
                .CreateHandlerForCommand(
                    ("for", (CommandDelegate<int, int?, int?, int?, IChannel?>) HandleSlowmodeFor),
                    ("enablepermanent", (CommandDelegate<int, IChannel?>) HandleSlowmodeEnable),
                    ("disable", (CommandDelegate<IChannel?>) HandleSlowmodeDisable)
                );

            SlashCommandInfoBuilder slowmodeBuilder = new SlashCommandInfoBuilder()
                .WithGuild(_options.GuildId)
                .WithPermission("management.messages.slowmode")
                .WithHandler(slowmodeHandler)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("slowmode")
                    .WithDescription("Controls slowmode of a channel")
                    .AddOption(GetForSlowmodeSubcommandBuilder())
                    .AddOption(GetEnablePermanentSlowmodeSubcommandBuilder())
                    .AddOption(GetDisableSlowmodeSubcommandBuilder()));

            ICommandExecutor executor = new CommandExecutorBuilder()
                .WithLogger(_logger)
                .WithPermissionsCheck(_resolver)
                .WithThreadPool()
                .Build();

            holder.AddCommand(slowmodeBuilder, executor);
            return Task.CompletedTask;
        }
    }
}