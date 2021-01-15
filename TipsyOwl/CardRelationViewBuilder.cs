using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bjerg;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WumpusHall;

namespace TipsyOwl
{
    public class CardRelationViewBuilder : CardViewBuilder
    {
        public CardRelationViewBuilder(ICatalogService catalogService, IOptionsSnapshot<TipsySettings> settings, ILogger<CardViewBuilder> logger) : base(catalogService, settings, logger)
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

            string desc;
            if (card.AssociatedCards.Count == 0)
            {
                desc = "*No related cards*";
            }
            else
            {
                var sb = new StringBuilder();
                foreach (ICard relCard in card.AssociatedCards)
                {
                    ICard homeRelCard = await GetHomeCard(relCard);
                    if (ExcludeRelatedCard(homeCard, homeRelCard)) { continue; }

                    sb.AppendLine(GetRelatedCardString(relCard, homeRelCard));
                }

                desc = sb.ToString();
            }

            EmbedBuilder eb = new EmbedBuilder()
                .WithTitle($"{card.Name}")
                .WithDescription(desc);
            return eb.Build();
        }

        private bool ExcludeRelatedCard(ICard homeCard, ICard homeRelCard)
        {
            if (homeCard.Supertype?.Name == "Champion" &&
                homeRelCard.Supertype?.Name == "Champion" &&
                homeCard.Name == homeRelCard.Name)
            {
                return true;
            }

            return false;
        }

        private string GetRelatedCardString(ICard card, ICard homeCard)
        {
            string emote = GetEmote(card, homeCard);
            string name = card.Name ?? card.Code.ToString();
            return $"{emote} {name}";
        }

        private string GetEmote(ICard card, ICard homeCard)
        {
            const string defaultEmote = "🔸";

            switch (homeCard.Type?.Name)
            {
                case "Ability":
                {
                    string[] emotes = GetEmotes("Skill").ToArray();
                    return emotes.Length == 0 ? defaultEmote : emotes[0];
                }
                case "Landmark":
                {
                    string[] emotes = GetEmotes("LandmarkVisualOnly").ToArray();
                    return emotes.Length == 0 ? defaultEmote : emotes[0];
                }
                case "Spell":
                {
                    string? key = card.SpellSpeed?.Key;
                    if (key is null) { return defaultEmote; }

                    string[] emotes = GetEmotes(key).ToArray();
                    return emotes.Length == 0 ? defaultEmote : emotes[0];
                }
                default:
                    return defaultEmote;
            }
        }
    }
}
