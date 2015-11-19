
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

        public T Visit<T>(Func<T1, T> f0)
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

#if false
    public class Variant<T1, T2> : Variant
    {
        public T1 Item0 { get { return (T1)value; } }
        public T2 Item1 { get { return (T2)value; } }

        public bool IsItem0 { get { return value is T1; } }
        public bool IsItem1 { get { return value is T2; } }

        public static implicit operator Variant<T1, T2>(T1 t)
        {
            return new Variant<T1, T2>(t);
        }

        public static implicit operator Variant<T1, T2>(T2 t)
        {
            return new Variant<T1, T2>(t);
        }

        public Variant(T1 t) : base(t) { }

        public Variant(T2 t) : base(t) { }

        public Variant(Variant v) : base(v) { }

        public T Visit<T>(Func<T1, T> f0, Func<T2, T> f1)
        {
            if (value is T1) return f0(this.Item0);
            else return f1(this.Item1);
        }

        public void Visit(Action<T1> f0, Action<T2> f1)
        {
            if (value is T1) f0(this.Item0);
            else f1(this.Item1);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T1, T2>;

            return
                e != null &&
                value.Equals(e.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }

    public class Variant<T1, T2, T3> : Variant
    {
        public T1 Item1 { get { return (T1)value; } }
        public T2 Item2 { get { return (T2)value; } }
        public T3 Item3 { get { return (T3)value; } }

        public bool IsItem1 { get { return value is T1; } }
        public bool IsItem2 { get { return value is T2; } }
        public bool IsItem3 { get { return value is T3; } }

        public static implicit operator Variant<T1, T2, T3>(T1 t)
        {
            return new Variant<T1, T2, T3>(t);
        }

        public static implicit operator Variant<T1, T2, T3>(T2 t)
        {
            return new Variant<T1, T2, T3>(t);
        }

        public static implicit operator Variant<T1, T2, T3>(T3 t)
        {
            return new Variant<T1, T2, T3>(t);
        }

        public Variant(T1 t) : base(t) { }

        public Variant(T2 t) : base(t) { }

        public Variant(T3 t) : base(t) { }

        public Variant(Variant v) : base(v) { }

        public T Visit<T>(
            Func<T1, T> f1, 
            Func<T2, T> f2,
            Func<T3, T> f3)
        {
            if (value is T1) return f1(Item1);
            else if (value is T2) return f2(Item2);
            else return f3(Item3);
        }

        public void Visit(
            Action<T1> f1,
            Action<T2> f2,
            Action<T3> f3)
        {
            if (value is T1) f1(Item1);
            else if (value is T2) f2(Item2);
            else f3(Item3);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T1, T2, T3>;

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
#endif