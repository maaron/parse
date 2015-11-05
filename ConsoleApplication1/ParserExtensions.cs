using System;
using Functional;

namespace Parse
{
    public static class ParserExtensions
    {
        public static Parser<T> And<T>(this Parser<T> p, Parser<T> next)
        {
            return Combinators.Sequence(p, next);
        }

        public static Parser<T, V> And<T, V>(this Parser<T, V> p, Parser<T> next)
        {
            return Combinators.Sequence(p, next);
        }

        public static Parser<T, V> And<T, V>(this Parser<T> p, Parser<T, V> next)
        {
            return Combinators.Sequence(p, next);
        }

        public static Parser<T, Tuple<V1, V2>> And<T, V1, V2>(this Parser<T, V1> p, Parser<T, V2> next)
        {
            return Combinators.Sequence(p, next);
        }

        public static Parser<T, Tuple<V1, V2, V3>> And<T, V1, V2, V3>(this Parser<T, Tuple<V1, V2>> p, Parser<T, V3> next)
        {
            return Combinators.Sequence(p, next);
        }

        public static Parser<T, Tuple<V1, V2, V3>> And<T, V1, V2, V3>(this Parser<T, V1> p, Parser<T, Tuple<V2, V3>> next)
        {
            return Combinators.Sequence(p, next);
        }

        public static Parser<T> Or<T>(this Parser<T> p, Parser<T> next)
        {
            return Combinators.Alternate(p, next);
        }

        public static Parser<T, Maybe<V>> Or<T, V>(this Parser<T, V> p, Parser<T> next)
        {
            return Combinators.Alternate(p, next);
        }

        public static Parser<T, Maybe<V>> Or<T, V>(this Parser<T> p, Parser<T, V> next)
        {
            return Combinators.Alternate(p, next);
        }

        public static Parser<T, Either<V1, V2>> Or<T, V1, V2>(this Parser<T, V1> p, Parser<T, V2> next)
        {
            return Combinators.Alternate(p, next);
        }
    }
}
