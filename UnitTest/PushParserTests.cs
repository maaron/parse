using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PushParse;
using PushParse.Combinators;
using Functional;

namespace UnitTest
{
    /// <summary>
    /// Summary description for PushParserTests
    /// </summary>
    [TestClass]
    public class PushParserTests
    {
        public PushParserTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestString()
        {
            var p = Chars.String("abc");
            var stack = p.Parse(m => Assert.AreEqual(m.Value, "abc"));

            stack.ProcessToken('a');
            Assert.IsTrue(stack.IsDone == false);
            Assert.IsTrue(stack.IsMatch == false);

            stack.ProcessToken('b');
            Assert.IsTrue(stack.IsDone == false);
            Assert.IsTrue(stack.IsMatch == false);

            stack.ProcessToken('c');
            Assert.IsTrue(stack.IsDone == true);
            Assert.IsTrue(stack.IsMatch == true);
        }

        [TestMethod]
        public void TestConst()
        {
            var p = Chars.Const('a');
            var stack = p.Parse(m => Assert.AreEqual(m.Value, 'a'));

            stack.ProcessToken('a');
            Assert.IsTrue(stack.IsDone == true);
            Assert.IsTrue(stack.IsMatch == true);

            stack = p.Parse(m => Assert.IsTrue(!m.HasValue));

            stack.ProcessToken('b');
            Assert.IsTrue(stack.IsDone == true);
            Assert.IsTrue(stack.IsMatch == false);

            stack = p.Parse(m => Assert.IsTrue(!m.HasValue));

            stack.ProcessToken(Maybe.None<char>());
            Assert.IsTrue(stack.IsDone == true);
            Assert.IsTrue(stack.IsMatch == false);
        }

        [TestMethod]
        public void TestOr()
        {
            var p = Chars.String("ab").Or(from s in Chars.String("aa") select 123);

            string r1 = null;
            var stack = p.Parse(m => { r1 = m.Value.Item1; });

            stack.ProcessToken('a');
            Assert.IsTrue(stack.IsDone == false);
            Assert.IsTrue(stack.IsMatch == false);

            stack.ProcessToken('b');
            Assert.IsTrue(stack.IsDone == true);
            Assert.IsTrue(stack.IsMatch == true);
            Assert.IsTrue(r1 == "ab");

            int r2 = 0;
            stack = p.Parse(m => { r2 = m.Value.Item2; });

            stack.ProcessToken('a');
            Assert.IsTrue(stack.IsDone == false);
            Assert.IsTrue(stack.IsMatch == false);

            stack.ProcessToken('a');
            Assert.IsTrue(stack.IsDone == true);
            Assert.IsTrue(stack.IsMatch == true);
            Assert.IsTrue(r2 == 123);
        }

        [TestMethod]
        public void TestAggregate()
        {
            var p = Chars.String("abc").Aggregate(() => new FList<string>(), (list, match) =>
            {
                list.Add(match);
                return list;
            });

            var stack = p.Parse(list => Assert.IsTrue(list.Equals(new FList<string>() { "abc", "abc" })));

            stack.ProcessToken('a');
            Assert.IsTrue(stack.IsDone == false);
            Assert.IsTrue(stack.IsMatch == false);

            stack.ProcessToken('b');
            Assert.IsTrue(stack.IsDone == false);
            Assert.IsTrue(stack.IsMatch == false);

            stack.ProcessToken('c');
            Assert.IsTrue(stack.IsDone == false);
            Assert.IsTrue(stack.IsMatch == false);

            stack.ProcessToken('a');
            Assert.IsTrue(stack.IsDone == false);
            Assert.IsTrue(stack.IsMatch == false);

            stack.ProcessToken('b');
            Assert.IsTrue(stack.IsDone == false);
            Assert.IsTrue(stack.IsMatch == false);

            stack.ProcessToken('c');
            Assert.IsTrue(stack.IsDone == false);
            Assert.IsTrue(stack.IsMatch == false);

            stack.ProcessToken('d');
            Assert.IsTrue(stack.IsDone == true);
            Assert.IsTrue(stack.IsMatch == true);
        }
    }
}
