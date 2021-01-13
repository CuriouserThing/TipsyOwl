using System;
using System.Collections.Generic;
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
    public class CardboardViewBuilder : IViewBuilder<ICard>
    {
        public CardboardViewBuilder(ICatalogService catalogService, IOptionsSnapshot<TipsySettings> settings, ILogger<CardboardViewBuilder> logger)
        {
            CatalogService = catalogService;
            Settings = settings.Value;
            Logger = logger;
        }

        private ICatalogService CatalogService { get; }

        private TipsySettings Settings { get; }

        private ILogger Logger { get; }

        private static Regex LinkRegex { get; } = new(@"<link=(.+?)>(.*?)<\/link>");
        private static Regex StyleRegex { get; } = new(@"<style=(.+?)>(.*?)<\/style>");
        private static Regex SpriteRegex { get; } = new(@"<sprite name=(.*?)>");
        private static Regex BrRegex { get; } = new(@"<br>");
        private static Regex NobrRegex { get; } = new(@"<nobr>(.*?)<\/nobr>");

        public async Task<MessageView> BuildView(ICard item)
        {
            Embed embed = await BuildEmbed(item);
            return new MessageView(embed);
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

        private string ProcessFormattedText(string text)
        {
            text = LinkRegex.Replace(text, EvalLinkMatch);
            text = StyleRegex.Replace(text, EvalStyleMatch);
            text = SpriteRegex.Replace(text, EvalSpriteMatch);
            text = BrRegex.Replace(text, EvalBrMatch);
            text = NobrRegex.Replace(text, EvalNobrMatch);

            return text;
        }

        private string GetKeywordString(LorKeyword keyword)
        {
            char abbr = keyword.Key[0];
            string emotes;

            if (Settings.KeywordSprites.TryGetValue(keyword.Key, out IList<string>? keywordSprites))
            {
                var sb = new StringBuilder();
                foreach (string keywordSprite in keywordSprites)
                {
                    if (Settings.SpriteEmotes.TryGetValue(keywordSprite, out ulong kw))
                    {
                        _ = sb.Append($"<:{abbr}:{kw}> ");
                    }
                    else
                    {
                        Logger.LogWarning($"{nameof(TipsySettings.KeywordSprites)} references the sprite {keywordSprite} for keyword {keyword.Key}, but this sprite wasn't found in {nameof(TipsySettings.SpriteEmotes)}. Ignoring it.");
                    }
                }

                emotes = sb.ToString();
            }
            else if (Settings.SpriteEmotes.TryGetValue(keyword.Key, out ulong kw))
            {
                emotes = $"<:{abbr}:{kw}> ";
            }
            else
            {
                emotes = string.Empty;
            }

            return $"[**{emotes}{keyword.Name}**]";
        }

        private string GetRegionString(LorFaction? region)
        {
            string regionKey, regionName, regionAbbr;
            if (region is null)
            {
                regionKey = "All";
                regionName = "No Region";
                regionAbbr = "x"; // use a dummy char for the emote name
            }
            else
            {
                regionKey = region.Key;
                regionName = region.Name;
                regionAbbr = region.Abbreviation; // use two-letter faction code for the emote name
            }

            return Settings.RegionIconEmotes.TryGetValue(regionKey, out ulong regionEmote)
                ? $"<:{regionAbbr}:{regionEmote}> {regionName}"
                : $"{regionName}";
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

        private async Task<Embed> BuildEmbed(ICard card)
        {
            Catalog homeCatalog = await CatalogService.GetHomeCatalog(card.Version);
            if (!homeCatalog.Cards.TryGetValue(card.Code, out ICard? homeCard))
            {
                homeCard = card;
                Logger.LogError($"The home catalog for v{card.Version} doesn't have a card with code {card.Code}. Attempting to use the provided card instead.");
            }

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

            if (card.Description != null)
            {
                string desc = ProcessFormattedText(card.Description);
                _ = descBuilder.AppendLine(desc);
            }

            EmbedBuilder eb = new EmbedBuilder()
                .WithTitle($"**{card.Name}**")
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
    }
}
