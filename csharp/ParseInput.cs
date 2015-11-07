using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public interface IParseInput<T>
    {
        T Current { get; }
        bool IsEnd { get; }
        IParseInput<T> Next();

        void OnFail();
        void OnMatch();
    }

    public class ErrorHeuristic<Position> where Position : IComparable
    {
        public Position LongestFailure { get; set; }
        public Position LongestMatch { get; set; }
        public Position LastMatch { get; set; }

        public ErrorHeuristic(Position init)
        {
            LastMatch = LongestMatch = LongestFailure = init;
        }

        public void OnFail(Position p)
        {
            if (LongestFailure.CompareTo(p) < 0)
                LongestFailure = p;
        }

        public void OnMatch(Position p)
        {
            if (LongestMatch.CompareTo(p) < 0)
                LongestMatch = p;

            LastMatch = p;
        }
    }

    public class ParseInput<T> : IParseInput<T>
    {
        T[] data;
        ErrorHeuristic<int> error;

        public int Position { get; private set; }

        public T Current
        {
            get { return data[Position]; }
        }

        public bool IsEnd
        {
            get { return Position >= data.Length; }
        }

        public IParseInput<T> Next()
        {
            return new ParseInput<T>(data, error, Position + 1);
        }

        protected ParseInput(T[] data, ErrorHeuristic<int> error, int pos)
        {
            this.data = data;
            this.error = error;
            this.Position = pos;
        }

        public ParseInput(IEnumerable<T> source)
        {
            this.data = source.ToArray();
            this.error = new ErrorHeuristic<int>(0);
            this.Position = 0;
        }

        public void OnMatch() { error.OnMatch(Position); }
        public void OnFail() { error.OnFail(Position); }

        public ErrorHeuristic<int> Error
        {
            get { return error; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ParseInput<T>)) return false;

            var other = (ParseInput<T>)obj;

            return Object.ReferenceEquals(data, other.data)
                && Position == other.Position;
        }

        public override int GetHashCode()
        {
            return data.GetHashCode() + Position;
        }
    }
}
