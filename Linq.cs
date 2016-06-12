using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    namespace Linq
    {
        public static class Extensions
        {
            public static Parser<T, R> Select<T, V, R>(
                this Parser<T, V> p, 
                Func<V, R> f)
            {
                return input => p(input).MapValue(f);
            }

            public static Parser<T, V> Where<T, V>(
                this Parser<T, V> parser,
                Func<V, bool> predicate)
            {
                return input =>
                {
                    return parser(input).Map(
                        (success) => predicate(success.Value) ?
                            Result.Match(success.Value, success.Remaining) :
                            Result.Fail<T, V>(input),
                        (failure) => Result.Fail<T, V>(input));
                };
            }

            public static Parser<T, V2> SelectMany<T, V1, V2>(
                this Parser<T, V1> parser,
                Func<V1, Parser<T, V2>> selector)
            {
                return input => parser(input).Map(
                    (success) => selector(success.Value)(success.Remaining),
                    (failure) => Result.Fail<T, V2>(failure.Remaining));
            }

            public static Parser<T, V3> SelectMany<T, V1, V2, V3>(
                this Parser<T, V1> parser,
                 Func<V1, Parser<T, V2>> selector,
                 Func<V1, V2, V3> projector)
            {
                return parser.SelectMany(
                    v1 => selector(v1).Select(
                        v2 => projector(v1, v2)));
            }
        }
    }
}

namespace Functional.Linq
{
    public static class MaybeExtensions
    {
        public static Maybe<R> Select<T, R>(this Maybe<T> m, Func<T, R> selector)
        {
            return m.HasValue ? Maybe.Some(selector(m.Value)) 
                : Maybe.None<R>();
        }

        public static Maybe<R> SelectMany<T, R>(
                this Maybe<T> m,
                Func<T, Maybe<R>> selector)
        {
            return m.HasValue ? selector(m.Value) 
                : Maybe.None<R>();
        }

        public static Maybe<R2> SelectMany<T, R1, R2>(
            this Maybe<T> parser,
             Func<T, Maybe<R1>> selector,
             Func<T, R1, R2> projector)
        {
            return parser.SelectMany(
                v1 => selector(v1).Select(
                    v2 => projector(v1, v2)));
        }

        public static Maybe<T> Where<T>(this Maybe<T> m, Func<T, bool> predicate)
        {
            return m.HasValue && predicate(m.Value) ? m 
                : Maybe.None<T>();
        }
    }

    public static class NullableExtensions
    {
        public static Nullable<R> Select<T, R>(this Nullable<T> m, Func<T, R> selector)
            where T : struct
            where R : struct
        {
            return m.HasValue ? new Nullable<R>(selector(m.Value))
                : new Nullable<R>();
        }

        public static Nullable<R> SelectMany<T, R>(
                this Nullable<T> m,
                Func<T, Nullable<R>> selector)
            where T : struct
            where R : struct
        {
            return m.HasValue ? selector(m.Value)
                : new Nullable<R>();
        }

        public static Nullable<R2> SelectMany<T, R1, R2>(
            this Nullable<T> parser,
             Func<T, Nullable<R1>> selector,
             Func<T, R1, R2> projector)
            where T : struct
            where R1 : struct
            where R2 : struct
        {
            return parser.SelectMany(
                v1 => selector(v1).Select(
                    v2 => projector(v1, v2)));
        }

        public static Maybe<T> AsMaybe<T>(this T? n)
            where T : struct
        {
            return n.HasValue ? Maybe.Some(n.Value)
                : Maybe.None<T>();
        }
    }
}
