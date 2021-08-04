using System;
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
using Christofel.Messages.Commands.Verifiers;
using Christofel.Messages.Options;
using Christofel.Messages.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Messages.Commands
{
    public class EmbedCommandGroup : ICommandGroup
    {
        private class EmbedData : IHasMessageChannel, IHasMessageId, IHasRestUserMessage, IHasEmbed
        {
            public IMessageChannel? Channel { get; set; }
            public ulong? MessageId { get; set; }
            public IUserMessage? UserMessage { get; set; }

            public Embed? Embed { get; set; }
        }

        private readonly ILogger<ReactCommandGroup> _logger;
        private readonly DiscordSocketClient _client;
        private readonly IPermissionsResolver _resolver;
        private readonly BotOptions _options;
        private readonly EmbedsProvider _embeds;

        public EmbedCommandGroup(ILogger<ReactCommandGroup> logger, IPermissionsResolver resolver,
            IOptions<BotOptions> options, EmbedsProvider embeds, DiscordSocketClient client)
        {
            _client = client;
            _embeds = embeds;
            _options = options.Value;
            _logger = logger;
            _resolver = resolver;
        }

        private async Task HandleEditEmbed(SocketSlashCommand command, Verified<EmbedData> verified,
            CancellationToken token = default)
        {
            if (verified.Success)
            {
                EmbedData data = verified.Result;
                if (data.Embed == null || data.UserMessage == null)
                {
                    throw new InvalidOperationException("Validation failed");
                }
                
                try
                {
                    await data.UserMessage.ModifyAsync(
                        props => props.Embeds = new[] {data.Embed},
                        new RequestOptions() {CancelToken = token});
                    await command.RespondAsync("Embed edited!", ephemeral: true,
                        options: new RequestOptions() {CancelToken = token});
                }
                catch (Exception)
                {
                    await command.RespondAsync("There was an error editing the embed, check permissions",
                        ephemeral: true, options: new RequestOptions() {CancelToken = token});
                    throw;
                }
            }
        }

        private async Task HandleCreateEmbed(SocketSlashCommand command, Verified<EmbedData> verified,
            CancellationToken token = default)
        {
            if (verified.Success)
            {
                EmbedData data = verified.Result;
                if (data.Embed == null || data.Channel == null)
                {
                    throw new InvalidOperationException("Validation failed");
                }

                try
                {
                    await data.Channel.SendMessageAsync(embed: data.Embed,
                        options: new RequestOptions() {CancelToken = token});
                    await command.RespondAsync("Embed sent!", ephemeral: true,
                        options: new RequestOptions() {CancelToken = token});
                }
                catch (Exception)
                {
                    await command.RespondAsync("There was an error editing the embed, check permissions",
                        ephemeral: true, options: new RequestOptions() {CancelToken = token});
                    throw;
                }
            }
        }

        public async Task HandleEditEmbedFromFile(SocketSlashCommand command, IChannel? channel, string messageId,
            string fileName, CancellationToken token = default)
        {
            Verified<EmbedData> verified = await
                new CommandVerifier<EmbedData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyMessageId(messageId)
                    .VerifyRestUserMessage(token: token)
                    .VerifyMessageAuthorChristofel()
                    .VerifyUserMessageHasEmbeds()
                    .VerifyFile(_embeds.EmbedsFolder, fileName, ".json")
                    .VerifyFileIsEmbedJson(_embeds, fileName)
                    .FinishVerificationAsync();

            await HandleEditEmbed(command, verified, token);
        }

        public async Task HandleEditEmbedFromMessage(SocketSlashCommand command, IChannel? channel, string messageId,
            string embed, CancellationToken token = default)
        {
            Verified<EmbedData> verified = await
                new CommandVerifier<EmbedData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyMessageId(messageId)
                    .VerifyRestUserMessage(token: token)
                    .VerifyMessageAuthorChristofel()
                    .VerifyUserMessageHasEmbeds()
                    .VerifyIsEmbedJson(_embeds, embed)
                    .FinishVerificationAsync();

            await HandleEditEmbed(command, verified, token);
        }

        public async Task HandleCreateEmbedFromMessage(SocketSlashCommand command, IChannel? channel, string embed,
            CancellationToken token = default)
        {
            Verified<EmbedData> verified = await
                new CommandVerifier<EmbedData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyIsEmbedJson(_embeds, embed)
                    .FinishVerificationAsync();

            await HandleCreateEmbed(command, verified, token);
        }

        public async Task HandleCreateEmbedFromFile(SocketSlashCommand command, IChannel? channel, string fileName,
            CancellationToken token = default)
        {
            Verified<EmbedData> verified = await
                new CommandVerifier<EmbedData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyFile(_embeds.EmbedsFolder, fileName, ".json")
                    .VerifyFileIsEmbedJson(_embeds, fileName)
                    .FinishVerificationAsync();

            await HandleCreateEmbed(command, verified, token);
        }

        public async Task HandleDeleteEmbed(SocketSlashCommand command, IChannel? channel, string messageId,
            CancellationToken token)
        {
            Verified<EmbedData> verified = await
                new CommandVerifier<EmbedData>(_client, command, _logger)
                    .VerifyMessageChannel(channel ?? command.Channel)
                    .VerifyMessageId(messageId)
                    .VerifyRestUserMessage(token: token)
                    .VerifyMessageAuthorChristofel()
                    .FinishVerificationAsync();

            if (verified.Success)
            {
                EmbedData data = verified.Result;
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
                
                await command.RespondAsync("Embed deleted!");
            }
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            ICommandExecutor executor = new CommandExecutorBuilder()
                .WithLogger(_logger)
                .WithPermissionsCheck(_resolver)
                .WithThreadPool()
                .Build();

            SlashCommandHandler handler = new SubCommandHandlerCreator()
                .CreateHandlerForCommand(
                    ("file send", (CommandDelegate<IChannel?, string>) HandleCreateEmbedFromFile),
                    ("file edit", (CommandDelegate<IChannel?, string, string>) HandleEditEmbedFromFile),
                    ("file delete", (CommandDelegate<IChannel?, string>) HandleDeleteEmbed),
                    ("msg send", (CommandDelegate<IChannel?, string>) HandleCreateEmbedFromMessage),
                    ("msg edit", (CommandDelegate<IChannel?, string, string>) HandleEditEmbedFromMessage),
                    ("msg delete", (CommandDelegate<IChannel?, string>) HandleDeleteEmbed)
                );

            SlashCommandInfoBuilder commandBuilder = new SlashCommandInfoBuilder()
                .WithHandler(handler)
                .WithGuild(_options.GuildId)
                .WithPermission("messages.embed")
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("embed")
                    .WithDescription("Manage embeds")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("file")
                        .WithDescription("Send or edit embeds using files on the server")
                        .WithType(ApplicationCommandOptionType.SubCommandGroup)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("send")
                            .WithDescription("Send an embed using a json file from the disk")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("filename")
                                .WithDescription("File name on the disk without extension where the embed is stored")
                                .WithRequired(true)
                                .WithType(ApplicationCommandOptionType.String))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("channel")
                                .WithDescription("Channel to send the message to")
                                .WithType(ApplicationCommandOptionType.Channel))
                        )
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("edit")
                            .WithDescription("Edit an embed using a json file from the disk")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("messageid")
                                .WithDescription("Id of the message to edit")
                                .WithRequired(true)
                                .WithType(ApplicationCommandOptionType.String))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("filename")
                                .WithDescription("File name on the disk without extension where the embed is stored")
                                .WithRequired(true)
                                .WithType(ApplicationCommandOptionType.String))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("channel")
                                .WithDescription("Channel to send the message to")
                                .WithType(ApplicationCommandOptionType.Channel)
                            ))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("delete")
                            .WithDescription("Delete an embed")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("messageid")
                                .WithDescription("Message id to delete")
                                .WithRequired(true)
                                .WithType(ApplicationCommandOptionType.String))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("channel")
                                .WithDescription("Channel to delete the message from. Default is the current one")
                                .WithType(ApplicationCommandOptionType.Channel))
                        ))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("msg")
                        .WithDescription("Send or edit embeds using text in message")
                        .WithType(ApplicationCommandOptionType.SubCommandGroup)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("send")
                            .WithDescription("Send an embed using json message")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("embed")
                                .WithDescription("Embed json to send")
                                .WithRequired(true)
                                .WithType(ApplicationCommandOptionType.String))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("channel")
                                .WithDescription("Channel to send the message to")
                                .WithType(ApplicationCommandOptionType.Channel))
                        )
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("edit")
                            .WithDescription("Edit an embed using a json file from the disk")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("messageid")
                                .WithDescription("Id of the message to edit")
                                .WithRequired(true)
                                .WithType(ApplicationCommandOptionType.String))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("embed")
                                .WithDescription("Embed json to send")
                                .WithRequired(true)
                                .WithType(ApplicationCommandOptionType.String))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("channel")
                                .WithDescription("Channel to send the message to")
                                .WithType(ApplicationCommandOptionType.Channel)
                            ))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("delete")
                            .WithDescription("Delete an embed")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("messageid")
                                .WithDescription("Message id to delete")
                                .WithRequired(true)
                                .WithType(ApplicationCommandOptionType.String))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("channel")
                                .WithDescription("Channel to delete the message from. Default is the current one")
                                .WithType(ApplicationCommandOptionType.Channel))
                        )
                    ));

            holder.AddCommand(commandBuilder, executor);
            return Task.CompletedTask;
        }
    }
}