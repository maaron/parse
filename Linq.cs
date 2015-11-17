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
                    return parser(input).Visit(
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
                return input => parser(input).Visit(
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
