using System.Collections.Generic;
using Bjerg;
using Discord;

namespace TipsyOwl
{
    public class DeckEmbeddable : IEmbeddable
    {
        public DeckEmbeddable(Deck deck, DeckEmbedFactory factory, Catalog homeCatalog)
        {
            Deck = deck;
            Factory = factory;
            HomeCatalog = homeCatalog;
        }

        public Deck Deck { get; }

        public DeckEmbedFactory Factory { get; }

        public Catalog HomeCatalog { get; }

        public string Name => Deck.Code;

        public Embed GetMainEmbed()
        {
            return Factory.BuildEmbed(Deck, HomeCatalog);
        }

        public IReadOnlyList<Embed> GetAllEmbeds()
        {
            return new[] { GetMainEmbed() };
        }
    }
}
