using System.Collections.Generic;
using System.Threading.Tasks;
using Bjerg;
using Bjerg.Lor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WumpusHall;

namespace TipsyOwl
{
    public abstract class CardViewBuilder : IViewBuilder<ICard>
    {
        public CardViewBuilder(ICatalogService catalogService, IOptionsSnapshot<TipsySettings> settings, ILogger<CardViewBuilder> logger)
        {
            CatalogService = catalogService;
            Settings = settings.Value;
            Logger = logger;
        }

        protected ICatalogService CatalogService { get; }

        protected TipsySettings Settings { get; }

        protected ILogger Logger { get; }

        public abstract Task<MessageView> BuildView(ICard item);

        protected async Task<ICard> GetHomeCard(ICard card)
        {
            Catalog homeCatalog = await CatalogService.GetHomeCatalog(card.Version);
            if (homeCatalog.Cards.TryGetValue(card.Code, out ICard? homeCard))
            {
                return homeCard;
            }
            else
            {
                Logger.LogError($"The home catalog for v{card.Version} doesn't have a card with code {card.Code}. Attempting to use the provided card instead.");
                return card;
            }
        }

        protected IEnumerable<string> GetEmotes(string key)
        {
            char abbr = key[0];

            if (Settings.KeywordSprites.TryGetValue(key, out IList<string>? keywordSprites))
            {
                foreach (string keywordSprite in keywordSprites)
                {
                    if (Settings.SpriteEmotes.TryGetValue(keywordSprite, out ulong kw))
                    {
                        yield return $"<:{abbr}:{kw}>";
                    }
                    else
                    {
                        Logger.LogWarning($"{nameof(TipsySettings.KeywordSprites)} references the sprite {keywordSprite} for keyword {key}, but this sprite wasn't found in {nameof(TipsySettings.SpriteEmotes)}. Ignoring it.");
                    }
                }
            }
            else if (Settings.SpriteEmotes.TryGetValue(key, out ulong kw))
            {
                yield return $"<:{abbr}:{kw}>";
            }
        }

        protected string GetRegionString(LorFaction? region)
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
    }
}
