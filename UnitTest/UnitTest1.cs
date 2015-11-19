using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parse;
using Parse.Combinators;
using Parse.InputExtensions;
using Parse.CharCombinators;
using Functional;
using Parse.Linq;
using GrammarAnalyzer.EBNF;

namespace UnitTest
{
    [TestClass]
    public class UnitTest
    {
        private static bool ValueEquals<A, B>(A a, B b)
        {
            return a.Equals(b);
        }

        private static void CheckMatch<V>(Parser<char, V> p, string input, V value)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(ValueEquals(result.Success.Value, value));
        }

        private static void CheckMatch(Parser<char, string> p, string input, string value)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Success.Value == value);
        }

        private static void CheckMatch(Parser<char, char> p, string input, char value)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Success.Value == value);
        }

        private static void CheckMatch<V>(Parser<char, V> p, string input)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsSuccess);
        }

        private static void CheckMatch<V>(Parser<char, V> p, string input, Func<V, bool> predicate)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(predicate(result.Success.Value));
        }

        private static void CheckMatch(Parser<char> p, string input)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsSuccess);
        }

        private static void CheckFail<V>(Parser<char, V> p, string input)
        {
            var result = p(new ParseInput<char>(input));
            Assert.IsTrue(result.IsFailure);
        }

        // This little helper is weird, but needed in order to construct an 
        // Either object with both type parameters the same.  Oddly, you can
        // construct it this way because the generic method binds to the first
        // Either constructor at compile time.  However, trying 
        // "new Either<T,T>(t)" doesn't compile because the call is ambiguous 
        // between Either's two constructors.
        private static Variant<A, B> SameEither<A, B>(A a)
        {
            return new Variant<A, B>(a);
        }

        [TestMethod]
        public void EitherStructuralEquality()
        {
            Assert.IsTrue(ValueEquals(
                new Variant<char, int>('a'),
                new Variant<char, int>('a')));

            Assert.IsTrue(ValueEquals(
                new Variant<char, Variant<int, bool>>(new Variant<int,bool>(false)),
                new Variant<char, Variant<int, bool>>(new Variant<int, bool>(false))));

            Assert.IsFalse(ValueEquals(
                new Variant<char, int>('a'),
                new Variant<char, int>(1)));

            Assert.IsTrue(SameEither<int, int>(1).IsItem1);
            Assert.IsTrue(SameEither<int, int>(1).IsItem2);
        }

        [TestMethod]
        public void Sequence()
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
        public void Alternate()
        {
            var Digit = Chars.Digit;
            var twoDigits = Digit.And(Digit);
            var letter = Chars.Letter;
            var two = twoDigits.Or(letter);

            CheckMatch(two, "a", new Variant<Tuple<char, char>, char>('a'));
            CheckMatch(two, "12", new Variant<Tuple<char, char>, char>(Tuple.Create('1', '2')));
            CheckFail(two, "");
            CheckFail(two, "1a");
        }

        [TestMethod]
        public void AlternateSameValueType()
        {
            var two = Combinator.AnyOf(Chars.Letter, Chars.Digit);

            CheckMatch(two, "a", 'a');
        }

        [TestMethod]
        public void Split()
        {
            var digit = Chars.Digit;
            var letter = Chars.Letter;
            var split = digit.SplitBy(letter);
            CheckMatch(split, "1a2b3c4d5", FList.Create('1', '2', '3', '4', '5'));

            var splitBy = digit.SplitBy(letter);
            CheckMatch(splitBy, "1a2b3c4d5", FList.Create('1', '2', '3', '4', '5'));
        }

        [TestMethod]
        public void Csv()
        {
            var comma = Chars.Const(',');
            var crlf = "\r\n".Or('\r').Or('\n');
            var c = Chars.Any.Except(comma.Or(crlf));
            var field = c.ZeroOrMore().Return(l => new String(l.ToArray()));
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
            var str = '"'.And(mapped.Except(term).ZeroOrMore()).And('"')
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
            Assert.IsTrue(result.IsFailure);
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
            public List<Variant<Expr, string>> items;

            public Expr()
            {
                items = new List<Variant<Expr, string>>();
            }

            public Expr(params Variant<Expr, string>[] list)
            {
                items = list.ToList();
            }

            public override bool Equals(object obj)
            {
                var e = obj as Expr;
                return e != null
                    && items.Equals(e.items);
            }

            public override int GetHashCode()
            {
                return items.GetHashCode();
            }
        };

        [TestMethod]
        public void Recursive()
        {
            // Lisp-style lists
            var token = Chars.Letter.AtLeastMany(1).ReturnString();

            var ws = Chars.Space.Ignored().ZeroOrMore();

            // This is a little "delayed-binding" trick in order to get a 
            // recursive parser.  An alternative is to use a normal function
            // instead, which can be self-referencing.  The key part is the 
            // exprRef lambda that captures the local expr variable, which 
            // doesn't actually get used until after it is initialized below.
            Parser<char, Expr> expr = null;
            Parser<char, Expr> exprRef = (i) => expr(i);
            expr = ws
                .And('(')
                .And(exprRef.Or(ws.And(token)).ZeroOrMore())
                .And(ws)
                .And(')')
                .Return(e => new Expr() { items = e });

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

            p.OnParse(r =>
            {
                Assert.IsTrue(r.IsSuccess);
                called++;
            })(new ParseInput<char>("a"));
            Assert.IsTrue(called == 1);

            p.OnParse(r =>
            {
                Assert.IsFalse(r.IsSuccess);
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

        [TestMethod]
        public void FilterParser()
        {
            List<int> numbers = new List<int>();
            var number = Chars.Digit.AtLeastMany(1).Return(l => int.Parse(new string(l.ToArray())));
            IParseInput<char> filter = new FilterParser<char>(
                new ParseInput<char>("1a12b123c1234d"), 
                number.OnMatch(
                    n => numbers.Add(n)).Ignored());

            Assert.IsTrue(filter.Current == 'a');
            filter = filter.Next();
            Assert.IsTrue(filter.Current == 'b');
            filter = filter.Next();
            Assert.IsTrue(filter.Current == 'c');
            filter = filter.Next();
            Assert.IsTrue(filter.Current == 'd');
            Assert.IsTrue(numbers.SequenceEqual(new[] { 1, 12, 123, 1234 }));
        }

        [TestMethod]
        public void Repeated()
        {
            var number = Chars.Digit.AtLeastMany(1).Return(l => int.Parse(new string(l.ToArray())));
            CheckMatch(number, "123", 123);
            CheckFail(number, "");
        }

        [TestMethod]
        public void VariantTemplate()
        {
            // Generate and compile code for a Variant(T0, T1, T2, T3) class
            var templ = new Templates.VariantTemplate(4);
            var code = templ.TransformText();
            var p = new Microsoft.CSharp.CSharpCodeProvider();
            var compiled = p.CompileAssemblyFromSource(
                new System.CodeDom.Compiler.CompilerParameters(new[] { "Parse.dll" }),
                code);

            Assert.IsTrue(compiled.Errors.Count == 0);

            // Get the generic class from the compiled assembly and supply 
            // type parameters to create a concrete class, i.e., 
            // Variant<char, int, string, bool>.
            var var4Class = compiled.CompiledAssembly.ExportedTypes.First()
                .MakeGenericType(typeof(char), typeof(int), typeof(string), typeof(bool));

            // Call the "string" constructor
            var ctors = var4Class.GetConstructors();
            var query = from c in ctors
                        from param in c.GetParameters()
                        where param.ParameterType == typeof(string)
                        select c;

            dynamic var4 = query.First().Invoke(new Object[] {"asdf"});

            dynamic visitRet = var4.Visit(
                new Func<char, string>(i => i + " char"),
                new Func<int, string>(i => i + " int"),
                new Func<string, string>(i => i + " string"),
                new Func<bool, string>(i => i + " bool"));

            Assert.IsTrue(visitRet == "asdf string");
        }

        [TestMethod]
        public void TransformParser()
        {
            var parser = Chars.Letter.AtLeastMany(1).ReturnString().And(' ');

            var input = new ParseInput<char>("asdf qwer zxcv ");
            IParseInput<string> transformed = new TransformParser<char, string>(input, parser);
            Assert.IsTrue(transformed.Current == "asdf");
            Assert.IsTrue(transformed.Next().Current == "qwer");
            Assert.IsTrue(transformed.Next().Next().Current == "zxcv");
        }

        [TestMethod]
        public void InputExtensions()
        {
            var input = new ParseInput<char>("asdf");
            Assert.IsTrue(input.Remaining().AsString() == "asdf");
            Assert.IsTrue(input.Next().Remaining().AsString() == "sdf");
            Assert.IsTrue(input.Remaining(input.Next().Next()).AsString() == "as");
        }

        [TestMethod]
        public void InputRange()
        {
            var input = new ParseInput<char>("12345678");
            var range = new InputRange<char>(
                input.Next().Next(),
                input.Next().Next().Next().Next().Next().Next());

            Assert.IsTrue(range.Remaining().SequenceEqual("3456"));
        }

        [TestMethod]
        public void Anchored()
        {
            var input = new ParseInput<char>("1234 567");
            var num = Chars.Digit.Ignored().AtLeastMany(1).ReturnString().Anchored();
            var parser = num.And(' ').And(num);
            var result = parser(input);
            
            Assert.IsTrue(result.IsSuccess);
            
            Assert.IsTrue(result.Success.Value.Item1.Start.CompareTo(
                input) == 0);

            Assert.IsTrue(result.Success.Value.Item1.End.CompareTo(
                input.Next().Next().Next().Next()) == 0);

            Assert.IsTrue(result.Success.Value.Item1.Value == "1234");

            Assert.IsTrue(result.Success.Value.Item2.Start.CompareTo(
                input.Next().Next().Next().Next().Next()) == 0);

            Assert.IsTrue(result.Success.Value.Item2.End.CompareTo(
                input.Next().Next().Next().Next().Next().Next().Next().Next()) == 0);

            Assert.IsTrue(result.Success.Value.Item2.Value == "567");
        }

        [TestMethod]
        public void Ebnf()
        {
            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.meta_identifier, "a123qwer");
            CheckFail(GrammarAnalyzer.EBNF.Ebnf.meta_identifier, "123qwer");

            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.terminal_string, "\"asdf\"", "asdf");
            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.terminal_string, "'asdf'", "asdf");

            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.single_definition, "identifier");
            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.single_definition, "{identifier}");
            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.single_definition, "[identifier]");
            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.single_definition, "(identifier)");
            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.single_definition, "identifier,identifier");
            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.single_definition, "\"terminal\"");

            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.syntax_rule, "rulename=identifier;");
            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.syntax, "rulename=identifier;");

            var rules = GrammarAnalyzer.EBNF.Ebnf.syntax(new ParseInput<char>("rulename=\"abc\",\"def\";"));
            var analysis = GrammarAnalyzer.EBNF.Ebnf.ParseRule("rulename", rules.Success.Value, new ParseInput<char>("abcdef"));
            Assert.IsTrue(analysis.IsMatch);

            CheckMatch(GrammarAnalyzer.EBNF.Ebnf.syntax, "rule1=asdf;rule2=qwer");
        }

        [TestMethod]
        public void Linq()
        {
            Parser<char, char> p1 = Chars.Any;
            Parser<char, char> p2 = Chars.Any;
            Parser<char, char> p3 = Chars.Any;

            var combined = from r1 in p1 //where r1 != 'a' 
                           from r2 in p2 //where r2 != 'b'
                           //from r3 in p3 where r3 != 'c'
                           select new { first = r1, second = r2 };
        }
    }
}
