using System.Collections.Generic;
using System.Linq;

namespace GrammarTools
{
    public class NormalizedRuleCollection
    {
        private readonly Dictionary<Word, HashSet<Word>> rules = new Dictionary<Word, HashSet<Word>>();

        public NormalizedRuleCollection()
        {

        }

        public NormalizedRuleCollection(IEnumerable<GrammarRule> rules)
        {
            foreach (var rule in rules)
                AddRule(rule);
        }

        public bool AddRule(GrammarRule rule)
        {
            if (rule.WordToReplace == rule.WordToInsert)
                return false;

            HashSet<Word> insertions;
            if (!rules.TryGetValue(rule.WordToReplace, out insertions))
            {
                insertions = new HashSet<Word>();
                rules[rule.WordToReplace] = insertions;
            }

            return insertions.Add(rule.WordToInsert);
        }

        public bool RemoveRule(GrammarRule rule)
        {
            HashSet<Word> insertions;
            if (rules.TryGetValue(rule.WordToReplace, out insertions) &&
                insertions.Remove(rule.WordToInsert))
            {
                if (insertions.Count == 0)
                    rules.Remove(rule.WordToReplace);
                return true;
            }

            return false;
        }

        public Word[] GetWordsToInsert(Word wordToReplace)
        {
            HashSet<Word> insertions;
            return rules.TryGetValue(wordToReplace, out insertions)
                ? insertions.ToArray() : new Word[0];
        }

        public IEnumerable<GrammarRule> GetRules()
        {
            return rules.SelectMany(kv => kv.Value, (kv, w) => new GrammarRule(kv.Key, w));
        }


        // To make debugging easier
        private string Text => ToString();

        public override string ToString()
        {
            var result = "";
            foreach (var kv in rules)
                result += $"{kv.Key} -> {string.Join(" | ", kv.Value.Select(w => w.ToString()))} \n";
            return result;
        }
    }
}