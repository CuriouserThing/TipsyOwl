using Discord.Commands;

namespace TipsyOwl
{
    public class TipsyRuntimeResult : RuntimeResult
    {
        private TipsyRuntimeResult(CommandError? error, string reason) : base(error, reason)
        {
        }

        public static RuntimeResult FromSuccess(string reason = "Command was successful.")
        {
            return new TipsyRuntimeResult(null, reason);
        }

        public static RuntimeResult FromError(string reason)
        {
            return new TipsyRuntimeResult(CommandError.Unsuccessful, reason);
        }
    }
}
