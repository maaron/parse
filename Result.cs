using System;
using Functional;

namespace Parse
{
    public class Result<T>
    {
        private Variant<Success<T>, Failure<T>> result;

        public Result(Success<T> success)
        {
            this.result = success;
        }

        public Result(Failure<T> failure)
        {
            this.result = failure;
        }

        public bool IsSuccess { get { return result.IsItem1; } }
        public Success<T> Success { get { return result.Item1; } }

        public bool IsFailure { get { return result.IsItem2; } }
        public Failure<T> Failure { get { return result.Item2; } }

        public R Visit<R>(Func<Success<T>, R> success, Func<Failure<T>, R> failure)
        {
            return result.Map(success, failure);
        }

        public void Visit(Action<Success<T>> success, Action<Failure<T>> failure)
        {
            result.Visit(success, failure);
        }

        public IParseInput<T> Remaining 
        {
            get 
            { 
                return IsSuccess ? Success.Remaining 
                    : Failure.Remaining; 
            }
        }
    }

    public class Result<T, V>
    {
        private Variant<Success<T, V>, Failure<T>> result;

        public Result(Success<T, V> success)
        {
            this.result = success;
        }

        public Result(Failure<T> failure)
        {
            this.result = failure;
        }

        public bool IsSuccess { get { return result.IsItem1; } }
        public Success<T, V> Success { get { return result.Item1; } }

        public bool IsFailure { get { return result.IsItem2; } }
        public Failure<T> Failure { get { return result.Item2; } }

        public R Visit<R>(Func<Success<T, V>, R> success, Func<Failure<T>, R> failure)
        {
            return result.Map(success, failure);
        }

        public void Visit(Action<Success<T, V>> success, Action<Failure<T>> failure)
        {
            result.Visit(success, failure);
        }
    }

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
        public static Result<T, V> Match<T, V>(V value, IParseInput<T> remainder)
        {
            remainder.OnMatch();
            return new Result<T, V>(new Success<T, V>(value, remainder));
        }

        public static Result<T> Match<T>(IParseInput<T> remainder)
        {
            remainder.OnMatch();
            return new Result<T>(new Success<T>(remainder));
        }

        public static Result<T, V> Fail<T, V>(IParseInput<T> remainder)
        {
            remainder.OnFail();
            return new Result<T, V>(new Failure<T>(remainder));
        }

        public static Result<T> Fail<T>(IParseInput<T> remainder)
        {
            remainder.OnFail();
            return new Result<T>(new Failure<T>(remainder));
        }

        public static Result<T, O> MapValue<T, I, O>(
            this Result<T, I> r, Func<I, O> f)
        {
            return r.Visit(
                (success) => Match(f(success.Value), success.Remaining),
                (failure) => Fail<T, O>(failure.Remaining));
        }

        public static Result<T, I> MapValue<T, I>(
            this Result<T, I> r)
        {
            return r.Visit(
                (success) => Match(success.Value, success.Remaining),
                (failure) => Fail<T, I>(failure.Remaining));
        }

        public static Result<T, O> MapValue<T, O>(
            this Result<T> r, Func<O> f)
        {
            return r.Visit(
                (success) => Match(f(), success.Remaining),
                (failure) => Fail<T, O>(failure.Remaining));
        }
    }
}
