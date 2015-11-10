using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public class Anchor<T, V>
    {
        public IParseInput<T> Location { get; private set; }
        public V Value { get; private set; }

        public Anchor(IParseInput<T> input, V value)
        {
            Location = input;
            Value = value;
        }
    }
}
