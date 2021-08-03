using System;
using System.Reflection.Metadata;
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
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Messages.Commands
{
    public class EchoCommandGroup : ICommandGroup
    {
        private class EchoData : IHasMessageChannel, IHasMessageId, IHasRestUserMessage
        {
            public IMessageChannel? Channel { get; set; }
            public ulong? MessageId { get; set; }
            public IUserMessage? UserMessage { get; set; }
        }

        private readonly ILogger<ReactCommandGroup> _logger;
        private readonly IPermissionsResolver _resolver;
        private readonly BotOptions _options;
        private readonly DiscordSocketClient _client;

        public EchoCommandGroup(ILogger<ReactCommandGroup> logger, IPermissionsResolver resolver,
            IOptions<BotOptions> options, DiscordSocketClient client)
        {
            _options = options.Value;
            _logger = logger;
            _resolver = resolver;
            _client = client;
        }

        public async Task HandleEcho(SocketSlashCommand command, IChannel? channel, string text,
            CancellationToken token = default)
        {
            Verified<EchoData> verified = await
                new CommandVerifier<EchoData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .FinishVerificationAsync();

            if (verified.Success)
            {
                EchoData data = verified.Result;
                if (data.Channel == null)
                {
                    throw new InvalidOperationException("Verification failed");
                }

                try
                {
                    await data.Channel.SendMessageAsync(text);
                    await command.RespondAsync("Message sent", ephemeral: true,
                        options: new RequestOptions {CancelToken = token});
                }
                catch (Exception)
                {
                    await command.RespondAsync("Message could not be sent, check permissions", ephemeral: true,
                        options: new RequestOptions {CancelToken = token});
                    throw;
                }
            }
        }

        public async Task HandleEdit(SocketSlashCommand command, IChannel? channel, string messageId, string text,
            CancellationToken token = default)
        {
            Verified<EchoData> verified = await
                new CommandVerifier<EchoData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyMessageId(messageId)
                    .VerifyRestUserMessage(token: token)
                    .VerifyMessageAuthorChristofel()
                    .FinishVerificationAsync();

            if (verified.Success)
            {
                EchoData data = verified.Result;
                if (data.UserMessage == null)
                {
                    throw new InvalidOperationException("Verification failed");
                }

                try
                {
                    await data.UserMessage.ModifyAsync(x => x.Content = text);
                    await command.RespondAsync("Message edited", ephemeral: true,
                        options: new RequestOptions {CancelToken = token});
                }
                catch (Exception)
                {
                    await command.RespondAsync("Message could not be edited, check permissions", ephemeral: true,
                        options: new RequestOptions {CancelToken = token});
                    throw;
                }
            }
        }

        public async Task HandleDelete(SocketSlashCommand command, IChannel? channel, string messageId,
            CancellationToken token = default)
        {
            Verified<EchoData> verified = await
                new CommandVerifier<EchoData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyMessageId(messageId)
                    .VerifyRestUserMessage(token: token)
                    .VerifyMessageAuthorChristofel()
                    .FinishVerificationAsync();
            if (verified.Success)
            {
                EchoData data = verified.Result;
                if (data.UserMessage == null)
                {
                    throw new InvalidOperationException("Verification failed");
                }

                try
                {
                    await data.UserMessage.DeleteAsync();
                }
                catch (Exception)
                {
                    await command.RespondAsync("Message could not be deleted, check permissions");
                    throw;
                }
            }


            await command.RespondAsync("Message deleted");
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            SubCommandHandlerCreator handlerCreator = new SubCommandHandlerCreator();
            SlashCommandHandler handler = handlerCreator.CreateHandlerForCommand(
                ("send", (CommandDelegate<IChannel?, string>) HandleEcho),
                ("edit", (CommandDelegate<IChannel?, string, string>) HandleEdit),
                ("delete", (CommandDelegate<IChannel?, string>) HandleDelete)
            );

            SlashCommandBuilder echoBuilder =
                new SlashCommandBuilderInfo()
                    .WithName("echo")
                    .WithDescription("Echo messages as the bot")
                    .WithGuild(_options.GuildId)
                    .WithPermission("messages.echo")
                    .WithHandler(handler)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("send")
                        .WithDescription("Send a new message")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("text")
                            .WithRequired(true)
                            .WithDescription("Text to send")
                            .WithType(ApplicationCommandOptionType.String))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("channel")
                            .WithDescription("Channel to send the text to. Default current channel")
                            .WithRequired(false)
                            .WithType(ApplicationCommandOptionType.Channel))
                    )
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("edit")
                        .WithDescription("Edit existing message by the bot")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("messageid")
                            .WithDescription("Message id to edit")
                            .WithRequired(true)
                            .WithType(ApplicationCommandOptionType.String))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("text")
                            .WithDescription("Text to send")
                            .WithRequired(true)
                            .WithType(ApplicationCommandOptionType.String)
                        )
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("channel")
                            .WithDescription("Channel to send the text to. Default is current channel")
                            .WithRequired(false)
                            .WithType(ApplicationCommandOptionType.Channel))
                    )
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("delete")
                        .WithDescription("Delete existing message")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("messageid")
                            .WithDescription("Message id to edit")
                            .WithRequired(true)
                            .WithType(ApplicationCommandOptionType.String))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("channel")
                            .WithDescription("Channel to send the text to. Default is current channel")
                            .WithRequired(false)
                            .WithType(ApplicationCommandOptionType.Channel)));

            ICommandExecutor executor = new CommandExecutorBuilder()
                .WithPermissionsCheck(_resolver)
                .WithThreadPool()
                .WithLogger(_logger)
                .Build();

            holder.AddCommand(echoBuilder, executor);
            return Task.CompletedTask;
        }
    }
}