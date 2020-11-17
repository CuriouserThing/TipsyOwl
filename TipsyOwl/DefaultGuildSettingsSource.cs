using System.Threading.Tasks;

namespace TipsyOwl
{
    public class DefaultGuildSettingsSource : IGuildSettingsSource
    {
        public CommandSettings DefaultCommandSettings { get; }

        public DefaultGuildSettingsSource(CommandSettings defaultCommandSettings)
        {
            DefaultCommandSettings = defaultCommandSettings;
        }

        public Task<CommandSettings> GetCommandSettings(ulong guild)
        {
            return Task.FromResult(DefaultCommandSettings);
        }
    }
}
