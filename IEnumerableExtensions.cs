using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Single<T>(this T t)
        {
            yield return t;
        }
    }
}
