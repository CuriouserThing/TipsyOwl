using Bjerg;

namespace TipsyOwl.CardWeighting
{
    public class PassthroughCardWeighter : ICardWeighter
    {
        public float GetWeightingFactor(ICard card)
        {
            return 1f;
        }
    }
}
