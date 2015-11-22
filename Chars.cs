using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parse.Combinators;
using Parse.InputExtensions;

namespace Parse
{
    namespace CharCombinators
    {
        public class Chars
        {
            public static Parser<char, char> Any = (input) =>
            {
                return input.IsEnd ? Result.Fail<char, char>(input)
                    : Result.Match(input.Current, input.Next());
            };

            public static Parser<char> Const(char c)
            {
                return (input) =>
                {
                    return input.IsEnd || input.Current != c ?
                        Result.Fail(input) :
                        Result.Match(input.Next());
                };
            }

            public static Parser<char> String(string s)
            {
                return (input) =>
                {
                    foreach (char c in s)
                    {
                        if (input.IsEnd || input.Current != c)
                            return Result.Fail(input);
                        else input = input.Next();
                    }
                    return Result.Match(input);
                };
            }

            public static Parser<char, string> String(Parser<char> p)
            {
                return (input) =>
                {
                    return p(input).Visit(
                        (success) => Result.Match(
                            input.Remaining(success.Remaining).AsString(),
                            success.Remaining),
                        (failure) => Result.Fail<char, string>(failure.Remaining));
                };
            }

            public static Parser<char, string> String<V>(Parser<char, V> p)
            {
                return (input) =>
                {
                    return p(input).Map(
                        (success) => Result.Match(
                            input.Remaining(success.Remaining).AsString(),
                            success.Remaining),
                        (failure) => Result.Fail<char, string>(failure.Remaining));
                };
            }

            public static Parser<char, char> Space = Any.If(System.Char.IsWhiteSpace);
            public static Parser<char, char> Control = Any.If(System.Char.IsControl);
            public static Parser<char, char> Digit = Any.If(System.Char.IsDigit);
            public static Parser<char, char> Letter = Any.If(System.Char.IsLetter);
            public static Parser<char, char> LetterOrDigit = Any.If(System.Char.IsLetterOrDigit);

            public static Parser<char, char> AnyOf(string chars)
            {
                return Any.If(c => chars.Contains(c));
            }
        }
    }
}
