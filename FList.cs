using System;
using System.Linq;
using System.Collections.Generic;

namespace Parse
{
    public static class FList
    {
        public static FList<T> Create<T>(params T[] items)
        {
            return new FList<T>(items);
        }

        public static FList<T> Create<T>(IEnumerable<T> items)
        {
            return new FList<T>(items);
        }
    }

    public class FList<T> : List<T>
    {
        public FList() : base()
        {
        }

        public FList(IEnumerable<T> items) : base(items)
        {
        }

        public override bool Equals(object obj)
        {
            var other = obj as IEnumerable<T>;
            return other != null
                && this.SequenceEqual(other);
        }

        public override int GetHashCode()
        {
            return this.Aggregate(0, (a, b) => a ^ b.GetHashCode());
        }
    }
}
