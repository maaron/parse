using System;
using Functional;

namespace Parse
{
    public delegate Either<Success<T>, Failure<T>> Parser<T>(IParseInput<T> input);
    public delegate Either<Success<T, V>, Failure<T>> Parser<T, V>(IParseInput<T> input);
}
