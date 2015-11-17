using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public class Anchor<T, V>
    {
        public IParseInput<T> Start { get; private set; }
        public IParseInput<T> End { get; private set; }

        public V Value { get; private set; }

        public Anchor(IParseInput<T> start, IParseInput<T> end, V value)
        {
            Start = start;
            End = end;
            Value = value;
        }
    }

    namespace Combinators
    {
        public static partial class Extensions
        {
            public static Parser<T, Anchor<T, V>> Anchored<T, V>(this Parser<T, V> p)
            {
                return input => p(input).Visit(
                    success => Result.Match(
                        new Anchor<T, V>(
                            input, success.Remaining, success.Value), 
                        success.Remaining),
                    failure => Result.Fail<T, Anchor<T, V>>(failure.Remaining));
            }
        }
    }
}
