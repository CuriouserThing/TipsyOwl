using Discord;
using System;
using System.Collections.Generic;

namespace TipsyOwl
{
    public class DeckEmbedBlueprint
    {
        public IReadOnlyList<string> RegionLines { get; internal set; } = Array.Empty<string>();

        public IReadOnlyList<string> WarningLines { get; internal set; } = Array.Empty<string>();

        public IReadOnlyList<EmbedFieldBuilder> CardFieldBuilders { get; internal set; } = Array.Empty<EmbedFieldBuilder>();

        public Embed BuildStandardEmbed()
        {
            const int titleLimit = 256; // Discord's limit on title length

            string joined = string.Join(Environment.NewLine, RegionLines);
            if (joined.Length > titleLimit)
            {
                joined = "Deck";
            }

            return new EmbedBuilder()
                .WithTitle(joined)
                .WithDescription(string.Join(Environment.NewLine, WarningLines))
                .WithFields(CardFieldBuilders)
                .Build();
        }
    }
}
