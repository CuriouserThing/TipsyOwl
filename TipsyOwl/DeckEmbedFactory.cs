using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjerg;
using Bjerg.Lor;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TipsyOwl
{
    public class DeckEmbedFactory
    {
        public DeckEmbedFactory(IOptionsSnapshot<TipsySettings> settings, ILogger<DeckEmbedFactory> logger)
        {
            Settings = settings.Value;
            Logger = logger;
        }

        private TipsySettings Settings { get; }

        private ILogger Logger { get; }

        private Dictionary<int, char> FullWidthNumbers { get; } = new Dictionary<int, char>
        {
            [0] = '０',
            [1] = '１',
            [2] = '２',
            [3] = '３',
            [4] = '４',
            [5] = '５',
            [6] = '６',
            [7] = '７',
            [8] = '８',
            [9] = '９'
        };

        private string GetCardLine(ICard card, int count)
        {
            if (!FullWidthNumbers.TryGetValue(count, out char number))
            {
                number = '０';
            }

            string name = card.Name ?? card.Code;
            if (card.Region != null && Settings.RegionIndicatorEmotes.TryGetValue(card.Region.Key, out ulong emote))
            {
                return $"**{number}×**<:c:{emote}>{name}";
            }
            else
            {
                return $"**{number}×** {name}";
            }
        }

        private IReadOnlyList<string> BuildFieldValues(IReadOnlyList<CardAndCount> ccs)
        {
            const int fieldLimit = 1024; // Discord's limit on field length
            const int newLineMax = 2; // max number of chars a new-line can be in any environment
            const int lineCushion = 2; // safety cushion to make sure a line doesn't push past the field limit

            var fieldValues = new List<string>();
            var sb = new StringBuilder();

            foreach ((ICard card, int count) in ccs
                .OrderBy(cc => cc.Card.Cost)
                .ThenBy(cc => cc.Card.Name)
                .ThenBy(cc => cc.Card.Code))
            {
                string cardLine = GetCardLine(card, count);
                int length = sb.Length + cardLine.Length + newLineMax + lineCushion;
                if (length > fieldLimit)
                {
                    fieldValues.Add(sb.ToString());
                    sb = new StringBuilder();
                }

                _ = sb.AppendLine(cardLine);
            }

            fieldValues.Add(sb.ToString());
            return fieldValues;
        }

        private IReadOnlyList<EmbedFieldBuilder> GetFieldBuilders(string name, IReadOnlyList<CardAndCount> ccs, ref bool tryInline)
        {
            IReadOnlyList<string> fieldValues = BuildFieldValues(ccs);
            var fieldBuilders = new EmbedFieldBuilder[fieldValues.Count];

            int count = fieldValues.Count;
            if (count == 0)
            {
                return fieldBuilders;
            }

            if (count > 1)
            {
                tryInline = false;
            }

            fieldBuilders[0] = new EmbedFieldBuilder()
                .WithName(name)
                .WithValue(fieldValues[0])
                .WithIsInline(tryInline);

            for (int i = 1; i < count; i++)
            {
                fieldBuilders[i] = new EmbedFieldBuilder()
                    .WithName($"{name} (cont.)")
                    .WithValue(fieldValues[i])
                    .WithIsInline(true);
            }

            return fieldBuilders;
        }

        public Embed BuildEmbed(Deck deck, Catalog homeCatalog)
        {
            var regionLines = new List<string>();
            var warningLines = new List<string>();
            var cardFieldBuilders = new List<EmbedFieldBuilder>();

            LorFaction[] regions = deck.Cards
                .GroupBy(cc => cc.Card.Region)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key.Name)
                .Select(g => g.Key)
                .ToArray();

            foreach (LorFaction region in regions)
            {
                if (Settings.RegionIconEmotes.TryGetValue(region.Key, out ulong emote))
                {
                    regionLines.Add($"<:{region.Abbreviation}:{emote}> {region.Name}");
                }
            }

            var champions = new List<CardAndCount>();
            var followers = new List<CardAndCount>();
            var spells = new List<CardAndCount>();
            var other = new List<CardAndCount>();

            int deckSize = 0;

            foreach (CardAndCount cc in deck.Cards)
            {
                (ICard card, int count) = cc;
                deckSize += count;

                if (!homeCatalog.Cards.TryGetValue(card.Code, out ICard? homeCard))
                {
                    Logger.LogWarning($"Couldn't find card {card.Code} in the home locale catalog. Using the provided deck card with locale {card.Locale} instead.");
                    other.Add(cc);
                }
                else
                {
                    if (homeCard.Supertype?.Name == "Champion")
                    {
                        champions.Add(cc);
                    }
                    else if (homeCard.Type?.Name == "Unit")
                    {
                        followers.Add(cc);
                    }
                    else if (homeCard.Type?.Name == "Spell")
                    {
                        spells.Add(cc);
                    }
                    else
                    {
                        other.Add(cc);
                    }
                }
            }

            bool tryInline = true;
            if (champions.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Champions", champions, ref tryInline));
            }

            if (followers.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Followers", followers, ref tryInline));
            }

            if (spells.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Spells", spells, ref tryInline));
            }

            if (other.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Other", other, ref tryInline));
            }

            if (deckSize < 40)
            {
                warningLines.Add($"⚠️ Invalid deck: too few cards ({deckSize}).");
            }
            else if (deckSize > 40)
            {
                warningLines.Add($"⚠️ Invalid deck: too many cards ({deckSize}).");
            }

            const int titleLimit = 256; // Discord's limit on title length

            string joined = string.Join('\n', regionLines);
            if (joined.Length > titleLimit)
            {
                joined = "Deck";
            }

            return new EmbedBuilder()
                .WithTitle(joined)
                .WithDescription(string.Join('\n', warningLines))
                .WithFields(cardFieldBuilders)
                .Build();
        }
    }
}
