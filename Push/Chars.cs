using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Functional;

namespace PushParse
{
    public static class Chars
    {
        public static Parser<char, Unit, char> Any = () =>
        {
            ProcessToken<char, Unit, char> process = (s, t) => s.Pop(t);
            return Tuple.Create(default(Unit), process);
        };

        public static Parser<char, Unit, char> Const(char c)
        {
            return Any.Where(t => t == c);
        }

        public static Parser<char, int, string> String(string str)
        {
            ProcessToken<char, int, string> process = (s, t) =>
            {
                if (str[s.Value++] != t) s.Pop(Maybe.None<string>());
                else if (s.Value >= str.Length) s.Pop(Maybe.Some(str));
            };

            return () => Tuple.Create(0, process);
        }
    }
}
