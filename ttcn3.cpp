
#include "parsers.h"
#include "operators.h"

#pragma warning( disable : 4503 )

namespace ttcn3
{
    using namespace parsing;

    never noimp;

    token<'a'> a;
    token<'b'> b;
    token<'c'> c;
    token<'d'> d;
    token<'e'> e;
    token<'f'> f;
    token<'g'> g;
    token<'h'> h;
    token<'i'> i;
    token<'j'> j;
    token<'k'> k;
    token<'l'> l;
    token<'m'> m;
    token<'n'> n;
    token<'o'> o;
    token<'p'> p;
    token<'q'> q;
    token<'r'> r;
    token<'s'> s;
    token<'t'> t;
    token<'u'> u;
    token<'v'> v;
    token<'w'> w;
    token<'x'> x;
    token<'y'> y;
    token<'z'> z;
    token<'A'> A;
    token<'B'> B;
    token<'C'> C;
    token<'D'> D;
    token<'E'> E;
    token<'F'> F;
    token<'G'> G;
    token<'H'> H;
    token<'I'> I;
    token<'J'> J;
    token<'K'> K;
    token<'L'> L;
    token<'M'> M;
    token<'N'> N;
    token<'O'> O;
    token<'P'> P;
    token<'Q'> Q;
    token<'R'> R;
    token<'S'> S;
    token<'T'> T;
    token<'U'> U;
    token<'V'> V;
    token<'W'> W;
    token<'X'> X;
    token<'Y'> Y;
    token<'Z'> Z;

    token<'0'> zero;
    token<'1'> one;
    token<'2'> two;
    token<'3'> three;
    token<'4'> four;
    token<'5'> five;
    token<'6'> six;
    token<'7'> seven;
    token<'8'> eight;
    token<'9'> nine;

    token<';'> semi;
    token<','> comma;
    token<'_'> Underscore;
    token<'"'> dquote;
    token<'{'> lcurly;
    token<'}'> rcurly;
    token<'('> lparen;
    token<')'> rparen;
    token<'+'> plus;
    token<'-'> minus;
    token<'*'> star;
    token<'/'> fslash;
    token<'&'> amp;
    token<'<'> lt;
    token<'>'> gt;
    token<'='> eq;
    token<'!'> bang;
    token<'@'> at;
    token<']'> rbrace;
    token<'['> lbrace;
    token<'.'> dot;
    token<' '> space;
    
    keyword mod("mod");
    keyword rem("rem");
    keyword not4b("not4b");
    keyword xor4b("xor4b");
    keyword or4b("or4b");
    keyword and4b("and4b");

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

    never Primary; //OpCall | Value | (lparen >> SingleExpression >> rparen);
    typedef never CompoundExpression;
    struct AddOp
    {
        token<'+'> plus;
        token<'-'> minus;
        token<'&'> StringOp;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (plus | minus | StringOp).parse(s);
        }
    };
    struct MultiplyOp
    {
        token <'*'> star;
        token <'/'> fslash;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (star | fslash | mod | rem).parse(s);
        }
    };
    auto UnaryOp = plus | minus;
    auto RelOp = (lt >> eq) | (gt >> eq) | lt | gt;
    auto EqualOp = (eq >> eq) | (bang >> eq);
    auto ShiftOp = (lt >> lt) | (gt >> gt) | (lt >> at) | (at >> gt);
    struct UnaryExpression
    {
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (!UnaryOp >> Primary).parse(s);
        }
    };
    struct MulExpression
    {
        delimited_list<UnaryExpression, decltype(MultiplyOp)> terms;
        CompoundExpression compound;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (terms | compound).parse(s);
        }
    };
    typedef BinaryExpression<MulExpression, AddOp> AddExpression;
    typedef BitNotExpression = !not4b >> AddExpression;
    typedef BitAndExpression = BitNotExpression % and4b;
    typedef BitXorExpression = BitAndExpression % xor4b;
    typedef BitOrExpression = BitXorExpression % or4b;
    typedef BinaryExpression<BitOrExpression, ShiftOp> ShiftExpression;
    auto RelExpression = (ShiftExpression >> !(RelOp >> ShiftExpression)) | CompoundExpression;
    auto EqualExpression = RelExpression % EqualOp;
    auto NotExpression = !not_ws >> EqualExpression;
    auto AndExpression = NotExpression % and_ws;
    auto XorExpression = AndExpression % xor_ws;
    
    struct SingleExpression
    {
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (XorExpression % or).parse(s);
        }
    };

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
