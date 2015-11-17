using System;

namespace Parse.Combinators
{
    public static partial class Extensions
    {
        public static Parser<T> Between<T>(this Parser<T> p, Parser<T> delim)
        {
            return delim.And(p).And(delim);
        }

        public static Parser<T, V> Between<T, V>(this Parser<T, V> p, Parser<T> delim)
        {
            return delim.And(p).And(delim);
        }

        public static Parser<T> Between<T>(this Parser<T> p, Parser<T> left, Parser<T> right)
        {
            return left.And(p).And(right);
        }

        public static Parser<T, V> Between<T, V>(this Parser<T, V> p, Parser<T> left, Parser<T> right)
        {
            return left.And(p).And(right);
        }
    }
}
