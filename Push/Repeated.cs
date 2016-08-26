using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Functional;
using Functional.Linq;

namespace Parse.Push
{
    public static class Repeated
    {
        public static Parser<T, Unit, V> NonZero<T, S, V>(this Parser<T, S, V> parser)
        {
            ProcessToken<T, Unit, V> process = (s, t) =>
            {
                int pos = s.Position;
                s.Push(parser, r => s.Pop(s.Position > pos ? r : Maybe.None<V>()));
            };

            return () => Tuple.Create(default(Unit), process);
        }

        public static Parser<T, R, R> Aggregate<T, S, V, R>(this Parser<T, S, V> parser, Func<R> seed, Func<R, V, R> accumulate)
        {
            ProcessToken<T, R, R> process = (s, t) =>
                s.Push(parser.NonZero(), m => m.Visit(
                    () => s.Pop(Maybe.Some(s.Value)),
                    some => s.Value = accumulate(s.Value, some)));

            return () => Tuple.Create(seed(), process);
        }

        public static Parser<T, FList<V>, FList<V>> ZeroOrMore<T, S, V>(this Parser<T, S, V> parser)
        {
            return parser.Aggregate(() => new FList<V>(), (list, r) =>
            {
                list.Add(r);
                return list;
            });
        }
    }
}
