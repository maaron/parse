// parse.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "parse.h"
#include <assert.h>

template <typename iterator_t>
struct fooa
{
    bool is_c;
};

struct foop : parse::parser<foop>
{
    typedef parse::tree::a<fooa> ast_spec;

    template <typename iterator_t>
    static bool parse_internal(iterator_t& start, iterator_t& end)
    {
        a.is_c = false;
        return start != end && *start++ == 'c';
    }

    template <typename iterator_t>
    static bool parse_internal(iterator_t& start, iterator_t& end, fooa<iterator_t>& a)
    {
        a.is_c = false;
        return start != end && *start++ == 'c';
    }
};

void test_custom()
{
    std::string data("c");
    fooa<std::string::iterator> ast;
    ast.is_c = true;
    assert(foop::parse_from(data.begin(), data.end(), ast));
    assert(!ast.is_c);

    data = "b";
    ast.is_c = true;
    assert(!foop::parse_from(data.begin(), data.end(), ast));
    assert(!ast.is_c);
}

void test_ast()
{
    using namespace parse;
    using namespace parse::operators;
    using namespace parse::terminals;

    u<'a'> a;

    std::string data("a");
    bool match = false;

    auto a_cap = a[_0];
    auto a_cap_ast = a_cap.make_ast(data);
    match = a_cap.parse_from(data.begin(), data.end(), a_cap_ast);

    auto a_cap_star = *a[_0];
    auto a_cap_star_ast = a_cap_star.make_ast(data);
    match = a_cap_star.parse_from(data.begin(), data.end(), a_cap_star_ast);

    auto a_cap_star_cap = (*a[_0])[_0];
    auto a_cap_star_cap_ast = a_cap_star_cap.make_ast(data);
    match = a_cap_star_cap.parse_from(data.begin(), data.end(), a_cap_star_cap_ast);

    auto a_cap_a_cap = a[_0] >> a[_1];
    auto a_cap_a_cap_ast = a_cap_a_cap.make_ast(data);
    match = a_cap_a_cap.parse_from(data.begin(), data.end(), a_cap_a_cap_ast);
    assert(a_cap_a_cap_ast[_0].matched && !a_cap_a_cap_ast[_1].matched);
}

template <typename parser_t>
typename parse::parser_ast<parser_t, std::string::iterator>::type test_parser(std::string data, parser_t& p)
{
    auto ast = p.make_ast(data);
    assert(p.parse_from(data.begin(), data.end(), ast));
    return ast;
}

void test_recursive()
{
    using namespace parse;
    using namespace parse::operators;
    using namespace parse::terminals;

    u<'a'> a;
    u<'('> lparen;
    u<')'> rparen;
    auto ws = +space();
    struct s_exp;

    auto term = !ws >> (a[_0] | reference<s_exp>()[_1]) >> !ws;
    typedef decltype(lparen >> (*term[_0])[_0] >> rparen) s_exp_t;

    struct s_exp : s_exp_t {};

    std::string data("( a (a a) a )");
    auto begin = data.begin();
    auto end = data.end();
    auto ast = s_exp::make_ast(data);
    auto match = s_exp::parse_from(begin, end, ast);
    auto& term_match = ast[_0].matches[1];
    assert(
        ast[_0].matches.size() == 3 && 
        ast[_0].matches[1][_0].matched && 
        !ast[_0].matches[1][_0][_0].matched &&
        ast[_0].matches[1][_0][_1].matched);
}

int _tmain(int argc, _TCHAR* argv[])
{
    using namespace parse;
    using namespace parse::terminals;
    using namespace parse::operators;
    
    test_ast();
    test_recursive();
    test_custom();

    u<'a'> a;

    auto ast = test_parser("a", a[_0]);
    assert(ast[_0].matched);

    assert(test_parser("aaa", +a[_0]).matches.size() == 3);

    assert(test_parser("aaa", +a[_0]).matches[2][_0].matched);

    return 0;
}
