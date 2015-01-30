
#pragma warning( disable : 4503 )

#include <cctype>
#include <string>
#include <vector>
#include <stdint.h>
#include <assert.h>

#include "parsers.h"
#include "operators.h"

namespace parsing
{
    struct uint32_decimal : parser<uint32_decimal>
    {
        uint32_t value;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            stream_t tmp = s;
            stream_t::value_type c;
            value = 0;

            while (s && isdigit(c = *s))
            {
                value = value * 10 + (c & 0x0f);
                s++;
            }

            return (s > tmp);
        }
    };

    token<' '> space;
    token<','> comma;
    token<'('> lparen;
    token<')'> rparen;
    auto ws = *space;

    

    struct alpha_t : single<alpha_t>
    {
        bool match(int c)
        {
            return std::isalpha(c) != 0;
        }
    } alpha;

    template <typename parser_t>
    struct string_match : parser<string_match<parser_t> >
    {
        std::string text;
        parser_t p;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            auto start = s;
            if (p.parse(s))
            {
                text.assign(start.begin, s.begin);
                return true;
            }
            else return false;
        }
    };

    struct identifier : parser<identifier>
    {
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (+alpha).parse(s);
        }
    };

    struct s_expr;

    struct elem : parser<elem>
    {
        alt<uint32_decimal> number;
        alt<ref<s_expr> > sub_expr;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (ws >> (number | sub_expr) >> ws).parse(s);
        }
    };

    struct s_expr : parser<s_expr>
    {
        star_ast<elem> elems;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return (lparen >> elems >> rparen).parse(s);
        }
    };

    void dump_expr(const parsing::s_expr& e, int level = 0)
    {
        for (int i = 0; i < level; i++) printf(" ");
        printf("expr\n");
        level++;
        for (auto elem = e.elems.begin(); elem != e.elems.end(); elem++)
        {
            if (elem->number.matched)
            {
                for (int i = 0; i < level; i++) printf(" ");
                printf("%d\n", elem->number.value);
            }
            else
            {
                dump_expr(elem->sub_expr.get(), level);
            }
        }
    }
}

int main()
{
    std::string data("(1 (321 0 2) ((2) 3 4))");
    parsing::s_expr p;
    parsing::checked_iterator<std::string::iterator> s(data.begin(), data.end());
    
    bool valid = p.parse(s);

    if (valid)
    {
        parsing::dump_expr(p);
    }

    printf("sizeof(s_expr)=%d\n", sizeof(parsing::s_expr));
    printf("sizeof(elem)=%d\n", sizeof(parsing::elem));
    printf("sizeof(ref<s_expr>)=%d\n", sizeof(parsing::ref<parsing::s_expr>));
}
