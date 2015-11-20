using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace GrammarTools
{
    public class Grammar
    {
        // TODO write better parser
        public static Grammar Create(string terminal, char startNonTerminal, string rules)
        {
            var nonTerminal = Regex.Matches(rules, "[A-Zαβ]+").Cast<Match>().Aggregate("", (current, match) => current + match.Value);

            var hs = new HashSet<char>(nonTerminal);

            return Create(string.Join("", hs), terminal, startNonTerminal,
                rules.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public static Grammar Create(string nonTerminal, string terminal, char startNonTerminal, string[] rules)
        {
            if (nonTerminal.Intersect(terminal).Any())
            {
                throw new ArgumentException("Non terminal and terminal symbols must be disjoint!");
            }

            var symbols =
                nonTerminal.Select<char, Symbol>(c => new NonTerminalSymbol(c.ToString()))
                    .Concat(terminal.Select(c => new TerminalSymbol(c.ToString()))).ToArray();

            Func<char, Symbol> findSymbol = c => symbols.First(s => s.Representation == c.ToString());

            var startSymbol = (NonTerminalSymbol)findSymbol(startNonTerminal);

            var grules = new List<GrammarRule>();

            foreach (var rule in rules)
            {
                var parts = rule.Split(new[] { "->" }, StringSplitOptions.None).Select(str => str.Trim()).ToArray();
                var r = parts[0];
                var rWord = new Word(r.Select(findSymbol).ToArray());
                var ls = parts[1].Split('|').Select(str => str.Trim()).ToArray();

                foreach (var l in ls)
                {
                    var lWord = new Word(l.Select(findSymbol).ToArray());

                    grules.Add(new GrammarRule(rWord, lWord));
                }
            }

            return new Grammar(symbols, startSymbol, grules.ToArray());
        }



        public Grammar(IEnumerable<Symbol> symbols, NonTerminalSymbol startSymbol, IEnumerable<GrammarRule> rules)
        {
            var symbolsArr = symbols as Symbol[] ?? symbols.ToArray();
            TerminalSymbols = symbolsArr.OfType<TerminalSymbol>().ToArray();
            NonTerminalSymbols = symbolsArr.OfType<NonTerminalSymbol>().ToArray();
            StartSymbol = startSymbol;
            Rules = rules.ToArray();
        }

        public IReadOnlyCollection<TerminalSymbol> TerminalSymbols { get; }
        public IReadOnlyCollection<NonTerminalSymbol> NonTerminalSymbols { get; }

        public IEnumerable<Symbol> Symbols => TerminalSymbols.OfType<Symbol>().Concat(NonTerminalSymbols);

        public NonTerminalSymbol StartSymbol { get; }

        public IReadOnlyCollection<GrammarRule> Rules { get; }
    }
}