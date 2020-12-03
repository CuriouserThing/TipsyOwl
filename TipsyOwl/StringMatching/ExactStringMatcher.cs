namespace TipsyOwl.StringMatching
{
    public class ExactStringMatcher : IStringMatcher
    {
        public string Source { get; }

        public ExactStringMatcher(string source)
        {
            Source = source;
        }

        public float GetMatchPct(string target)
        {
            return string.Equals(Source, target) ? 1.0f : 0.0f;
        }
    }
}
