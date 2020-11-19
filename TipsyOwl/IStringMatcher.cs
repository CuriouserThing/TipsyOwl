namespace TipsyOwl
{
    /// <summary>
    /// Interface for fuzzy-matching a lookup string to a candidate string, ranging from 100% (perfect match) to 0% (no match).
    /// </summary>
    public interface IStringMatcher
    {
        float GetMatchPct(string lookup, string candidate);
    }
}
