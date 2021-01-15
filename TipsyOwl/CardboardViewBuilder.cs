using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bjerg;
using Bjerg.Lor;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WumpusHall;

namespace TipsyOwl
{
    public class CardboardViewBuilder : CardViewBuilder
    {
        public CardboardViewBuilder(ICatalogService catalogService, IOptionsSnapshot<TipsySettings> settings, ILogger<CardboardViewBuilder> logger) : base(catalogService, settings, logger)
        {
        }

        private static Regex LinkRegex { get; } = new(@"<link=(.+?)>(.*?)<\/link>");
        private static Regex StyleRegex { get; } = new(@"<style=(.+?)>(.*?)<\/style>");
        private static Regex SpriteRegex { get; } = new(@"<sprite name=(.*?)>");
        private static Regex BrRegex { get; } = new(@"<br>");
        private static Regex NobrRegex { get; } = new(@"<nobr>(.*?)<\/nobr>");

        public override async Task<MessageView> BuildView(ICard item)
        {
            Embed embed = await BuildEmbed(item);
            return new MessageView(embed);
        }

        private async Task<Embed> BuildEmbed(ICard card)
        {
            ICard homeCard = await GetHomeCard(card);

            var descBuilder = new StringBuilder(); // for the embed description

            var keywordStrings = new List<string>();
            foreach (LorKeyword keyword in card.Keywords)
            {
                if (!Settings.HiddenCardKeywords.Contains(keyword.Key))
                {
                    keywordStrings.Add(GetKeywordString(keyword));
                }
            }

            if (keywordStrings.Count > 0)
            {
                _ = descBuilder.AppendLine(string.Join(" ", keywordStrings));
            }

            if (!string.IsNullOrWhiteSpace(card.Description))
            {
                string desc = ProcessFormattedText(card.Description);
                _ = descBuilder.AppendLine(desc);
            }

            EmbedBuilder eb = new EmbedBuilder()
                .WithTitle($"{card.Name}")
                .WithDescription(descBuilder.ToString());

            if (card.GameArtPath != null)
            {
                _ = eb.WithThumbnailUrl(card.GameArtPath.AbsoluteUri);
            }

            if (!string.IsNullOrWhiteSpace(card.LevelupDescription))
            {
                string levelup = ProcessFormattedText(card.LevelupDescription);
                _ = eb.AddField("Level Up", levelup);
            }

            if (card.Subtypes.Count == 1)
            {
                _ = eb.AddField("Subtype", card.Subtypes[0]);
            }
            else if (card.Subtypes.Count > 1)
            {
                _ = eb.AddField("Subtypes", string.Join(", ", card.Subtypes));
            }

            if (homeCard.Type != null)
            {
                if (homeCard.Type.Name == "Unit" || homeCard.Type.Name == "Spell" || homeCard.Type.Name == "Landmark")
                {
                    _ = eb.AddField("Cost", card.Cost, true);
                }

                if (homeCard.Type.Name == "Unit")
                {
                    _ = eb.AddField("Stats", $"{card.Attack}|{card.Health}", true);
                }
            }

            var rb = new StringBuilder();
            _ = rb.Append(GetRegionString(card.Region));
            if (card.Rarity != null && card.Rarity.Key != "None")
            {
                _ = rb.Append($" {GetRarityString(card.Rarity)}");
            }

            _ = eb.AddField("Region", rb.ToString(), true);

            return eb.Build();
        }

        private string GetKeywordString(LorKeyword keyword)
        {
            IEnumerable<string> values = GetEmotes(keyword.Key).Append(keyword.Name);
            string joined = string.Join(' ', values);
            return $"[**{joined}**]";
        }

        private string GetRarityString(LorRarity rarity)
        {
            if (Settings.RarityIconEmotes.TryGetValue(rarity.Key, out ulong rarityEmote))
            {
                return $"<:{rarity.Key}:{rarityEmote}> {rarity.Name}";
            }
            else
            {
                return rarity.Name;
            }
        }

        private string ProcessFormattedText(string text)
        {
            text = LinkRegex.Replace(text, EvalLinkMatch);
            text = StyleRegex.Replace(text, EvalStyleMatch);
            text = SpriteRegex.Replace(text, EvalSpriteMatch);
            text = BrRegex.Replace(text, EvalBrMatch);
            text = NobrRegex.Replace(text, EvalNobrMatch);

            return text;
        }

        private string EvalLinkMatch(Match match)
        {
            //string link = match.Groups[1].Value;
            string text = match.Groups[2].Value;

            return $"{text}";
        }

        private string EvalStyleMatch(Match match)
        {
            string style = match.Groups[1].Value;
            string text = match.Groups[2].Value;

            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            switch (style)
            {
                case "AssociatedCard":
                    return $"__**{text}**__";

                case "Keyword":
                case "Vocab":
                    return $"**{text}**";

                case "Parentheses":
                    return $"*{text}*";

                case "Variable":
                default:
                    return $"{text}";
            }
        }

        private string EvalSpriteMatch(Match match)
        {
            string sprite = match.Groups[1].Value;

            return Settings.SpriteEmotes.TryGetValue(sprite, out ulong emote) ? $"<:{sprite[0]}:{emote}> " : string.Empty;
        }

        private string EvalBrMatch(Match match)
        {
            return Environment.NewLine;
        }

        private string EvalNobrMatch(Match match)
        {
            string text = match.Groups[1].Value;

            return $"{text}";
        }
    }
}
