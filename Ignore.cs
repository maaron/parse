using System;

namespace Parse.Combinators
{
    public static partial class Extensions
    {
        public static Parser<T> Ignored<T, V>(
            this Parser<T, V> parser)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => Result.Match(success.Remaining),
                    (failure) => Result.Fail(input));
            };
        }
    }
}
