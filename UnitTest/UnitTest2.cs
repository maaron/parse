using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Functional;
using ParserClass;

namespace UnitTest2
{
    public class UnitTest2
    {
        public void TestParser2()
        {
            var AnyChar = Combinator.AnyChar;
            Parser<char, char> p1 = AnyChar;
            Parser<char, int> p2 = from c in AnyChar select (int)c;
            Parser<char, char> p3 = Combinator.Const('c');

            var p = from c in p1
                    from i in p2
                    from c2 in p3
                    select Tuple.Create(c, i, c2);

            var digit = from c in AnyChar where char.IsDigit(c) select c;
            var digit2 = AnyChar.Where(c => char.IsDigit(c));
        }
    }
}
