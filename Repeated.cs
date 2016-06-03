using System;
using System.Collections.Generic;
using System.Linq;
using Functional;

using Parse.Linq;
using Parse.Combinators;

namespace Parse.Combinators
{
    public static partial class Extensions
    {
        private static IEnumerable<Success<T, V>> Matches<T, V>(this Parser<T, V> parser, IParseInput<T> input)
        {
            while (true)
            {
                var result = parser(input);
                if (result.IsFailure || result.Remaining.Equals(input)) break;

                yield return result.Success;
                input = result.Remaining;
            }
        }

        public static Parser<T, FList<V>> ZeroOrMore<T, V>(
            this Parser<T, V> parser)
        {
            return parser.ZeroOrMoreAggregate(() => new FList<V>(), (list, value) =>
            {
                list.Add(value);
                return list;
            });
        }

        public static Parser<T> ZeroOrMore<T>(
            this Parser<T> parser)
        {
            return (input) =>
            {
                bool failed = false;
                while (!failed)
                {
                    parser(input).Visit(
                        (success) =>
                        {
                            if (input.Equals(success.Remaining)) failed = true;
                            else input = success.Remaining;
                        },
                        (failure) => { failed = true; });
                }
                return Result.Match(input);
            };
        }

        public static Parser<T, R> ZeroOrMoreAggregate<T, V, R>(
            this Parser<T, V> parser, Func<R> seed, Func<R, V, R> func)
        {
            return input => parser.Matches(input).Aggregate(
                Result.Match(seed(), input),
                (accum, success) => Result.Match(
                    func(accum.Success.Value, success.Value), 
                    success.Remaining));
        }

        public static Parser<T, FList<V>> AtMost<T, V>(
            this Parser<T, V> parser, int count)
        {
            return (input) =>
            {
                var matches = new FList<V>();
                bool failed = false;
                for (int i = 0; i < count && !failed; i++)
                {
                    parser(input).Visit(
                        (success) =>
                        {
                            if (input.Equals(success.Remaining)) failed = true;
                            else
                            {
                                input = success.Remaining;
                                matches.Add(success.Value);
                            }
                        },
                        (failure) => { failed = true; });
                }
                return Result.Match(matches, input);
            };
        }

        public static Parser<T> AtMost<T>(
            this Parser<T> parser, int count)
        {
            return (input) =>
            {
                bool failed = false;
                for (int i = 0; i < count && !failed; i++)
                {
                    parser(input).Visit(
                        (success) =>
                        {
                            if (input.Equals(success.Remaining)) failed = true;
                            else input = success.Remaining;
                        },
                        (failure) => { failed = true; });
                }
                return Result.Match(input);
            };
        }

        public static Parser<T> Repeat<T>(
            this Parser<T> parser, int count)
        {
            return (input) =>
            {
                bool failed = false;
                for (int i = 0; i < count; i++)
                {
                    parser(input).Visit(
                        (success) =>
                        {
                            if (input.Equals(success.Remaining)) failed = true;
                            else input = success.Remaining;
                        },
                        (failure) => { failed = true; });
                }
                return failed ? Result.Fail(input)
                    : Result.Match(input);
            };
        }

        public static Parser<T, FList<V>> Repeat<T, V>(
            this Parser<T, V> parser, int count)
        {
            return (input) =>
            {
                var matches = new FList<V>();
                bool failed = false;
                for (int i = 0; i < count; i++)
                {
                    parser(input).Visit(
                        (success) =>
                        {
                            matches.Add(success.Value);
                            input = success.Remaining;
                        },
                        (failure) => { failed = true; });
                }
                return failed ? Result.Fail<T, FList<V>>(input)
                    : Result.Match(matches, input);
            };
        }

        public static Parser<T, FList<V>> Many<T, V>(this Parser<T, V> parser, int min)
        {
            return ZeroOrMore(parser).Where(l => l.Count >= min);
        }

        public static Parser<T> Many<T>(this Parser<T> parser, int min)
        {
            return Repeat(parser, min).And(ZeroOrMore(parser));
        }

        public static Parser<T> Many<T>(this Parser<T> parser, int min, int max)
        {
            return parser.Repeat(min).And(AtMost(parser, max));
        }
    }
}
