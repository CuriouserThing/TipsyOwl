using System.Collections.Generic;
using System.Text;
using Bjerg.Lor;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TipsyOwl
{
    public class KeywordEmbedFactory
    {
        public KeywordEmbedFactory(IOptionsSnapshot<TipsySettings> settings, ILogger<CardEmbedFactory> logger)
        {
            Settings = settings.Value;
            Logger = logger;
        }

        private TipsySettings Settings { get; }

        private ILogger Logger { get; }

        internal string GetKeywordString(LorKeyword keyword)
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

        public Embed BuildEmbed(LorKeyword keyword)
        {
            return new EmbedBuilder()
                .WithTitle(GetKeywordString(keyword))
                .WithDescription(keyword.Description)
                .Build();
        }
    }
}
