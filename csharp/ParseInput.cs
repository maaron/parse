﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public interface IParseInput<T> : IComparable<IParseInput<T>>
    {
        T Current { get; }
        bool IsEnd { get; }
        IParseInput<T> Next();

        void OnFail();
        void OnMatch();
    }

    public class ErrorHeuristic<Position> where Position : IComparable<Position>
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

        public int CompareTo(IParseInput<T> other)
        {
            if (!(other is ParseInput<T>))
                throw new ArgumentException(
                    "Argument must be a ParseInput(T) type");

            return Position.CompareTo(((ParseInput<T>)other).Position);
        }
    }

    public struct LineColumn : IComparable<LineColumn>
    {
        public int Line;
        public int Column;

        public LineColumn(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int CompareTo(LineColumn other)
        {
            var l = Line.CompareTo(other.Line);
            return l != 0 ? l : Column.CompareTo(other.Column);
        }

        public static bool operator==(LineColumn a, LineColumn b)
        {
            return a.Line == b.Line && a.Column == b.Column;
        }

        public static bool operator !=(LineColumn a, LineColumn b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj != null &&
                obj is LineColumn &&
                this == (LineColumn)obj;
        }

        public override int GetHashCode()
        {
            return Line + Column;
        }
    }

    public abstract class AdaptedParseInput<T> : IParseInput<T>
    {
        protected IParseInput<T> adapted;

        public AdaptedParseInput(IParseInput<T> input)
        {
            this.adapted = input;
        }

        public T Current
        {
            get
            {
                return adapted.Current;
            }
        }

        public bool IsEnd
        {
            get
            {
                return adapted.IsEnd;
            }
        }

        public virtual void OnFail()
        {
            adapted.OnFail();
        }

        public virtual void OnMatch()
        {
            adapted.OnMatch();
        }

        public override bool Equals(object obj)
        {
            return adapted.Equals(obj);
        }

        public override int GetHashCode()
        {
            return adapted.GetHashCode();
        }

        public abstract IParseInput<T> Next();

        public int CompareTo(IParseInput<T> other)
        {
            return adapted.CompareTo(other);
        }
    }

    public class LineTrackingInput : AdaptedParseInput<char>
    {
        public LineColumn Position { get; private set; }
        public ErrorHeuristic<LineColumn> Error
        {
            get; private set;
        }

        public LineTrackingInput(IParseInput<char> input) 
            : base(input)
        {
            Position = new LineColumn(0, 0);
            Error = new ErrorHeuristic<LineColumn>(Position);
        }

        private LineTrackingInput(IParseInput<char> input, int line, int column, ErrorHeuristic<LineColumn> error)
            : base(input)
        {
            this.adapted = input;
            this.Position = new LineColumn(line, column);
            this.Error = error;
        }

        public override IParseInput<char> Next()
        {
            var next = adapted.Next();
            if ((Current == '\r' && (next.IsEnd || next.Current != '\n'))
                || Current == '\n')
            {
                return new LineTrackingInput(
                    next, Position.Line + 1, 0, Error);
            }
            else
            {
                return new LineTrackingInput(
                    next, Position.Line, Position.Column + 1, Error);
            }
        }

        public override void OnFail()
        {
            Error.OnFail(Position);
        }

        public override void OnMatch()
        {
            Error.OnMatch(Position);
        }
    }

    public class LineBasedInput : IParseInput<char>
    {
        List<string> lines;
        ErrorHeuristic<LineColumn> error;

        public LineColumn Position { get; private set; }

        public char Current
        {
            get { return lines[Position.Line][Position.Column]; }
        }

        public bool IsEnd
        {
            get { return Position.Line >= lines.Count; }
        }

        public IParseInput<char> Next()
        {
            return new LineBasedInput(lines, error,
                Position.Column >= lines[Position.Line].Length ?
                    new LineColumn(Position.Line + 1, 0) :
                    new LineColumn(Position.Line, Position.Column + 1));
        }

        protected LineBasedInput(List<string> lines, ErrorHeuristic<LineColumn> error, LineColumn pos)
        {
            this.lines = lines;
            this.error = error;
            this.Position = pos;
        }

        public LineBasedInput(IEnumerable<char> source)
        {
            var sb = new StringBuilder();
            bool cr = false;
            foreach (char c in source)
            {
                sb.Append(c);
                if (c == '\r') cr = true;
                else if (c == '\n' || cr)
                {
                    lines.Add(sb.ToString());
                    sb = new StringBuilder();
                    cr = false;
                }
            }
            this.error = new ErrorHeuristic<LineColumn>(new LineColumn());
            this.Position = new LineColumn(0, 0);
        }

        public void OnMatch() { error.OnMatch(Position); }
        public void OnFail() { error.OnFail(Position); }

        public ErrorHeuristic<LineColumn> Error
        {
            get { return error; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is LineBasedInput)) return false;

            var other = (LineBasedInput)obj;

            return Object.ReferenceEquals(lines, other.lines)
                && Position == other.Position;
        }

        public override int GetHashCode()
        {
            return lines.GetHashCode() ^ Position.GetHashCode();
        }

        public int CompareTo(IParseInput<char> other)
        {
            if (!(other is LineBasedInput)) throw new ArgumentException(
                "Argument must be a LineBaseInput type");

            return Position.CompareTo(((LineBasedInput)other).Position);
        }
    }
}
