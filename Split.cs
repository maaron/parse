using System;
using System.Collections.Generic;
using Functional;
using Parse.Combinators;

namespace Parse.Combinators
{
    public static partial class Extensions
    {
        public static Parser<T, FList<V>> SplitBy<T, V>(
            this Parser<T, V> parser,
            Parser<T> delimiter)
        {
            return parser.And(delimiter.And(parser).ZeroOrMore()).Return(t =>
            {
                t.Item2.Insert(0, t.Item1); return t.Item2;
            });
        }

        public static Parser<T, FList<V1>> SplitBy<T, V1, V2>(
            this Parser<T, V1> parser,
            Parser<T, V2> delimiter)
        {
            return SplitBy(parser, delimiter.Ignored());
        }
    }
}
