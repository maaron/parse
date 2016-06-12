﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Functional;

namespace PushParse
{
    public static class Repeated
    {
        public static Parser<T, Unit, V> NonZero<T, S, V>(this Parser<T, S, V> parser)
        {
            ProcessToken<T, Unit, V> process = (s, t) =>
            {
                int pos = s.Position;
                s.Push(parser, r => s.Pop(r));
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
    }
}
