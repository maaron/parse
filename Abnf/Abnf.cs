using System;
using System.Linq;
using Parse;
using Parse.Combinators;
using Parse.CharCombinators;
using Functional;
using System.Globalization;

namespace Parse.Abnf
{
    public class Abnf
    {
        struct Alternation
        {
        }

        struct Rulename
        {
            public string Value { get; private set; }
            public Rulename(string value) { Value = value; }
        }

        struct Group
        {
            public Alternation Alternation { get; private set; }
            public Group(Alternation value) { Alternation = value; }
        }

        struct Option
        {
            public Alternation Alternation { get; private set; }
            public Option(Alternation value) { Alternation = value; }
        }

        static Parser<char> ALPHA = Chars.Letter.Ignored();
        static Parser<char> BIT = Chars.AnyOf("01").Ignored();
        static Parser<char> CHAR = Chars.Any.Except('\0').Ignored();
        static Parser<char> CRLF = Chars.String("\r\n");
        static Parser<char> CTL = Chars.Control.Ignored();
        static Parser<char> DIGIT = Chars.Digit.Ignored();
        static Parser<char> DQUOTE = Chars.Const('"');
        static Parser<char> HEXDIG = Chars.AnyOf("0123456789ABCDEF").Ignored();
        static Parser<char> HTAB = Chars.Const('\t');
        static Parser<char> SP = Chars.Const(' ');
        static Parser<char> WSP = SP.Or(HTAB);
        static Parser<char> LWSP = WSP.Or(CRLF.And(WSP)).ZeroOrMore();
        static Parser<char> OCTET = Chars.Any.Ignored();
        static Parser<char> VCHAR = Chars.Any.Except(Chars.Control).Ignored();
        static Parser<char> comment = ';'.And(WSP.Or(VCHAR).ZeroOrMore()).And(CRLF);

        Parser<char, Abnf> parser;

        static Abnf()
        {
            var c_nl = comment.Or(CRLF);
            var c_wsp = WSP.Or(c_nl.And(WSP));
            var c_wsps = c_wsp.ZeroOrMore();
            var defined_as = c_wsps.And('='.Or("=/")).ReturnString().And(c_wsps);
            var num = DIGIT.AtLeastMany(1).ReturnInt();
            var repeat = num.And('*').And(num).Or(num);
            Parser<char, Alternation> alternationDef = null;
            Parser<char, Alternation> alternation = i => alternationDef(i);

            var rulename = ALPHA.And(ALPHA.Or(DIGIT).Or('-'))
                .ReturnString().Return(s => new Rulename(s));

            var group = alternation.Between(c_wsp.ZeroOrMore()).Between('(', ')')
                .Return(a => new Group(a));

            var option = alternation.Between(c_wsp.ZeroOrMore()).Between('[', ']')
                .Return(a => new Option(a));

            var char_val = VCHAR.Except('"').ReturnString().Between(DQUOTE);

            Func<char, Parser<char>, Parser<char, string>> val = (c, p) =>
                c
                .And(p.Repeat(1))
                .And(Extensions.Optional(
                    '.'.And(p.Repeat(1)).Repeat(1)
                    .Or('-'.And(p.Repeat(1))))).ReturnString();

            var bin_val = val('b', BIT).Return(
                s => s.Aggregate(0, (accum, i) => (accum << 1) + i == '1' ? 1 : 0));

            var dec_val = val('d', DIGIT).ReturnInt();

            var hex_val = val('x', HEXDIG).Return(
                s => int.Parse(s, NumberStyles.AllowHexSpecifier));

            var num_val = '%'.And(Combinator.AnyOf(bin_val, dec_val, hex_val));

            var prose_val = Chars.Any.Except(Chars.AnyOf("<>")).Ignored().ZeroOrMore().ReturnString().Between('<', '>');

            var element = rulename.Or(group).Or(option).Or(char_val).Or(num_val).Or(prose_val);
            var repetition = Extensions.Optional(repeat).And(element);
            var concatenation = repetition.SplitBy(c_wsp.Repeat(1));
            alternationDef = concatenation.SplitBy(c_wsps.And('/').And(c_wsps));
            var elements = alternation.And(c_wsp.ZeroOrMore());
            var rule = rulename.And(defined_as).And(elements).And(c_nl);
            var rulelist = rule.Repeat(1);
        }
    }
}
