using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parse;

namespace Parse
{
    public class Chars : Combinators
    {
        public static Parser<char, char> Any = (input) =>
        {
            return input.IsEnd ? Result.Fail<char, char>(input)
                : Result.Match(input.Current, input.Next());
        };

        public static Parser<char> Const(char c)
        {
            return Ignore(If(Any, a => a == c));
        }

        public static Parser<char, char> Space = If(Any, System.Char.IsWhiteSpace);
        public static Parser<char, char> Control = If(Any, System.Char.IsControl);
        public static Parser<char, char> Digit = If(Any, System.Char.IsDigit);
        public static Parser<char, char> Letter = If(Any, System.Char.IsLetter);
    }
}
