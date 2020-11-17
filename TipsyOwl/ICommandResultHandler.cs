using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace TipsyOwl
{
    public interface ICommandResultHandler
    {
        Task HandleResult(Optional<CommandInfo> info, ICommandContext context, IResult result);
    }
}
