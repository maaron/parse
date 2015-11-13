

using System;
using System.Text;

namespace FunctionalTest
{
    public class Variant<T1,T2>
    {
        Object value;

        public T1 Item1 { get { return (T1)value; } }
        public T2 Item2 { get { return (T2)value; } }

        public bool IsItem1 { get { return value is T1; } }
        public bool IsItem2 { get { return value is T2; } }

        public static implicit operator Variant<T1,T2>(T1 t)
        {
            return new Variant<T1,T2>(t);
        }
        public static implicit operator Variant<T1,T2>(T2 t)
        {
            return new Variant<T1,T2>(t);
        }

        public Variant(T1 t)
        {
            this.value = t;
        }
        public Variant(T2 t)
        {
            this.value = t;
        }

        public T Visit<T>(Func<T1, T> f1,Func<T2, T> f2)
        {
            if (value is T1) return f1(this.Item1);
            else return f2(this.Item2);
        }

        public void Visit(Action<T1> f1,Action<T2> f2)
        {
            if (value is T1) f1(this.Item1);
            else f2(this.Item2);
        }

        public override bool Equals(object other)
        {
            return Util.IfSame(this, other, (a, b) =>
                a.value.Equals(b.value));
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}