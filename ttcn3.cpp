
#include "parsers.h"
#include "operators.h"

#pragma warning( disable : 4503 )

namespace ttcn3
{
    using namespace parsing;

    never noimp;

    typedef token<'a'> char_a;
    typedef token<'b'> char_b;
    typedef token<'c'> char_c;
    typedef token<'d'> char_d;
    typedef token<'e'> char_e;
    typedef token<'f'> char_f;
    typedef token<'g'> char_g;
    typedef token<'h'> char_h;
    typedef token<'i'> char_i;
    typedef token<'j'> char_j;
    typedef token<'k'> char_k;
    typedef token<'l'> char_l;
    typedef token<'m'> char_m;
    typedef token<'n'> char_n;
    typedef token<'o'> char_o;
    typedef token<'p'> char_p;
    typedef token<'q'> char_q;
    typedef token<'r'> char_r;
    typedef token<'s'> char_s;
    typedef token<'t'> char_t;
    typedef token<'u'> char_u;
    typedef token<'v'> char_v;
    typedef token<'w'> char_w;
    typedef token<'x'> char_x;
    typedef token<'y'> char_y;
    typedef token<'z'> char_z;
    typedef token<'A'> char_A;
    typedef token<'B'> char_B;
    typedef token<'C'> char_C;
    typedef token<'D'> char_D;
    typedef token<'E'> char_E;
    typedef token<'F'> char_F;
    typedef token<'G'> char_G;
    typedef token<'H'> char_H;
    typedef token<'I'> char_I;
    typedef token<'J'> char_J;
    typedef token<'K'> char_K;
    typedef token<'L'> char_L;
    typedef token<'M'> char_M;
    typedef token<'N'> char_N;
    typedef token<'O'> char_O;
    typedef token<'P'> char_P;
    typedef token<'Q'> char_Q;
    typedef token<'R'> char_R;
    typedef token<'S'> char_S;
    typedef token<'T'> char_T;
    typedef token<'U'> char_U;
    typedef token<'V'> char_V;
    typedef token<'W'> char_W;
    typedef token<'X'> char_X;
    typedef token<'Y'> char_Y;
    typedef token<'Z'> char_Z;

    typedef token<'0'> zero;
    typedef token<'1'> one;
    typedef token<'2'> two;
    typedef token<'3'> three;
    typedef token<'4'> four;
    typedef token<'5'> five;
    typedef token<'6'> six;
    typedef token<'7'> seven;
    typedef token<'8'> eight;
    typedef token<'9'> nine;

    typedef token<';'> semi;
    typedef token<','> comma;
    typedef token<'_'> Underscore;
    typedef token<'"'> dquote;
    typedef token<'{'> lcurly;
    typedef token<'}'> rcurly;
    typedef token<'('> lparen;
    typedef token<')'> rparen;
    typedef token<'+'> plus;
    typedef token<'-'> minus;
    typedef token<'*'> star;
    typedef token<'/'> fslash;
    typedef token<'&'> amp;
    typedef token<'<'> lt;
    typedef token<'>'> gt;
    typedef token<'='> eq;
    typedef token<'!'> bang;
    typedef token<'@'> at;
    typedef token<']'> rbrace;
    typedef token<'['> lbrace;
    typedef token<'.'> dot;
    typedef token<' '> space;

#define KEYWORD(name) struct keyword_##name : keyword { keyword_##name() : keyword(#name) {} };
    KEYWORD(not4b);
    KEYWORD(and4b);
    KEYWORD(xor4b);
    KEYWORD(or4b);
    
    auto NonZeroNum = one | two | three | four | five | six | seven | eight | nine;
    auto Num = zero | NonZeroNum;
    auto UpperAlpha = A | B | C | D | E | F | G | H | I | J | K | L | M | N | O | P | Q | R | S | T | U | V | W | X | Y | Z;
    auto LowerAlpha = a | b | c | d | e | f | g | h | i | j | k | l | m | n | o | p | q | r | s | t | u | v | w | x | y | z;
    auto Alpha = UpperAlpha | LowerAlpha;
    auto AlphaNum = Alpha | Num;
    auto Identifier = Alpha >> *(AlphaNum | Underscore);

    auto FreeText = dquote >> *((dquote >> dquote) | ~dquote) >> dquote;

    template <typename next_t, typename op_t>
    struct BinaryExpression
    {
        delimited_list<next_t, delim_t> terms;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return terms.parse(s);
        }
    };

    typedef never Primary; //OpCall | Value | (lparen >> SingleExpression >> rparen);
    typedef never CompoundExpression;
    struct AddOp
    {
        alt<'+'> plus;
        alt<'-'> minus;
        alt<'&'> StringOp;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (plus | minus | StringOp).parse(s);
        }
    };
    struct MultiplyOp
    {
        alt<'*'> star;
        alt<'/'> fslash;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (star | fslash | mod | rem).parse(s);
        }
    };
    struct UnaryOp 
    {
        alt<'+'> plus;
        alt<'-'> minus;
        
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (plus | minus).parse(s);
        }
    };
    struct RelOp
    {
        alt<less_equal> lte;
        alt<greater_equal> gte;
        alt<less> lt;
        alt<greater> gt;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (lte | gte | lt | gt).parse(s);
        }
    };
    struct EqualOp
    {
        alt<equal_equal> eq;
        alt<bang_equal> ne;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (eq | ne).parse(s);
        }
    };
    struct ShiftOp
    {
        alt<less_less> rshift;
        alt<greater_greater> lshift;
        alt<less_at> lshift_circ;
        alt<at_greater> rshift_circ;
        
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (rshift | lshift | lshift_circ | rshift_circ).parse(s);
        }
    };
    struct UnaryExpression
    {
        alt<UnaryOp> unaryOp;
        Primary primary;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (!unaryOp >> primary).parse(s);
        }
    };
    struct MulExpression
    {
        BinaryExpression<UnaryExpression, MultiplyOp> terms;
        CompoundExpression compound;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (terms | compound).parse(s);
        }
    };
    typedef BinaryExpression<MulExpression, AddOp> AddExpression;
    struct BitNotExpression
    {
        alt<keyword_not4b> not4b;
        AddExpression add;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (!not4b >> add).parse(s);
        }
    };
    typedef BinaryExpression<BitNotExpression, keyword_and4b> BitAndExpression;
    typedef BinaryExpression<BitAndExpression, keyword_xor4b> BitXorExpression;
    typedef BinaryExpression<BitXorExpression, keyword_or4b> BitOrExpression;
    typedef BinaryExpression<BitOrExpression, ShiftOp> ShiftExpression;
    struct RelExpression
    {
        alt<ShiftExpression> shiftExpr1;
        alt<RelOp> op;
        alt<ShiftExpression> shiftExpr2;
        alt<CompoundExpression> compoundExpr;
        
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return ((shiftExpr1 >> !(op >> shiftExpr2)) | compoundExpr).parse(s);
        }
    };
    typedef BinaryExpression<RelExpression, EqualOp> EqualExpression;
    struct NotExpression
    {
        alt<keyword_not> not;
        EqualExpression equalExpression;
        
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (!not >> equalExpression).parse(s);
        }
    };
    typedef BinaryExpression<NotExpression, keyword_and> AndExpression;
    typedef BinaryExpression<AndExpression, keyword_xor> XorExpression;    
    typedef BinaryExpression<XorExpression, keyword_or> SingleExpression;

#if 0
    auto LanguageKeyword = l | a | n | g | u | a | g | e;
    auto LanguageSpec = LanguageKeyword >> FreeText >> *(comma >> FreeText);
    auto TTCN3ModuleKeyword = m >> o >> d >> u >> l >> e;
    auto ModuleId = Identifier >> !LanguageSpec;
    auto PublicKeyword = p >> u >> b >> l >> i >> c;
    auto PrivateKeyword = p >> r >> i >> v >> a >> t >> e;
    auto FriendKeyword = f >> r >> i >> e >> n >> d;
    auto Visibility = PublicKeyword | PrivateKeyword | FriendKeyword;

    auto or_ws = !ws >> o >> r >> !ws;
    auto and_ws = !ws >> a >> n >> d >> !ws;
    auto xor_ws = !ws >> x >> o >> r >> !ws;
    auto not_ws = !ws >> n >> o >> t >> !ws;
    auto mod = !ws >> m >> o >> d >> !ws;
    auto rem = !ws >> r >> e >> m >> !ws;
    auto not4b = !ws >> n >> o >> t >> four >> b >> !ws;
    auto and4b = !ws >> a >> n >> d >> four >> b >> !ws;
    auto xor4b = !ws >> x >> o >> r >> four >> b >> !ws;
    auto or4b = !ws >> o >> r >> four >> b >> !ws;
    auto rbrace_ws = !ws >> rbrace >> !ws;
    auto lbrace_ws = !ws >> lbrace >> !ws;
    auto dotdot_ws = !ws >> dot >> dot >> !ws;
    //auto OpCall = ConfigurationOps | GetLocalVerdict | TimerOps | TestcaseInstance | (FunctionInstance >> !ExtendedFieldReference) | (TemplateOps !ExtendedFieldReference) | ActivateOp;
    auto Primary = noimp; //OpCall | Value | (lparen >> SingleExpression >> rparen);
    auto CompoundExpression = noimp;
    auto StringOp = amp;
    auto AddOp = !ws >> (plus | minus | StringOp) >> !ws;
    auto MultiplyOp = !ws >> (star | fslash | mod | rem) >> !ws;
    auto UnaryOp = plus | minus;
    auto RelOp = (lt >> eq) | (gt >> eq) | lt | gt;
    auto EqualOp = (eq >> eq) | (bang >> eq);
    auto ShiftOp = (lt >> lt) | (gt >> gt) | (lt >> at) | (at >> gt);
    auto UnaryExpression = !UnaryOp >> Primary;
    auto MulExpression = (UnaryExpression % MultiplyOp) | CompoundExpression;
    auto AddExpression = MulExpression % AddOp;
    auto BitNotExpression = !not4b >> AddExpression;
    auto BitAndExpression = BitNotExpression % and4b;
    auto BitXorExpression = BitAndExpression % xor4b;
    auto BitOrExpression = BitXorExpression % or4b;
    auto ShiftExpression = BitOrExpression % ShiftOp;
    auto RelExpression = (ShiftExpression >> !(RelOp >> ShiftExpression)) | CompoundExpression;
    auto EqualExpression = RelExpression % EqualOp;
    auto NotExpression = !not_ws >> EqualExpression;
    auto AndExpression = NotExpression % and_ws;
    auto XorExpression = AndExpression % xor_ws;
    auto SingleExpression = !ws >> (XorExpression % or_ws) >> !ws;


    auto TypeDefKeyword = !ws >> t >> y >> p >> e >> !ws;
    auto SubTypeDef = noimp;

    // This is simplified from the formal grammar- Type is really composed 
    // of either a PredefinedType (built-in charstring, integer, etc) or a 
    // ReferencedType, but this parses both just fine.  The processes of 
    // differentiating is up to the caller.
    auto Type = Identifier;
    auto NestedType = noimp;
    auto ArrayDef = lbrace_ws >> SingleExpression >> !(dotdot_ws >> SingleExpression) >> rbrace_ws;

    auto AddressKeyword = !ws >> a >> d >> d >> r >> e >> s >> s >> !ws;
    auto RecordKeyword = !ws >> r >> e >> c >> o >> r >> d >> !ws;
    auto OptionalKeyword = !ws >> o >> p >> t >> i >> o >> n >> a >> l >> !ws;
    auto SubTypeSpec = noimp;
    auto StructFieldDef = (Type[_0] | NestedType[_1]) >> Identifier[_2] >> !ArrayDef[_3] >> !SubTypeSpec[_4] >> !OptionalKeyword[_5];
    auto StructDefBody = (Identifier[_0] | AddressKeyword[_1]) >> lcurly_ws >> !(StructFieldDef[_0] % comma_ws)[_2] >> rcurly_ws;
    auto RecordDef = RecordKeyword >> StructDefBody;

    auto UnionDef = noimp;
    auto SetDef = noimp;
    auto RecordOfDef = noimp;
    auto SetOfDef = noimp;
    auto EnumDef = noimp;
    auto PortDef = noimp;
    auto ComponentDef = noimp;
    auto StructuredTypeDef = RecordDef[_0] | UnionDef | SetDef | RecordOfDef | SetOfDef | EnumDef | PortDef | ComponentDef;
    auto TypeDefBody = StructuredTypeDef[_0] | SubTypeDef[_1];
    auto TypeDef = TypeDefKeyword >> TypeDefBody;

    auto ConstDef = noimp;
    auto TemplateDef = noimp;
    auto ModuleParDef = noimp;
    auto FunctionDef = noimp;
    auto SignatureDef = noimp;
    auto TestcaseDef = noimp;
    auto AltstepDef = noimp;
    auto ImportDef = noimp;
    auto ExtFunctionDef = noimp;
    auto ExtConstDef = noimp;
    auto GroupDef = noimp;
    auto FriendModuleDef = noimp;
    auto WithStatement = noimp;

    auto ModuleDefinition = ((
        !Visibility >> (
        TypeDef[_0] |
        ConstDef |
        TemplateDef |
        ModuleParDef |
        FunctionDef |
        SignatureDef |
        TestcaseDef |
        AltstepDef |
        ImportDef |
        ExtFunctionDef |
        ExtConstDef)) |
        (!PublicKeyword >> GroupDef) |
        (!PrivateKeyword >> FriendModuleDef)) >> !WithStatement;

    auto ModuleControlPart = noimp;

    auto ModuleDefinitionsList = +(ModuleDefinition[_0] >> !semi_ws);
    auto TTCN3Module = TTCN3ModuleKeyword >> ws >> ModuleId[_0] >> lcurly_ws >> !ModuleDefinitionsList[_1] >> !ModuleControlPart >> rcurly_ws >> !WithStatement >> semi_ws;
#endif
}
