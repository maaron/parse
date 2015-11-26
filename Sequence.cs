using System;
using System.Collections.Generic;
using Functional;

namespace Parse.Combinators
{
    public static partial class Extensions
    {
        public static Parser<T, Tuple<V1, V2, V3>> And<T, V1, V2, V3>(
            this Parser<T, Tuple<V1, V2>> left,
            Parser<T, V3> right)
        {
            return And(left, right,
                (l, r) => Tuple.Create(l.Item1, l.Item2, r));
        }

        public static Parser<T, Tuple<V1, V2, V3>> And<T, V1, V2, V3>(
            this Parser<T, V1> left,
            Parser<T, Tuple<V2, V3>> right)
        {
            return And(left, right,
                (l, r) => Tuple.Create(l, r.Item1, r.Item2));
        }

        public static Parser<T, Tuple<V1, V2>> And<T, V1, V2>(
            this Parser<T, V1> left,
            Parser<T, V2> right)
        {
            return And(left, right,
                (l, r) => Tuple.Create(l, r));
        }

        public static Parser<T, V> And<T, V>(
            this Parser<T> left,
            Parser<T, V> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => right(success.Remaining),
                    (failure) => Result.Fail<T, V>(input));
            };
        }

        public static Parser<T, V> And<T, V>(
            this Parser<T, V> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Map(
                    (success) => right(success.Remaining).MapValue(() => success.Value),
                    (failure) => Result.Fail<T, V>(input));
            };
        }

        public static Parser<T> And<T>(
            this Parser<T> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => right(success.Remaining),
                    (failure) => Result.Fail<T>(input));
            };
        }

        // Used for Sequence chains whose result types have two or more 
        // values.  Function f returns the desired value by combining values 
        // from left and right parsers.
        public static Parser<T, R> And<T, V1, V2, R>(
            this Parser<T, V1> left,
            Parser<T, V2> right,
            Func<V1, V2, R> f)
        {
            return (input) =>
            {
                return left(input).Map(
                    (success) => right(success.Remaining).MapValue(r => f(success.Value, r)),
                    (failure) => Result.Fail<T, R>(input));
            };
        }
    }
}

namespace Parse
{
    public static partial class Combinator
    {
        public static Parser<T> Sequence<T>(
            IEnumerable<Parser<T>> parsers)
        {
            return input =>
            {
                Result<T> result = null;
                foreach (var p in parsers)
                {
                    result = p(input);
                    if (result.IsFailure) break;
                    input = result.Success.Remaining;
                }
                return result;
            };
        }
    }
}
