using System;
using System.Collections.Generic;
using System.Linq;
using Parse;
using Parse.Combinators;
using Parse.CharCombinators;
using Functional;
using System.Globalization;
using System.Diagnostics;

namespace GrammarAnalyzer
{
    using Element = Variant<
        Abnf.Rulename,
        Abnf.Group,
        Abnf.Option,
        Abnf.CharVal,
        Abnf.TerminalSpec,
        Abnf.ProseVal>;

    public class Abnf
    {
        public struct TerminalSpec
        {
            public Variant<FList<int>, Tuple<int, int>> SequenceOrRange { get; set; }
        }

        public struct Repeat
        {
            public Variant<Tuple<Maybe<int>, Maybe<int>>, int> RangeOrCount { get; set; }
        }

        public struct ProseVal
        {
            public string Value { get; set; }
        }

        public struct CharVal
        {
            public string Value { get; set; }
        }

        public struct Repetition
        {
            public Maybe<Repeat> Repeat { get; set; }
            public Element Element { get; set; }
        }

        public struct Alternation
        {
            public FList<Repetition> Repetitions { get; set; }
        }

        public struct Rulename
        {
            public string Value { get; set; }
        }

        public struct Group
        {
            public FList<Alternation> Alternations { get; set; }
        }

        public struct Option
        {
            public FList<Alternation> Alternations { get; set; }
        }

        public struct Rule
        {
            public string Name { get; set; }
            public bool IsAdditional { get; set; }
            public FList<Alternation> Alternations { get; set; }
        }

        public static Parser<char, FList<Rule>> syntax;

        static Abnf()
        {
            var ALPHA = Chars.Letter.Ignored();
            var BIT = Chars.AnyOf("01").Ignored();
            var CHAR = Chars.Any.Except('\0').Ignored();
            var CRLF = Chars.String("\r\n").Or('\r').Or('\n');
            var CTL = Chars.Control.Ignored();
            var DIGIT = Chars.Digit.Ignored();
            var DQUOTE = Chars.Const('"');
            var HEXDIG = Chars.AnyOf("0123456789ABCDEFabcdef").Ignored();
            var HTAB = Chars.Const('\t');
            var SP = Chars.Const(' ');
            var WSP = SP.Or(HTAB);
            var LWSP = WSP.Or(CRLF.And(WSP)).ZeroOrMore();
            var OCTET = Chars.Any.Ignored();
            var VCHAR = Chars.Any.Except(Chars.Control).Ignored();

            var comment = ';'.And(WSP.Or(VCHAR).ZeroOrMore()).And(CRLF);

            var c_nl = comment.Or(CRLF);

            var c_wsp = WSP.Or(c_nl.And(WSP));

            var c_wsps = c_wsp.ZeroOrMore();

            var dec_num = DIGIT.Many(1).ReturnInt();

            var repeat = dec_num.Optional().And('*').And(dec_num.Optional()).Or(dec_num)
                .Return(r => new Repeat() { RangeOrCount = r });

            Parser<char, FList<Alternation>> alternationDef = null;
            Parser<char, FList<Alternation>> alternation = i => alternationDef(i);

            var group = alternation.Between(c_wsp.ZeroOrMore()).Between('(', ')')
                .Return(r => new Group() { Alternations = r });

            var option = alternation.Between(c_wsp.ZeroOrMore()).Between('[', ']')
                .Return(r => new Option() { Alternations = r });

            var char_val = VCHAR.Except('"').Many(1).ReturnString().Between(DQUOTE)
                .Return(r => new CharVal(){ Value = r });

            // This is quite a monstronsity, but abstracts the following pattern in the ABNF spec:
            // 
            // bin-val = "b" 1*BIT [ 1*("." 1*BIT) / ("-" 1*BIT) ]
            // 
            // For example, "b0110", "b0110.1010.111.00001110", "b10-0111001", 
            // etc.  The semantic result is either a list of numbers, or a 
            // "start, end" pair describing a range.  The same pattern is 
            // used for the dec-val and hex-val rules.  The first parameter is
            // the character prefix, i.e., 'b', 'd' or 'x', and the second
            // parameter is a parser that returns an individual number in the
            // associated base.
            Func<char, Parser<char, int>, Parser<char, TerminalSpec>> val = (prefix, num) =>
                prefix
                .And(num)
                .And('.'.And(num).Many(1)
                    .Or('-'.And(num)).Optional())
                .Return(tuple => new TerminalSpec()
                {
                    SequenceOrRange = tuple.Item2.Map(
                        () => new Variant<FList<int>,Tuple<int,int>>(FList.Create(tuple.Item1)),
                        some => some.Map(
                            list => 
                            {
                                list.Insert(0, tuple.Item1);
                                return new Variant<FList<int>,Tuple<int,int>>(list);
                            },
                            single => new Variant<FList<int>,Tuple<int,int>>(Tuple.Create(tuple.Item1, single))))
                });

            var bin_num = BIT.ZeroOrMore().ReturnString().Return(
                s => s.Aggregate(0, 
                    (accum, i) => (accum << 1) + (i == '1' ? 1 : 0)));

            var bin_val = val('b', bin_num);

            var dec_val = val('d', dec_num);

            var hex_num = HEXDIG.Many(1).ReturnString().Return(
                s => int.Parse(s, NumberStyles.AllowHexSpecifier));

            var hex_val = val('x', hex_num);

            var num_val = '%'.And(Combinator.AnyOf(bin_val, dec_val, hex_val));

            var prose_val = Chars.Any.Except(Chars.AnyOf("<>")).Ignored().ZeroOrMore().ReturnString().Between('<', '>')
                .Return(r => new ProseVal(){ Value = r });

            var rulename = ALPHA.And(ALPHA.Or(DIGIT).Or('-').ZeroOrMore())
                .ReturnString().OnMatch((s) => Trace.TraceInformation("rulename ({0})", s));

            var element = rulename.Return(r => new Rulename() { Value = r })
                .Or(group)
                .Or(option)
                .Or(char_val)
                .Or(num_val)
                .Or(prose_val);

            var repetition = repeat.Optional().And(element)
                .Return(r => new Repetition()
                {
                    Repeat = r.Item1,
                    Element = r.Item2
                });

            var concatenation = repetition.And(c_wsps).Many(1)
                .Return(r => new Alternation(){ Repetitions = r });
            
            alternationDef = concatenation.SplitBy(c_wsps.And('/').And(c_wsps));

            var elements = alternation.And(c_wsps);

            var definedOp = Chars.String("=/").Return(() => true)
                .OrSame(Chars.Const('=').Return(() => false));

            var defined_as = c_wsps.And(definedOp).And(c_wsps);
            
            var rule = rulename.And(defined_as).And(elements).And(c_nl)
                .Return(r => new Rule()
                {
                    Name = r.Item1,
                    IsAdditional = r.Item2,
                    Alternations = r.Item3
                });
            
            syntax = rule.Or(c_wsps.And(c_nl)).Many(1)
                .Return(r => FList.Create(from rl in r where rl.IsValid select rl.Value));
        }

        private static Parser<char> BuildAlternations(
            FList<Alternation> alternations,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            Func<Element, Parser<char>> BuildElement = element => 
            {
                return element.Map(
                    rulename => rules[rulename.Value],
                    group => BuildAlternations(group.Alternations, analysis, rules),
                    option => BuildAlternations(option.Alternations, analysis, rules).Optional().Ignored(),
                    charval => Chars.String(charval.Value),
                    terminal => terminal.SequenceOrRange.Map(
                        seq => Combinator.Sequence(seq.Select(c => Chars.Const((char)c)).ToArray()),
                        range => Chars.Any.If(c => c >= range.Item1 && c <= range.Item2).Ignored()),
                    proseval => Chars.String(proseval.Value));
            };

            return Combinator.AnyOf(alternations.Select(
                alt => Combinator.Sequence(alt.Repetitions.Select(rep => 
                {
                    var elem = BuildElement(rep.Element);
                    return rep.Repeat.Map(
                        () => elem,
                        some => some.RangeOrCount.Map(
                            range => range.Item1.IsValid && range.Item2.IsValid ? elem.Many(range.Item1.Value, range.Item2.Value)
                                : range.Item1.IsValid ? elem.Many(range.Item1.Value)
                                : range.Item2.IsValid ? elem.AtMost(range.Item2.Value)
                                : elem.ZeroOrMore(),
                            count => elem.Repeat(count)));
                }))));
        }

        public static Analysis<char> ParseRule(
            string ruleName, FList<Rule> rules, IParseInput<char> input)
        {
            var analysis = new AnalysisBuilder<char>();
            var ruleTable = new Dictionary<string, Parser<char>>();
            
            foreach (var rule in rules)
                ruleTable.Add(rule.Name, 
                    BuildAlternations(rule.Alternations, analysis, ruleTable)
                        .Analyzed("rule " + rule.Name, analysis));
            
            ruleTable[ruleName](input);
            return analysis.ToAnalysis();
        }
    }
}
