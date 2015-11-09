using System;
using Functional;

namespace Parse
{
    public delegate Result<T> Parser<T>(IParseInput<T> input);
    public delegate Result<T, V> Parser<T, V>(IParseInput<T> input);
}
