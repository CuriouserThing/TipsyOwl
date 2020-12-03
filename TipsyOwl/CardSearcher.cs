using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bjerg;

namespace TipsyOwl
{
    public class CardSearcher
    {
        public CardSearcher(Catalog localCatalog, Catalog homeCatalog, StringMatcherFactory stringMatcherFactory, ICardMatchSelector matchSelector, ICardWeighter cardWeighter)
        {
            LocalCatalog = localCatalog;
            HomeCatalog = homeCatalog;
            StringMatcherFactory = stringMatcherFactory;
            MatchSelector = matchSelector;
            CardWeighter = cardWeighter;
        }

        private Catalog LocalCatalog { get; }

        private Catalog HomeCatalog { get; }

        private StringMatcherFactory StringMatcherFactory { get; }

        private ICardMatchSelector MatchSelector { get; }

        private ICardWeighter CardWeighter { get; }

        public float MatchThreshold { get; set; } = 0.5f;

        public SearchResult<ICard> SearchByName(string lookup)
        {
            CultureInfo cultureInfo = LocalCatalog.Locale.CultureInfo;
            lookup = lookup.ToLower(cultureInfo);
            IStringMatcher matcher = StringMatcherFactory.CreateStringMatcher(lookup);
            var cardNameMatches = new List<(float, IEnumerable<ICard>)>();

            foreach (IGrouping<string, ICard> cardGroup in LocalCatalog.Cards.Values
                .Where(c => c.Name != null)
                .GroupBy(c => c.Name!))
            {
                string name = cardGroup.Key.ToLower(cultureInfo);
                float m = matcher.GetMatchPct(name);
                if (m > MatchThreshold)
                {
                    cardNameMatches.Add((m, cardGroup));
                }
            }

            if (cardNameMatches.Count == 0)
            {
                return SearchResult<ICard>.FromFailedSearch();
            }

            (float, ICard)[] cardMatches = cardNameMatches
                .Select(ReduceAndWeight)
                .OrderByDescending(mc => mc.Item1)
                .ToArray();

            ICard match = cardMatches[0].Item2;
            IReadOnlyList<ICard> expandedMatch = MatchSelector.Expand(match, LocalCatalog, HomeCatalog);
            if (expandedMatch.Count < 2)
            {
                expandedMatch = new[] {match};
            }

            if (cardMatches.Length == 1)
            {
                return SearchResult<ICard>.FromSuccessfulSearch(match, expandedMatch);
            }
            else
            {
                ICard[] weakerMatches = cardMatches
                    .Skip(1)
                    .Select(mc => mc.Item2)
                    .ToArray();
                return SearchResult<ICard>.FromSuccessfulSearch(match, expandedMatch, weakerMatches);
            }
        }

        private (float, ICard) ReduceAndWeight((float, IEnumerable<ICard>) cardNameMatches)
        {
            (float m, IEnumerable<ICard> cards) = cardNameMatches;
            ICard card = MatchSelector.Reduce(cards);
            m *= CardWeighter.GetWeightingFactor(card);
            return (m, card);
        }
    }
}
