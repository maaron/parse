using System;
using Functional;

namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T> Alternate<T>(
            Parser<T> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => right(success.Remaining),
                    (failure) => Result.Fail<T>(failure.Remaining));
            };
        }

        public static Parser<T, Maybe<V>> Alternate<T, V>(
            Parser<T, V> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Result.Match(new Maybe<V>(success.Value), success.Remaining),
                    (failure) => right(input).MapValue(() => new Maybe<V>()));
            };
        }

        public static Parser<T, Maybe<V>> Alternate<T, V>(
            Parser<T> left,
            Parser<T, V> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Result.Match(new Maybe<V>(), success.Remaining),
                    (failure) => right(input).MapValue(v => new Maybe<V>(v)));
            };
        }

        public static Parser<T, Either<V1, V2>> Alternate<T, V1, V2>(
            Parser<T, V1> left,
            Parser<T, V2> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Result.Match(new Either<V1, V2>(success.Value), success.Remaining),
                    (failure) => right(input).MapValue(v => new Either<V1, V2>(v)));
            };
        }

        public static Parser<T, V> AlternateSame<T, V>(
            Parser<T, V> left,
            Parser<T, V> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Result.Match(success.Value, success.Remaining),
                    (failure) => right(input));
            };
        }
    }
}
