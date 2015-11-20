using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Shields.DataStructures;

namespace GrammarTools
{
    public class GrammarAnalysis
    {
        public Grammar Grammar { get; }

        private GrammarChomskyType? grammarType;

        public GrammarAnalysis(Grammar grammar)
        {
            Grammar = grammar;
        }

        public GrammarChomskyType GrammarType
        {
            get
            {
                if (grammarType != null) return grammarType.Value;

                var usedStartSymbol = false;
                var containsStartToEpsilon = false;
                var canBeType1 = true;
                var canBeType3 = true;
                var canBeType2 = true;

                foreach (var rule in Grammar.Rules.TakeWhile(rule => canBeType1 || canBeType2 || canBeType3))
                {
                    if (!usedStartSymbol && rule.WordToInsert.Contains(Grammar.StartSymbol))
                    {
                        usedStartSymbol = true;
                        if (containsStartToEpsilon)
                            canBeType1 = false;
                    }

                    if (canBeType1)
                    {
                        if (rule.WordToReplace.Length > rule.WordToInsert.Length)
                        {
                            if (rule.WordToReplace.Length == 1 && rule.WordToReplace[0] == Grammar.StartSymbol)
                            {
                                containsStartToEpsilon = true;
                                if (usedStartSymbol)
                                    canBeType1 = false;
                            }
                            else
                                canBeType1 = false;
                        }
                    }

                    if (canBeType2)
                    {
                        if (rule.WordToReplace.Length != 1)
                            canBeType2 = false;
                        else if (rule.WordToReplace.IsTerminalWord)
                            canBeType2 = false;
                    }

                    if (canBeType3)
                    {
                        if (rule.WordToReplace.Length != 1)
                            canBeType3 = false;
                        else if (rule.WordToReplace.IsTerminalWord)
                            canBeType3 = false;
                        else if (rule.WordToInsert.Length > 2)
                            canBeType3 = false;
                        else if (rule.WordToInsert.Length == 2 && rule.WordToInsert[0] is NonTerminalSymbol)
                            canBeType3 = false;
                    }
                }

                if (canBeType3)
                    grammarType = GrammarChomskyType.Type3;
                else if (canBeType2)
                    grammarType = GrammarChomskyType.Type2;
                else if (canBeType1)
                    grammarType = GrammarChomskyType.Type1;
                else
                    grammarType = GrammarChomskyType.Type0;

                return grammarType.Value;
            }
        }


        public IEnumerable<Tuple<Word, int>> Derive()
        {
            var derivedWords = new HashSet<string>();
            var pq = new PairingHeap<Word, int>();

            pq.Add(new Word(Grammar.StartSymbol), 0);

            var rules = Grammar.Rules.OrderBy(r => r.WordToInsert.Length - r.WordToReplace.Length).ToArray();

            while (pq.Count > 0)
            {
                var elementToDerive = pq.Min;
                pq.Remove(elementToDerive);

                var wordToDerive = elementToDerive.Key;

                foreach (var nw in rules.SelectMany(rule => wordToDerive.ReplaceAll(rule.WordToReplace, rule.WordToInsert))
                        .Where(nw => !derivedWords.Contains(nw.ToString())))
                {
                    derivedWords.Add(nw.ToString());
                    pq.Add(nw, nw.Length);

                    yield return Tuple.Create(nw, wordToDerive.Length);
                }
            }
        }

        public Grammar ToChomskyNormalform()
        {
            return ChomskyNormalform.Transform(this);
        }
    }
}