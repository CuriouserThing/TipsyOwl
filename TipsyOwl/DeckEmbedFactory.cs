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

        private string GetCardLine(ICard card, int count, int digits)
        {
            string number;
            string emote;
            string name = card.Name ?? card.Code;

            if (digits > 0)
            {
                var sb = new StringBuilder();
                var d = 0;
                do
                {
                    int n = count % 10;
                    count /= 10;
                    sb.Insert(0, "０１２３４５６７８９"[n]);
                    d++;
                } while (count > 0);

                while (d < digits)
                {
                    sb.Insert(0, '　'); // full-width space
                    d++;
                }

                number = $"**{sb}×**";
            }
            else
            {
                number = string.Empty;
            }

            if (card.Region != null && Settings.RegionIndicatorEmotes.TryGetValue(card.Region.Key, out ulong emoteId))
            {
                emote = $"<:c:{emoteId}>";
            }
            else
            {
                emote = number.Length > 0 ? " " : string.Empty;
            }

            return $"{number}{emote}{name}";
        }

        private static int CountDigits(int n)
        {
            var d = 0;
            do
            {
                n /= 10;
                d++;
            } while (n > 0);

            return d;
        }

        private IReadOnlyList<string> BuildFieldValues(IReadOnlyList<CardAndCount> ccs, bool hideCounts)
        {
            const int fieldLimit = 1024; // Discord's limit on field length
            const int newLineMax = 2;    // max number of chars a new-line can be in any environment
            const int lineCushion = 2;   // safety cushion to make sure a line doesn't push past the field limit

            var fieldValues = new List<string>();
            var sb = new StringBuilder();

            CardAndCount[] cards = ccs
                .OrderBy(cc => cc.Card.Cost)
                .ThenBy(cc => cc.Card.Name)
                .ThenBy(cc => cc.Card.Code)
                .ToArray();

            int digits = hideCounts ? 0 : cards.Max(c => CountDigits(c.Count));

            foreach ((ICard card, int count) in cards)
            {
                string cardLine = GetCardLine(card, count, digits);
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

        private IReadOnlyList<EmbedFieldBuilder> GetFieldBuilders(string name, IReadOnlyList<CardAndCount> ccs, bool hideCounts)
        {
            IReadOnlyList<string> fieldValues = BuildFieldValues(ccs, hideCounts);
            int count = fieldValues.Count;
            var fieldBuilders = new EmbedFieldBuilder[count];

            if (count == 0)
            {
                return fieldBuilders;
            }

            if (count == 1)
            {
                fieldBuilders[0] = new EmbedFieldBuilder()
                    .WithName(name)
                    .WithValue(fieldValues[0])
                    .WithIsInline(true);
                return fieldBuilders;
            }

            for (var i = 0; i < count; i++)
            {
                fieldBuilders[i] = new EmbedFieldBuilder()
                    .WithName($"{name} ({i + 1}/{count})")
                    .WithValue(fieldValues[i])
                    .WithIsInline(true);
            }

            return fieldBuilders;
        }

        public Embed BuildEmbed(Deck deck, Catalog homeCatalog)
        {
            LorFaction[] regions = deck.Cards
                .Where(cc => cc.Card.Region != null)
                .GroupBy(cc => cc.Card.Region!)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key.Name)
                .Select(g => g.Key)
                .ToArray();

            var champions = new List<CardAndCount>();
            var followers = new List<CardAndCount>();
            var spells = new List<CardAndCount>();
            var landmarks = new List<CardAndCount>();
            var other = new List<CardAndCount>();

            var deckSize = 0;
            var uncollectibleCount = 0;

            foreach (CardAndCount cc in deck.Cards)
            {
                (ICard card, int count) = cc;
                deckSize += count;
                if (!card.Collectible) { uncollectibleCount += count; }

                if (!homeCatalog.Cards.TryGetValue(card.Code, out ICard? homeCard))
                {
                    Logger.LogWarning($"Couldn't find card {card.Code} in the home locale catalog. Using the provided deck card with locale {card.Locale} instead.");
                    homeCard = card;
                }

                if (homeCard.Supertype?.Name == "Champion")
                {
                    champions.Add(cc);
                }
                else
                {
                    switch (homeCard.Type?.Name)
                    {
                        case "Unit":
                            followers.Add(cc);
                            break;
                        case "Spell":
                            spells.Add(cc);
                            break;
                        case "Landmark":
                            landmarks.Add(cc);
                            break;
                        default:
                            other.Add(cc);
                            break;
                    }
                }
            }

            bool singleton = deckSize == deck.Cards.Count;

            var cardFieldBuilders = new List<EmbedFieldBuilder>();
            if (champions.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Champions", champions, singleton));
            }

            if (followers.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Followers", followers, singleton));
            }

            if (spells.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Spells", spells, singleton));
            }

            if (landmarks.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Landmarks", landmarks, singleton));
            }

            if (other.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Other", other, singleton));
            }

            var regionsBuilder = new StringBuilder();
            foreach (LorFaction region in regions)
            {
                if (Settings.RegionIconEmotes.TryGetValue(region.Key, out ulong emote))
                {
                    regionsBuilder.AppendLine($"<:{region.Abbreviation}:{emote}> **{region.Name}**");
                }
            }

            var noticesBuilder = new StringBuilder();

            if (deckSize < 40)
            {
                noticesBuilder.AppendLine($"⚠️ Invalid constructed deck: too few cards ({deckSize}).");
            }
            else if (deckSize > 40)
            {
                noticesBuilder.AppendLine($"⚠️ Invalid constructed deck: too many cards ({deckSize}).");
            }

            if (uncollectibleCount > 0)
            {
                string copies = uncollectibleCount == 1 ? "copy" : "copies";
                noticesBuilder.AppendLine($"⚠️ Invalid deck: has {uncollectibleCount} uncollectible card {copies}.");
            }

            if (singleton)
            {
                noticesBuilder.AppendLine("ℹ️ Singleton deck.");
            }

            string desc;
            if (regionsBuilder.Length > 0)
            {
                desc = noticesBuilder.Length > 0 ? $"{regionsBuilder}\n{noticesBuilder}" : $"{regionsBuilder}";
            }
            else
            {
                desc = noticesBuilder.Length > 0 ? $"{noticesBuilder}" : "";
            }

            return new EmbedBuilder()
                .WithDescription(desc)
                .WithFields(cardFieldBuilders)
                .Build();
        }
    }
}
