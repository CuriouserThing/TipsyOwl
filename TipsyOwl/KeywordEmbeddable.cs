using System.Collections.Generic;
using System.Linq;
using Bjerg.Lor;
using Discord;

namespace TipsyOwl
{
    public class KeywordEmbeddable : IEmbeddable
    {
        public KeywordEmbeddable(LorKeyword keyword, IReadOnlyList<LorKeyword> keywordExpansion, KeywordEmbedFactory factory)
        {
            Keyword = keyword;
            KeywordExpansion = keywordExpansion;
            Factory = factory;
        }

        public LorKeyword Keyword { get; }

        public IReadOnlyList<LorKeyword> KeywordExpansion { get; }

        public KeywordEmbedFactory Factory { get; }

        public string Name => Factory.GetKeywordString(Keyword);

        public Embed GetMainEmbed()
        {
            return Factory.BuildEmbed(Keyword);
        }

        public IReadOnlyList<Embed> GetAllEmbeds()
        {
            return KeywordExpansion
                .Select(c => Factory.BuildEmbed(c))
                .ToArray();
        }
    }
}
