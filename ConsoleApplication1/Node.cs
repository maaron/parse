using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Functional;

namespace Parse
{
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

        public Node(string name, List<Either<Attribute, Node>> data)
        {
            Name = name;
            Data = data;
#if DEBUG
            Console.WriteLine("Node");
#endif
        }

        public static Node Parse(IEnumerable<char> data)
        {
            var result = Grammar.ParseNode(new ParseInput<char>(data));
            return result.Visit(
                (success) => success.Value,
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
    }

    public class Grammar : Combinators
    {
        public static Parser<char, Node> ParseNode = (input) =>
        {
            var parser = ParseString
                .And(InWhitespace(Const('{')))
                .And(ZeroOrMore(ParseAttribute.Or(ParseNode)))
                .And(InWhitespace(Const('}')));

            return parser(input).MapValue(v => new Node(v.Item1, v.Item2));
        };

        public static Parser<char, Attribute> ParseAttribute = (input) =>
        {
            var parser = ParseString
                .And(InWhitespace(Const('=')))
                .And(SkipWhitespace)
                .And(ParseString);

            return parser(input).MapValue(
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
            while (!input.IsEnd && char.IsWhiteSpace(input.Current)) input = input.Next();
            return Result.Match(input);
        };

        public static Parser<char, string> ParseString = (input) =>
        {
            var parser = SkipWhitespace
                .And(Const('"'))
                .And(ZeroOrMore(If(ParseQuoteEscape, (esc) => esc.c != '"' || esc.escaped)))
                .And(Const('"'));

            return parser(input).MapValue(
                v => new String(v.Select((esc) => esc.c).ToArray()));
        };

        public static Parser<char> Const(char c)
        {
            return (input) =>
            {
                if (!input.IsEnd && input.Current == c)
                {
                    return Result.Match(input.Next());
                }
                else return Result.Fail<char>(input);
            };
        }

        public static Parser<char, char> Single = (input) =>
        {
            if (!input.IsEnd)
            {
                char c = input.Current;
                return Result.Match(c, input.Next());
            }
            else return Result.Fail<char, char>(input);
        };

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
            return parser(input).MapValue(v => new Escaped(v, true));
        };

        public static Parser<char, Escaped> ParseQuoteEscape = (input) =>
        {
            var parser = ParseEscapeChar.Or(Single);

            return parser(input).MapValue(v => v.Visit(
                    (escaped) => escaped,
                    (single) => new Escaped(single, false)));
        };
    }
}
