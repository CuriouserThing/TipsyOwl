using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bjerg.Lor;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WumpusHall;

namespace TipsyOwl
{
    public class KeywordViewBuilder : IViewBuilder<LorKeyword>
    {
        public KeywordViewBuilder(IOptionsSnapshot<TipsySettings> settings, ILogger<CardboardViewBuilder> logger)
        {
            Settings = settings.Value;
            Logger = logger;
        }

        private TipsySettings Settings { get; }

        private ILogger Logger { get; }

        public Task<MessageView> BuildView(LorKeyword item)
        {
            var view = new MessageView(BuildEmbed(item));
            return Task.FromResult(view);
        }

        private Embed BuildEmbed(LorKeyword keyword)
        {
            return new EmbedBuilder()
                .WithTitle(GetKeywordString(keyword))
                .WithDescription(keyword.Description)
                .Build();
        }

        public string GetKeywordString(LorKeyword keyword)
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

            return $"{emotes}{keyword.Name}";
        }
    }
}
