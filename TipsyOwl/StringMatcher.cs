using System;

namespace TipsyOwl
{
    public class StringMatcher
    {
        private Func<string, ISourceStringMatcher> SourceMatcherDel { get; }

        public StringMatcher(Func<string, ISourceStringMatcher> sourceMatcherDel)
        {
            SourceMatcherDel = sourceMatcherDel;
        }

        public ISourceStringMatcher GetSourceStringMatcher(string source)
        {
            return SourceMatcherDel(source);
        }
    }
}
