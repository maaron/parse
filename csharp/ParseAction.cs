using System;
using Functional;
using Parse.Extensions;

namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T, V> ParseAction<T, V>(
            Parser<T, V> parser,
            Action<Either<Success<T, V>, Failure<T>>> action)
        {
            return (input) =>
            {
                var result = parser(input);
                action(result);
                return result;
            };
        }

        public static Parser<T, V> MatchAction<T, V>(
            Parser<T, V> parser,
            Action<V> action)
        {
            return parser.Return(v =>
                {
                    action(v);
                    return v;
                });
        }

        public static Parser<T, V> MatchAction<T, V>(
            Parser<T, V> parser,
            Action<V, IParseInput<T>> action)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) =>
                    {
                        action(success.Value, success.Remaining);
                        return Result.Match(success.Value, success.Remaining);
                    },
                    (failure) => Result.Fail<T, V>(failure.Remaining));
            };
        }

        public static Parser<T, V> FailAction<T, V>(
            Parser<T, V> parser,
            Action action)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) =>
                    {
                        return Result.Match(success.Value, success.Remaining);
                    },
                    (failure) =>
                    {
                        action();
                        return Result.Fail<T, V>(failure.Remaining);
                    });
            };
        }
    }
}
