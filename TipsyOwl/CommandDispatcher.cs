using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace TipsyOwl
{
    public class CommandDispatcher
    {
        public CommandDispatcher(DiscordSocketClient client, CommandService commands, IServiceProvider services, IGuildSettingsSource guildSettingsSource)
        {
            Client = client;
            Commands = commands;
            Services = services;
            GuildSettingsSource = guildSettingsSource;
        }

        private DiscordSocketClient Client { get; }

        private CommandService Commands { get; }

        private IServiceProvider Services { get; }

        private IGuildSettingsSource GuildSettingsSource { get; }

        public async Task DispatchFromMessageReceivedAsync(SocketMessage message)
        {
            // Never reply to messages from non-humans >:{
            if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot)
            {
                return;
            }

            var context = new SocketCommandContext(Client, userMessage);

            // Flag as valid if user mentions bot (regardless of current channel type), and advance argPos past mention string.
            var argPos = 0;
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
                    GuildSettings settings = await GuildSettingsSource.GetSettings(guildChannel.Guild.Id);

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
                _ = await Commands.ExecuteAsync(context, argPos, Services);
            }
        }

        private async Task HandleInlineCommandsAsync(string text, ICommandContext context, GuildSettings settings)
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
            for (var i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                string command = match.Groups[1].Value.Trim();
                _ = await Commands.ExecuteAsync(context, $"{alias} {command}", Services);
            }
        }
    }
}
