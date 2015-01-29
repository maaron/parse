#include "parse.h"

namespace parse
{

    // This meta-function returns the specified "t" type if non-void, and 
    // default_t otherwise.
    template <typename t, typename default_t>
    struct void_default { typedef t type; };
    template <typename default_t>
    struct void_default<void, default_t> { typedef default_t type; };

    // This parser matches a delimited list.  It works very similar to a common
    // string split algorithm.  It matches as many elem_t's as possible, with
    // each one preceeding a delim_t.  The last element may not preceed a
    // delimiter, but it will be consumed if present.  elem_t is the sub-parser
    // for an individual element, and delim_t is the parser for the delimiter.
    template <typename elem_t, typename delim_t>
    struct delimited_list : parser<delimited_list<elem_t, delim_t> >
    {
        typedef captured_parser<elem_t, 0> captured_elem_type;

        typedef typename tree::make_dynamic<typename elem_t::ast_spec>::type ast_spec;

        template <typename iterator_t, typename ast_t>
        static bool parse_internal(iterator_t& start, iterator_t& end, ast_t& ast)
        {
            while (true)
            {
                tree::from_spec<iterator_t, typename elem_t::ast_spec>::type elem_ast;
                if (!elem_t::parse_from(start, end, elem_ast)) break;
                ast.matches.push_back(elem_ast);
                if (!delim_t::parse_from(start, end)) break;
            }
            return true;
        }

        template <typename iterator_t>
        static bool parse_internal(iterator_t& start, iterator_t& end)
        {
            while (true)
            {
                if (!elem_t::parse_from(start, end)) break;
                if (!delim_t::parse_from(start, end)) break;
            }
            return true;
        }
    };

    namespace operators
    {
        // This is a function that is convenient to use in a parser 
        // expression for specifying a delimited list.  The Boost.Spirit 
        // equivalent (I think) is the '%' operator.
        template <typename elem_t, typename delim_t>
        delimited_list<elem_t, delim_t> operator%(const elem_t&, const delim_t&)
        {
            return delimited_list<elem_t, delim_t>();
        }
    }

}