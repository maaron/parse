using System;
using System.Collections.Generic;
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
                return left(input).Map(
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
                return left(input).Map(
                    (success) => Result.Match(new Variant<V1, V2>(success.Value), success.Remaining),
                    (failure) => right(input).MapValue(v => new Variant<V1, V2>(v)));
            };
        }

        public static Parser<T, Variant<V1, V2, V3>> Or<T, V1, V2, V3>(
            this Parser<T, Variant<V1, V2>> left,
            Parser<T, V3> right)
        {
            return OrImpl(left, right,
                l => new Variant<V1, V2, V3>(l));
        }

        public static Parser<T, Variant<V1, V2, V3, V4>> Or<T, V1, V2, V3, V4>(
            this Parser<T, Variant<V1, V2, V3>> left,
            Parser<T, V4> right)
        {
            return OrImpl(left, right,
                l => new Variant<V1, V2, V3, V4>(l));
        }

        public static Parser<T, Variant<V1, V2, V3, V4, V5>> Or<T, V1, V2, V3, V4, V5>(
            this Parser<T, Variant<V1, V2, V3, V4>> left,
            Parser<T, V5> right)
        {
            return OrImpl(left, right,
                l => new Variant<V1, V2, V3, V4, V5>(l));
        }

        public static Parser<T, Variant<V1, V2, V3, V4, V5, V6>> Or<T, V1, V2, V3, V4, V5, V6>(
            this Parser<T, Variant<V1, V2, V3, V4, V5>> left,
            Parser<T, V6> right)
        {
            return OrImpl(left, right,
                l => new Variant<V1, V2, V3, V4, V5, V6>(l));
        }

        public static Parser<T, Variant<V1, V2, V3, V4, V5, V6, V7>> Or<T, V1, V2, V3, V4, V5, V6, V7>(
            this Parser<T, Variant<V1, V2, V3, V4, V5, V6>> left,
            Parser<T, V7> right)
        {
            return OrImpl(left, right,
                l => new Variant<V1, V2, V3, V4, V5, V6, V7>(l));
        }

        public static Parser<T, Variant<V1, V2, V3, V4, V5, V6, V7, V8>> Or<T, V1, V2, V3, V4, V5, V6, V7, V8>(
            this Parser<T, Variant<V1, V2, V3, V4, V5, V6, V7>> left,
            Parser<T, V8> right)
        {
            return OrImpl(left, right,
                l => new Variant<V1, V2, V3, V4, V5, V6, V7, V8>(l));
        }

        public static Parser<T, V3> OrImpl<T, V1, V2, V3>(
            Parser<T, V1> left,
            Parser<T, V2> right,
            Func<Variant, V3> f) where V1 : Variant
        {
            return input =>
            {
                return left(input).Map(
                    success => Result.Match(f(success.Value), success.Remaining),
                    failure => right(input).MapValue(r => f(new Variant<V2>(r))));
            };
        }

        public static Parser<T, V> OrSame<T, V>(
            this Parser<T, V> left,
            Parser<T, V> right)
        {
            return input => left(input).Map(
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

        public static Parser<T> AnyOf<T>(
            params Parser<T>[] parsers)
        {
            return AnyOf((IEnumerable<Parser<T>>)parsers);
        }

        public static Parser<T> AnyOf<T>(IEnumerable<Parser<T>> parsers)
        {
            return (input) =>
            {
                return parsers.Select(p => p(input))
                    .Where(r => r.IsSuccess).FirstOrDefault()
                        ?? Result.Fail<T>(input);
            };
        }
    }
}
