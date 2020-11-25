namespace TipsyOwl
{
    /// <summary>
    /// Interface for fuzzy-matching a single source string to any target string.
    /// </summary>
    public interface ISourceStringMatcher
    {
        /// <summary>
        /// Fuzzy-match the source string to a target string. Not guaranteed to be thread-safe.
        /// </summary>
        /// <param name="target">The string to match the source string to.</param>
        /// <returns>A percentage ranging from 100% (perfect match) to 0% (no match).</returns>
        float GetMatchPct(string target);
    }
}
