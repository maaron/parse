using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace vdproj
{
    public interface IParseInput<T>
    {
        T Current { get; }
        bool IsEnd { get; }
        void Next();
    }

    public struct ParseInput<T>
    {
        T[] data;
        int index;

        public T Current
        {
            get { return data[index]; }
        }

        public bool IsEnd
        {
            get { return index >= data.Length; }
        }

        public void Next()
        {
            index++;
        }

        public ParseInput(T[] source)
        {
            this.data = source;
            this.index = 0;
        }

        public ParseInput(IEnumerable<T> source)
        {
            this.data = source.ToArray();
            this.index = 0;
        }
    }

    public class Maybe<T>
    {
        private T value;
        private bool valid;

        public Maybe(T value)
        {
            this.valid = true;
            this.value = value;
        }

        public Maybe()
        {
            this.valid = false;
        }

        public bool IsValid { get { return valid; } }
        
        public T Value 
        {
            get 
            { 
                if (!IsValid) throw new Exception("Maybe value not valid");
                return value;
            }
            set
            {
                valid = true;
            }
        }

        public R Visit<R>(Func<R> none, Func<T, R> some)
        {
            if (valid) return some(value);
            else return none();
        }

        public void Visit(Action none, Action<T> some)
        {
            if (valid) some(value);
            else none();
        }
    }

    public class Either<L, R>
    {
        Object value;

        public L Left { get { return (L)value; } }
        public R Right { get { return (R)value; } }

        public bool IsLeft { get { return value is L; } }
        public bool IsRight { get { return value is R; } }

        public Either(L left)
        {
            this.value = left;
        }

        public Either(R right)
        {
            this.value = right;
        }

        public T Visit<T>(Func<L, T> left, Func<R, T> right)
        {
            if (value is L) return left(this.Left);
            else return right(this.Right);
        }

        public void Visit(Action<L> left, Action<R> right)
        {
            if (value is L) left(this.Left);
            else right(this.Right);
        }
    }

    public class Success<T> : Location<T>
    {
        public Success(ParseInput<T> remainder) : base(remainder)
        {
        }
    }

    public class Success<T, V> : Success<T>
    {
        public V value;

        public Success(V value, ParseInput<T> remainder)
            : base(remainder)
        {
            this.value = value;
        }
    }

    public class Failure<T> : Location<T>
    {
        public Failure(ParseInput<T> remainder)
            : base(remainder)
        {
        }
    }

    public class Location<T>
    {
        public ParseInput<T> Remaining { get; private set; }

        public Location(ParseInput<T> remainder)
        {
            Remaining = remainder;
        }
    }

    public delegate Either<Success<T>, Failure<T>> Parser<T>(ParseInput<T> input);
    public delegate Either<Success<T, V>, Failure<T>> Parser<T, V>(ParseInput<T> input);

    public class Attribute
    {
        public String Name;
        public String Value;

        public Attribute(String name, String value)
        {
            this.Name = name;
            this.Value = value;

#if DEBUG
            Console.WriteLine("Attribute: " + name + " = " + value);
#endif
        }
    }

    public class Node
    {
        public string Name { get; set; }
        public List<Either<Attribute, Node>> Data { get; set; }
        public IEnumerable<Attribute> Attributes { get { return from c in Data where c.IsLeft select c.Left; } }
        public IEnumerable<Node> Children { get { return from c in Data where c.IsRight select c.Right; } }

        public Node()
        {
#if DEBUG
            Console.WriteLine("Node");
#endif
        }

        public static Node Parse(IEnumerable<char> data)
        {
            var result = ParseNode(new ParseInput<char>(data));
            return result.Visit(
                (success) => success.value,
                (failure) => { throw new Exception("Parse error"); });
        }

        public static void Write(TextWriter writer, Node node)
        {
            WriteNode(writer, node, 0);
        }

        private static void WriteNode(TextWriter writer, Node node, int indent)
        {
            WriteIndent(writer, indent);
            writer.WriteLine("\"{0}\"", node.Name);

            WriteIndent(writer, indent);
            writer.WriteLine("{");
            
            foreach (var d in node.Data)
            {
                d.Visit(
                    (attribute) => WriteAttribute(writer, attribute, indent),
                    (childNode) => WriteNode(writer, childNode, indent + 1));
            }

            WriteIndent(writer, indent);
            writer.WriteLine("}");
        }

        private static void WriteIndent(TextWriter writer, int indent)
        {
            for (int i = 0; i < indent * 4; i++) writer.Write(' ');
        }

        public static void WriteAttribute(TextWriter writer, Attribute a, int indent)
        {
            WriteIndent(writer, indent);
            writer.WriteLine("\"{0}\" = \"{1}\"", a.Name, EscapeString(a.Value));
        }

        public static String EscapeString(String s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                if (c == '"' || c == '\\') sb.Append('\\').Append(c);
                else sb.Append(c);
            }
            return sb.ToString();
        }

        public static Parser<char, Node> ParseNode = (input) =>
        {
            var parser = ParseString
                .And(InWhitespace(Const('{')))
                .And(ZeroOrMore(ParseAttribute.Or(ParseNode)))
                .And(InWhitespace(Const('}')));

            return MapValue(parser(input),
                (v) => new Node()
                {
                    Name = v.Item1,
                    Data = v.Item2
                });
        };

        private static Either<Success<T, O>, Failure<T>> MapValue<T, I, O>(
            Either<Success<T, I>, Failure<T>> r, Func<I, O> f)
        {
            return r.Visit(
                (success) => Match(f(success.value), success.Remaining),
                (failure) => Fail<T, O>(failure.Remaining));
        }

        private static Either<Success<T, I>, Failure<T>> MapValue<T, I>(
            Either<Success<T, I>, Failure<T>> r)
        {
            return r.Visit(
                (success) => Match(success.value, success.Remaining),
                (failure) => Fail<T, I>(failure.Remaining));
        }

        private static Either<Success<T, O>, Failure<T>> MapValue<T, O>(
            Either<Success<T>, Failure<T>> r, Func<O> f)
        {
            return r.Visit(
                (success) => Match(f(), success.Remaining),
                (failure) => Fail<T, O>(failure.Remaining));
        }

        public static Parser<char, Attribute> ParseAttribute = (input) =>
        {
            var parser = ParseString
                .And(InWhitespace(Const('=')))
                .And(SkipWhitespace)
                .And(ParseString);

            return MapValue(parser(input),
                v => new Attribute(v.Item1,v.Item2));
        };

        public static Parser<char> InWhitespace(
            Parser<char> parser)
        {
            return SkipWhitespace
                .And(parser)
                .And(SkipWhitespace);
        }

        public static Parser<char, V> InWhitespace<V>(
            Parser<char, V> parser)
        {
            return SkipWhitespace
                .And(parser)
                .And(SkipWhitespace);
        }

        public static Parser<char> SkipWhitespace = (input) =>
        {
            while (!input.IsEnd && char.IsWhiteSpace(input.Current)) input.Next();
            return Match(input);
        };

        public static Parser<char, string> ParseString = (input) =>
        {
            var parser = SkipWhitespace
                .And(Const('"'))
                .And(ZeroOrMore(If(ParseQuoteEscape, (esc) => esc.c != '"' || esc.escaped)))
                .And(Const('"'));

            return MapValue(parser(input),
                v => new String(v.Select((esc) => esc.c).ToArray()));
        };

        public static Parser<T, Tuple<V1, V2, V3>> Sequence<T, V1, V2, V3>(
            Parser<T, Tuple<V1, V2>> left,
            Parser<T, V3> right)
        {
            return SequenceImpl(left, right,
                (l, r) => Tuple.Create(l.Item1, l.Item2, r));
        }

        public static Parser<T, Tuple<V1, V2, V3>> Sequence<T, V1, V2, V3>(
            Parser<T, V1> left,
            Parser<T, Tuple<V2, V3>> right)
        {
            return SequenceImpl(left, right, 
                (l, r) => Tuple.Create(l, r.Item1, r.Item2));
        }

        public static Parser<T, Tuple<V1, V2>> Sequence<T, V1, V2>(
            Parser<T, V1> left,
            Parser<T, V2> right)
        {
            return SequenceImpl(left, right,
                (l, r) => Tuple.Create(l, r));
        }

        public static Parser<T, V> Sequence<T, V>(
            Parser<T> left,
            Parser<T, V> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => MapValue(right(success.Remaining)),
                    (failure) => Fail<T, V>(failure.Remaining));
            };
        }

        public static Parser<T, V> Sequence<T, V>(
            Parser<T, V> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => MapValue(right(success.Remaining), () => success.value),
                    (failure) => Fail<T, V>(failure.Remaining));
            };
        }

        public static Parser<T> Sequence<T>(
            Parser<T> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => right(success.Remaining),
                    (failure) => Fail<T>(failure.Remaining));
            };
        }

        // Used for Sequence chains whose result types have two or more 
        // values.  Function f returns the desired value by combining values 
        // from left and right parsers.
        private static Parser<T, V3> SequenceImpl<T, V1, V2, V3>(
            Parser<T, V1> left,
            Parser<T, V2> right,
            Func<V1, V2, V3> f)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => MapValue(right(success.Remaining), r => f(success.value, r)),
                    (failure) => Fail<T, V3>(failure.Remaining));
            };
        }

        public static Either<Success<T, V>, Failure<T>> Match<T, V>(V value, ParseInput<T> remainder)
        {
            return new Either<Success<T, V>, Failure<T>>(new Success<T, V>(value, remainder));
        }

        public static Either<Success<T>, Failure<T>> Match<T>(ParseInput<T> remainder)
        {
            return new Either<Success<T>, Failure<T>>(new Success<T>(remainder));
        }

        public static Either<Success<T, V>, Failure<T>> Fail<T, V>(ParseInput<T> remainder)
        {
            return new Either<Success<T, V>, Failure<T>>(new Failure<T>(remainder));
        }

        public static Either<Success<T>, Failure<T>> Fail<T>(ParseInput<T> remainder)
        {
            return new Either<Success<T>, Failure<T>>(new Failure<T>(remainder));
        }

        public static Parser<T, List<V>> ZeroOrMore<T, V>(
            Parser<T, V> parser)
        {
            return (input) =>
            {
                var matches = new List<V>();
                bool failed = false;
                while (!failed)
                {
                    parser(input).Visit(
                        (success) => 
                        {
                            input = success.Remaining;
                            matches.Add(success.value);
                        },
                        (failure) => { failed = true; });
                }
                return Match(matches, input);
            };
        }

        public static Parser<T, Maybe<V>> Optional<T, V>(Parser<T, V> parser)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => Match(new Maybe<V>(success.value), success.Remaining),
                    (failure) => Match(new Maybe<V>(), input));
            };
        }

        public static Parser<T> Not<T, V>(Parser<T, V> parser, T value)
        {
            return (input) => 
            {
                return parser(input).Visit(
                    (success) => Fail<T>(success.Remaining),
                    (failure) => Match<T>(input));
            };
        }

        public static Parser<T, V> If<T, V>(
            Parser<T, V> parser, 
            Func<V, bool> predicate)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => predicate(success.value) ? 
                        Match(success.value, success.Remaining) :
                        Fail<T, V>(input),
                    (failure) => Fail<T, V>(failure.Remaining));
            };
        }

        public static Parser<char> Const(char c)
        {
            return (input) =>
            {
                if (!input.IsEnd && input.Current == c)
                {
                    input.Next();
                    return Match(input);
                }
                else return Fail<char>(input);
            };
        }

        public static Parser<char, char> Single = (input) =>
        {
            if (!input.IsEnd)
            {
                char c = input.Current;
                input.Next();
                return Match(c, input);
            }
            else return Fail<char, char>(input);
        };

        public static Parser<T> Alternate<T>(
            Parser<T> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => right(success.Remaining),
                    (failure) => Fail<T>(failure.Remaining));
            };
        }

        public static Parser<T, Maybe<V>> Alternate<T, V>(
            Parser<T, V> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Match(new Maybe<V>(success.value), success.Remaining),
                    (failure) => MapValue(right(input), () => new Maybe<V>()));
            };
        }

        public static Parser<T, Maybe<V>> Alternate<T, V>(
            Parser<T> left,
            Parser<T, V> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Match(new Maybe<V>(), success.Remaining),
                    (failure) => MapValue(right(input), (v) => new Maybe<V>(v)));
            };
        }

        public static Parser<T, Either<V1, V2>> Alternate<T, V1, V2>(
            Parser<T, V1> left,
            Parser<T, V2> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Match(new Either<V1, V2>(success.value), success.Remaining),
                    (failure) => MapValue(right(input), (v) => new Either<V1, V2>(v)));
            };
        }

        public class Escaped
        {
            public bool escaped;
            public char c;

            public Escaped(char c, bool escaped)
            {
                this.c = c;
                this.escaped = escaped;
            }
        }

        private static Parser<char, Escaped> ParseEscapeChar = (input) =>
        {
            var parser = Const('\\').And(Single);
            return MapValue(parser(input),
                v => new Escaped(v, true));
        };

        public static Parser<char, Escaped> ParseQuoteEscape = (input) =>
        {
            var parser = ParseEscapeChar.Or(Single);

            return MapValue(parser(input),
                v => v.Visit(
                    (escaped) => escaped,
                    (single) => new Escaped(single, false)));
        };
    }

    public static class ParserExtensions
    {
        public static Parser<T> And<T>(this Parser<T> p, Parser<T> next)
        {
            return Node.Sequence(p, next);
        }

        public static Parser<T, V> And<T, V>(this Parser<T, V> p, Parser<T> next)
        {
            return Node.Sequence(p, next);
        }

        public static Parser<T, V> And<T, V>(this Parser<T> p, Parser<T, V> next)
        {
            return Node.Sequence(p, next);
        }

        public static Parser<T, Tuple<V1, V2>> And<T, V1, V2>(this Parser<T, V1> p, Parser<T, V2> next)
        {
            return Node.Sequence(p, next);
        }

        public static Parser<T, Tuple<V1, V2, V3>> And<T, V1, V2, V3>(this Parser<T, Tuple<V1, V2>> p, Parser<T, V3> next)
        {
            return Node.Sequence(p, next);
        }

        public static Parser<T, Tuple<V1, V2, V3>> And<T, V1, V2, V3>(this Parser<T, V1> p, Parser<T, Tuple<V2, V3>> next)
        {
            return Node.Sequence(p, next);
        }

        public static Parser<T> Or<T>(this Parser<T> p, Parser<T> next)
        {
            return Node.Alternate(p, next);
        }

        public static Parser<T, Maybe<V>> Or<T, V>(this Parser<T, V> p, Parser<T> next)
        {
            return Node.Alternate(p, next);
        }

        public static Parser<T, Maybe<V>> Or<T, V>(this Parser<T> p, Parser<T, V> next)
        {
            return Node.Alternate(p, next);
        }

        public static Parser<T, Either<V1, V2>> Or<T, V1, V2>(this Parser<T, V1> p, Parser<T, V2> next)
        {
            return Node.Alternate(p, next);
        }
    }
}
