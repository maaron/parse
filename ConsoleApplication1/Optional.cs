using System;
using Functional;

namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T, Maybe<V>> Optional<T, V>(Parser<T, V> parser)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => Result.Match(new Maybe<V>(success.Value), success.Remaining),
                    (failure) => Result.Match(new Maybe<V>(), input));
            };
        }
    }
}
