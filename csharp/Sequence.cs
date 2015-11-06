using System;
using Functional;

namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T, Tuple<V1, V2, V3>> Sequence<T, V1, V2, V3>(
                Parser<T, Tuple<V1, V2>> left,
                Parser<T, V3> right)
        {
            return SequenceImpl(left, right,
                (l, r) => Tuple.Create(l.Item1, l.Item2, r));
        }

        public static Parser<T, Tuple<V1, V2, V3>> Sequence<T, V1, V2, V3>(
            Parser<T, V1> left,
            Parser<T, Tuple<V2, V3>> right)
        {
            return SequenceImpl(left, right,
                (l, r) => Tuple.Create(l, r.Item1, r.Item2));
        }

        public static Parser<T, Tuple<V1, V2>> Sequence<T, V1, V2>(
            Parser<T, V1> left,
            Parser<T, V2> right)
        {
            return SequenceImpl(left, right,
                (l, r) => Tuple.Create(l, r));
        }

        public static Parser<T, V> Sequence<T, V>(
            Parser<T> left,
            Parser<T, V> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => right(success.Remaining).MapValue(),
                    (failure) => Result.Fail<T, V>(input));
            };
        }

        public static Parser<T, V> Sequence<T, V>(
            Parser<T, V> left,
            Parser<T> right)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => right(success.Remaining).MapValue(() => success.Value),
                    (failure) => Result.Fail<T, V>(input));
            };
        }

        public static Parser<T> Sequence<T>(
            Parser<T> left,
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
        private static Parser<T, V3> SequenceImpl<T, V1, V2, V3>(
            Parser<T, V1> left,
            Parser<T, V2> right,
            Func<V1, V2, V3> f)
        {
            return (input) =>
            {
                return left(input).Visit(
                    (success) => right(success.Remaining).MapValue(r => f(success.Value, r)),
                    (failure) => Result.Fail<T, V3>(input));
            };
        }
    }
}
