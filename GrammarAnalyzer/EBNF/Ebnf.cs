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

            // The following is the first part of the formal grammar from the 
            // EBNF spec.  However, it isn't done this way because it isn't 
            // quite clear how this can be fed to the next part of the 
            // grammar.
#if false
            var concatenate_symbol = Chars.Const(',');
            var defining_symbol = Chars.Const('=');
            var definition_separator_symbol = Chars.AnyOf("|/!").Ignored();
            var end_comment_symbol = Chars.String("*)");
            var end_group_symbol = Chars.Const(')');
            var end_option_symbol = ']'.Or("/)");
            var end_repeat_symbol = '}'.Or(":)");
            var except_symbol = Chars.Const('-');
            var first_quote_symbol = Chars.Const('\'');
            var repetition_symbol = Chars.Const('*');
            var second_quote_symbol = Chars.Const('"');
            var special_sequence_symbol = Chars.Const('?');
            var start_comment_symbol = Chars.String("(*");
            var start_group_symbol = Chars.Const('(');
            var start_option_symbol = '['.Or("(/");
            var start_repeat_symbol = '{'.Or("(:");
            var terminator_symbol = ';'.Or('.');
            var other_character = Chars.AnyOf(" :+_%@&#$<>\\^`~").Ignored();
            var newline = "\r\n".Or('\r').Or('\n');
            var htab = Chars.Const('\t');
            var terminal_character = Combinator.AnyOf(
                letter,
                digit,
                concatenate_symbol,
                defining_symbol,
                definition_separator_symbol,
                end_comment_symbol,
                end_option_symbol,
                end_group_symbol,
                end_repeat_symbol,
                except_symbol,
                first_quote_symbol,
                repetition_symbol,
                second_quote_symbol,
                special_sequence_symbol,
                start_comment_symbol,
                start_group_symbol,
                start_option_symbol,
                start_repeat_symbol,
                terminator_symbol,
                other_character);

            var gap_free_symbol = terminator_symbol.Except(
                first_quote_symbol.Or(second_quote_symbol));

            var first_terminal_character = terminal_character.Except(
                first_quote_symbol);

            var second_terminal_character = terminal_character.Except(
                second_quote_symbol);

            var terminal_string =
                first_quote_symbol
                .And(first_terminal_character)
                .And(first_terminal_character.ZeroOrMore())
                .And(first_quote_symbol)
                .Or(second_quote_symbol
                    .And(second_terminal_character)
                    .And(second_terminal_character.ZeroOrMore())
                    .And(second_quote_symbol));

            var gap_separator = Chars.Space.Ignored();

            var syntax = gap_separator.ZeroOrMore().And(
                gap_free_symbol.And(gap_separator.ZeroOrMore()).AtLeastMany(1));
#endif
            
            // Old, informal implementation
            var ws = Chars.Space.Ignored().ZeroOrMore();
            var lbracket = Chars.Const('[');
            var rbracket = Chars.Const(']');
            var lbrace = Chars.Const('{');
            var rbrace = Chars.Const('}');
            var lparen = Chars.Const('(');
            var rparen = Chars.Const(')');
            var star = '*';
            var dash = '-';
            var integer = ws.And(digit.Many(1)).ReturnString().Return(s => int.Parse(s));

            definitions_list = i => definitions_list_def(i);

            optional_sequence = definitions_list.Between(lbracket, rbracket);

            repeated_sequence = definitions_list.Between(lbrace, rbrace);

            grouped_sequence = definitions_list.Between(lparen, rparen);

            terminal_string = Quote(character, '"')
                .OrSame(Quote(character, '\''));

            var meta_identifier_word = letter.And(letter.Or(digit).ZeroOrMore()).ReturnString();
            meta_identifier = meta_identifier_word.And(ws).Many(1).Return(l => String.Join(" ", l));

            special_sequence = Quote(character, '?');
            
            primary =
                terminal_string.Return(s => new TerminalString() { Value = s })
                .Or(meta_identifier.Return(s => new MetaIdentifier() { Value = s }))
                .Or(special_sequence.Return(s => new Special() { Value = s }))
                .Or(repeated_sequence.Return(r => new RepeatedSequence() { Definitions = r }))
                .Or(optional_sequence.Return(r => new OptionalSequence() { Definitions = r }))
                .Or(grouped_sequence.Return(r => new GroupedSequence() { Definitions = r }))
                .Return(r => new Primary(){ Type = r });

            factor = integer.And(ws).And(star).Optional().And(ws).And(primary)
                .Return(r => new Factor()
                {
                    Repeat = r.Item1,
                    Primary = r.Item2
                });

            term = factor.And(ws.And(dash).And(ws).And(factor).Optional())
                .Return(r => new Term()
                {
                    Factor = r.Item1,
                    Exception = r.Item2
                });

            single_definition = ws.And(term).SplitBy(Chars.Const(','))
                .Return(r => new SingleDefinition()
                {
                    Terms = r
                });

            definitions_list_def = single_definition.SplitBy(ws.And(Chars.Const('|')));

            syntax_rule = 
                ws.And(meta_identifier)
                .And(ws).And('=')
                .And(definitions_list)
                .And(ws).And(';')
                .Return(r => new SyntaxRule()
                {
                    Name = r.Item1,
                    Definitions = r.Item2
                });

            syntax = syntax_rule.Many(1);
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
            var parser = BuildDefinitions(rule.Definitions, analysis, rules)
                .Analyzed(rule.Name, analysis);

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
            return Combinator.Sequence(definition.Terms.Select(t => BuildTerm(t, analysis, rules)));
        }

        private static Parser<char> BuildTerm(
            Term term,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            var factor = BuildFactor(term.Factor, analysis, rules);
            return term.Exception.Map(
                () => factor,
                except => factor.Except(BuildFactor(except, analysis, rules)));
        }

        private static Parser<char> BuildFactor(
            Factor factor,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            var primary = BuildPrimary(factor.Primary, analysis, rules);
            return factor.Repeat.Map(
                () => primary,
                repeat => primary.Repeat(repeat));
        }

        private static Parser<char> BuildPrimary(
            Primary primary,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            return primary.Type.Map(
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
            return Chars.String(terminal.Value)
                .Analyzed("terminal " + terminal.Value, analysis);
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
            return definitions.ZeroOrMore()
                .Analyzed("repeated sequence", analysis);
        }

        private static Parser<char> BuildOptional(
            OptionalSequence optional,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            return BuildDefinitions(optional.Definitions, analysis, rules)
                .Optional().Ignored()
                .Analyzed("optional sequence", analysis);
        }

        private static Parser<char> BuildGrouped(
            GroupedSequence grouped,
            AnalysisBuilder<char> analysis,
            Dictionary<string, Parser<char>> rules)
        {
            return BuildDefinitions(grouped.Definitions, analysis, rules)
                .Analyzed("grouped sequence", analysis);
        }
    }
}
