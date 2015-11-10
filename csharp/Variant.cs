
using System;
using System.Text;

namespace Functional
{
    public static class Variant
    {
        public static string Generate(int size)
        {
            return String.Format(String.Join(Environment.NewLine, new string[]{
                "using System;",
                "",
                "namespace Functional",
                "{",

            }));
        }
    }
    public class Variant<T0, T1>
    {
        Object value;

        public T0 Item0 { get { return (T0)value; } }
        public T1 Item1 { get { return (T1)value; } }

        public bool IsItem0 { get { return value is T0; } }
        public bool IsItem1 { get { return value is T1; } }

        public static implicit operator Variant<T0, T1>(T0 t)
        {
            return new Variant<T0, T1>(t);
        }

        public static implicit operator Variant<T0, T1>(T1 t)
        {
            return new Variant<T0, T1>(t);
        }

        public Variant(T0 t)
        {
            this.value = t;
        }

        public Variant(T1 t)
        {
            this.value = t;
        }

        public T Visit<T>(Func<T0, T> f0, Func<T1, T> f1)
        {
            if (value is T0) return f0(this.Item0);
            else return f1(this.Item1);
        }

        public void Visit(Action<T0> f0, Action<T1> f1)
        {
            if (value is T0) f0(this.Item0);
            else f1(this.Item1);
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

    public class Variant<T0, T1, T2>
    {
        Object value;

        public T0 Item0 { get { return (T0)value; } }
        public T1 Item1 { get { return (T1)value; } }
        public T2 Item2 { get { return (T2)value; } }

        public bool IsItem0 { get { return value is T0; } }
        public bool IsItem1 { get { return value is T1; } }
        public bool IsItem2 { get { return value is T2; } }

        public static implicit operator Variant<T0, T1, T2>(T0 t)
        {
            return new Variant<T0, T1, T2>(t);
        }

        public static implicit operator Variant<T0, T1, T2>(T1 t)
        {
            return new Variant<T0, T1, T2>(t);
        }

        public static implicit operator Variant<T0, T1, T2>(T2 t)
        {
            return new Variant<T0, T1, T2>(t);
        }

        public Variant(T0 t)
        {
            this.value = t;
        }

        public Variant(T1 t)
        {
            this.value = t;
        }

        public Variant(T2 t)
        {
            this.value = t;
        }

        public T Visit<T>(
            Func<T0, T> f0, 
            Func<T1, T> f1,
            Func<T2, T> f2)
        {
            if (value is T0) return f0(this.Item0);
            else if (value is T1) return f1(this.Item1);
            else return f2(this.Item2);
        }

        public void Visit(
            Action<T0> f0,
            Action<T1> f1,
            Action<T2> f2)
        {
            if (value is T0) f0(this.Item0);
            else if (value is T1) f1(this.Item1);
            else f2(this.Item2);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T0, T1, T2>;

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