using System.Collections.Generic;

namespace TipsyOwl
{
    public class TipsySettings
    {
        public IList<int> LatestVersion { get; init; } = new List<int>(capacity: 0);

        public IList<string> HiddenCardKeywords { get; init; } = new List<string>(capacity: 0);

        public IDictionary<string, ulong> RegionIconEmotes { get; init; } = new Dictionary<string, ulong>(capacity: 0);

        public IDictionary<string, ulong> RegionIndicatorEmotes { get; init; } = new Dictionary<string, ulong>(capacity: 0);

        public IDictionary<string, ulong> SetIconEmotes { get; init; } = new Dictionary<string, ulong>(capacity: 0);

        public IDictionary<string, IList<string>> KeywordSprites { get; init; } = new Dictionary<string, IList<string>>(capacity: 0);

        public IDictionary<string, ulong> SpriteEmotes { get; init; } = new Dictionary<string, ulong>(capacity: 0);
    }
}
