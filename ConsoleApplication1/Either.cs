
using System;

namespace Functional
{
    public class Either<L, R>
    {
        Object value;

        public L Left { get { return (L)value; } }
        public R Right { get { return (R)value; } }

        public bool IsLeft { get { return value is L; } }
        public bool IsRight { get { return value is R; } }

        public Either(L left)
        {
            this.value = left;
        }

        public Either(R right)
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
    }
}