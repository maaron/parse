using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public interface IParseInput<T>
    {
        T Current { get; }
        bool IsEnd { get; }
        IParseInput<T> Next();
    }

    public class ParseInput<T> : IParseInput<T>
    {
        T[] data;
        int index;

        public T Current
        {
            get { return data[index]; }
        }

        public bool IsEnd
        {
            get { return index >= data.Length; }
        }

        public IParseInput<T> Next()
        {
            return new ParseInput<T>(data, index + 1);
        }

        public ParseInput(T[] source)
        {
            this.data = source;
            this.index = 0;
        }

        public ParseInput(T[] source, int offset)
        {
            this.data = source;
            this.index = offset;
        }

        public ParseInput(IEnumerable<T> source)
        {
            this.data = source.ToArray();
            this.index = 0;
        }
    }
}
