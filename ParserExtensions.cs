using System;
using System.Collections.Generic;
using Functional;

namespace Parse
{
    namespace Extensions
    {
        public static class ParserExtensions
        {
            public static Parser<T> And<T>(this Parser<T> p, Parser<T> next)
            {
                return Combinators.Sequence(p, next);
            }

            public static Parser<T, V> And<T, V>(this Parser<T, V> p, Parser<T> next)
            {
                return Combinators.Sequence(p, next);
            }

            public static Parser<T, V> And<T, V>(this Parser<T> p, Parser<T, V> next)
            {
                return Combinators.Sequence(p, next);
            }

            public static Parser<T, Tuple<V1, V2>> And<T, V1, V2>(this Parser<T, V1> p, Parser<T, V2> next)
            {
                return Combinators.Sequence(p, next);
            }

            public static Parser<T, Tuple<V1, V2, V3>> And<T, V1, V2, V3>(this Parser<T, Tuple<V1, V2>> p, Parser<T, V3> next)
            {
                return Combinators.Sequence(p, next);
            }

            public static Parser<T, Tuple<V1, V2, V3>> And<T, V1, V2, V3>(this Parser<T, V1> p, Parser<T, Tuple<V2, V3>> next)
            {
                return Combinators.Sequence(p, next);
            }

            public static Parser<T> Or<T>(this Parser<T> p, Parser<T> next)
            {
                return Combinators.Alternate(p, next);
            }

            public static Parser<T, Maybe<V>> Or<T, V>(this Parser<T, V> p, Parser<T> next)
            {
                return Combinators.Alternate(p, next);
            }

            public static Parser<T, Maybe<V>> Or<T, V>(this Parser<T> p, Parser<T, V> next)
            {
                return Combinators.Alternate(p, next);
            }

            public static Parser<T, Variant<V1, V2>> Or<T, V1, V2>(this Parser<T, V1> p, Parser<T, V2> next)
            {
                return Combinators.Alternate(p, next);
            }

            public static Parser<T, Variant<V1, V2, V3>> Or<T, V1, V2, V3>(this Parser<T, Variant<V1, V2>> p, Parser<T, V3> next)
            {
                return Combinators.Alternate(p, next);
            }

            // This is a special case of alternation where both parsers return 
            // the same value type.  With normal Or(), you'd still wind up with 
            // an Variant<T, T>.  OrSame, on the other hand, coalesces 
            // Variant<T,T> to just T.  This is particularly useful, for example, 
            // when building character classes, e.g., Digit.OrSame(Letter).  
            // Unfortunately, type inference fails if we where to rename this
            // method to just Or().
            public static Parser<T, V> OrSame<T, V>(this Parser<T, V> p, Parser<T, V> next)
            {
                return Combinators.AlternateSame(p, next);
            }

            public static Parser<T, FList<V>> SplitBy<T, V>(this Parser<T, V> p, Parser<T> delimiter)
            {
                return Combinators.Split(p, delimiter);
            }

            public static Parser<T, FList<V1>> SplitBy<T, V1, V2>(this Parser<T, V1> p, Parser<T, V2> delimiter)
            {
                return Combinators.Split(p, delimiter);
            }

            public static Parser<T, V2> Return<T, V1, V2>(this Parser<T, V1> p, Func<V1, V2> f)
            {
                return (input) => p(input).MapValue(f);
            }

            public static Parser<T, V> Return<T, V>(this Parser<T> p, Func<V> f)
            {
                return (input) => p(input).MapValue(f);
            }

            public static Parser<T, V> If<T, V>(this Parser<T, V> p, Func<V, bool> predicate)
            {
                return Combinators.Condition(p, predicate);
            }

            public static Parser<T> If<T>(this Parser<T> p, Func<bool> predicate)
            {
                return Combinators.Condition(p, predicate);
            }

            public static Parser<T> Repeated<T>(this Parser<T> p)
            {
                return Combinators.ZeroOrMore(p);
            }

            public static Parser<T, FList<V>> Repeated<T, V>(this Parser<T, V> p)
            {
                return Combinators.ZeroOrMore(p);
            }

            public static Parser<T, FList<V>> Repeated<T, V>(this Parser<T, V> p, int min)
            {
                return Combinators.AtLeastMany(p, min);
            }

            public static Parser<T> Repeated<T>(this Parser<T> p, int min)
            {
                return Combinators.AtLeastMany(p, min);
            }

            public static Parser<T> Except<T>(this Parser<T> p, Parser<T> not)
            {
                return Combinators.Not(not).And(p);
            }

            public static Parser<T, V> Except<T, V>(this Parser<T, V> p, Parser<T> not)
            {
                return Combinators.Not(not).And(p);
            }

            public static Parser<T> Except<T, V>(this Parser<T> p, Parser<T, V> not)
            {
                return Combinators.Not(not).And(p);
            }

            public static Parser<T, V1> Except<T, V1, V2>(this Parser<T, V1> p, Parser<T, V2> not)
            {
                return Combinators.Not(not).And(p);
            }

            public static Parser<T, V> OnParse<T, V>(this Parser<T, V> p, Action<Result<T, V>> action)
            {
                return Combinators.ParseAction(p, action);
            }

            public static Parser<T, V> OnMatch<T, V>(this Parser<T, V> p, Action<V> action)
            {
                return Combinators.MatchAction(p, action);
            }

            public static Parser<T, V> OnMatch<T, V>(this Parser<T, V> p, Action<V, IParseInput<T>> action)
            {
                return Combinators.MatchAction(p, action);
            }

            public static Parser<T, V> OnFail<T, V>(this Parser<T, V> p, Action action)
            {
                return Combinators.FailAction(p, action);
            }

            public static Parser<T> Ignored<T, V>(this Parser<T, V> p)
            {
                return Combinators.Ignore(p);
            }

            public static Parser<T> Between<T>(this Parser<T> p, Parser<T> delim)
            {
                return delim.And(p).And(delim);
            }

            public static Parser<T, V> Between<T, V>(this Parser<T, V> p, Parser<T> delim)
            {
                return delim.And(p).And(delim);
            }

            public static Parser<T> Between<T>(this Parser<T> p, Parser<T> left, Parser<T> right)
            {
                return left.And(p).And(right);
            }

            public static Parser<T, V> Between<T, V>(this Parser<T, V> p, Parser<T> left, Parser<T> right)
            {
                return left.And(p).And(right);
            }

            public static Parser<T, Anchor<T, V>> Anchored<T, V>(this Parser<T, V> p)
            {
                return (input) => p(input).Visit(
                    (success) => Result.Match(
                        new Anchor<T, V>(
                            input, 
                            success.Remaining, 
                            success.Value), 
                        success.Remaining),
                    (failure) => Result.Fail<T, Anchor<T, V>>(failure.Remaining));
            }
        }
    }
}
