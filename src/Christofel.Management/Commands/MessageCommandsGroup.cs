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

        public async Task HandleSlowmodeFor(SocketSlashCommand command, long interval, long? hours, long? minutes,
            long? seconds, IChannel? channel, CancellationToken token = default)
        {
            await command.RespondAsync("Not implemented yet");
            throw new NotImplementedException();
        }

        public async Task HandleSlowmodeEnable(SocketSlashCommand command, long interval, IChannel? channel,
            CancellationToken token = default)
        {
            Verified<SlowmodeData> verified = await new CommandVerifier<SlowmodeData>(_client, command, _logger)
                .VerifyTextChannel(channel ?? command.Channel)
                //.VerifyMinMax(interval, 1, 3600)
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
                    await command.RespondAsync("Slowmode enabled");
                }
                catch (Exception)
                {
                    await command.RespondAsync("Could not change the slowmode interval, check permissions");
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
                    await command.RespondAsync("Slowmode disabled");
                }
                catch (Exception)
                {
                    await command.RespondAsync("Could not change the slowmode interval, check permissions");
                    throw;
                }
            }
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            SlashCommandHandler slowmodeHandler = new SubCommandHandlerCreator()
                .CreateHandlerForCommand(
                    ("for", (CommandDelegate<long, long?, long?, long?, IChannel?>) HandleSlowmodeFor),
                    ("enablepermanent", (CommandDelegate<long, IChannel?>) HandleSlowmodeEnable),
                    ("disable", (CommandDelegate<IChannel?>) HandleSlowmodeDisable)
                );

            SlashCommandInfoBuilder slowmodeBuilder = new SlashCommandInfoBuilder()
                .WithGuild(_options.GuildId)
                .WithPermission("management.messages.slowmode")
                .WithHandler(slowmodeHandler)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("slowmode")
                    .WithDescription("Controls slowmode of a channel")
                    .AddOption(new SlashCommandOptionBuilder()
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
                            .WithType(ApplicationCommandOptionType.Channel)))
                    .AddOption(new SlashCommandOptionBuilder()
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
                            .WithType(ApplicationCommandOptionType.Channel)))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("disable")
                        .WithDescription("Disable slowmode in channel")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("channel")
                            .WithDescription("Channel where to disable slowmode")
                            .WithRequired(false)
                            .WithType(ApplicationCommandOptionType.Channel))));

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