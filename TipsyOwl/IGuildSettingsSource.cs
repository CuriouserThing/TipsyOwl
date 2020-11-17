using System.Threading.Tasks;

namespace TipsyOwl
{
    public interface IGuildSettingsSource
    {
        Task<CommandSettings> GetCommandSettings(ulong guild);
    }
}
