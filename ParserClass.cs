using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Functional;

// This is an experimental "branch" of the delegate-based parsers used 
// normally.  The primary driver behind this is to achieve a (more) generic 
// input type, as opposed to using IParseInput<T> everywhere explicitly.  One
// possible benefit is type safety in cases where a specialized IParseInput<T>
// implementation is used.  Instead of having to cast the 
// Success<T, V>.Remaining property to the appropiate input implementation, 
// the Remaining property's type will just depend on the input.  Since it 
// isn't possible to have an instance of a delegate without specifying all 
// type parameters, we use a Parser<T, V> interface, instead.  Parser 
// implementations can then provide a generic Parse() method, which is 
// parameterized over the input type.
namespace ParserClass
{
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
            return result.Visit(success, failure);
        }

        public void Visit(Action<Success<T, V>> success, Action<Failure<T>> failure)
        {
            result.Visit(success, failure);
        }
    }

    public class Success<T, V>
    {
        public V Value { get; private set; }
        public T Remaining { get; private set; }

        public Success(T remainder, V value)
        {
            Value = value;
            Remaining = remainder;
        }
    }

    public class Failure<T>
    {
        public T Remaining { get; private set; }

        public Failure(T remainder)
        {
            Remaining = remainder;
        }
    }

    public static class Result
    {
        public static Result<T, V> Match<T, V>(T remainder, V value)
        {
            //remainder.OnMatch();
            return new Result<T, V>(new Success<T, V>(remainder, value));
        }

        public static Result<T, V> Fail<T, V>(T remainder)
        {
            //remainder.OnFail();
            return new Result<T, V>(new Failure<T>(remainder));
        }
    }

    public interface Parser<T, V>
    {
        Result<I, V> Parse<I>(I input) where I : IInput<I, T>;
    }

    public class SelectParser<T, V1, V2> : Parser<T, V2>
    {
        Parser<T, V1> p;
        Func<V1, V2> f;

        public SelectParser(Parser<T, V1> parser, Func<V1, V2> function)
        {
            p = parser;
            f = function;
        }

        public Result<I, V2> Parse<I>(I input) where I : IInput<I, T>
        {
            return p.Parse(input).Visit(
                success => Result.Match(success.Remaining, f(success.Value)),
                failure => Result.Fail<I, V2>(failure.Remaining));
        }
    }

    public class WhereParser<T, V> : Parser<T, V>
    {
        Parser<T, V> p;
        Func<V, bool> f;

        public WhereParser(Parser<T, V> parser, Func<V, bool> function)
        {
            p = parser;
            f = function;
        }

        public Result<I, V> Parse<I>(I input) where I : IInput<I, T>
        {
            return p.Parse<I>(input).Visit(
                success => f(success.Value) ?
                    Result.Match(success.Remaining, success.Value) :
                    Result.Fail<I, V>(input),
                failure => Result.Fail<I, V>(failure.Remaining));
        }
    }

    public class SelectManyParser<T, V1, V2> : Parser<T, V2>
    {
        Parser<T, V1> p;
        Func<V1, Parser<T, V2>> f;

        public SelectManyParser(Parser<T, V1> parser, Func<V1, Parser<T, V2>> function)
        {
            p = parser;
            f = function;
        }

        public Result<I, V2> Parse<I>(I input) where I : IInput<I, T>
        {
            return p.Parse<I>(input).Visit(
                success => f(success.Value).Parse<I>(success.Remaining),
                failure => Result.Fail<I, V2>(failure.Remaining));
        }
    }

    public static class LinqExtensions
    {
        public static Parser<T, V2> Select<T, V1, V2>(
                this Parser<T, V1> p,
                Func<V1, V2> f)
        {
            return new SelectParser<T, V1, V2>(p, f);
        }

        public static Parser<T, V> Where<T, V>(
            this Parser<T, V> parser,
            Func<V, bool> predicate)
        {
            return new WhereParser<T, V>(parser, predicate);
        }

        public static Parser<T, V2> SelectMany<T, V1, V2>(
            this Parser<T, V1> parser,
            Func<V1, Parser<T, V2>> selector)
        {
            return new SelectManyParser<T, V1, V2>(parser, selector);
        }

        public static Parser<T, V3> SelectMany<T, V1, V2, V3>(
            this Parser<T, V1> parser,
             Func<V1, Parser<T, V2>> selector,
             Func<V1, V2, V3> projector)
        {
            return parser.SelectMany(
                v1 => selector(v1).Select(
                    v2 => projector(v1, v2)));
        }
    }

    public interface IInput<Impl, T>
    {
        T Current { get; }
        bool IsEnd { get; }
        Impl Next();
    }

    public class AnyParser<T> : Parser<T, T>
    {
        public Result<I, T> Parse<I>(I input) where I : IInput<I, T>
        {
            return input.IsEnd ? Result.Fail<I, T>(input)
                : Result.Match(input.Next(), input.Current);
        }
    }

    public class ConstParser<T> : Parser<T, T>
    {
        private T t;

        public ConstParser(T token) { t = token; }

        public Result<I, T> Parse<I>(I input) where I : IInput<I, T>
        {
            return input.IsEnd || !input.Current.Equals(t) ? Result.Fail<I, T>(input)
                : Result.Match(input.Next(), input.Current);
        }
    }

    public static class Combinator
    {
        public static readonly Parser<char, char> AnyChar = new AnyParser<char>();

        public static Parser<T, T> Const<T>(T token)
        {
            return new ConstParser<T>(token);
        }

        public class FirstOfParser<T, V> : Parser<T, V>
        {
            IEnumerable<Parser<T, V>> ps;
            
            public FirstOfParser(IEnumerable<Parser<T, V>> parsers)
            {
                ps = parsers;
            }

            public Result<I, V> Parse<I>(I input) where I : IInput<I, T>
            {
                return ps.Select(p => p.Parse(input))
                    .Where(r => r.IsSuccess)
                    .DefaultIfEmpty(Result.Fail<I, V>(input))
                    .First();
            }
        }

        public static Parser<T, V> FirstOf<T, V>(params Parser<T, V>[] parsers)
        {
            return new FirstOfParser<T, V>(parsers);
        }
    }
}
