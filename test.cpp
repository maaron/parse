
#pragma warning( disable : 4503 )

#include <cctype>
#include <string>
#include <vector>
#include <stdint.h>

namespace parsing
{
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

    template <typename t1, typename t2, typename t3>
    seq3<t1, t2, t3> seq(t1& p1, t2& p2, t3& p3)
    {
        return seq3<t1, t2, t3>(p1, p2, p3);
    }

    struct uint32_decimal
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
    struct constant
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

    template <typename parser_t>
    struct star
    {
        std::vector<parser_t> matches;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            while (s)
            {
                matches.emplace_back(parser_t());
                if (!matches.back().parse(s))
                {
                    matches.pop_back();
                    break;
                }
            }
            return true;
        }
    };

    typedef star<constant<' '> > ws_t;
    ws_t ws;
    constant<','> comma;

    struct comma_delim_t
    {
        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return seq(ws, comma, ws).parse(s);
        }
    };

    comma_delim_t comma_delim;

    struct my_parser
    {
        uint32_decimal a;
        uint32_decimal b;

        template <typename stream_t>
        bool parse(stream_t& s)
        {
            return seq(a, comma_delim, b).parse(s);
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
}

int main()
{
    std::string data("123, 456");
    parsing::my_parser p;
    parsing::checked_iterator<std::string::iterator> s(data.begin(), data.end());
    
    bool valid = p.parse(s);
}
