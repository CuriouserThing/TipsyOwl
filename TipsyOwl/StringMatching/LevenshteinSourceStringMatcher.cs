using System;
using System.Collections.Generic;
using System.Linq;

namespace TipsyOwl.StringMatching
{
    public class LevenshteinSourceStringMatcher : ISourceStringMatcher
    {
        public string Source { get; }

        private string[] SourceWords { get; }

        public float InsertionTaper { get; }

        public LevenshteinSourceStringMatcher(string source, float insertionTaper)
        {
            Source = source;
            SourceWords = Source.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (insertionTaper < 0f || insertionTaper > 1f)
            {
                throw new ArgumentException($"", nameof(insertionTaper));
            }
            InsertionTaper = insertionTaper;
        }

        private static void FindDistance(string source, string target, float insertionDistance, float insertionDistanceMult, out float distance, out float distanceMax)
        {
            // This is a Wagner-Fischer impl with weights for the insertion column.

            // TODO: if too much GC pressure, pool these arrays in a dic keyed by target length.
            var d = new float[source.Length + 1, target.Length + 1];

            for (int i = 1; i <= source.Length; i++)
            {
                d[i, 0] = i;
            }

            float ins = insertionDistance;
            for (int j = 1; j <= target.Length; j++)
            {
                d[0, j] = d[0, j - 1] + ins;
                ins *= insertionDistanceMult;
            }

            for (int i = 1; i <= source.Length; i++)
            {
                char ic = source[i - 1];
                for (int j = 1; j <= target.Length; j++)
                {
                    char jc = target[j - 1];
                    if (ic == jc)
                    {
                        d[i, j] = d[i - 1, j - 1];
                    }
                    else
                    {
                        ins = d[0, j] - d[0, j - 1];
                        d[i, j] = Math.Min(
                            d[i - 1, j - 1] + 1f, // substitution
                            Math.Min(
                                d[i - 1, j] + 1f, // deletion
                                d[i, j - 1] + ins)); // insertion
                    }
                }
            }

            // The distance
            distance = d[source.Length, target.Length];

            // The max distance between arbitrary permutations of characters the lengths of the source and the target
            distanceMax = source.Length;
            if (target.Length > source.Length)
            {
                distanceMax += d[0, target.Length] - d[0, source.Length];
            }
        }

        public float GetMatchPct(string target)
        {
            List<string> tWords = target.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            float distance = 0f;
            float distanceMax = 0f;
            float insertionDistance = 1f;

            for (int i = 0; i < SourceWords.Length; i++)
            {
                string sWord = SourceWords[i];

                if (tWords.Count == 0)
                {
                    distance += sWord.Length;
                    continue;
                }

                float sd = float.PositiveInfinity;
                float sdMax = float.PositiveInfinity;
                int sdIndex = -1;
                for (int j = 0; j < tWords.Count; j++)
                {
                    string tWord = tWords[j];
                    FindDistance(sWord, tWord, insertionDistance, InsertionTaper, out float d, out float dMax);
                    if (d < sd)
                    {
                        sd = d;
                        sdMax = dMax;
                        sdIndex = j;
                    }
                }

                int length = tWords[sdIndex].Length;
                for (int j = 0; j < length; j++)
                {
                    insertionDistance *= InsertionTaper;
                }
                tWords.RemoveAt(sdIndex);
                distance += sd;
                distanceMax += sdMax;
            }

            int insLength = tWords.Sum(t => t.Length);
            for (int i = 0; i <= insLength; i++)
            {
                distance += insertionDistance;
                distanceMax += insertionDistance;
                insertionDistance *= InsertionTaper;
            }

            return 1f - distance / distanceMax;
        }
    }
}
