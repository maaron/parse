using System;
using System.Collections.Generic;
using Functional;
using Parse.Extensions;

namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T, List<V>> ZeroOrMore<T, V>(
            Parser<T, V> parser)
        {
            return (input) =>
            {
                var matches = new List<V>();
                bool failed = false;
                while (!failed)
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

        public static Parser<T> ZeroOrMore<T>(
            Parser<T> parser)
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

        public static Parser<T, List<V>> AtLeastMany<T, V>(Parser<T, V> parser, int min)
        {
            return ZeroOrMore(parser).If(l => l.Count >= min);
        }
    }
}
