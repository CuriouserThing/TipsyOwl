using System.Collections.Generic;
using System.Linq;
using Bjerg;
using Bjerg.CatalogSearching;
using Bjerg.Lor;

namespace TipsyOwl
{
    public class OmniSearcher
    {
        public OmniSearcher(Catalog homeCatalog, Catalog outputCatalog, CatalogItemSearcher<ICard> cardSearcher, CatalogItemSearcher<LorKeyword> keywordSearcher, DeckEmbedFactory deckEmbedFactory, CardEmbedFactory cardEmbedFactory, KeywordEmbedFactory keywordEmbedFactory)
        {
            HomeCatalog = homeCatalog;
            OutputCatalog = outputCatalog;
            CardSearcher = cardSearcher;
            KeywordSearcher = keywordSearcher;
            DeckEmbedFactory = deckEmbedFactory;
            CardEmbedFactory = cardEmbedFactory;
            KeywordEmbedFactory = keywordEmbedFactory;
        }

        private Catalog HomeCatalog { get; }

        private Catalog OutputCatalog { get; }

        private CatalogItemSearcher<ICard> CardSearcher { get; }

        private CatalogItemSearcher<LorKeyword> KeywordSearcher { get; }

        private DeckEmbedFactory DeckEmbedFactory { get; }

        private CardEmbedFactory CardEmbedFactory { get; }

        private KeywordEmbedFactory KeywordEmbedFactory { get; }

        public IReadOnlyList<IEmbeddable> Search(string lookup)
        {
            var matches = new List<IEmbeddable>();

            if (SearchDeckByCode && Deck.TryFromCode(lookup, OutputCatalog, out Deck? deck))
            {
                var deckEmbeddable = new DeckEmbeddable(deck!, DeckEmbedFactory, HomeCatalog);
                matches.Add(deckEmbeddable);
            }

            if (SearchCardByCode && OutputCatalog.Cards.TryGetValue(lookup, out ICard? card))
            {
                IReadOnlyList<ICard> expansion = CardSearcher.ItemSelector.Expand(card);
                var cardEmbeddable = new CardEmbeddable(card, expansion, CardEmbedFactory, HomeCatalog);
                matches.Add(cardEmbeddable);
            }

            var fuzzyMatches = new List<(IEmbeddable, float)>();

            if (SearchCardsByName)
            {
                IEnumerable<(IEmbeddable, float)> cardMatches = CardSearcher.Search(lookup)
                    .Select(m => (new CardEmbeddable(m.Item, m.ItemExpansion, CardEmbedFactory, HomeCatalog) as IEmbeddable, m.MatchPct));
                fuzzyMatches.AddRange(cardMatches);
            }

            if (SearchKeywordsByName)
            {
                IEnumerable<(IEmbeddable, float)> keywordMatches = KeywordSearcher.Search(lookup)
                    .Select(m => (new KeywordEmbeddable(m.Item, m.ItemExpansion, KeywordEmbedFactory) as IEmbeddable, m.MatchPct));
                fuzzyMatches.AddRange(keywordMatches);
            }

            IEnumerable<IEmbeddable> sortedMatches = fuzzyMatches
                .OrderByDescending(m => m.Item2)
                .Select(m => m.Item1);

            matches.AddRange(sortedMatches);
            return matches;
        }

        #region Config

        public bool SearchDeckByCode { get; init; } = false;

        public bool SearchCardByCode { get; init; } = false;

        public bool SearchCardsByName { get; init; } = false;

        public bool SearchKeywordsByName { get; init; } = false;

        #endregion
    }
}
