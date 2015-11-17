using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class Util
    {
        public static bool IfSame<T>(T t, object rhs, Func<T, T, bool> f)
        {
            if (rhs is T)
            {
                return f(t, (T)rhs);
            }
            else return false;
        }
    }
}
