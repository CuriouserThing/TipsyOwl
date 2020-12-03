using Bjerg;

namespace TipsyOwl
{
    public interface ICardWeighter
    {
        public float GetWeightingFactor(ICard card);
    }
}
