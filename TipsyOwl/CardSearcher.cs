using Bjerg;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TipsyOwl
{
    public class CardSearcher
    {
        private Catalog LocalCatalog { get; }

        private Catalog HomeCatalog { get; }

        private IStringMatcher StringMatcher { get; }

        private ICardMatchSelector MatchSelector { get; }

        public CardSearcher(Catalog localCatalog, Catalog homeCatalog, IStringMatcher stringMatcher, ICardMatchSelector matchSelector)
        {
            LocalCatalog = localCatalog;
            HomeCatalog = homeCatalog;
            StringMatcher = stringMatcher;
            MatchSelector = matchSelector;
        }

        public float AbsoluteMatchThreshold { get; set; } = 0.5f;

        public float RelativeMatchThreshold { get; set; } = 0.25f;

        public CardSearchResult SearchByName(string lookup)
        {
            CultureInfo cultureInfo = LocalCatalog.Locale.CultureInfo;
            lookup = lookup.ToLower(cultureInfo);
            var cards = new List<(float, IEnumerable<ICard>)>();

            foreach (IGrouping<string, ICard> cardGroup in LocalCatalog.Cards.Values
                .Where(c => c.Name != null)
                .GroupBy(c => c.Name!))
            {
                string name = cardGroup.Key.ToLower(cultureInfo);
                float m = StringMatcher.GetMatchPct(lookup, name);
                if (m > AbsoluteMatchThreshold)
                {
                    cards.Add((m, cardGroup));
                }
            }

            if (cards.Count == 0)
            {
                return CardSearchResult.FromFailedSearch();
            }

            cards.Sort(CompareCandidatesDescending);

            ICard match = MatchSelector.Reduce(cards[0].Item2);
            IReadOnlyList<ICard> expandedMatch = MatchSelector.Expand(match, LocalCatalog, HomeCatalog);
            if (expandedMatch.Count < 2)
            {
                expandedMatch = new[] {match};
            }

            if (cards.Count == 1)
            {
                return CardSearchResult.FromSuccessfulSearch(match, expandedMatch);
            }
            else
            {
                float mTop = cards[0].Item1;
                ICard[] weakerMatches = cards
                    .Skip(1)
                    .Where(cs => cs.Item1 > mTop - RelativeMatchThreshold)
                    .Select(cs => MatchSelector.Reduce(cs.Item2))
                    .ToArray();
                return CardSearchResult.FromSuccessfulSearch(match, expandedMatch, weakerMatches);
            }
        }

        private static int CompareCandidatesDescending((float, IEnumerable<ICard>) x, (float, IEnumerable<ICard>) y)
        {
            (float mx, _) = x;
            (float my, _) = y;
            if (mx < my)
            {
                return +1;
            }
            else if (mx > my)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
