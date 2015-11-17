using System;
using System.Collections.Generic;
using Parse.Combinators;

namespace Parse
{
    public class FilterParser<T> : AdaptedParseInput<T>
    {
        public Parser<T> Parser { get; private set; }

        public FilterParser(IParseInput<T> input, Parser<T> parser) 
            : base(input)
        {
            this.Parser = parser;
            adapted = Parser.ZeroOrMore()(adapted).Visit(
                (success) => success.Remaining,
                (failure) => failure.Remaining);
        }

        public override IParseInput<T> Next()
        {
            return new FilterParser<T>(adapted.Next(), Parser);
        }
    }
}
