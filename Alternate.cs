using System;
using System.Linq;
using Functional;

namespace Parse.Combinators
{
    public static partial class ParserExtensions
    {
        public static Parser<T> Or<T>(
            this Parser<T> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Result.Match(success.Remaining),
                    (failure) => right(input));
            };
        }

        public static Parser<T, Maybe<V>> Or<T, V>(
            this Parser<T, V> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Result.Match(new Maybe<V>(success.Value), success.Remaining),
                    (failure) => right(input).MapValue(() => new Maybe<V>()));
            };
        }

        public static Parser<T, Maybe<V>> Or<T, V>(
            this Parser<T> left,
            Parser<T, V> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Result.Match(new Maybe<V>(), success.Remaining),
                    (failure) => right(input).MapValue(v => new Maybe<V>(v)));
            };
        }

        public static Parser<T, Variant<V1, V2>> Or<T, V1, V2>(
            this Parser<T, V1> left,
            Parser<T, V2> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => Result.Match(new Variant<V1, V2>(success.Value), success.Remaining),
                    (failure) => right(input).MapValue(v => new Variant<V1, V2>(v)));
            };
        }

        public static Parser<T, Variant<V1, V2, V3>> Or<T, V1, V2, V3>(
            this Parser<T, Variant<V1, V2>> left,
            Parser<T, V3> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    success => Result.Match(new Variant<V1, V2, V3>(success.Value), success.Remaining),
                    failure => right(input).MapValue(v => new Variant<V1, V2, V3>(v)));
            };
        }

        public static Parser<T, V> OrSame<T, V>(
            this Parser<T, V> left,
            Parser<T, V> right)
        {
            return input => left(input).Visit(
                success => Result.Match(success.Value, success.Remaining),
                failure => right(input));
        }
    }
}

namespace Parse
{
    public static partial class Combinator
    {
        public static Parser<T, V> AnyOf<T, V>(
            params Parser<T, V>[] parsers)
        {
            return (input) =>
            {
                return parsers.Select(p => p(input))
                    .Where(r => r.IsSuccess).FirstOrDefault() 
                        ?? Result.Fail<T, V>(input);
            };
        }
    }
}
