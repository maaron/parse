using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public class Anchor<T, V>
    {
        public IParseInput<T> Start { get; private set; }
        public IParseInput<T> End { get; private set; }

        public V Value { get; private set; }

        public Anchor(IParseInput<T> start, IParseInput<T> end, V value)
        {
            Start = start;
            End = end;
            Value = value;
        }
    }
}
