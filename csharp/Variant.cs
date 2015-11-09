
using System;
using System.Collections;

namespace Functional
{
    public class Variant<L, R>
    {
        Object value;

        public L Left { get { return (L)value; } }
        public R Right { get { return (R)value; } }

        public bool IsLeft { get { return value is L; } }
        public bool IsRight { get { return value is R; } }

        public static implicit operator Variant<L, R>(L left)
        {
            return new Variant<L, R>(left);
        }

        public static implicit operator Variant<L, R>(R right)
        {
            return new Variant<L, R>(right);
        }

        public Variant(L left)
        {
            this.value = left;
        }

        public Variant(R right)
        {
            this.value = right;
        }

        public T Visit<T>(Func<L, T> left, Func<R, T> right)
        {
            if (value is L) return left(this.Left);
            else return right(this.Right);
        }

        public void Visit(Action<L> left, Action<R> right)
        {
            if (value is L) left(this.Left);
            else right(this.Right);
        }

        public override bool Equals(object other)
        {
            var e = other as Variant<L, R>;

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