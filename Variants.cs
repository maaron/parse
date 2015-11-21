

namespace Functional
{
    public class Variant<T1,T2> : Variant
    {
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

		public Variant(Variant v) : base(v) {}

        public Variant(T1 t) : base(t) {}
        public Variant(T2 t) : base(t) {}

        public T Map<T>(System.Func<T1, T> f1,System.Func<T2, T> f2)
        {
            if (value is T1) return f1(this.Item1);
            else return f2(this.Item2);
        }

        public void Visit(System.Action<T1> f1,System.Action<T2> f2)
        {
            if (value is T1) f1(this.Item1);
            else f2(this.Item2);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T1,T2>;

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


namespace Functional
{
    public class Variant<T1,T2,T3> : Variant
    {
        public T1 Item1 { get { return (T1)value; } }
        public T2 Item2 { get { return (T2)value; } }
        public T3 Item3 { get { return (T3)value; } }

        public bool IsItem1 { get { return value is T1; } }
        public bool IsItem2 { get { return value is T2; } }
        public bool IsItem3 { get { return value is T3; } }

        public static implicit operator Variant<T1,T2,T3>(T1 t)
        {
            return new Variant<T1,T2,T3>(t);
        }
        public static implicit operator Variant<T1,T2,T3>(T2 t)
        {
            return new Variant<T1,T2,T3>(t);
        }
        public static implicit operator Variant<T1,T2,T3>(T3 t)
        {
            return new Variant<T1,T2,T3>(t);
        }

		public Variant(Variant v) : base(v) {}

        public Variant(T1 t) : base(t) {}
        public Variant(T2 t) : base(t) {}
        public Variant(T3 t) : base(t) {}

        public T Map<T>(System.Func<T1, T> f1,System.Func<T2, T> f2,System.Func<T3, T> f3)
        {
            if (value is T1) return f1(this.Item1);
            if (value is T2) return f2(this.Item2);
            else return f3(this.Item3);
        }

        public void Visit(System.Action<T1> f1,System.Action<T2> f2,System.Action<T3> f3)
        {
            if (value is T1) f1(this.Item1);
            if (value is T2) f2(this.Item2);
            else f3(this.Item3);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T1,T2,T3>;

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


namespace Functional
{
    public class Variant<T1,T2,T3,T4> : Variant
    {
        public T1 Item1 { get { return (T1)value; } }
        public T2 Item2 { get { return (T2)value; } }
        public T3 Item3 { get { return (T3)value; } }
        public T4 Item4 { get { return (T4)value; } }

        public bool IsItem1 { get { return value is T1; } }
        public bool IsItem2 { get { return value is T2; } }
        public bool IsItem3 { get { return value is T3; } }
        public bool IsItem4 { get { return value is T4; } }

        public static implicit operator Variant<T1,T2,T3,T4>(T1 t)
        {
            return new Variant<T1,T2,T3,T4>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4>(T2 t)
        {
            return new Variant<T1,T2,T3,T4>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4>(T3 t)
        {
            return new Variant<T1,T2,T3,T4>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4>(T4 t)
        {
            return new Variant<T1,T2,T3,T4>(t);
        }

		public Variant(Variant v) : base(v) {}

        public Variant(T1 t) : base(t) {}
        public Variant(T2 t) : base(t) {}
        public Variant(T3 t) : base(t) {}
        public Variant(T4 t) : base(t) {}

        public T Map<T>(System.Func<T1, T> f1,System.Func<T2, T> f2,System.Func<T3, T> f3,System.Func<T4, T> f4)
        {
            if (value is T1) return f1(this.Item1);
            if (value is T2) return f2(this.Item2);
            if (value is T3) return f3(this.Item3);
            else return f4(this.Item4);
        }

        public void Visit(System.Action<T1> f1,System.Action<T2> f2,System.Action<T3> f3,System.Action<T4> f4)
        {
            if (value is T1) f1(this.Item1);
            if (value is T2) f2(this.Item2);
            if (value is T3) f3(this.Item3);
            else f4(this.Item4);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T1,T2,T3,T4>;

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


namespace Functional
{
    public class Variant<T1,T2,T3,T4,T5> : Variant
    {
        public T1 Item1 { get { return (T1)value; } }
        public T2 Item2 { get { return (T2)value; } }
        public T3 Item3 { get { return (T3)value; } }
        public T4 Item4 { get { return (T4)value; } }
        public T5 Item5 { get { return (T5)value; } }

        public bool IsItem1 { get { return value is T1; } }
        public bool IsItem2 { get { return value is T2; } }
        public bool IsItem3 { get { return value is T3; } }
        public bool IsItem4 { get { return value is T4; } }
        public bool IsItem5 { get { return value is T5; } }

        public static implicit operator Variant<T1,T2,T3,T4,T5>(T1 t)
        {
            return new Variant<T1,T2,T3,T4,T5>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5>(T2 t)
        {
            return new Variant<T1,T2,T3,T4,T5>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5>(T3 t)
        {
            return new Variant<T1,T2,T3,T4,T5>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5>(T4 t)
        {
            return new Variant<T1,T2,T3,T4,T5>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5>(T5 t)
        {
            return new Variant<T1,T2,T3,T4,T5>(t);
        }

		public Variant(Variant v) : base(v) {}

        public Variant(T1 t) : base(t) {}
        public Variant(T2 t) : base(t) {}
        public Variant(T3 t) : base(t) {}
        public Variant(T4 t) : base(t) {}
        public Variant(T5 t) : base(t) {}

        public T Map<T>(System.Func<T1, T> f1,System.Func<T2, T> f2,System.Func<T3, T> f3,System.Func<T4, T> f4,System.Func<T5, T> f5)
        {
            if (value is T1) return f1(this.Item1);
            if (value is T2) return f2(this.Item2);
            if (value is T3) return f3(this.Item3);
            if (value is T4) return f4(this.Item4);
            else return f5(this.Item5);
        }

        public void Visit(System.Action<T1> f1,System.Action<T2> f2,System.Action<T3> f3,System.Action<T4> f4,System.Action<T5> f5)
        {
            if (value is T1) f1(this.Item1);
            if (value is T2) f2(this.Item2);
            if (value is T3) f3(this.Item3);
            if (value is T4) f4(this.Item4);
            else f5(this.Item5);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T1,T2,T3,T4,T5>;

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


namespace Functional
{
    public class Variant<T1,T2,T3,T4,T5,T6> : Variant
    {
        public T1 Item1 { get { return (T1)value; } }
        public T2 Item2 { get { return (T2)value; } }
        public T3 Item3 { get { return (T3)value; } }
        public T4 Item4 { get { return (T4)value; } }
        public T5 Item5 { get { return (T5)value; } }
        public T6 Item6 { get { return (T6)value; } }

        public bool IsItem1 { get { return value is T1; } }
        public bool IsItem2 { get { return value is T2; } }
        public bool IsItem3 { get { return value is T3; } }
        public bool IsItem4 { get { return value is T4; } }
        public bool IsItem5 { get { return value is T5; } }
        public bool IsItem6 { get { return value is T6; } }

        public static implicit operator Variant<T1,T2,T3,T4,T5,T6>(T1 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6>(T2 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6>(T3 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6>(T4 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6>(T5 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6>(T6 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6>(t);
        }

		public Variant(Variant v) : base(v) {}

        public Variant(T1 t) : base(t) {}
        public Variant(T2 t) : base(t) {}
        public Variant(T3 t) : base(t) {}
        public Variant(T4 t) : base(t) {}
        public Variant(T5 t) : base(t) {}
        public Variant(T6 t) : base(t) {}

        public T Map<T>(System.Func<T1, T> f1,System.Func<T2, T> f2,System.Func<T3, T> f3,System.Func<T4, T> f4,System.Func<T5, T> f5,System.Func<T6, T> f6)
        {
            if (value is T1) return f1(this.Item1);
            if (value is T2) return f2(this.Item2);
            if (value is T3) return f3(this.Item3);
            if (value is T4) return f4(this.Item4);
            if (value is T5) return f5(this.Item5);
            else return f6(this.Item6);
        }

        public void Visit(System.Action<T1> f1,System.Action<T2> f2,System.Action<T3> f3,System.Action<T4> f4,System.Action<T5> f5,System.Action<T6> f6)
        {
            if (value is T1) f1(this.Item1);
            if (value is T2) f2(this.Item2);
            if (value is T3) f3(this.Item3);
            if (value is T4) f4(this.Item4);
            if (value is T5) f5(this.Item5);
            else f6(this.Item6);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T1,T2,T3,T4,T5,T6>;

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


namespace Functional
{
    public class Variant<T1,T2,T3,T4,T5,T6,T7> : Variant
    {
        public T1 Item1 { get { return (T1)value; } }
        public T2 Item2 { get { return (T2)value; } }
        public T3 Item3 { get { return (T3)value; } }
        public T4 Item4 { get { return (T4)value; } }
        public T5 Item5 { get { return (T5)value; } }
        public T6 Item6 { get { return (T6)value; } }
        public T7 Item7 { get { return (T7)value; } }

        public bool IsItem1 { get { return value is T1; } }
        public bool IsItem2 { get { return value is T2; } }
        public bool IsItem3 { get { return value is T3; } }
        public bool IsItem4 { get { return value is T4; } }
        public bool IsItem5 { get { return value is T5; } }
        public bool IsItem6 { get { return value is T6; } }
        public bool IsItem7 { get { return value is T7; } }

        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7>(T1 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7>(T2 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7>(T3 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7>(T4 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7>(T5 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7>(T6 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7>(T7 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7>(t);
        }

		public Variant(Variant v) : base(v) {}

        public Variant(T1 t) : base(t) {}
        public Variant(T2 t) : base(t) {}
        public Variant(T3 t) : base(t) {}
        public Variant(T4 t) : base(t) {}
        public Variant(T5 t) : base(t) {}
        public Variant(T6 t) : base(t) {}
        public Variant(T7 t) : base(t) {}

        public T Map<T>(System.Func<T1, T> f1,System.Func<T2, T> f2,System.Func<T3, T> f3,System.Func<T4, T> f4,System.Func<T5, T> f5,System.Func<T6, T> f6,System.Func<T7, T> f7)
        {
            if (value is T1) return f1(this.Item1);
            if (value is T2) return f2(this.Item2);
            if (value is T3) return f3(this.Item3);
            if (value is T4) return f4(this.Item4);
            if (value is T5) return f5(this.Item5);
            if (value is T6) return f6(this.Item6);
            else return f7(this.Item7);
        }

        public void Visit(System.Action<T1> f1,System.Action<T2> f2,System.Action<T3> f3,System.Action<T4> f4,System.Action<T5> f5,System.Action<T6> f6,System.Action<T7> f7)
        {
            if (value is T1) f1(this.Item1);
            if (value is T2) f2(this.Item2);
            if (value is T3) f3(this.Item3);
            if (value is T4) f4(this.Item4);
            if (value is T5) f5(this.Item5);
            if (value is T6) f6(this.Item6);
            else f7(this.Item7);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T1,T2,T3,T4,T5,T6,T7>;

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


namespace Functional
{
    public class Variant<T1,T2,T3,T4,T5,T6,T7,T8> : Variant
    {
        public T1 Item1 { get { return (T1)value; } }
        public T2 Item2 { get { return (T2)value; } }
        public T3 Item3 { get { return (T3)value; } }
        public T4 Item4 { get { return (T4)value; } }
        public T5 Item5 { get { return (T5)value; } }
        public T6 Item6 { get { return (T6)value; } }
        public T7 Item7 { get { return (T7)value; } }
        public T8 Item8 { get { return (T8)value; } }

        public bool IsItem1 { get { return value is T1; } }
        public bool IsItem2 { get { return value is T2; } }
        public bool IsItem3 { get { return value is T3; } }
        public bool IsItem4 { get { return value is T4; } }
        public bool IsItem5 { get { return value is T5; } }
        public bool IsItem6 { get { return value is T6; } }
        public bool IsItem7 { get { return value is T7; } }
        public bool IsItem8 { get { return value is T8; } }

        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7,T8>(T1 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7,T8>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7,T8>(T2 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7,T8>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7,T8>(T3 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7,T8>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7,T8>(T4 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7,T8>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7,T8>(T5 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7,T8>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7,T8>(T6 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7,T8>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7,T8>(T7 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7,T8>(t);
        }
        public static implicit operator Variant<T1,T2,T3,T4,T5,T6,T7,T8>(T8 t)
        {
            return new Variant<T1,T2,T3,T4,T5,T6,T7,T8>(t);
        }

		public Variant(Variant v) : base(v) {}

        public Variant(T1 t) : base(t) {}
        public Variant(T2 t) : base(t) {}
        public Variant(T3 t) : base(t) {}
        public Variant(T4 t) : base(t) {}
        public Variant(T5 t) : base(t) {}
        public Variant(T6 t) : base(t) {}
        public Variant(T7 t) : base(t) {}
        public Variant(T8 t) : base(t) {}

        public T Map<T>(System.Func<T1, T> f1,System.Func<T2, T> f2,System.Func<T3, T> f3,System.Func<T4, T> f4,System.Func<T5, T> f5,System.Func<T6, T> f6,System.Func<T7, T> f7,System.Func<T8, T> f8)
        {
            if (value is T1) return f1(this.Item1);
            if (value is T2) return f2(this.Item2);
            if (value is T3) return f3(this.Item3);
            if (value is T4) return f4(this.Item4);
            if (value is T5) return f5(this.Item5);
            if (value is T6) return f6(this.Item6);
            if (value is T7) return f7(this.Item7);
            else return f8(this.Item8);
        }

        public void Visit(System.Action<T1> f1,System.Action<T2> f2,System.Action<T3> f3,System.Action<T4> f4,System.Action<T5> f5,System.Action<T6> f6,System.Action<T7> f7,System.Action<T8> f8)
        {
            if (value is T1) f1(this.Item1);
            if (value is T2) f2(this.Item2);
            if (value is T3) f3(this.Item3);
            if (value is T4) f4(this.Item4);
            if (value is T5) f5(this.Item5);
            if (value is T6) f6(this.Item6);
            if (value is T7) f7(this.Item7);
            else f8(this.Item8);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<T1,T2,T3,T4,T5,T6,T7,T8>;

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

