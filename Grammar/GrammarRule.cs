using System;

namespace GrammarTools
{
    public class GrammarRule
    {
        public GrammarRule(Word wordToReplace, Word wordToInsert)
        {
            if (wordToReplace.IsEmpty)
                throw new ArgumentException("Must not be the empty word.", nameof(wordToReplace));

            WordToReplace = wordToReplace;
            WordToInsert = wordToInsert;
        }

        public Word WordToReplace { get; }
        public Word WordToInsert { get; }

        public override string ToString()
        {
            return WordToReplace + " -> " + WordToInsert;
        }
    }
}