using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parse;
using Parse.Extensions;
using Parse.Character;
using Functional;

namespace UnitTest
{
    [TestClass]
    public class UnitTest
    {
        private static bool ValueEquals<A, B>(A a, B b)
        {
            return a.Equals(b);
        }

        private static void CheckMatches<T>(Parser<char, List<T>> p, string input, IEnumerable<T> value)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsLeft);
            Assert.IsFalse(value.Zip(result.Left.Value, (a, b) => a.Equals(b)).Contains(false));
        }

        private static void CheckMatch<V>(Parser<char, V> p, string input, V value)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsLeft);
            Assert.IsTrue(ValueEquals(result.Left.Value, value));
        }

        private static void CheckMatch(Parser<char, string> p, string input, string value)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsLeft);
            Assert.IsTrue(result.Left.Value == value);
        }

        private static void CheckMatch(Parser<char, char> p, string input, char value)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsLeft);
            Assert.IsTrue(result.Left.Value == value);
        }

        private static void CheckMatch<V>(Parser<char, V> p, string input)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsLeft);
        }

        private static void CheckMatch<V>(Parser<char, V> p, string input, Func<V, bool> predicate)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsLeft);
            Assert.IsTrue(predicate(result.Left.Value));
        }

        private static void CheckMatch(Parser<char> p, string input)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsLeft);
        }

        private static void CheckFail<V>(Parser<char, V> p, string input)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsRight);
        }

        // This little helper is weird, but needed in order to construct an 
        // Either object with both type parameters the same.  Oddly, you can
        // construct it this way because the generic method binds to the first
        // Either constructor at compile time.  However, trying 
        // "new Either<T,T>(t)" doesn't compile because the call is ambiguous 
        // between Either's two constructors.
        private static Either<A, B> SameEither<A, B>(A a)
        {
            return new Either<A, B>(a);
        }

        [TestMethod]
        public void TestEitherStructuralEquality()
        {
            Assert.IsTrue(ValueEquals(
                new Either<char, int>('a'),
                new Either<char, int>('a')));

            Assert.IsTrue(ValueEquals(
                new Either<char, Either<int, bool>>(new Either<int,bool>(false)),
                new Either<char, Either<int, bool>>(new Either<int, bool>(false))));

            Assert.IsFalse(ValueEquals(
                new Either<char, int>('a'),
                new Either<char, int>(1)));

            Assert.IsTrue(SameEither<int, int>(1).IsLeft);
            Assert.IsTrue(SameEither<int, int>(1).IsRight);
        }

        [TestMethod]
        public void TestSequence()
        {
            var Digit = Chars.Digit;
            var two = Digit.And(Digit);
            var three = two.And(Digit);

            CheckMatch(two, "12", Tuple.Create('1', '2'));
            CheckFail(two, "1a");
            CheckFail(two, "a1");

            CheckMatch(three, "456", Tuple.Create('4', '5', '6'));
            CheckFail(three, "45.");
        }

        [TestMethod]
        public void TestAlternate()
        {
            var Digit = Chars.Digit;
            var twoDigits = Digit.And(Digit);
            var letter = Chars.Letter;
            var two = twoDigits.Or(letter);

            CheckMatch(two, "a", new Either<Tuple<char, char>, char>('a'));
            CheckMatch(two, "12", new Either<Tuple<char, char>, char>(Tuple.Create('1', '2')));
            CheckFail(two, "");
            CheckFail(two, "1a");
        }

        [TestMethod]
        public void TestAlternateSameValueType()
        {
            var two = Chars.Letter.OrSame(Chars.Digit);

            CheckMatch(two, "a", 'a');
        }

        [TestMethod]
        public void TestSplit()
        {
            var digit = Chars.Digit;
            var letter = Chars.Letter;
            var split = Combinators.Split(digit, letter);
            CheckMatches(split, "1a2b3c4d5", new[]{ '1', '2', '3', '4', '5' });

            var splitBy = digit.SplitBy(letter);
            CheckMatches(splitBy, "1a2b3c4d5", new[] { '1', '2', '3', '4', '5' });
        }

        [TestMethod]
        public void TestCsv()
        {
            var comma = Chars.Const(',');
            var crlf = "\r\n".Or('\r').Or('\n');
            var c = Chars.Any.Except(comma.Or(crlf));
            var field = c.Repeated().Return(l => new String(l.ToArray()));
            var line = field.SplitBy(comma);
            var lines = line.SplitBy(crlf);

            CheckMatch(lines, "a,b,c");
            CheckMatch(lines, "asdf,qwer,zxcv");
            CheckMatch(lines,
                "asdf,qwer,zxcv\n" +
                "1   , 23454, d c f sd , waefwef,,\r" +
                ",,,\r\n" +
                "asdf",
                csv => csv.Count == 4
                    && csv[0].SequenceEqual(new[] {"asdf","qwer","zxcv"})
                    && csv[1].SequenceEqual(new[] {"1   "," 23454"," d c f sd "," waefwef","",""})
                    && csv[2].SequenceEqual(new[] {"","","",""})
                    && csv[3].SequenceEqual(new[] {"asdf"}));
        }

        [TestMethod]
        public void StringEscape()
        {
            Func<char,char> map = (c) =>
                  c == 'n' ? '\n'
                : c == 'r' ? '\r'
                : c == 't' ? '\t'
                : c;

            var escaped = '\\'.And(Chars.Any).Return(map);
            var nonescaped = Chars.Any.Except(escaped);
            var term = nonescaped.If(c => c == '"');
            var mapped = escaped.OrSame(Chars.Any);
            var str = '"'.And(mapped.Except(term).Repeated()).And('"')
                .Return(l => new String(l.ToArray()));

            CheckMatch(str, "\"asdf\"", "asdf");
            CheckMatch(str, "\"\\\\\"", "\\");
            CheckMatch(str, "\"\\\"hello\\\"\"", "\"hello\"");
            CheckMatch(str, "\"hello\\t\\r\\nthere\"", "hello\t\r\nthere");
        }

        [TestMethod]
        public void ErrorHandling()
        {
            var parser = '1'.And("23".And('4').Or("24")).Or("12").And('4');

            var input = new ParseInput<char>("123");
            var result = parser(input);
            Assert.IsTrue(result.IsRight);
            Assert.IsTrue(input.Error.LongestFailure == 3);
            Assert.IsTrue(input.Error.LongestMatch == 3);
            Assert.IsTrue(input.Error.LastMatch == 2);
        }

        [TestMethod]
        public void LineTrackingInput()
        {

            IParseInput<char> input = new LineTrackingInput(new ParseInput<char>(
                "abc\r\n" +
                "def\n" +
                "ghi\r" +
                "jkl\r" +
                "\r\n" + 
                "\n" +
                "\r"));

            Action<int, int> CheckPos = (l, c) =>
            {
                Assert.IsTrue(
                ((LineTrackingInput)input).Position == new LineColumn(l, c));

                if (!input.IsEnd) input = input.Next();
            };

            CheckPos(0, 0);
            CheckPos(0, 1);
            CheckPos(0, 2);
            CheckPos(0, 3);
            CheckPos(0, 4);
            CheckPos(1, 0);
            CheckPos(1, 1);
            CheckPos(1, 2);
            CheckPos(1, 3);
            CheckPos(2, 0);
            CheckPos(2, 1);
            CheckPos(2, 2);
            CheckPos(2, 3);
            CheckPos(3, 0);
            CheckPos(3, 1);
            CheckPos(3, 2);
            CheckPos(3, 3);
            CheckPos(4, 0);
            CheckPos(4, 1);
            CheckPos(5, 0);
            CheckPos(6, 0);
            CheckPos(7, 0);
        }

        class Expr
        {
            public List<Either<Expr, string>> items;

            public Expr()
            {
                items = new List<Either<Expr, string>>();
            }

            public Expr(params Either<Expr, string>[] list)
            {
                items = list.ToList();
            }

            public override bool Equals(object obj)
            {
                var e = obj as Expr;
                return obj != null
                    && obj is Expr
                    && items.SequenceEqual(e.items);
            }

            public override int GetHashCode()
            {
                return items.Aggregate(0, (x, y) => x ^ y.GetHashCode());
            }
        };

        public class Ref<T>
        {
            public T Value { get; private set; }

            public Ref(T value)
            {
                Value = value;
            }
        }

        [TestMethod]
        public void Recursive()
        {
            // Lisp-style lists
            var token = Chars.Letter.Repeated(1).Return(
                l => new String(l.ToArray()));

            var ws = Combinators.Ignore(Chars.Space).Repeated();

            // This is a little "delayed-binding" trick in order to get a 
            // recursive parser.  An alternative is to use a normal function
            // instead, which can be self-referencing.  The key part is the 
            // exprRef lambda that captures the local expr variable, which 
            // doesn't actually get used until after it is initialized below.
            Parser<char, Expr> expr = null;
            Parser<char, Expr> exprRef = (i) => expr(i);
            expr = ws
                .And('(')
                .And(ws.And(exprRef.Or(token)).Repeated())
                .And(ws)
                .And(')').Return(
                    e => new Expr() { items = e });

            CheckMatch(expr, "()", new Expr());

            CheckMatch(expr, "(())", new Expr(new Expr()));

            CheckMatch(expr, "(asdf qwer)", new Expr("asdf", "qwer"));

            CheckMatch(expr, "(a (b b (c c c (d d d d))))", new Expr(
                "a", new Expr(
                    "b", "b", new Expr(
                        "c", "c", "c", new Expr(
                            "d", "d", "d", "d")))));

            CheckFail(expr, "(");
        }

        [TestMethod]
        public void OnParse()
        {
            int called = 0;
            var p = Chars.Const('a').Return(() => 1);

            p.OnParse(v =>
            {
                Assert.IsTrue(v.IsValid);
                called++;
            })(new ParseInput<char>("a"));
            Assert.IsTrue(called == 1);

            p.OnParse(v =>
            {
                Assert.IsFalse(v.IsValid);
                called++;
            })(new ParseInput<char>("b"));
            Assert.IsTrue(called == 2);

            p.OnMatch(v =>
            {
                called++;
            })(new ParseInput<char>("a"));
            Assert.IsTrue(called == 3);

            p.OnMatch(v =>
            {
                called++;
            })(new ParseInput<char>("b"));
            Assert.IsTrue(called == 3);

            p.OnFail(() =>
            {
                called++;
            })(new ParseInput<char>("a"));
            Assert.IsTrue(called == 3);

            p.OnFail(() =>
            {
                called++;
            })(new ParseInput<char>("b"));
            Assert.IsTrue(called == 4);
        }
    }
}
