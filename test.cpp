
#pragma warning( disable : 4503 )

#include <cctype>
#include <string>
#include <vector>
#include <stdint.h>
#include <assert.h>

namespace parsing
{
    template <typename derived_t>
    struct parser
    {
        derived_t& derived()
        {
            return static_cast<derived_t&>(*this);
        }
    };

    template <typename parser_t>
    struct ref
    {
        parser_t* ptr;

        ref() : ptr(nullptr)
        {
        }

        ~ref()
        {
            if (ptr) delete ptr;
        }

        ref& operator=(const ref& rhs)
        {
            if (this != &rhs)
            {
                delete ptr;

                if (rhs.ptr)
                    ptr = new parser_t(*rhs.ptr);
                else
                    ptr = nullptr;
            }
            return *this;
        }

        ref& operator=(ref&& rhs)
        {
            ptr = rhs.ptr;
            rhs.ptr = nullptr;
        }

        ref(ref&& other)
        {
            ptr = other.ptr;
            other.ptr = nullptr;
        }

        ref(const ref& rhs)
            : ptr(nullptr)
        {
            if (rhs.ptr)
                ptr = new parser_t(*rhs.ptr);
        }

        parser_t& get()
        {
            assert(ptr);
            return *ptr;
        }

        const parser_t& get() const
        {
            assert(ptr);
            return *ptr;
        }

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            ptr = new parser_t();
            if (!ptr->parse(s))
            {
                delete ptr;
                ptr = nullptr;
                return false;
            }
            else return true;
        }
    };

    template <typename left_t, typename right_t>
    struct seq : parser<seq<left_t, right_t> >
    {
        left_t& left;
        right_t& right;

        seq(left_t& left, right_t& right)
            : left(left), right(right) {}

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            auto tmp = s;

            if (left.parse(s) && right.parse(s)) return true;
            else { s = tmp; return false; }
        }
    };

    template <typename left_t, typename right_t>
    seq<left_t, right_t> operator>>(parser<left_t>& left, parser<right_t>& right)
    {
        return seq<left_t, right_t>(left.derived(), right.derived());
    }

    template <typename left_t, typename right_t>
    struct alt : parser<alt<left_t, right_t> >
    {
        left_t& left;
        right_t& right;

        alt(left_t& left, right_t& right)
            : left(left), right(right) {}

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return left.parse(s) || right.parse(s);
        }
    };

    template <typename left_t, typename right_t>
    struct alt_ptr : parser<alt_ptr<left_t, right_t> >
    {
        left_t*& left;
        right_t*& right;

        alt_ptr(left_t*& left, right_t*& right)
            : left(left), right(right) {}

        template <typename stream_t, typename ptr_t>
        bool parse_ptr(stream_t& s, ptr_t*& ptr)
        {
            ptr = new ptr_t();
            if (!ptr->parse(s))
            {
                delete ptr;
                ptr = nullptr;
                return false;
            }
            else return true;
        }

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return 
                parse_ptr(s, left) || 
                parse_ptr(s, right);
        }
    };

    template <typename left_t, typename right_t>
    alt<left_t, right_t> operator|(parser<left_t>& left, parser<right_t>& right)
    {
        return alt<left_t, right_t>(left.derived(), right.derived());
    }

    template <typename left_t, typename right_t>
    alt_ptr<left_t, right_t> operator|(ref<left_t>& left, ref<right_t>& right)
    {
        return alt_ptr<left_t, right_t>(left.ptr, right.ptr);
    }

    template <typename t1, typename t2, typename t3>
    struct seq3
    {
        t1& p1;
        t2& p2;
        t3& p3;

        seq3(t1& p1, t2& p2, t3& p3)
            : p1(p1), p2(p2), p3(p3) {}

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            stream_t tmp = s;
            bool match =
                p1.parse(s) &&
                p2.parse(s) &&
                p3.parse(s);

            if (!match) s = tmp;
            return match;
        }
    };

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

    template <int c>
    struct constant : parser<constant<c> >
    {
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            if (s && *s == c)
            {
                s++;
                return true;
            }
            else return false;
        }
    };

    template <typename parser_t, typename container_t = std::vector<parser_t> >
    struct star : parser<star<parser_t, container_t> >
    {
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            while (s)
            {
                auto tmp = s;
                parser_t p;
                if (!p.parse(s) || tmp == s) break;
            }
            return true;
        }
    };

    template <typename parser_t, typename container_t = std::vector<parser_t> >
    struct star_ast : parser<star_ast<parser_t, container_t> >, container_t
    {
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            while (s)
            {
                auto tmp = s;
                push_back(parser_t());
                if (!back().parse(s))
                {
                    pop_back();
                    break;
                }
                else if (tmp == s)
                    break;
            }
            return true;
        }
    };

    template <typename parser_t>
    star<parser_t> operator*(parser<parser_t>& p)
    {
        return star<parser_t>();
    }

    template <typename parser_t, typename container_t = std::vector<parser_t> >
    struct plus : parser<plus<parser_t, container_t> >
    {
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            parser_t p;
            if (!p.parse(s)) return false;

            while (s)
            {
                auto tmp = s;
                if (!p.parse(s) || tmp == s) break;
            }
            return true;
        }
    };

    template <typename parser_t>
    plus<parser_t> operator+(parser<parser_t>& p)
    {
        return plus<parser_t>();
    }

    template <typename parser_t>
    struct opt_ast : parser<opt_ast<parser_t> >, parser_t
    {
        bool matched;

        opt_ast() : matched(false) {}

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return matched = parser_t::parse(s);
        }
    };

    constant<' '> space;
    constant<','> comma;
    constant<'('> lparen;
    constant<')'> rparen;
    auto ws = *space;

    template <typename single_t>
    struct single : parser<single<single_t> >
    {
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            if (s && static_cast<single_t*>(this)->match(*s))
            {
                s++; return true;
            }
            else return false;
        }
    };

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

    template <typename iter_t>
    struct checked_iterator
    {
        iter_t begin, end;
        typedef typename iter_t::value_type value_type;

        checked_iterator(iter_t& b, iter_t& e)
            : begin(b), end(e) {}

        checked_iterator(const checked_iterator& other)
            : begin(other.begin), end(other.end) {}

        operator bool() { return begin != end; }
        value_type& operator*() { return *begin; }
        checked_iterator& operator++() { begin++; return *this; }
        checked_iterator operator++(int) { checked_iterator tmp(*this); begin++; return tmp; }
        bool operator==(const checked_iterator& rhs) { return begin == rhs.begin; }
    };

    template <typename iter_t>
    bool operator<(const checked_iterator<iter_t>& lhs, const checked_iterator<iter_t>& rhs)
    {
        return lhs.begin < rhs.begin;
    }

    template <typename iter_t>
    bool operator>(const checked_iterator<iter_t>& lhs, const checked_iterator<iter_t>& rhs)
    {
        return lhs.begin > rhs.begin;
    }

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
        ref<uint32_decimal> number;
        ref<s_expr> sub_expr;

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
            if (elem->number.ptr)
            {
                for (int i = 0; i < level; i++) printf(" ");
                printf("%d\n", elem->number.get().value);
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
