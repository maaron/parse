
using System;
using System.Collections;

namespace Functional
{
    public class Variant<T0, T1>
    {
        Object value;

        public T0 Item0 { get { return (T0)value; } }
        public T1 Item1 { get { return (T1)value; } }

        public bool IsItem0 { get { return value is T0; } }
        public bool IsItem1 { get { return value is T1; } }

        public static implicit operator Variant<T0, T1>(T0 left)
        {
            return new Variant<T0, T1>(left);
        }

        public static implicit operator Variant<T0, T1>(T1 right)
        {
            return new Variant<T0, T1>(right);
        }

        public Variant(T0 left)
        {
            this.value = left;
        }

        public Variant(T1 right)
        {
            this.value = right;
        }

        public T Visit<T>(Func<T0, T> left, Func<T1, T> right)
        {
            if (value is T0) return left(this.Item0);
            else return right(this.Item1);
        }

        public void Visit(Action<T0> left, Action<T1> right)
        {
            if (value is T0) left(this.Item0);
            else right(this.Item1);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T0, T1>;

            return
                e != null &&
                value.Equals(e.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}