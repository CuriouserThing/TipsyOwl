using System.Text;
using System.Threading.Tasks;
using Bjerg;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WumpusHall;

namespace TipsyOwl
{
    public class CardFlavorViewBuilder : CardViewBuilder
    {
        public CardFlavorViewBuilder(ICatalogService catalogService, IOptionsSnapshot<TipsySettings> settings, ILogger<CardViewBuilder> logger) : base(catalogService, settings, logger)
        {
        }

        public override async Task<MessageView> BuildView(ICard item)
        {
            Embed embed = await BuildEmbed(item);
            return new MessageView(embed);
        }

        private async Task<Embed> BuildEmbed(ICard card)
        {
            ICard homeCard = await GetHomeCard(card);

            var descBuilder = new StringBuilder();

            string reg = GetRegionString(card.Region);
            string cat = GetFlavorCategory(card, homeCard);
            descBuilder.AppendLine($"{reg} {cat}");

            if (!string.IsNullOrWhiteSpace(card.FlavorText))
            {
                descBuilder.AppendLine();
                descBuilder.AppendLine(card.FlavorText);
            }

            EmbedBuilder eb = new EmbedBuilder()
                .WithTitle($"{card.Name}")
                .WithDescription(descBuilder.ToString());

            if (!string.IsNullOrWhiteSpace(card.ArtistName))
            {
                _ = eb.AddField("Illustration", card.ArtistName);
            }

            if (card.FullArtPath != null)
            {
                _ = eb.WithImageUrl(card.FullArtPath.AbsoluteUri);
            }

            return eb.Build();
        }

        private string GetFlavorCategory(ICard card, ICard homeCard)
        {
            // As far as I can tell, this approximates the rules for the header on each card flavor screen
            
            if (homeCard.Supertype?.Name == "Champion")
            {
                return card.Supertype?.Name ?? "Champion";
            }

            if (card.Subtypes.Count > 0)
            {
                return string.Join(' ', card.Subtypes);
            }

            if (homeCard.Type != null)
            {
                return card.Type?.Name ?? homeCard.Type.Name;
            }

            return "Card";
        }
    }
}
