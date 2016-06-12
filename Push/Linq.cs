using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Functional;
using Functional.Linq;

namespace PushParse
{
    public static class Linq
    {
        public static Parser<T, S, R> Select<T, S, V, R>(
            this Parser<T, S, V> p,
            Func<V, R> f)
        {
            ProcessToken<T, S, R> process = (s, t) => s.Push(
                p,
                mr => s.Pop(from r in mr select f(r)));

            return () => Tuple.Create(
                default(S), process);
        }

        public static Parser<T, S, V> Where<T, S, V>(
            this Parser<T, S, V> p,
            Func<V, bool> f)
        {
            ProcessToken<T, S, V> process = (s, t) => s.Push(
                p,
                mr => s.Pop(from r in mr where f(r) select r));

            return () => Tuple.Create(
                default(S), process);
        }

        public static Parser<T, Unit, V2> SelectMany<T, S1, S2, V1, V2>(
            this Parser<T, S1, V1> parser,
            Func<V1, Parser<T, S2, V2>> selector)
        {
            ProcessToken<T, Unit, V2> process = (s, t) =>
            {
                s.Push(parser, r => r.Visit(
                    () => s.Pop(r),
                    some => s.Push(
                        selector(some), 
                        r2 => s.Pop(r2))));
            };

            return () => Tuple.Create(default(Unit), process);
        }

        public static Parser<T, Unit, V3> SelectMany<T, S1, S2, V1, V2, V3>(
            this Parser<T, S1, V1> parser,
                Func<V1, Parser<T, S2, V2>> selector,
                Func<V1, V2, V3> projector)
        {
            return parser.SelectMany(
                v1 => selector(v1).Select(
                    v2 => projector(v1, v2)));
        }
    }
}
