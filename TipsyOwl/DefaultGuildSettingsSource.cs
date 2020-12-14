using System.Threading.Tasks;

namespace TipsyOwl
{
    public class DefaultGuildSettingsSource : IGuildSettingsSource
    {
        public GuildSettings DefaultSettings { get; }

        public DefaultGuildSettingsSource(GuildSettings defaultSettings)
        {
            DefaultSettings = defaultSettings;
        }

        public Task<GuildSettings> GetSettings(ulong guild)
        {
            return Task.FromResult(DefaultSettings);
        }
    }
}
