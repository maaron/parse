using System;
using Parse.Combinators;
using Functional;

namespace Parse.CharCombinators
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

        public static Parser<char, int> ReturnInt(this Parser<char> p)
        {
            return Chars.String(p).Return(s => int.Parse(s));
        }

        public static Parser<char, int> ReturnInt<V>(this Parser<char, V> p)
        {
            return Chars.String(p).Return(s => int.Parse(s));
        }
    }

    public static class CharParserExtensions
    {
        public static Parser<char, V> And<V>(this char c, Parser<char, V> p)
        {
            return Chars.Const(c).And(p);
        }

        public static Parser<char> And(this char c, Parser<char> p)
        {
            return Chars.Const(c).And(p);
        }

        public static Parser<char, Maybe<V>> Or<V>(this char c, Parser<char, V> p)
        {
            return Chars.Const(c).Or(p);
        }

        public static Parser<char> Or(this char c, Parser<char> p)
        {
            return Chars.Const(c).Or(p);
        }
    }

    public static class ParserCharExtensions
    {
        public static Parser<char, V> And<V>(this Parser<char, V> p, char c)
        {
            return p.And(Chars.Const(c));
        }

        public static Parser<char> And(this Parser<char> p, char c)
        {
            return p.And(Chars.Const(c));
        }

        public static Parser<char, Maybe<V>> Or<V>(this Parser<char, V> p, char c)
        {
            return p.Or(Chars.Const(c));
        }

        public static Parser<char> Or(this Parser<char> p, char c)
        {
            return p.Or(Chars.Const(c));
        }

        public static Parser<char, V> Except<V>(this Parser<char, V> p, char c)
        {
            return (input) =>
                input.Current == c ? Result.Fail<char, V>(input)
                    : p(input);
        }

        public static Parser<char> Except(this Parser<char> p, char c)
        {
            return (input) => 
                input.Current == c ? Result.Fail(input)
                    : p(input);
        }

        public static Parser<char> Between(this Parser<char> p, char c)
        {
            return Chars.Const(c).And(p).And(Chars.Const(c));
        }

        public static Parser<char, V> Between<V>(this Parser<char, V> p, char c)
        {
            return Chars.Const(c).And(p).And(Chars.Const(c));
        }

        public static Parser<char> Between(this Parser<char> p, char c1, char c2)
        {
            return Chars.Const(c1).And(p).And(Chars.Const(c2));
        }

        public static Parser<char, V> Between<V>(this Parser<char, V> p, char c1, char c2)
        {
            return Chars.Const(c1).And(p).And(Chars.Const(c2));
        }
    }

    public static class StringParserExtensions
    {
        public static Parser<char, Maybe<V>> Or<V>(this string s, Parser<char, V> p)
        {
            return Chars.String(s).Or(p);
        }

        public static Parser<char> Or(this string s, Parser<char> p)
        {
            return Chars.String(s).Or(p);
        }

        public static Parser<char, V> And<V>(this string s, Parser<char, V> p)
        {
            return Chars.String(s).And(p);
        }

        public static Parser<char> And(this string s, Parser<char> p)
        {
            return Chars.String(s).And(p);
        }
    }

    public static class ParserStringExtensions
    {
        public static Parser<char, Maybe<V>> Or<V>(this Parser<char, V> p, string s)
        {
            return p.Or(Chars.String(s));
        }

        public static Parser<char> Or(this Parser<char> p, string s)
        {
            return p.Or(Chars.String(s));
        }

        public static Parser<char, V> And<V>(this Parser<char, V> p, string s)
        {
            return p.And(Chars.String(s));
        }

        public static Parser<char> And(this Parser<char> p, string s)
        {
            return p.And(Chars.String(s));
        }
    }

    public static class CharCharExtensions
    {
        public static Parser<char> And(this char c1, char c2)
        {
            return Chars.Const(c1).And(Chars.Const(c2));
        }

        public static Parser<char> Or(this char c1, char c2)
        {
            return Chars.Const(c1).Or(Chars.Const(c2));
        }
    }

    public static class CharStringExtensions
    {
        public static Parser<char> And(this char c, string s)
        {
            return Chars.Const(c).And(Chars.String(s));
        }

        public static Parser<char> Or(this char c, string s)
        {
            return Chars.Const(c).Or(Chars.String(s));
        }
    }

    public static class StringCharExtensions
    {
        public static Parser<char> And(this string s, char c)
        {
            return Chars.String(s).And(Chars.Const(c));
        }

        public static Parser<char> Or(this string s, char c)
        {
            return Chars.String(s).Or(Chars.Const(c));
        }
    }

    public static class StringStringExtensions
    {
        public static Parser<char> And(this string c1, string c2)
        {
            return Chars.String(c1).And(Chars.String(c2));
        }

        public static Parser<char> Or(this string c1, string c2)
        {
            return Chars.String(c1).Or(Chars.String(c2));
        }
    }
}
