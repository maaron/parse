using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parse.Push;
using Parse.Push.Linq;
using Parse.Push.Combinators;
using Functional;

namespace UnitTest
{
    /// <summary>
    /// Summary description for PushParserTests
    /// </summary>
    [TestClass]
    public class PushParserTests
    {
        [TestMethod]
        public void String()
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
        public void Const()
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
        public void Or()
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
        public void Aggregate()
        {
            var p = Chars.String("abc").Aggregate(() => new FList<string>(), (list, match) =>
            {
                list.Add(match);
                return list;
            });

            FList<string> actual = null;
            var stack = p.Parse(list => { actual = list.Value; });

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
            Assert.IsTrue(actual.Equals(new FList<string>() { "abc", "abc" }));
        }
    }
}
