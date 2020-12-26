using System.Threading.Tasks;

namespace TipsyOwl
{
    public class DefaultGuildSettingsSource : IGuildSettingsSource
    {
        public DefaultGuildSettingsSource(GuildSettings defaultSettings)
        {
            DefaultSettings = defaultSettings;
        }

        public GuildSettings DefaultSettings { get; }

        public Task<GuildSettings> GetSettings(ulong guild)
        {
            return Task.FromResult(DefaultSettings);
        }
    }
}
