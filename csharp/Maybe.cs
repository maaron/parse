using System;

namespace Functional
{
    public class Maybe<T>
    {
        private T value;
        private bool valid;

        public Maybe(T value)
        {
            this.valid = true;
            this.value = value;
        }

        public Maybe()
        {
            this.valid = false;
        }

        public bool IsValid { get { return valid; } }

        public T Value
        {
            get
            {
                if (!IsValid) throw new Exception("Maybe value not valid");
                return value;
            }
            set
            {
                valid = true;
            }
        }

        public R Visit<R>(Func<R> none, Func<T, R> some)
        {
            if (valid) return some(value);
            else return none();
        }

        public void Visit(Action none, Action<T> some)
        {
            if (valid) some(value);
            else none();
        }
    }
}