using System;
using System.Collections.Generic;

namespace Functional
{
    public static class Maybe
    {
        public static Maybe<T> Some<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public static Maybe<T> None<T>()
        {
            return new Maybe<T>();
        }

        public static T? AsNullable<T>(this Maybe<T> m) where T : struct
        {
            return m.HasValue ? new T?(m.Value) : new T?();
        }
    }

    public struct Maybe<T> : IEquatable<Maybe<T>>
    {
        private T value;
        private bool valid;

        public Maybe(T value)
        {
            this.valid = true;
            this.value = value;
        }

        public bool HasValue { get { return valid; } }

        public T Value
        {
            get
            {
                if (!HasValue) throw new InvalidOperationException();
                return value;
            }
            set
            {
                valid = true;
            }
        }

        public R Map<R>(Func<R> none, Func<T, R> some)
        {
            if (valid) return some(value);
            else return none();
        }

        public void Visit(Action none, Action<T> some)
        {
            if (valid) some(value);
            else none();
        }

        public static implicit operator Maybe<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public override bool Equals(object obj)
        {
            return obj != null
                && obj is Maybe<T>
                && Equals((Maybe<T>)obj);
        }

        public bool Equals(Maybe<T> other)
        {
            return HasValue == other.HasValue
                && (!HasValue 
                    || EqualityComparer<T>.Default.Equals(Value, other.Value));
        }

        public override int GetHashCode()
        {
            return HasValue ? Value.GetHashCode()
                : base.GetHashCode();
        }

        public static bool operator ==(Maybe<T> lhs, Maybe<T> rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Maybe<T> lhs, Maybe<T> rhs)
        {
            return !(lhs == rhs);
        }
    }
}