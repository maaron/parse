using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parse;
using GrammarAnalyzer;

namespace UnitTest
{
    [TestClass]
    public class AbnfTests
    {
        // This is just to force the Abnf class static constructor to run, so 
        // that unit test duration measurements don't include it.
        static bool result = GrammarAnalyzer.Abnf.syntax(new ParseInput<char>("rule=asdf\n")).IsSuccess;

        [TestMethod]
        public void Syntax()
        {
            var result = Abnf.syntax(new ParseInput<char>("rule=identifier\r\n"));
            Assert.IsTrue(result.IsSuccess);
            var value = result.Success.Value;
            Assert.IsTrue(value.Count == 1);
            Assert.IsTrue(value[0].Name.Value == "rule");
            Assert.IsTrue(value[0].IsAdditional == false);
            Assert.IsTrue(value[0].Alternations[0].Repetitions[0].Element.Item1.Value == "identifier");

            UnitTest.CheckMatch(Abnf.syntax, "rule=identifier identifier\n", r =>
                r[0].Alternations[0].Repetitions.Count == 2);

            UnitTest.CheckMatch(Abnf.syntax, "another=\"token\"\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item4.Value == "token");

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=identifier identifier\n" +
                "another=\"token\"\n", r =>
                    r[0].Alternations[0].Repetitions[0].Element.Item1.Value == "identifier" &&
                    r[0].Alternations[0].Repetitions[1].Element.Item1.Value == "identifier" &&
                    r[1].Alternations[0].Repetitions[0].Element.Item4.Value == "token");

            Console.WriteLine("asdf");

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=%x61\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item5.SequenceOrRange.Item1[0] == 0x61);

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=%x61.62.63\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item5.SequenceOrRange.Item1.Equals(
                    FList.Create(0x61, 0x62, 0x63)));

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=%x61-ff\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item5.SequenceOrRange.Item2.Equals(
                    Tuple.Create(0x61, 0xff)));

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=%d123\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item5.SequenceOrRange.Item1[0] == 123);

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=%d1.2.3\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item5.SequenceOrRange.Item1.Equals(
                    FList.Create(1, 2, 3)));

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=%d123-512\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item5.SequenceOrRange.Item2.Equals(
                    Tuple.Create(123, 512)));

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=/identifier\r", r =>
                r[0].IsAdditional == true);

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=identifier\r", r =>
                r[0].IsAdditional == false);

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=%b0110\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item5.SequenceOrRange.Item1[0] == 6);

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=%b0001.0010.0011\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item5.SequenceOrRange.Item1.Equals(
                    FList.Create(1, 2, 3)));

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=%b010-111\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item5.SequenceOrRange.Item2.Equals(
                    Tuple.Create(2, 7)));

            UnitTest.CheckFail(Abnf.syntax, "rule=%x61-ff00000000\n");

            UnitTest.CheckMatch(Abnf.syntax,
                "rule=identifier1\r identifier2\n", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item1.Value == "identifier1" &&
                r[0].Alternations[0].Repetitions[1].Element.Item1.Value == "identifier2");

            UnitTest.CheckMatch(Abnf.syntax,
                "rule = *identifier\r", r =>
                !r[0].Alternations[0].Repetitions[0].Repeat.Value.RangeOrCount.Item1.Item1.IsValid &&
                !r[0].Alternations[0].Repetitions[0].Repeat.Value.RangeOrCount.Item1.Item2.IsValid &&
                r[0].Alternations[0].Repetitions[0].Element.Item1.Value == "identifier");

            UnitTest.CheckMatch(Abnf.syntax,
                "rule = 1*identifier\r", r =>
                r[0].Alternations[0].Repetitions[0].Repeat.Value.RangeOrCount.Item1.Item1.Value == 1 &&
                !r[0].Alternations[0].Repetitions[0].Repeat.Value.RangeOrCount.Item1.Item2.IsValid &&
                r[0].Alternations[0].Repetitions[0].Element.Item1.Value == "identifier");

            UnitTest.CheckMatch(Abnf.syntax,
                "rule = 1identifier\r", r =>
                r[0].Alternations[0].Repetitions[0].Repeat.Value.RangeOrCount.Item2 == 1 &&
                r[0].Alternations[0].Repetitions[0].Element.Item1.Value == "identifier");

            UnitTest.CheckMatch(Abnf.syntax,
                "rule = abc / def\r", r =>
                r[0].Alternations[0].Repetitions[0].Element.Item1.Value == "abc" &&
                r[0].Alternations[1].Repetitions[0].Element.Item1.Value == "def");
        }
    }
}
