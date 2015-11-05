using System;

namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T> Ignore<T, V>(
            Parser<T, V> parser)
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
