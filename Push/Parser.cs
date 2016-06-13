using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Functional;

namespace PushParse
{
    public interface IParserStack<T>
    {
        void Push<S, V>(Parser<T, S, V> parser, Action<Maybe<V>> resultHandler);
        void Pop<V>(Maybe<V> result);
        IParserStack<T> Backtrack();
        object State { get; set; }
        int Position { get; }
    }
     
    public class ParserStack<T, V> : IParserStack<T>
    {
        public int Position { get; set; }
        protected List<T> Buffer { get; private set; }
        protected Stack<ParserState> States { get; private set; }
        private bool isEnd = false;

        public bool IsMatch { get; private set; }
        public bool IsDone { get; private set; }

        public ParserStack()
        {
            Buffer = new List<T>();
            States = new Stack<ParserState>();
            IsMatch = false;
            IsDone = false;
        }

        public void Push<S, R>(Parser<T, S, R> parser, Action<Maybe<R>> resultHandler)
        {
            var tuple = parser();
            var typedStack = new ParserFrame<T, S>(this);
            States.Push(new ParserState()
            {
                State = tuple.Item1,
                TokenHandler = t => tuple.Item2(typedStack, t),
                ResultHandler = resultHandler,
                Start = Position
            });

            // Process any buffered data, stopping before the current token
            while (Position < Buffer.Count - 1 && States.Count > 0)
            {
                States.Peek().TokenHandler(Buffer[Position]);
                Position++;
            }

            // Process the current token
            if (States.Count > 0)
            {
                if (Position < Buffer.Count)
                    States.Peek().TokenHandler(Buffer[Position]);
                else if (isEnd)
                    States.Peek().TokenHandler(Maybe.None<T>());
            }
        }

        public void Pop<R>(Maybe<R> result)
        {
            if (States.Count > 0)
            {
                var parserState = States.Pop();
                ((Action<Maybe<R>>)parserState.ResultHandler)(result);

                if (States.Count == 0 && !IsDone)
                {
                    IsDone = true;
                    IsMatch = result.HasValue;
                }
            }
        }

        public void ProcessToken(Maybe<T> token)
        {
            if (token.HasValue) Buffer.Add(token.Value);
            else isEnd = true;
            States.Peek().TokenHandler(token);
            Position++;
        }

        public IParserStack<T> Backtrack()
        {
            Position = States.Peek().Start;
            return this;
        }

        public object State
        {
            get { return States.Peek().State; }
            set { States.Peek().State = value; }
        }

        protected class ParserState
        {
            public Action<Maybe<T>> TokenHandler;
            public object ResultHandler;
            public object State;
            public int Start;
        }
    }

    public class ParserFrame<T, S> : IParserStack<T>
    {
        private IParserStack<T> Stack;

        public ParserFrame(IParserStack<T> stack)
        {
            Stack = stack;
        }

        public int Position
        {
            get
            {
                return Stack.Position;
            }
        }

        public object State
        {
            get { return Stack.State; }
            set { Stack.State = value; }
        }

        public S Value
        {
            get { return (S)State; }
            set { State = value; }
        }

        public IParserStack<T> Backtrack()
        {
            return Stack.Backtrack();
        }

        public void Pop<V>(Maybe<V> result)
        {
            Stack.Pop<V>(result);
        }

        public void Push<S2, V>(Parser<T, S2, V> parser, Action<Maybe<V>> resultHandler)
        {
            Stack.Push<S2, V>(parser, resultHandler);
        }
    }

    public delegate Tuple<S, ProcessToken<T, S, V>> Parser<T, S, V>();
    public delegate void ProcessToken<T, S, V>(ParserFrame<T, S> stack, Maybe<T> token);

    public static class ParserExtensions
    {
        public static ParserStack<T, V> Parse<T, S, V>(this Parser<T, S, V> parser, Action<Maybe<V>> handler)
        {
            var stack = new ParserStack<T, V>();
            stack.Push(parser, handler);
            return stack;
        }
    }
}
