namespace GrammarTools
{
    public abstract class Symbol
    {
        internal Symbol(string representation)
        {
            Representation = representation;
        }

        public string Representation { get; }

        public override string ToString()
        {
            return Representation;
        }
    }
}