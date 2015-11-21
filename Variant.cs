
using System;
using System.Text;

namespace Functional
{
    public class Variant
    {
        protected Object value;

        protected Variant(Object v)
        {
            value = v;
        }

        protected Variant(Variant other)
        {
            value = other.value;
        }
    }

    public class Variant<T1> : Variant
    {
        public T1 Item0 { get { return (T1)value; } }

        public bool IsItem0 { get { return value is T1; } }

        public static implicit operator Variant<T1>(T1 t)
        {
            return new Variant<T1>(t);
        }

        public Variant(T1 t) : base(t) { }

        public Variant(Variant v) : base(v) { }

        public T Map<T>(Func<T1, T> f0)
        {
            return f0(this.Item0);
        }

        public void Visit(Action<T1> f0)
        {
            f0(this.Item0);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T1>;

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