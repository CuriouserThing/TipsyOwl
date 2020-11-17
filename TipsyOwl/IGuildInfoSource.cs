using System.Threading.Tasks;

namespace TipsyOwl
{
    public interface IGuildInfoSource
    {
        Task<CommandHandlingInfo> GetCommandHandlingInfo(ulong guild);
    }
}
