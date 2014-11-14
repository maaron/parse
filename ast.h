#pragma once

#include <type_traits>

namespace parse
{
    namespace tree2
    {
        // This namespace contains structures that can be composed into a 
        // description of an AST's structure.
        namespace node_types
        {
            template <size_t i>
            struct leaf
            {
                static const size_t index = i;
            };

            template <typename left_t, typename right_t>
            struct branch
            {
                left_t left;
                right_t right;
            };

            template <typename branch_t>
            struct sprout
            {
            };

            template <typename branch_t>
            struct dynamic
            {
            };
        }

        template <typename iterator_t>
        struct match
        {
            iterator_t start, end;
            bool matched;

            match() : matched(false) {}
        };

        template <typename iterator_t, typename root_t>
        struct ast
        {
        };

        template <typename iterator_t, typename left_t, typename right_t>
        struct ast<iterator_t, branch<left_t, right_t> >
            : ast<iterator_t, left_t>,
            ast<iterator_t, right_t>
        {
            typedef ast<iterator_t, left_t> left_type;
            typedef ast<iterator_t, right_t> right_type;
            typedef ast<iterator_t, branch<left_t, right_t> > self_type;

            template <size_t i>
            struct has_key
            {
                static const bool value =
                    left_type::template has_key<i>::type ||
                    right_type::template has_key<i>::value;
            };

            template <size_t i>
            struct get_leaf_type
            {
                typedef typename left_type::template get_leaf_type<i>::type left_base;
                typedef typename right_type::template get_leaf_type<i>::type right_base;
                typedef typename std::conditional<std::is_void<left_base>::value, right_base, left_base>::type type;
            };

            template <size_t i>
            typename get_leaf_type<i>::type::value_type& operator[] (const placeholders::index<i>& ph)
            {
                typedef typename get_leaf_type<i>::type leaf_type;
                static_assert(!std::is_void<leaf_type>::value, "Element index out of range");
                return leaf_type::value;
            }

            left_type& left() { return static_cast<left_type&>(*this); }
            right_type& right() { return static_cast<right_type&>(*this); }

            template <typename attach_t>
            struct join
            {
                typedef ast<iterator_t, branch<branch<left_t, right_t>, attach_t> > type;
            };
        };

        template <typename iterator_t, size_t i>
        struct ast<iterator_t, leaf<i> > : match<iterator_t>
        {
            typedef leaf<i, value_t> self_type;
            typedef value_t value_type;

            value_type value;

            static const size_t idx = i;

            template <size_t i>
            struct has_key
            {
                static const bool value = i == idx;
            };

            template <size_t i> struct get_leaf_type { typedef void type; };
            template <> struct get_leaf_type<idx> { typedef self_type type; };

            value_type& operator[] (const placeholders::index<idx>&)
            {
                return value;
            }

            template <typename attach_t>
            struct join
            {
                typedef ast<iterator_t, branch<leaf<i>, attach_t> > type;
            };
        };

    }
}