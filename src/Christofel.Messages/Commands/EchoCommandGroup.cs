using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib;
using Microsoft.Extensions.Logging;

namespace Christofel.Messages.Commands
{
    public class EchoCommandGroup : ICommandGroup
    {
        private class EchoData : IHasMessageChannel, IHasMessageId, IHasUserMessage
        {
            public IMessageChannel? Channel { get; set; }
            public ulong? MessageId { get; set; }
            public IUserMessage? UserMessage { get; set; }
        }

        private readonly ILogger<ReactCommandGroup> _logger;
        private readonly ICommandPermissionsResolver<PermissionSlashInfo> _resolver;
        private readonly DiscordSocketClient _client;

        public EchoCommandGroup(ILogger<ReactCommandGroup> logger,
            ICommandPermissionsResolver<PermissionSlashInfo> resolver, DiscordSocketClient client)
        {
            _logger = logger;
            _resolver = resolver;
            _client = client;
        }

        public async Task HandleEcho(SocketInteraction command, IChannel? channel, string text,
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
                        options: new RequestOptions { CancelToken = token });
                }
                catch (Exception)
                {
                    await command.RespondAsync("Message could not be sent, check permissions", ephemeral: true,
                        options: new RequestOptions { CancelToken = token });
                    throw;
                }
            }
        }

        public async Task HandleEdit(SocketInteraction command, IChannel? channel, string messageId, string text,
            CancellationToken token = default)
        {
            Verified<EchoData> verified = await
                new CommandVerifier<EchoData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyMessageId(messageId)
                    .VerifyUserMessage(token: token)
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
                        options: new RequestOptions { CancelToken = token });
                }
                catch (Exception)
                {
                    await command.RespondAsync("Message could not be edited, check permissions", ephemeral: true,
                        options: new RequestOptions { CancelToken = token });
                    throw;
                }
            }
        }

        public async Task HandleDelete(SocketInteraction command, IChannel? channel, string messageId,
            CancellationToken token = default)
        {
            Verified<EchoData> verified = await
                new CommandVerifier<EchoData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyMessageId(messageId)
                    .VerifyUserMessage(token: token)
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

        public SlashCommandOptionBuilder GetSendSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
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
                    .WithType(ApplicationCommandOptionType.Channel));
        }

        public SlashCommandOptionBuilder GetEditSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
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
                    .WithType(ApplicationCommandOptionType.Channel));
        }

        public SlashCommandOptionBuilder GetDeleteSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
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
                    .WithType(ApplicationCommandOptionType.Channel));
        }

        public Task SetupCommandsAsync(IInteractionHolder holder, CancellationToken token = new CancellationToken())
        {
            SubCommandHandlerCreator handlerCreator = new SubCommandHandlerCreator();
            DiscordInteractionHandler handler = handlerCreator.CreateHandlerForCommand(
                ("send", (CommandDelegate<IChannel?, string>)HandleEcho),
                ("edit", (CommandDelegate<IChannel?, string, string>)HandleEdit),
                ("delete", (CommandDelegate<IChannel?, string>)HandleDelete)
            );

            PermissionSlashInfoBuilder echoBuilder =
                new PermissionSlashInfoBuilder()
                    .WithPermission("messages.echo")
                    .WithHandler(handler)
                    .WithBuilder(new SlashCommandBuilder()
                        .WithName("echo")
                        .WithDescription("Echo messages as the bot")
                        .AddOption(GetSendSubcommandBuilder())
                        .AddOption(GetEditSubcommandBuilder())
                        .AddOption(GetDeleteSubcommandBuilder()));

            IInteractionExecutor executor = new InteractionExecutorBuilder<PermissionSlashInfo>()
                .WithPermissionCheck(_resolver)
                .WithThreadPool()
                .WithLogger(_logger)
                .Build();

            holder.AddInteraction(echoBuilder.Build(), executor);
            return Task.CompletedTask;
        }
    }
}