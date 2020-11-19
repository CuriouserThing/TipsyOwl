using Bjerg;
using System.Collections.Generic;
using System.Linq;

namespace TipsyOwl.CardMatchSelection
{
    public class BasicCardMatchSelector : ICardMatchSelector
    {
        public ICard Reduce(IEnumerable<ICard> cards)
        {
            return cards
                .OrderByDescending(c => c.Collectible) // favor collectibles
                .ThenBy(c => c.Code.TNumber) // favor "root" cards without a T number (0)
                .ThenBy(c => c.Code) // simple non-arbitrary tiebreaker
                .First();
        }

        public IReadOnlyList<ICard> Expand(ICard card, Catalog localCatalog, Catalog homeCatalog)
        {
            ICard homeCard = homeCatalog.Cards[card.Code];

            // Expand the selected card if it's a lv. 1 champ
            if (card.Collectible && card.Code.TNumber == 0 && homeCard.Supertype?.Name == "Champion")
            {
                return ExpandChamp(card, localCatalog, homeCatalog);
            }
            else
            {
                return new[] {card};
            }
        }

        private IReadOnlyList<ICard> ExpandChamp(ICard card, Catalog localCatalog, Catalog homeCatalog)
        {
            // The potential lv. 2 champs for this card are cards that share its code (sans T number)
            CardCode? bc = card.Code;
            ICard[] champCards = localCatalog.Cards.Values
                .Where(c =>
                    c.Code.Number == bc.Number &&
                    c.Code.TNumber != 0 &&
                    c.Code.Faction == bc.Faction &&
                    c.Code.Set == bc.Set)
                .ToArray();

            // Of these, we [currently] only recognize the lv. 2 champ as the *single* card that either:

            // - Shares its name with the lv. 1 champ (e.g. Anivia but not Eggnivia)
            ICard[] sameNames = champCards
                .Where(c => c.Name == card.Name)
                .ToArray();
            if (sameNames.Length == 1) { return new[] {card, sameNames[0]}; }

            // - Is a Champion Unit (e.g. Spider Queen Elise)
            ICard[] champUnits = champCards
                .Select(c => homeCatalog.Cards[c.Code])
                .Where(c => c.Supertype?.Name == "Champion" && c.Type?.Name == "Unit")
                .ToArray();
            if (champUnits.Length == 1) { return new[] {card, champUnits[0]}; }

            // ...otherwise, just return the lv. 1 champ
            return new[] {card};
        }
    }
}
