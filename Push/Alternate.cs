using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Functional;
using Functional.Linq;

namespace PushParse.Combinators
{
    public static class Alternate
    {
        public static Parser<T, Unit, Variant<V1, V2>> Or<T, S1, S2, V1, V2>(this Parser<T, S1, V1> p1, Parser<T, S2, V2> p2)
        {
            ProcessToken<T, Unit, Variant<V1, V2>> process = (s, t) =>
                s.Push(p1, r1 => r1.Visit(
                    () => s.Backtrack().Push(p2, 
                        r2 => s.Pop(from r in r2 select new Variant<V1, V2>(r))),
                    some => s.Pop(Maybe.Some(new Variant<V1, V2>(some)))));

            return () => Tuple.Create(default(Unit), process);
        }
    }
}
