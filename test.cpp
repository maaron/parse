
#pragma warning( disable : 4503 )

#include "parse.h"
#include "delimited_list.h"
//#include "ast.h"

int main()
{
    using namespace parse;
    using namespace parse::operators;
    using namespace parse::terminals;

    auto a0 = terminals::u<'a'>();
    auto comma = terminals::u<','>();
    terminals::u<'+'> plus;
    terminals::u<'-'> minus;
    terminals::u<'.'> dot;
    terminals::u<' '> sp;
    terminals::u<'='> eq;
    terminals::u<'&'> amp;
    terminals::u<'$'> dollar;
    terminals::u<'*'> star;
    terminals::u<'@'> at;
    terminals::u<'!'> bang;
    terminals::u<'%'> pct;
    terminals::u<'/'> fslash;

    typedef decltype(a0[_0] % fslash) a1;
    typedef decltype(a1()[_0] % pct) a2;
    typedef decltype(a2()[_0] % bang) a3;
    typedef decltype(a3()[_0] % at) a4;
    typedef decltype(a4()[_0] % star) a5;
    typedef decltype(a5()[_0] % dollar) a6;
    typedef decltype(a6()[_0] % amp) a7;
    typedef decltype(a7()[_0] % eq) a8;
    typedef decltype(a8()[_0] % sp) a9;
    typedef decltype(a9()[_0] % dot) a10;
    typedef decltype(a10()[_0] % minus) a11;
    typedef decltype(a11()[_0] % plus) a12;
    typedef decltype(a12()[_0] % comma) a13;

    typedef a13 p;

    std::string data("a,a+a+a,a-a+a-a");
    auto ast = p::make_ast(data);
    auto begin = data.begin();
    auto end = data.end();
    bool valid = p::parse_from(begin, end, ast);
}
