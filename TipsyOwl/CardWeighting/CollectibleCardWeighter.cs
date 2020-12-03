using Bjerg;

namespace TipsyOwl.CardWeighting
{
    public class CollectibleCardWeighter : ICardWeighter
    {
        public CollectibleCardWeighter(float collectibleFactor)
        {
            CollectibleFactor = collectibleFactor;
        }

        public float CollectibleFactor { get; }

        public float GetWeightingFactor(ICard card)
        {
            return card.Collectible ? CollectibleFactor : 1f;
        }
    }
}
