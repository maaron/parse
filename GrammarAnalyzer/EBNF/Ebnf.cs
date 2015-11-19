using System;
using System.Collections.Generic;
using System.Linq;
using Parse;
using Parse.Combinators;
using Parse.CharCombinators;
using Functional;

namespace GrammarAnalyzer.EBNF
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
            TerminalString,
            MetaIdentifier,
            Special,
            RepeatedSequence,
            OptionalSequence,
            GroupedSequence> Type { get; set; }
    }
    
    public struct TerminalString
    {
        public string Value { get; set; }
    }
    public class MetaIdentifier
    {
        public string Value { get; set; }
    }
    public class Special
    {
        public string Value { get; set; }
    }
    public class GroupedSequence
    {
        public FList<SingleDefinition> Definitions { get; set; }
    }
    public class RepeatedSequence
    {
        public FList<SingleDefinition> Definitions { get; set; }
    }
    public class OptionalSequence
    {
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

        private static Parser<char, string> Quote(Parser<char> c, char quote)
        {
            return c.Except(quote).ZeroOrMore().ReturnString().Between(quote);
        }

        static Ebnf()
        {
            var letter = Chars.Letter.Ignored();
            var digit = Chars.Digit.Ignored();
            var character = Chars.Any.Ignored();
            var ws = Chars.Space.Ignored().ZeroOrMore();
            var lbracket = ws.And(Chars.Const('[')).And(ws);
            var rbracket = ws.And(Chars.Const(']')).And(ws);
            var lbrace = ws.And(Chars.Const('{')).And(ws);
            var rbrace = ws.And(Chars.Const('}')).And(ws);
            var lparen = ws.And(Chars.Const('(')).And(ws);
            var rparen = ws.And(Chars.Const(')')).And(ws);
            var integer = digit.Repeat(1).ReturnString().Return(s => int.Parse(s));

            definitions_list = i => definitions_list_def(i);

            optional_sequence = definitions_list.Between(lbracket, rbracket);

            repeated_sequence = definitions_list.Between(lbrace, rbrace);

            grouped_sequence = definitions_list.Between(lparen, rparen);

            terminal_string = Quote(character, '"')
                .OrSame(Quote(character, '\''));

            meta_identifier = letter.And(letter.Or(digit).ZeroOrMore()).ReturnString();

            special_sequence = Quote(character, '?');
            
            primary =
                terminal_string.Return(s => new TerminalString() { Value = s })
                .Or(meta_identifier.Return(s => new MetaIdentifier() { Value = s }))
                .Or(special_sequence.Return(s => new Special() { Value = s }))
                .Or(repeated_sequence.Return(r => new RepeatedSequence() { Definitions = r }))
                .Or(optional_sequence.Return(r => new OptionalSequence() { Definitions = r }))
                .Or(grouped_sequence.Return(r => new GroupedSequence() { Definitions = r }))
                .Return(r => new Primary(){ Type = r });

            factor = integer.And('*').Optional().And(primary)
                .Return(r => new Factor()
                {
                    Repeat = r.Item1,
                    Primary = r.Item2
                });

            term = factor.And('-'.And(factor).Optional())
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

            syntax = syntax_rule.AtLeastMany(1);
        }

        private static Parser<char> Analyzed(
            string name, 
            AnalysisBuilder<char> analysis, 
            Parser<char> parser)
        {
            return input =>
            {
                analysis.PushFrame(name, input);
                var result = parser(input);
                analysis.PopFrame(result);
                return result;
            };
        }

        public static Analysis<char> ParseRule(
            string ruleName,  FList<SyntaxRule> rules,  IParseInput<char> input)
        {
            var analysis = new AnalysisBuilder<char>();
            var ruleTable = new Dictionary<string, Parser<char>>();
            foreach (var rule in rules) BuildRule(rule, analysis, ruleTable);
            ruleTable[ruleName](input);
            return analysis.ToAnalysis();
        }

        private static Parser<char> BuildRule(
            SyntaxRule rule, 
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            var parser = Analyzed(rule.Name, analysis,
                BuildDefinitions(rule.Definitions, analysis, rules));
            rules.Add(rule.Name, parser);
            return parser;
        }

        private static Parser<char> BuildDefinitions(
            FList<SingleDefinition> definitions,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            var parsers = definitions.Select(d => BuildDefinition(d, analysis, rules)).ToArray();
            return Combinator.AnyOf(parsers);
        }

        private static Parser<char> BuildDefinition(
            SingleDefinition definition,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            var parsers = definition.Terms.Select(t => BuildTerm(t, analysis, rules)).ToArray();
            return input =>
            {
                Result<char> result = null;
                if (parsers.Length == 0) throw new Exception("Expected non-empty list of terms");
                foreach (var p in parsers)
                {
                    result = p(input);
                    if (result.IsFailure) break;
                    input = result.Success.Remaining;
                }
                return result;
            };
        }

        private static Parser<char> BuildTerm(
            Term term,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            var factor = BuildFactor(term.Factor, analysis, rules);
            return term.Exception.Visit(
                () => factor,
                except => factor.Except(BuildFactor(except, analysis, rules)));
        }

        private static Parser<char> BuildFactor(
            Factor factor,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            var primary = BuildPrimary(factor.Primary, analysis, rules);
            return factor.Repeat.Visit(
                () => primary,
                repeat => primary.Repeat(repeat));
        }

        private static Parser<char> BuildPrimary(
            Primary primary,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            return primary.Type.Visit(
                terminal => BuildTerminalString(terminal, analysis, rules),
                meta => BuildMetaIdentifier(meta, analysis, rules),
                special => BuildSpecial(special, analysis, rules),
                repeated => BuildRepeated(repeated, analysis, rules),
                optional => BuildOptional(optional, analysis, rules),
                grouped => BuildGrouped(grouped, analysis, rules));
        }

        private static Parser<char> BuildTerminalString(
            TerminalString terminal,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            return Analyzed("terminal " + terminal.Value, analysis,
                Chars.String(terminal.Value));
        }

        private static Parser<char> BuildMetaIdentifier(
            MetaIdentifier identifier,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            return input =>
            {
                Parser<char> parser = null;
                if (rules.TryGetValue(identifier.Value, out parser))
                    return rules[identifier.Value](input);
                else
                    throw new Exception("Rule " + identifier.Value + " not defined");
            };
        }

        private static Parser<char> BuildSpecial(
            Special primary,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            throw new NotImplementedException();
        }

        private static Parser<char> BuildRepeated(
            RepeatedSequence repeated,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            var definitions = BuildDefinitions(repeated.Definitions, analysis, rules);
            return Analyzed("repeated sequence", analysis, definitions.ZeroOrMore());
        }

        private static Parser<char> BuildOptional(
            OptionalSequence optional,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            return Analyzed("optional sequence", analysis, 
                BuildDefinitions(optional.Definitions, analysis, rules)
                .Optional().Ignored());
        }

        private static Parser<char> BuildGrouped(
            GroupedSequence grouped,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            return Analyzed("grouped sequence", analysis, 
                BuildDefinitions(grouped.Definitions, analysis, rules));
        }
    }
}
