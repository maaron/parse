using System;
using System.Collections.Generic;
using Functional;
using Parse.Extensions;

namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T, List<V>> Split<T, V>(
            Parser<T, V> parser,
            Parser<T> delimiter)
        {
            return parser.And(delimiter.And(parser).Repeated()).Return(t =>
            {
                t.Item2.Insert(0, t.Item1); return t.Item2;
            });
        }

        public static Parser<T, List<V1>> Split<T, V1, V2>(
            Parser<T, V1> parser,
            Parser<T, V2> delimiter)
        {
            return Split(parser, Ignore(delimiter));
        }
    }
}
