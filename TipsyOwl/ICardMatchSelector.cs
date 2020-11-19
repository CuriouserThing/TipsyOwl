using Bjerg;
using System.Collections.Generic;

namespace TipsyOwl
{
    /// <summary>
    /// Interface for a) selecting a single matched card from a collection of same-named matched cards, and b) optionally expanding that match into an ordered list of arbitrary cards from a catalog.
    /// </summary>
    public interface ICardMatchSelector
    {
        ICard Reduce(IEnumerable<ICard> cards);

        IReadOnlyList<ICard> Expand(ICard card, Catalog localCatalog, Catalog homeCatalog);
    }
}
