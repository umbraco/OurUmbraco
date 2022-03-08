using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// Represents phrases in a body of text.
    /// </summary>
    public class PhraseGroup
    {
        /// <summary>
        /// Creates a <see cref="PhraseGroup"/>.
        /// </summary>
        /// <param name="regex">
        /// <para>The regex expression for the <see cref="PhraseGroup"/>.</para>
        /// <para>This value is required.</para>
        /// </param>
        /// <param name="includedMatches">
        /// <para>The indices of the regex matches included in the <see cref="PhraseGroup"/>.</para>
        /// <para>This array can contain negative values. If a value is <c>-n</c>, the nth last match is included. For example,
        /// if a value is <c>-2</c>, the 2nd last match is included.</para>
        /// <para>If this value is <c>null</c> or empty, all matches are included.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="regex"/> is <c>null</c>.</exception>
        public PhraseGroup(string regex, int[] includedMatches)
        {
            Regex = regex ?? throw new ArgumentNullException(nameof(regex));
            IncludedMatches = includedMatches;
        }

        /// <summary>
        /// Gets the regex expression for the <see cref="PhraseGroup"/>.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate, Required = Required.Always)]
        public string Regex { get; }

        /// <summary>
        /// Gets the indices of the regex matches included in the <see cref="PhraseGroup"/>.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int[] IncludedMatches { get; }

        /// <summary>
        /// Searches for and returns the <see cref="Phrase"/>s defined by the <see cref="PhraseGroup"/> in a body of text.
        /// </summary>
        /// <param name="text">The body of text to search for <see cref="Phrase"/>s in.</param>
        /// <param name="result">The <see cref="List{T}"/> to add found <see cref="Phrase"/>s to.</param>
        /// <exception cref="OptionsException">Thrown if <see cref="IncludedMatches"/> contains an index that is out of range.</exception>
        public void GetPhrases(string text, List<Phrase> result)
        {
            MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(text, Regex, RegexOptions.Singleline);
            int numMatches = matches.Count;

            if (numMatches == 0)
            {
                return;
            }

            if (!(IncludedMatches?.Length > 0))
            {
                foreach (Match match in matches)
                {
                    AddMatch(match, result);
                }
            }
            else
            {
                int maxIndex = numMatches - 1;
                foreach (int includedMatchIndex in IncludedMatches)
                {
                    int normalizedIncludedMatchIndex = includedMatchIndex < 0 ? maxIndex + includedMatchIndex + 1 : includedMatchIndex;

                    if (normalizedIncludedMatchIndex > maxIndex)
                    {
                        throw new OptionsException(nameof(IncludedMatches),
                            string.Format(
                                Strings.OptionsException_PhraseGroup_IncludedMatchIndexOutOfRange,
                                ToString(),
                                numMatches,
                                includedMatchIndex
                            ));
                    }

                    AddMatch(matches[normalizedIncludedMatchIndex], result);
                }
            }
        }

        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return $"{nameof(Regex)}: {Regex}, {nameof(IncludedMatches)}: {(IncludedMatches == null ? "null" : $"[{string.Join(",", IncludedMatches)}]")}";
        }

        /// <summary>
        /// Checks for value equality between the <see cref="PhraseGroup"/> and an object.
        /// </summary>
        /// <param name="obj">The object to check for value equality.</param>
        /// <returns>True if the <see cref="PhraseGroup"/>'s value is equal to the object's value, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is PhraseGroup group &&
                   Regex == group.Regex &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(IncludedMatches, group.IncludedMatches);
        }

        /// <summary>
        /// Returns the hash code for the object.
        /// </summary>
        /// <returns>The hash code for the object.</returns>
        public override int GetHashCode()
        {
            int hashCode = 2109131697;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Regex);
            return hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(IncludedMatches);
        }

        private void AddMatch(Match match, List<Phrase> result)
        {
            GroupCollection groups = match.Groups;
            int numGroups = groups.Count;
            for (int i = numGroups > 1 ? 1 : 0; i < numGroups; i++)
            {
                Group group = groups[i];
                int length = group.Length;

                if (length == 0)
                {
                    continue;
                }

                int index = group.Index;
                result.Add(new Phrase(index, index + group.Length - 1));
            }
        }
    }
}
