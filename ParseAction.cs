using System;

namespace Parse.Combinators
{
    public static partial class Extensions
    {
        public static Parser<T, V> OnParse<T, V>(
            this Parser<T, V> parser,
            Action<Result<T, V>> action)
        {
            return (input) =>
            {
                var result = parser(input);
                action(result);
                return result;
            };
        }

        public static Parser<T, V> OnMatch<T, V>(
            this Parser<T, V> parser,
            Action<V> action)
        {
            return parser.Return(v =>
                {
                    action(v);
                    return v;
                });
        }

        public static Parser<T, V> OnMatch<T, V>(
            this Parser<T, V> parser,
            Action<V, IParseInput<T>> action)
        {
            return (input) =>
            {
                return parser(input).Map(
                    (success) =>
                    {
                        action(success.Value, success.Remaining);
                        return Result.Match(success.Value, success.Remaining);
                    },
                    (failure) => Result.Fail<T, V>(failure.Remaining));
            };
        }

        public static Parser<T, V> OnFail<T, V>(
            this Parser<T, V> parser,
            Action action)
        {
            return (input) =>
            {
                return parser(input).Map(
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
