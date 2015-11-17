using System;
using Functional;

namespace Parse.Combinators
{
    public static partial class Extensions
    {
        public static Parser<T, Maybe<V>> Optional<T, V>(this Parser<T, V> parser)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => Result.Match(new Maybe<V>(success.Value), success.Remaining),
                    (failure) => Result.Match(new Maybe<V>(), input));
            };
        }

        public static Parser<T, bool> Optional<T>(this Parser<T> parser)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => Result.Match(true, success.Remaining),
                    (failure) => Result.Match(false, input));
            };
        }
    }
}
