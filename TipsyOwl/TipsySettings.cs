using System.Collections.Generic;

namespace TipsyOwl
{
    public class TipsySettings
    {
        public IList<int> LatestVersion { get; set; } = new List<int>(capacity: 0);

        public IList<string> HiddenCardKeywords { get; set; } = new List<string>(capacity: 0);

        public IDictionary<string, ulong> RegionIconEmotes { get; set; } = new Dictionary<string, ulong>(capacity: 0);

        public IDictionary<string, ulong> RegionIndicatorEmotes { get; set; } = new Dictionary<string, ulong>(capacity: 0);

        public IDictionary<string, ulong> SetIconEmotes { get; set; } = new Dictionary<string, ulong>(capacity: 0);

        public IDictionary<string, IList<string>> KeywordSprites { get; set; } = new Dictionary<string, IList<string>>(capacity: 0);

        public IDictionary<string, ulong> SpriteEmotes { get; set; } = new Dictionary<string, ulong>(capacity: 0);
    }
}
