using System;

namespace TipsyOwl
{
    public class StringMatcherFactory
    {
        private Func<string, IStringMatcher> MatcherDel { get; }

        public StringMatcherFactory(Func<string, IStringMatcher> matcherDel)
        {
            MatcherDel = matcherDel;
        }

        public IStringMatcher CreateStringMatcher(string source)
        {
            return MatcherDel(source);
        }
    }
}
