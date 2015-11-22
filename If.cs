using System;

namespace Parse.Combinators
{
    public static partial class Extensions
    {
        public static Parser<T, V> If<T, V>(
            this Parser<T, V> parser,
            Func<V, bool> predicate)
        {
            return (input) =>
            {
                return parser(input).Map(
                    (success) => predicate(success.Value) ?
                        Result.Match(success.Value, success.Remaining) :
                        Result.Fail<T, V>(input),
                    (failure) => Result.Fail<T, V>(input));
            };
        }

        public static Parser<T> If<T>(
            this Parser<T> parser,
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
