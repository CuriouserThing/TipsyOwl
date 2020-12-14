using System.Threading.Tasks;

namespace TipsyOwl
{
    public interface IGuildSettingsSource
    {
        Task<GuildSettings> GetSettings(ulong guild);
    }
}
