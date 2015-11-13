using System;
using System.Collections.Generic;
using System.Linq;
using Parse.Extensions;
using Parse.Character;
using Functional;

namespace Parse.EBNF
{
    public struct SyntaxRule
    {
        public string Name { get; set; }
        public FList<SingleDefinition> Definitions { get; set; }
    }
    public struct SingleDefinition
    {
        public FList<Term> Terms { get; set; }
    }
    public struct Term
    {
        public Factor Factor { get; set; }
        public Maybe<Factor> Exception { get; set; }
    }
    public struct Factor
    {
        public Maybe<int> Repeat { get; set; }
        public Primary Primary { get; set; }
    }
    public struct Primary
    {
        public Variant<
            PrimaryTerminal,
            PrimaryNonTerminal> Type { get; set; }
    }
    public enum TerminalType { TerminalString, MetaIdentifier, Special }
    public struct PrimaryTerminal
    {
        public TerminalType Type { get; set; }
        public string Value { get; set; }
    }
    public enum NonTerminalType { Grouped, Repeated, Optional }
    public struct PrimaryNonTerminal
    {
        public NonTerminalType Type { get; set; }
        public FList<SingleDefinition> Definitions { get; set; }
    }

    public enum BinaryOp { Sequence, Alternate };

    public class Ebnf
    {
        public readonly static Parser<char, string> meta_identifier;
        public readonly static Parser<char, string> terminal_string;
        public readonly static Parser<char, string> special_sequence;
        public readonly static Parser<char, SyntaxRule> syntax_rule;
        public readonly static Parser<char, FList<SyntaxRule>> syntax;
        public readonly static Parser<char, FList<SingleDefinition>> grouped_sequence;
        public readonly static Parser<char, FList<SingleDefinition>> repeated_sequence;
        public readonly static Parser<char, FList<SingleDefinition>> optional_sequence;
        public readonly static Parser<char, FList<SingleDefinition>> definitions_list_def;
        public readonly static Parser<char, FList<SingleDefinition>> definitions_list;
        public readonly static Parser<char, SingleDefinition> single_definition;
        public readonly static Parser<char, Term> term;
        public readonly static Parser<char, Factor> factor;
        public readonly static Parser<char, Primary> primary;
        public readonly static Parser<char, PrimaryTerminal> primary_terminal;
        public readonly static Parser<char, PrimaryNonTerminal> primary_nonterminal;

        private static Parser<char, string> Quote(Parser<char> c, char quote)
        {
            return c.Except(quote).Repeated().ReturnString().Between(quote);
        }

        static Ebnf()
        {
            var letter = Chars.Letter.Ignored();
            var digit = Chars.Digit.Ignored();
            var character = Chars.Any.Ignored();
            var ws = Chars.Space.Ignored().Repeated(0);
            var lbracket = ws.And(Chars.Const('[')).And(ws);
            var rbracket = ws.And(Chars.Const(']')).And(ws);
            var lbrace = ws.And(Chars.Const('{')).And(ws);
            var rbrace = ws.And(Chars.Const('}')).And(ws);
            var lparen = ws.And(Chars.Const('(')).And(ws);
            var rparen = ws.And(Chars.Const(')')).And(ws);
            var integer = digit.Repeated(1).ReturnString().Return(s => int.Parse(s));

            definitions_list = i => definitions_list_def(i);

            optional_sequence = definitions_list.Between(lbracket, rbracket);

            repeated_sequence = definitions_list.Between(lbrace, rbrace);

            grouped_sequence = definitions_list.Between(lparen, rparen);

            terminal_string = Quote(character, '"')
                .OrSame(Quote(character, '\''));

            meta_identifier = letter.And(letter.Or(digit).Repeated()).ReturnString();

            special_sequence = Quote(character, '?');
            
            primary_nonterminal =
                repeated_sequence.Return(r => new PrimaryNonTerminal()
                {
                    Type = NonTerminalType.Repeated,
                    Definitions = r
                })
                .OrSame(optional_sequence.Return(r => new PrimaryNonTerminal()
                {
                    Type = NonTerminalType.Optional,
                    Definitions = r
                }))
                .OrSame(grouped_sequence.Return(r => new PrimaryNonTerminal()
                {
                    Type = NonTerminalType.Grouped,
                    Definitions = r
                }));

            primary_terminal =
                terminal_string.Return(s => new PrimaryTerminal()
                {
                    Type = TerminalType.TerminalString,
                    Value = s
                })
                .OrSame(meta_identifier.Return(s => new PrimaryTerminal()
                {
                    Type = TerminalType.MetaIdentifier,
                    Value = s
                }))
                .OrSame(special_sequence.Return(s => new PrimaryTerminal()
                {
                    Type = TerminalType.Special,
                    Value = s
                }));

            primary = primary_terminal.Or(primary_nonterminal)
                .Return(r => r.Visit(
                    terminal => new Primary() { Type = terminal },
                    nonterminal => new Primary() { Type = nonterminal }));

            factor = Combinators.Optional(integer.And('*')).And(primary)
                .Return(r => new Factor()
                {
                    Repeat = r.Item1,
                    Primary = r.Item2
                });

            term = factor.And(Combinators.Optional('-'.And(factor)))
                .Return(r => new Term()
                {
                    Factor = r.Item1,
                    Exception = r.Item2
                });

            single_definition = term.SplitBy(Chars.Const(','))
                .Return(r => new SingleDefinition()
                {
                    Terms = r
                });

            definitions_list_def = single_definition.SplitBy(Chars.Const('|'));

            syntax_rule = meta_identifier.And('=').And(definitions_list).And(';')
                .Return(r => new SyntaxRule()
                {
                    Name = r.Item1,
                    Definitions = r.Item2
                });

            syntax = syntax_rule.Repeated(1);
        }
    }
}
