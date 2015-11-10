using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public class TransformParser<T, V> : IParseInput<V>
    {
        private IParseInput<T> adapted;
        private Parser<T, V> parser;
        private V current;

        public TransformParser(IParseInput<T> input, Parser<T, V> parser)
        {
            parser(input).Visit(
                (success) =>
                {
                    this.adapted = success.Remaining;
                    current = success.Value;
                },
                (failure) =>
                {
                    throw new Exception("Parse error");
                });

            this.parser = parser;
        }

        public V Current
        {
            get { return current; }
        }

        public bool IsEnd
        {
            get { return adapted.IsEnd; }
        }

        public int CompareTo(IParseInput<V> other)
        {
            var rhs = other as TransformParser<T, V>;
            if (rhs == null) return 1;
            return adapted.CompareTo(rhs.adapted);
        }

        public IParseInput<V> Next()
        {
            return new TransformParser<T, V>(adapted.Next(), parser);
        }

        public void OnFail()
        {
            throw new NotImplementedException();
        }

        public void OnMatch()
        {
            throw new NotImplementedException();
        }
    }
}
