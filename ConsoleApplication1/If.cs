using System;

namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T, V> If<T, V>(
            Parser<T, V> parser,
            Func<V, bool> predicate)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => predicate(success.Value) ?
                        Result.Match(success.Value, success.Remaining) :
                        Result.Fail<T, V>(input),
                    (failure) => Result.Fail<T, V>(failure.Remaining));
            };
        }
    }
}
