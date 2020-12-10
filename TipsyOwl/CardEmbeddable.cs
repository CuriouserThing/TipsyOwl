using System.Collections.Generic;
using System.Linq;
using Bjerg;
using Discord;

namespace TipsyOwl
{
    public class CardEmbeddable : IEmbeddable
    {
        public CardEmbeddable(ICard card, IReadOnlyList<ICard> cardExpansion, CardEmbedFactory factory, Catalog homeCatalog)
        {
            Card = card;
            CardExpansion = cardExpansion;
            Factory = factory;
            HomeCatalog = homeCatalog;
        }

        public ICard Card { get; }

        public IReadOnlyList<ICard> CardExpansion { get; }

        public CardEmbedFactory Factory { get; }

        public Catalog HomeCatalog { get; }

        public string Name
        {
            get
            {
                if (Card.Collectible)
                {
                    return Factory.GetRegionCardString(Card);
                }
                else
                {
                    return Card.Name ?? "Unknown Card";
                }
            }
        }

        public Embed GetMainEmbed()
        {
            return Factory.BuildEmbed(Card, HomeCatalog);
        }

        public IReadOnlyList<Embed> GetAllEmbeds()
        {
            return CardExpansion
                .Select(c => Factory.BuildEmbed(c, HomeCatalog))
                .ToArray();
        }
    }
}
