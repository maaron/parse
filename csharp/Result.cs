using System;
using Functional;

namespace Parse
{
    public class Success<T>
    {
        public IParseInput<T> Remaining { get; private set; }

        public Success(IParseInput<T> remainder)
        {
            Remaining = remainder;
        }
    }

    public class Success<T, V>
    {
        public V Value { get; private set; }
        public IParseInput<T> Remaining { get; private set; }

        public Success(V value, IParseInput<T> remainder)
        {
            Value = value;
            Remaining = remainder;
        }
    }

    public class Failure<T>
    {
        public IParseInput<T> Remaining { get; private set; }

        public Failure(IParseInput<T> remainder)
        {
            Remaining = remainder;
        }
    }

    public static class Result
    {
        public static Either<Success<T, V>, Failure<T>> Match<T, V>(V value, IParseInput<T> remainder)
        {
            remainder.OnMatch();
            return new Either<Success<T, V>, Failure<T>>(new Success<T, V>(value, remainder));
        }

        public static Either<Success<T>, Failure<T>> Match<T>(IParseInput<T> remainder)
        {
            remainder.OnMatch();
            return new Either<Success<T>, Failure<T>>(new Success<T>(remainder));
        }

        public static Either<Success<T, V>, Failure<T>> Fail<T, V>(IParseInput<T> remainder)
        {
            remainder.OnFail();
            return new Either<Success<T, V>, Failure<T>>(new Failure<T>(remainder));
        }

        public static Either<Success<T>, Failure<T>> Fail<T>(IParseInput<T> remainder)
        {
            remainder.OnFail();
            return new Either<Success<T>, Failure<T>>(new Failure<T>(remainder));
        }

        public static Either<Success<T, O>, Failure<T>> MapValue<T, I, O>(
            this Either<Success<T, I>, Failure<T>> r, Func<I, O> f)
        {
            return r.Visit(
                (success) => Match(f(success.Value), success.Remaining),
                (failure) => Fail<T, O>(failure.Remaining));
        }

        public static Either<Success<T, I>, Failure<T>> MapValue<T, I>(
            this Either<Success<T, I>, Failure<T>> r)
        {
            return r.Visit(
                (success) => Match(success.Value, success.Remaining),
                (failure) => Fail<T, I>(failure.Remaining));
        }

        public static Either<Success<T, O>, Failure<T>> MapValue<T, O>(
            this Either<Success<T>, Failure<T>> r, Func<O> f)
        {
            return r.Visit(
                (success) => Match(f(), success.Remaining),
                (failure) => Fail<T, O>(failure.Remaining));
        }
    }
}
