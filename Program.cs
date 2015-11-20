using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrammarTools
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = new GrammarAnalysis(Grammar.Create("ab", 'S', File.ReadAllText("input.txt")));

            var transformedGrammar = ChomskyNormalform.Transform(input);


        }

        static void Main1(string[] args)
        {

            var input = new GrammarAnalysis(Grammar.Create("ab", 'S', File.ReadAllText("input.txt")));
            var reference = new GrammarAnalysis(Grammar.Create("ab", 'S', File.ReadAllText("reference.txt")));

            Console.WriteLine("Input Type: " + input.GrammarType);
            Console.WriteLine("Reference Type: " + reference.GrammarType);

            int len1 = 0;
            int len2 = 0;

            var words1 = new List<string>();
            var words2 = new List<string>();

            ThreadStart a = () =>
            {
                foreach (var w in input.Derive())
                {
                    var word = w.Item1;
                    len1 = w.Item2;
                    if (word.IsTerminalWord)
                    {
                        Console.WriteLine(word);
                        lock (words1)
                        {
                            words1.Add(word.ToString());
                        }
                    }
                }
            };

            ThreadStart b = () =>
            {
                foreach (var w in reference.Derive())
                {
                    var word = w.Item1;
                    len2 = w.Item2;
                    if (word.IsTerminalWord)
                    {
                        lock (words2)
                        {
                            words2.Add(word.ToString());
                        }
                    }
                }
            };

            int minLen = 0;

            new Thread(a).Start();
            new Thread(b).Start();

            while (true)
            {
                Thread.Sleep(100);
                var m = Math.Min(len1, len2) - 1;
                if (m != minLen)
                {
                    minLen = m;

                    lock (words1)
                    {
                        lock (words2)
                        {
                            Comparison<string> c = (w1, w2) =>
                                    (w1.Length != w2.Length)
                                        ? w1.Length.CompareTo(w2.Length)
                                        : String.Compare(w1, w2, StringComparison.Ordinal);

                            words1.Sort(c);
                            words2.Sort(c);

                            foreach (var word in words2.Where(w => w.Length <= minLen).ToArray())
                            {
                                words2.RemoveAt(0);

                                while (true)
                                {
                                    var fst = words1.FirstOrDefault();

                                    if (fst == null || c(word, fst) < 0)
                                    {
                                        Console.WriteLine("Word '" + word +
                                                            "' is not in input grammar, but should be");
                                        break;
                                    }

                                    words1.RemoveAt(0);

                                    if (fst == word)
                                        break;

                                    Console.WriteLine("Word '" + fst +
                                                            "' is in input grammar, but it should not");
                                }
                            }

                            Console.WriteLine("Cur len: " + m);
                        }
                    }
                }
            }
        }
    }
}
