using Bjerg;
using System;
using System.Collections.Generic;

namespace TipsyOwl
{
    public class CardSearchResult
    {
        /// <summary>
        /// Whether the search successfully found a card match.
        /// </summary>
        public bool MatchFound { get; private init; } = false;

        /// <summary>
        /// Null if <see cref="MatchFound"/> is false. Otherwise, the primary card that the search matched.
        /// </summary>
        public ICard? Match { get; private init; } = null;

        /// <summary>
        /// Empty if <see cref="MatchFound"/> is false. Otherwise, either a list containing just <see cref="Match"/> or a longer, ordered list of cards that the search expanded from the primary match.
        /// </summary>
        public IReadOnlyList<ICard> ExpandedMatch { get; private init; } = Array.Empty<ICard>();

        /// <summary>
        /// Empty if <see cref="MatchFound"/> is false. Otherwise, a possibly-empty, ordered list of cards that the search matched less strongly than the primary match. All cards in this list have distinct <see cref="ICard.Name"/> values.
        /// </summary>
        public IReadOnlyList<ICard> WeakerMatches { get; private init; } = Array.Empty<ICard>();

        public static CardSearchResult FromSuccessfulSearch(ICard match, IReadOnlyList<ICard> expandedMatch, IReadOnlyList<ICard> weakerMatches)
        {
            return new CardSearchResult
            {
                MatchFound = true,
                Match = match,
                ExpandedMatch = expandedMatch,
                WeakerMatches = weakerMatches,
            };
        }

        public static CardSearchResult FromSuccessfulSearch(ICard match, IReadOnlyList<ICard> expandedMatch)
        {
            return new CardSearchResult
            {
                MatchFound = true,
                Match = match,
                ExpandedMatch = expandedMatch
            };
        }

        public static CardSearchResult FromFailedSearch()
        {
            return new CardSearchResult();
        }
    }
}
