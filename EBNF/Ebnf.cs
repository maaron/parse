using System;
using System.Collections.Generic;
using System.Linq;
using Parse.Extensions;
using Parse.Character;
using Functional;

namespace Parse.EBNF
{
    public class IExpr
    {
    };
    public class Identifier : IExpr
    {
        public string Name { get; private set; }
        public Identifier(string name)
        {
            Name = name;
        }
    }
    public class Terminal : IExpr
    {
        public string Identifier { get; private set; }
        public Terminal(string identifier)
        {
            Identifier = identifier;
        }
    };
    public class BinaryExpression : IExpr
    {
        public IExpr Left { get; private set; }
        public IExpr Right { get; private set; }

        public BinaryExpression(IExpr left, IExpr right)
        {
            Left = left;
            Right = right;
        }
    }
    public class UnaryExpression : IExpr
    {
        public IExpr Expr { get; private set; }

        public UnaryExpression(IExpr expr)
        {
            Expr = expr;
        }
    }
    public class BracketExpr : UnaryExpression
    {
        public BracketExpr(IExpr expr) : base(expr)
        {
        }
    };
    public class ParenExpr : UnaryExpression
    {
        public ParenExpr(IExpr expr) : base(expr)
        {
        }
    };
    public class BraceExpr : UnaryExpression
    {
        public BraceExpr(IExpr expr) : base(expr)
        {
        }
    };
    public class PipeExpr : BinaryExpression
    {
        public PipeExpr(IExpr left, IExpr right) 
            : base(left, right)
        {
        }
    };
    public class CommaExpr : BinaryExpression
    {
        public CommaExpr(IExpr left, IExpr right)
            : base(left, right)
        {
        }
    };

    public class Ebnf
    {
        private static Parser<char, Ebnf> parser;

        static Ebnf()
        {
            /*
                letter = "A" | "B" | "C" | "D" | "E" | "F" | "G"
                       | "H" | "I" | "J" | "K" | "L" | "M" | "N"
                       | "O" | "P" | "Q" | "R" | "S" | "T" | "U"
                       | "V" | "W" | "X" | "Y" | "Z" ;
                digit = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;
                symbol = "[" | "]" | "{" | "}" | "(" | ")" | "<" | ">"
                       | "'" | '"' | "=" | "|" | "." | "," | ";" ;
                character = letter | digit | symbol | "_" ;
 
                identifier = letter , { letter | digit | "_" } ;
                terminal = "'" , character , { character } , "'" 
                         | '"' , character , { character } , '"' ;
 
                lhs = identifier ;
                rhs = identifier
                     | terminal
                     | "[" , rhs , "]"
                     | "{" , rhs , "}"
                     | "(" , rhs , ")"
                     | rhs , "|" , rhs
                     | rhs , "," , rhs ;

                rule = lhs , "=" , rhs , ";" ;
                grammar = { rule } ;
             */

            var letter = Chars.Letter.Ignored();
            var digit = Chars.Digit.Ignored();
            var symbol = Chars.AnyOf("[]{}()<>'\"=|.,;").Ignored();
            var character = letter.Or(digit).Or(symbol).Or('_');
            var identifier = letter.And(letter.Or(digit).Or('_')).ReturnString();
            var terminal = character.Repeated(1).ReturnString().Between('"');
            var lhs = identifier;
            Parser<char, IExpr> rhs = null;
            Parser<char, IExpr> rhsRef = i => rhs(i);
            var special = Chars.Any.Except('?').Between('?');

            rhs = identifier.Return(r => new Identifier(r) as IExpr)
                .OrSame('{'.And(rhsRef).And('}').Return(r => new BraceExpr(r) as IExpr))
                .OrSame('['.And(rhsRef).And(']').Return(r => new BracketExpr(r) as IExpr))
                .OrSame('('.And(rhsRef).And(')').Return(r => new ParenExpr(r) as IExpr))
                .OrSame(rhsRef.And('|').And(rhsRef).Return(r => new PipeExpr(r.Item1, r.Item2) as IExpr))
                .OrSame(rhsRef.And(',').And(rhsRef).Return(r => new CommaExpr(r.Item1, r.Item2) as IExpr))
                .OrSame(terminal.Return(r => new Terminal(r) as IExpr));

            var rule = lhs.And('=').And(rhs).And(';');
            var grammar = rule.Repeated();
        }

        public Dictionary<string, IExpr> Productions { get; private set; }
    }
}
