using System;
using System.Collections.Generic;
using Functional;

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
                            input = success.Remaining;
                            matches.Add(success.Value);
                        },
                        (failure) => { failed = true; });
                }
                return Result.Match(matches, input);
            };
        }
    }
}
