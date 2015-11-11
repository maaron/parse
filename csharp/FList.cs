using System;
using System.Linq;
using System.Collections.Generic;

namespace Parse
{
    public static class FList
    {
        public static FList<T> Create<T>(params T[] items)
        {
            var flist = new FList<T>();
            foreach (var i in items) flist.Add(i);
            return flist;
        }
    }
    public class FList<T> : List<T>
    {
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
