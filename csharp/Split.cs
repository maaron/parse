using System;
using System.Collections.Generic;
using Functional;
using Parse.Extensions;

namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T, List<V>> Split<T, V>(
            Parser<T, V> parser,
            Parser<T> delimiter)
        {
            var both = parser.And(Combinators.Optional(delimiter));

            return (input) =>
            {
                var matches = new List<V>();
                bool failed = false;
                bool stop = false;
                while (!stop && !failed)
                {
                    both(input).Visit(
                        (success) =>
                        {
                            if (input.Equals(success.Remaining))
                            {
                                stop = true;
                                matches.Add(success.Value.Item1);
                            }
                            else
                            {
                                stop = !success.Value.Item2;
                                input = success.Remaining;
                                matches.Add(success.Value.Item1);
                            }
                        },
                        (failure) => failed = true);
                }
                return failed ? Result.Fail<T, List<V>>(input)
                    : Result.Match(matches, input);
            };
        }

        public static Parser<T, List<V1>> Split<T, V1, V2>(
            Parser<T, V1> parser,
            Parser<T, V2> delimiter)
        {
            return Split(parser, Ignore(delimiter));
        }
    }
}
