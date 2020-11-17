using System.Threading.Tasks;

namespace TipsyOwl
{
    public class DefaultGuildInfoSource : IGuildInfoSource
    {
        public CommandHandlingInfo DefaultCommandHandlingInfo { get; }

        public DefaultGuildInfoSource(CommandHandlingInfo defaultCommandHandlingInfo)
        {
            DefaultCommandHandlingInfo = defaultCommandHandlingInfo;
        }

        public Task<CommandHandlingInfo> GetCommandHandlingInfo(ulong guild)
        {
            return Task.FromResult(DefaultCommandHandlingInfo);
        }
    }
}
