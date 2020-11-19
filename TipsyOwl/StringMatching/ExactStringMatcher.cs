namespace TipsyOwl.StringMatching
{
    public class ExactStringMatcher : IStringMatcher
    {
        public float GetMatchPct(string lookup, string candidate)
        {
            return string.Equals(lookup, candidate) ? 1.0f : 0.0f;
        }
    }
}
