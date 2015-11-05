using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parse;
using Functional;

namespace UnitTest
{
    [TestClass]
    public class UnitTest
    {
        private static bool ValueEquals(IStructuralEquatable a, IStructuralEquatable b)
        {
            return a.Equals(b, StructuralComparisons.StructuralEqualityComparer);
        }

        private static void CheckMatches<T>(Parser<char, List<T>> p, string input, IEnumerable<T> value)
        {
            var result = p(new StringInput(input));
            Assert.IsTrue(result.IsLeft);
            Assert.IsFalse(value.Zip(result.Left.Value, (a, b) => a.Equals(b)).Contains(false));
        }

        private static void CheckMatch<V>(Parser<char, V> p, string input, V value) where V : IStructuralEquatable
        {
            var result = p(new StringInput(input));
            Assert.IsTrue(result.IsLeft);
            Assert.IsTrue(ValueEquals(result.Left.Value, value));
        }

        private static void CheckMatch(Parser<char, char> p, string input, char value)
        {
            var result = p(new StringInput(input));
            Assert.IsTrue(result.IsLeft);
            Assert.IsTrue(result.Left.Value == value);
        }

        private static void CheckMatch<V>(Parser<char, V> p, string input)
        {
            var result = p(new StringInput(input));
            Assert.IsTrue(result.IsLeft);
        }

        private static void CheckFail<V>(Parser<char, V> p, string input)
        {
            var result = p(new StringInput(input));
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

            var splitBy = Combinators.Split(digit, letter);
            CheckMatches(splitBy, "1a2b3c4d5", new[] { '1', '2', '3', '4', '5' });
        }
    }
}
