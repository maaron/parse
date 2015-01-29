
#pragma warning( disable : 4503 )

#include <cctype>
#include <string>

namespace parsing
{
    struct int32_decimal
    {
        int value;

        template <typename iter_t>
        bool parse(iter_t& start, iter_t& end)
        {
            iter_t tmp = start;
            iter_t::value_type c;
            value = 0;

            while (tmp != end && isdigit(c = *tmp++))
                value = value * 10 + (c & 0x0f);

            return (tmp > start);
        }
    };

    struct comma_delim
    {

        template <typename iter_t>
        bool parse(iter_t& start, iter_t& end)
        {

        }
    };

    struct a_and_b
    {
        int32_decimal a;
        int32_decimal b;

        template <typename iter_t>
        bool parse(iter_t& start, iter_t& end)
        {
            return seq(a, b);
        }
    };

    struct a_or_b
    {
        int32_decimal a, b;

        template <typename iter_t>
        bool parse(iter_t& start, iter_t& end)
        {
            return alt(a, b);
        }
    };

    struct a_star
    {
        int a;

        template <typename iter_t>
        bool parse(iter_t& start, iter_t& end)
        {
            return star(a);
        }
    };
}

int main()
{
    parsing::int32_decimal i;
    std::string data("23473498");
    
    bool valid = i.parse(data.begin(), data.end());
}
