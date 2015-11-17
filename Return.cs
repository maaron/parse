using System;

namespace Parse.Combinators
{
    public static partial class Extensions
    {
        public static Parser<T, V2> Return<T, V1, V2>(this Parser<T, V1> p, Func<V1, V2> f)
        {
            return (input) => p(input).MapValue(f);
        }

        public static Parser<T, V> Return<T, V>(this Parser<T> p, Func<V> f)
        {
            return (input) => p(input).MapValue(f);
        }
    }
}
