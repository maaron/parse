using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.Combinators
{
    public static partial class Extensions
    {
        public static Parser<T> Except<T>(this Parser<T> p, Parser<T> not)
        {
            return Not(not).And(p);
        }

        public static Parser<T, V> Except<T, V>(this Parser<T, V> p, Parser<T> not)
        {
            return Not(not).And(p);
        }

        public static Parser<T> Except<T, V>(this Parser<T> p, Parser<T, V> not)
        {
            return Not(not).And(p);
        }

        public static Parser<T, V1> Except<T, V1, V2>(this Parser<T, V1> p, Parser<T, V2> not)
        {
            return Not(not).And(p);
        }
    }
}
