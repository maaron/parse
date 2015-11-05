using System;
using System.Collections.Generic;
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

        // This is a special case of alternation where both parsers return 
        // the same value type.  With normal Or(), you'd still wind up with 
        // an Either<T, T>.  OrSame, on the other hand, coalesces 
        // Either<T,T> to just T.  This is particularly useful, for example, 
        // when building character classes, e.g., Digit.OrSame(Letter).  
        // Unfortunately, type inference fails if we where to rename this
        // method to just Or().
        public static Parser<T, V> OrSame<T, V>(this Parser<T, V> p, Parser<T, V> next)
        {
            return Combinators.AlternateSame(p, next);
        }

        public static Parser<T, List<V>> SplitBy<T, V>(this Parser<T, V> p, Parser<T> delimiter)
        {
            return Combinators.Split(p, delimiter);
        }

        public static Parser<T, List<V1>> SplitBy<T, V1, V2>(this Parser<T, V1> p, Parser<T, V2> delimiter)
        {
            return Combinators.Split(p, delimiter);
        }
    }
}
