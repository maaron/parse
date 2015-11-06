﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parse.Extensions;

namespace Parse
{
    namespace Character
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

            public static Parser<char, char> Space = Any.If(System.Char.IsWhiteSpace);
            public static Parser<char, char> Control = Any.If(System.Char.IsControl);
            public static Parser<char, char> Digit = Any.If(System.Char.IsDigit);
            public static Parser<char, char> Letter = Any.If(System.Char.IsLetter);
        }
    }
}
