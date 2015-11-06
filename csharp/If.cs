using System;

namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T, V> Condition<T, V>(
            Parser<T, V> parser,
            Func<V, bool> predicate)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => predicate(success.Value) ?
                        Result.Match(success.Value, success.Remaining) :
                        Result.Fail<T, V>(input),
                    (failure) => Result.Fail<T, V>(input));
            };
        }

        public static Parser<T> Condition<T>(
            Parser<T> parser,
            Func<bool> predicate)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => predicate() ?
                        Result.Match(success.Remaining) :
                        Result.Fail<T>(input),
                    (failure) => Result.Fail<T>(input));
            };
        }
    }
}
