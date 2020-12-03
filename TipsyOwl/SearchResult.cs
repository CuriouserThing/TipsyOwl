using System;
using System.Collections.Generic;

namespace TipsyOwl
{
    public class SearchResult<T> where T : class
    {
        /// <summary>
        ///     Whether the search successfully found an item match.
        /// </summary>
        public bool MatchFound { get; private init; }

        /// <summary>
        ///     Null if <see cref="MatchFound" /> is false. Otherwise, the primary item that the search matched.
        /// </summary>
        public T? Match { get; private init; }

        /// <summary>
        ///     Empty if <see cref="MatchFound" /> is false. Otherwise, either a list containing just <see cref="Match" /> or a
        ///     longer, ordered list of items that the search expanded from the primary match.
        /// </summary>
        public IReadOnlyList<T> ExpandedMatch { get; private init; } = Array.Empty<T>();

        /// <summary>
        ///     Empty if <see cref="MatchFound" /> is false. Otherwise, a possibly-empty, ordered list of items that the search
        ///     matched less strongly than the primary match.
        /// </summary>
        public IReadOnlyList<T> WeakerMatches { get; private init; } = Array.Empty<T>();

        public static SearchResult<T> FromSuccessfulSearch(T match, IReadOnlyList<T> expandedMatch, IReadOnlyList<T> weakerMatches)
        {
            return new SearchResult<T>
            {
                MatchFound = true,
                Match = match,
                ExpandedMatch = expandedMatch,
                WeakerMatches = weakerMatches
            };
        }

        public static SearchResult<T> FromSuccessfulSearch(T match, IReadOnlyList<T> expandedMatch)
        {
            return new SearchResult<T>
            {
                MatchFound = true,
                Match = match,
                ExpandedMatch = expandedMatch
            };
        }

        public static SearchResult<T> FromFailedSearch()
        {
            return new SearchResult<T>();
        }
    }
}
