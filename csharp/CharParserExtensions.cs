using System;
using Functional;

namespace Parse
{
    namespace Character
    {
        public static class ParserExtensions
        {
            public static Parser<char, string> ReturnString(this Parser<char> p)
            {
                return Chars.String(p);
            }

            public static Parser<char, string> ReturnString<V>(this Parser<char, V> p)
            {
                return Chars.String(p);
            }
        }

        public static class CharParserExtensions
        {
            public static Parser<char, V> And<V>(this char c, Parser<char, V> p)
            {
                return Combinators.Sequence(Chars.Const(c), p);
            }

            public static Parser<char> And(this char c, Parser<char> p)
            {
                return Combinators.Sequence(Chars.Const(c), p);
            }

            public static Parser<char, Maybe<V>> Or<V>(this char c, Parser<char, V> p)
            {
                return Combinators.Alternate(Chars.Const(c), p);
            }

            public static Parser<char> Or(this char c, Parser<char> p)
            {
                return Combinators.Alternate(Chars.Const(c), p);
            }
        }

        public static class ParserCharExtensions
        {
            public static Parser<char, V> And<V>(this Parser<char, V> p, char c)
            {
                return Combinators.Sequence(p, Chars.Const(c));
            }

            public static Parser<char> And(this Parser<char> p, char c)
            {
                return Combinators.Sequence(p, Chars.Const(c));
            }

            public static Parser<char, Maybe<V>> Or<V>(this Parser<char, V> p, char c)
            {
                return Combinators.Alternate(p, Chars.Const(c));
            }

            public static Parser<char> Or(this Parser<char> p, char c)
            {
                return Combinators.Alternate(p, Chars.Const(c));
            }
        }

        public static class StringParserExtensions
        {
            public static Parser<char, Maybe<V>> Or<V>(this string s, Parser<char, V> p)
            {
                return Combinators.Alternate(Chars.String(s), p);
            }

            public static Parser<char> Or(this string s, Parser<char> p)
            {
                return Combinators.Alternate(Chars.String(s), p);
            }

            public static Parser<char, V> And<V>(this string s, Parser<char, V> p)
            {
                return Combinators.Sequence(Chars.String(s), p);
            }

            public static Parser<char> And(this string s, Parser<char> p)
            {
                return Combinators.Sequence(Chars.String(s), p);
            }
        }

        public static class ParserStringExtensions
        {
            public static Parser<char, Maybe<V>> Or<V>(this Parser<char, V> p, string s)
            {
                return Combinators.Alternate(p, Chars.String(s));
            }

            public static Parser<char> Or(this Parser<char> p, string s)
            {
                return Combinators.Alternate(p, Chars.String(s));
            }

            public static Parser<char, V> And<V>(this Parser<char, V> p, string s)
            {
                return Combinators.Sequence(p, Chars.String(s));
            }

            public static Parser<char> And(this Parser<char> p, string s)
            {
                return Combinators.Sequence(p, Chars.String(s));
            }
        }

        public static class CharCharExtensions
        {
            public static Parser<char> And(this char c1, char c2)
            {
                return Combinators.Sequence(Chars.Const(c1), Chars.Const(c2));
            }

            public static Parser<char> Or(this char c1, char c2)
            {
                return Combinators.Alternate(Chars.Const(c1), Chars.Const(c2));
            }
        }

        public static class CharStringExtensions
        {
            public static Parser<char> And(this char c, string s)
            {
                return Combinators.Sequence(Chars.Const(c), Chars.String(s));
            }

            public static Parser<char> Or(this char c, string s)
            {
                return Combinators.Alternate(Chars.Const(c), Chars.String(s));
            }
        }

        public static class StringCharExtensions
        {
            public static Parser<char> And(this string s, char c)
            {
                return Combinators.Sequence(Chars.String(s), Chars.Const(c));
            }

            public static Parser<char> Or(this string s, char c)
            {
                return Combinators.Alternate(Chars.String(s), Chars.Const(c));
            }
        }

        public static class StringStringExtensions
        {
            public static Parser<char> And(this string c1, string c2)
            {
                return Combinators.Sequence(Chars.String(c1), Chars.String(c2));
            }

            public static Parser<char> Or(this string c1, string c2)
            {
                return Combinators.Alternate(Chars.String(c1), Chars.String(c2));
            }
        }
    }
}
