using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TipsyOwl
{
    public class CommandHandler : IDisposable
    {
        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IGuildSettingsSource GuildSettingsSource { get; }
        private ICommandResultHandler ResultHandler { get; }
        private IServiceProvider Services { get; }

        private CommandHandler(DiscordSocketClient client, CommandService commands, IGuildSettingsSource guildSettingsSource, ICommandResultHandler resultHandler, IServiceProvider services)
        {
            Client = client;
            Commands = commands;
            GuildSettingsSource = guildSettingsSource;
            ResultHandler = resultHandler;
            Services = services;
        }

        private void Register()
        {
            Client.MessageReceived += ClientOnMessageReceivedAsync;
            Commands.CommandExecuted += CommandsOnCommandExecutedAsync;
        }

        private void Deregister()
        {
            Client.MessageReceived -= ClientOnMessageReceivedAsync;
            Commands.CommandExecuted -= CommandsOnCommandExecutedAsync;
        }

        public static CommandHandler CreateAndRegister(DiscordSocketClient client, CommandService commands, IGuildSettingsSource guildSettingsSource, ICommandResultHandler resultHandler, IServiceProvider services)
        {
            var handler = new CommandHandler(client, commands, guildSettingsSource, resultHandler, services);
            handler.Register();
            return handler;
        }

        private async Task ClientOnMessageReceivedAsync(SocketMessage message)
        {
            // Never reply to messages from non-humans >:{
            if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot)
            {
                return;
            }

            var context = new SocketCommandContext(Client, userMessage);

            // Flag as valid if user mentions bot (regardless of current channel type), and advance argPos past mention string.
            int argPos = 0;
            bool isValid = userMessage.HasMentionPrefix(Client.CurrentUser, ref argPos);

            switch (userMessage.Channel)
            {
                // DMs are always valid.
                case SocketDMChannel _:
                case SocketGroupChannel _:
                    isValid = true;
                    break;

                // Guild channels are valid with a string prefix.
                case SocketTextChannel guildChannel:
                {
                    CommandSettings settings = await GuildSettingsSource.GetCommandSettings(guildChannel.Id);

                    string? prefix = settings.CommandPrefix;
                    if (prefix != null && userMessage.HasStringPrefix(prefix, ref argPos))
                    {
                        isValid = true;
                    }

                    // (While we're here, handle inline commands found past the mention/string prefix.)
                    string text = userMessage.Content.Substring(argPos);
                    await HandleInlineCommandsAsync(text, context, settings);

                    break;
                }
            }

            // Try to execute a command if any of the above checks passed!
            if (isValid)
            {
                using IServiceScope scope = Services.CreateScope();
                _ = await Commands.ExecuteAsync(context, argPos, scope.ServiceProvider);
            }
        }

        private async Task HandleInlineCommandsAsync(string text, SocketCommandContext context, CommandSettings settings)
        {
            if (!settings.AllowInlineCommands)
            {
                return;
            }

            string? opener = settings.InlineCommandOpener;
            string? closer = settings.InlineCommandCloser;
            string? alias = settings.InlineCommandAlias;
            if (opener is null || closer is null || alias is null)
            {
                return;
            }

            var regex = new Regex(Regex.Escape(opener) + @"(.+?)" + Regex.Escape(closer));
            MatchCollection matches = regex.Matches(text);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                string command = match.Groups[1].Value.Trim();
                using IServiceScope s = Services.CreateScope();
                _ = await Commands.ExecuteAsync(context, $"{alias} {command}", s.ServiceProvider);
            }
        }

        private async Task CommandsOnCommandExecutedAsync(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
        {
            await ResultHandler.HandleResult(arg1, arg2, arg3);
        }

        #region IDisposable

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Deregister();
                }

                _disposedValue = true;
            }
        }

        ~CommandHandler()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
