using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bjerg;
using Discord;
using Microsoft.Extensions.Options;
using WumpusHall;

namespace TipsyOwl
{
    public class LocaleService
    {
        public LocaleService(IGuildSettingsSource guildSettingsSource, IOptionsSnapshot<TipsySettings> tipsySettings)
        {
            GuildSettingsSource = guildSettingsSource;
            TipsySettings = tipsySettings.Value;
        }

        private IGuildSettingsSource GuildSettingsSource { get; }

        private TipsySettings TipsySettings { get; }

        private static Locale HomeLocale { get; } = new("en", "US");

        public bool TryParseLocale(string s, out Locale? result)
        {
            // Dashes and underscores are both valid
            s = s.Replace('_', '-');

            // Stray whitespace on either side of either segment is valid
            string[] splits = s.Split('-', StringSplitOptions.TrimEntries);

            if (splits.Length != 2)
            {
                result = null;
                return false;
            }
            else
            {
                result = new Locale(splits[0], splits[1]);
                return true;
            }
        }

        public bool LocaleIsRecognized(Locale locale)
        {
            foreach (string s in TipsySettings.Locales)
            {
                if (TryParseLocale(s, out Locale? other) && locale == other!)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<Locale> GetGuildLocaleAsync(IGuild? guild)
        {
            if (guild is null)
            {
                return HomeLocale;
            }

            GuildSettings settings = await GuildSettingsSource.GetSettings(guild.Id);
            if (settings.Locale is null)
            {
                return HomeLocale;
            }

            return TryParseLocale(settings.Locale, out Locale? locale) ? locale! : HomeLocale;
        }

        public IEnumerable<Locale> GetRecognizedLocales()
        {
            foreach (string s in TipsySettings.Locales)
            {
                if (TryParseLocale(s, out Locale? other))
                {
                    yield return other!;
                }
            }
        }
    }
}
