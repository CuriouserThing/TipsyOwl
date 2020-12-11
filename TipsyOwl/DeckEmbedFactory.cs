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
                int d = 0;
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
            int d = 0;
            do
            {
                n /= 10;
                d++;
            } while (n > 0);

            return d;
        }

        private IReadOnlyList<string> BuildFieldValues(IReadOnlyList<CardAndCount> ccs, bool singleton)
        {
            const int fieldLimit = 1024; // Discord's limit on field length
            const int newLineMax = 2; // max number of chars a new-line can be in any environment
            const int lineCushion = 2; // safety cushion to make sure a line doesn't push past the field limit

            var fieldValues = new List<string>();
            var sb = new StringBuilder();

            CardAndCount[] cards = ccs
                .OrderBy(cc => cc.Card.Cost)
                .ThenBy(cc => cc.Card.Name)
                .ThenBy(cc => cc.Card.Code)
                .ToArray();

            int digits = singleton ? 0 : cards.Max(c => CountDigits(c.Count));

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

        private IReadOnlyList<EmbedFieldBuilder> GetFieldBuilders(string name, IReadOnlyList<CardAndCount> ccs, bool singleton, ref bool tryInline)
        {
            IReadOnlyList<string> fieldValues = BuildFieldValues(ccs, singleton);
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

            int deckSize = 0;
            int uncollectibleCount = 0;

            foreach (CardAndCount cc in deck.Cards)
            {
                (ICard card, int count) = cc;
                deckSize += count;
                if (!card.Collectible) { uncollectibleCount += count; }

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
                    else if (homeCard.Type?.Name == "Landmark")
                    {
                        landmarks.Add(cc);
                    }
                    else
                    {
                        other.Add(cc);
                    }
                }
            }

            bool singleton = deckSize == deck.Cards.Count;
            var cardFieldBuilders = new List<EmbedFieldBuilder>();

            bool tryInline = true;
            if (champions.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Champions", champions, singleton, ref tryInline));
            }

            if (followers.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Followers", followers, singleton, ref tryInline));
            }

            if (spells.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Spells", spells, singleton, ref tryInline));
            }

            if (landmarks.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Landmarks", landmarks, singleton, ref tryInline));
            }

            if (other.Count > 0)
            {
                cardFieldBuilders.AddRange(GetFieldBuilders("Other", other, singleton, ref tryInline));
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
