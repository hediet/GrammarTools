using System;
using System.Collections.Generic;
using System.Linq;

namespace GrammarTools
{
    internal static class ChomskyNormalform
    {
        private static IEnumerable<Word> ReplaceAllEx(Word word, Word wordToReplace, Word wordToInsert, int startIdx = 0)
        {
            var i = startIdx;
            while (true)
            {
                var nw = word.Replace(wordToReplace, wordToInsert, out i, i);

                if (i < 0)
                    break;

                yield return nw;

                foreach (var w in ReplaceAllEx(nw, wordToReplace, wordToInsert, i))
                    yield return w;

                i++;
            }
        }

        public static Grammar Transform(GrammarAnalysis grammar)
        {
            if (grammar.GrammarType < GrammarChomskyType.Type2)
                throw new ArgumentException("Must be type 2 grammar", nameof(grammar));

            bool containsEmptyWord;

            var rules = new NormalizedRuleCollection(grammar.Grammar.Rules);

            var repeat = true;
            while (repeat)
            {
                repeat = false;
                foreach (var rule in rules.GetRules().ToArray())
                {
                    if (rule.WordToInsert.IsEmpty)
                    {
                        if (rule.WordToReplace == grammar.Grammar.StartSymbol)
                            containsEmptyWord = true;

                        foreach (var r2 in rules.GetRules().Where(r => r.WordToInsert.Contains(rule.WordToReplace)))
                            foreach (var w in ReplaceAllEx(r2.WordToInsert, rule.WordToReplace, Word.Empty))
                                rules.AddRule(new GrammarRule(r2.WordToReplace, w));

                        rules.RemoveRule(rule);
                        repeat = true;
                    }
                }
            }

            repeat = true;
            while (repeat)
            {
                repeat = false;
                foreach (var rule in rules.GetRules().ToArray())
                {
                    if (rule.WordToInsert.IsNonTerminalSymbol)
                    {
                        foreach (var r2 in rules.GetRules().Where(r => r.WordToReplace == rule.WordToInsert))
                            rules.AddRule(new GrammarRule(rule.WordToReplace, r2.WordToInsert));

                        rules.RemoveRule(rule);
                        repeat = true;
                    }
                }
            }

            // TODO: Split A -> ABc to A -> AB', B' -> BC, C -> c

            return new Grammar(grammar.Grammar.Symbols, grammar.Grammar.StartSymbol, rules.GetRules());
        }
    }
}