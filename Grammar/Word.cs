using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GrammarTools
{
    public class Word : IEnumerable<Symbol>, IComparable<Word>, IEquatable<Word>
    {
        public static readonly Word Empty = new Word(new Symbol[0]);


        public Word(params Symbol[] symbols)
        {
            if (symbols == null) throw new ArgumentNullException(nameof(symbols));
            this.symbols = symbols;
        }

        private readonly Symbol[] symbols;

        public IEnumerator<Symbol> GetEnumerator()
        {
            return ((IEnumerable<Symbol>)symbols).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int CompareTo(Word other)
        {
            if (Length != other.Length)
                return Length.CompareTo(other.Length);
            return 0;
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "ε";
            return string.Concat(symbols.Select(s => s.Representation));
        }

        public int Length => symbols.Length;

        public Symbol this[int index] => symbols[index];

        public bool IsTerminalWord => !symbols.OfType<NonTerminalSymbol>().Any();

        public bool IsNonTerminalSymbol => symbols.Length == 1 && symbols[0] is NonTerminalSymbol;
        public bool IsEmpty { get { return symbols.Length == 0; } }

        public bool Contains(Word word)
        {
            return IndexOf(word, 0) >= 0;
        }

        public int IndexOf(Word word, int startIndex)
        {
            for (var i = startIndex; i <= Length - word.Length; i++)
            {
                for (var j = 0; j < word.Length; j++)
                    if (symbols[i + j] != word[j])
                        goto cont;

                return i;

                cont:;
            }

            return -1;
        }

        public IEnumerable<Word> ReplaceAll(Word wordToReplace, Word wordToInsert)
        {
            var i = 0;
            while (true)
            {
                var nw = Replace(wordToReplace, wordToInsert, out i, i);
                if (i < 0)
                    break;

                i++;
                yield return nw;
            }
        }

        public Word Replace(Word wordToReplace, Word wordToInsert, out int index, int startIndex)
        {
            index = IndexOf(wordToReplace, startIndex);
            if (index < 0)
                return this;

            var newWord = new Symbol[Length - wordToReplace.Length + wordToInsert.Length];

            for (var j = 0; j < index; j++)
                newWord[j] = symbols[j];
            for (var j = 0; j < wordToInsert.Length; j++)
                newWord[index + j] = wordToInsert[j];
            for (var j = 0; j < newWord.Length - index - wordToInsert.Length; j++)
                newWord[index + wordToInsert.Length + j] = symbols[index + wordToReplace.Length + j];

            var nw = new Word(newWord);

            return nw;
        }

        public bool Equals(Word other)
        {
            return symbols.SequenceEqual(other.symbols);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Word && Equals((Word) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                for (int i = 0; i < symbols.Length; i++)
                    hash = (hash ^ symbols[i].GetHashCode()) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }

        public static bool operator ==(Word left, Word right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Word left, Word right)
        {
            return !left.Equals(right);
        }

        public static implicit operator Word(Symbol symbol)
        {
            return new Word(symbol);
        }
    }
}