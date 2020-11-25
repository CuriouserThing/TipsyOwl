namespace TipsyOwl.StringMatching
{
    public class ExactSourceStringMatcher : ISourceStringMatcher
    {
        public string Source { get; }

        public ExactSourceStringMatcher(string source)
        {
            Source = source;
        }

        public float GetMatchPct(string target)
        {
            return string.Equals(Source, target) ? 1.0f : 0.0f;
        }
    }
}
