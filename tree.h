#pragma once

#include "list.h"
#include <memory>
#include <vector>

namespace parse
{

    namespace tree
    {
        // AST Structure:
        // branch contains:
        //   left branch/leaf
        //   right branch/leaf
        // leaf contains:
        //   ID that is unique within the tree
        //   match information (start, end, matched)
        
        //   optional meta-function taking an iterator type to another type (which may contain additional AST's)
        // root contains:
        //   iterator type
        //   branch/leaf

        // Meta Functions:
        //   make_root(iterator, leaf) - creates a new AST with a single leaf
        //   make_root(iterator, branch) - creates a new AST with a single branch
        //   join(iterator, leaf/branch, leaf/branch) - creates a new AST by joining the specified branches/leaves.

        // AST structure definitions.  These types are used to describe the 
        // structure of an AST.  They don't contain the AST data themselves, 
        // in order to limit the size of the template instanciation.  In 
        // this light, these can be seen as a compressed version of the real 
        // AST type.  The compression is essentially due to the iterator 
        // type being factored out.
        //   b: branch (contains a left and right node)
        //   l: leaf
        //   r: leaf with a value that is another AST
        //   d: leaf with a value that is a vector of another AST
        //   e: empty AST
        template <typename left_t, typename right_t> struct b {};
        template <size_t i> struct l {};
        template <size_t i, typename root_t> struct r {};
        template <size_t i, typename root_t> struct d {};
        struct e {};

        // This structure represents a possible match.
        template <typename iterator_t>
        struct match
        {
            iterator_t start, end;
            bool matched;

            match() : matched(false) {}
        };

        // A leaf node in an AST.  All leaf node types (ast's with l, r and 
        // d) can use this as a base implementation.
        template <size_t i, typename derived_t>
        struct leaf
        {
            static const size_t idx = i;

            template <size_t i>
            struct has_key
            {
                static const bool value = i == idx;
            };

            template <size_t i> struct get_leaf_type { typedef void type; };
            template <> struct get_leaf_type<idx> { typedef derived_t type; };

            derived_t& operator[] (const placeholders::index<idx>&)
            {
                return static_cast<derived_t&>(*this);
            }
        };

        template <typename t>
        struct always_false { enum { value = false }; };

        template <typename iterator_t, typename root_t>
        struct ast
        {
            static_assert(always_false<iterator_t>::value, "oops");
        };

        template <typename iterator_t, typename left_t, typename right_t>
        struct ast<iterator_t, b<left_t, right_t> >
            : ast<iterator_t, left_t>, ast<iterator_t, right_t>
        {
            typedef ast<iterator_t, left_t> left_type;
            typedef ast<iterator_t, right_t> right_type;
            typedef ast<iterator_t, b<left_t, right_t> > self_type;

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
            typename get_leaf_type<i>::type& operator[] (const placeholders::index<i>& ph)
            {
                typedef typename get_leaf_type<i>::type leaf_type;
                static_assert(!std::is_void<leaf_type>::value, "Element index out of range");
                return static_cast<leaf_type&>(*this);
            }

            left_type& left() { return static_cast<left_type&>(*this); }
            right_type& right() { return static_cast<right_type&>(*this); }
        };

        template <typename iterator_t, size_t i>
        struct ast<iterator_t, l<i> >
            : leaf<i, ast<iterator_t, l<i> > >
        {
            match<iterator_t> match;
        };

        template <typename iterator_t, size_t i, typename root_t>
        struct ast<iterator_t, r<i, root_t> >
            : leaf<i, ast<iterator_t, r<i, root_t> > >
        {
            match<iterator_t> match;
            ast<iterator_t, root_t> root;
        };

        template <typename iterator_t, size_t i, typename root_t>
        struct ast<iterator_t, d<i, root_t> >
            : leaf<i, ast<iterator_t, d<i, root_t> > >
        {
            match<iterator_t> match;
            std::vector<ast<iterator_t, root_t> > roots;
        };

        // A branch is a container for two other branch/leaf nodes.
        template <typename left_t, typename right_t>
        struct branch
            : left_t, right_t
        {
            typedef left_t left_type;
            typedef right_t right_type;
            typedef branch<left_t, right_t> self_type;

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
        };

        template <size_t i, typename spec>
        struct make_root
        {
            typedef r<i, spec> type;
        };

        template <size_t i>
        struct make_root<i, e>
        {
            typedef l<i> type;
        };

        template <typename iterator_t>
        struct make_empty
        {
            typedef ast<iterator_t, e> type;
        };

        template <typename iterator_t, typename left_t, typename right_t>
        struct make_branch
        {
            typedef ast<iterator_t, b<left_t, right_t> > type;
        };

        template <typename iterator_t, typename right_t>
        struct make_branch<iterator_t, e, right_t>
        {
            typedef ast<iterator_t, right_t> type;
        };

        template <typename iterator_t, typename left_t>
        struct make_branch<iterator_t, left_t, e>
        {
            typedef ast<iterator_t, left_t> type;
        };

        template <typename iterator_t>
        struct make_branch<iterator_t, e, e>
        {
            typedef ast<iterator_t, e> type;
        };

        /*
        // This meta-function returns true if the supplied type is a branch 
        // or a leaf.
        template <typename t>
        struct is_tree { static const bool value = false; };

        template <typename t1, typename t2>
        struct is_tree<branch<t1, t2> > { static const bool value = true; };

        template <size_t i, typename t>
        struct is_tree<leaf<i, t> > { static const bool value = true; };

        // This meta-function returns a bool indicating whether the branch 
        // contains a given key.
        template <typename branch_t, size_t key>
        struct contains_key;

        template <typename left_t, typename right_t, size_t key>
        struct contains_key<branch<left_t, right_t>, key>
        {
            static const bool value = 
                contains_key<left_t, key>::value ||
                contains_key<right_t, key>::value;
        };
        template <size_t i, typename value_t, size_t key>
        struct contains_key<leaf<i, value_t>, key>
        {
            static const bool value = i == key;
        };

        // This meta-function is used to determine whether two AST's have 
        // common indeces.
        template <typename branch1_t, typename branch2_t>
        struct is_unique
        {
            static const bool value =
            is_unique<typename branch1_t::left_type, branch2_t>::value &&
            is_unique<typename branch1_t::right_type, branch2_t>::value;
        };
        template <size_t i, typename value_t, typename branch_t>
        struct is_unique<leaf<i, value_t>, branch_t>
        {
            static const bool value = !contains_key<branch_t, i>::value;
        };
        template <typename branch_t, size_t i, typename value_t>
        struct is_unique<branch_t, leaf<i, value_t> >
        {
            static const bool value = !contains_key<branch_t, i>::value;
        };
        template <size_t i1, typename value1_t, size_t i2, typename value2_t>
        struct is_unique<leaf<i1, value1_t>, leaf<i2, value2_t> >
        {
            static const bool value = i1 != i2;
        };

        // This meta-function creates a new AST type by joining to AST's 
        // into a branch.  It also verifies that the indeces are unique.
        template <typename branch1_t, typename branch2_t>
        struct join
        {
            static_assert(is_unique<branch1_t, branch2_t>::value, "Element indeces not unique.");
            typedef typename branch<branch1_t, branch2_t> type;
        };
        */

        template <typename parser_ast_t, typename iterator_t>
        struct repetition
        {
            typedef std::vector<parser_ast_t> container_type;
            container_type matches;
            parser_ast_t partial;
        };

        template <typename parser_t, typename iterator_t>
        struct optional
        {
            typename parser_t::template get_ast<iterator_t>::type option;
        };

        template <typename parser_t, typename iterator_t>
        class reference
        {
            typedef typename parser_t::template get_ast<iterator_t>::type parser_ast_type;
            
            std::shared_ptr<parser_ast_type> ptr;

        public:
            parser_ast_type& get()
            {
                if (ptr.get() == nullptr) { ptr.reset(new parser_ast_type()); }
                return *ptr;
            }
        };

        // This function looks for a terminal that didn't match at a given 
        // location.
        template <typename t1, typename iterator_t>
        iterator_t last_match(optional<t1, iterator_t>& opt)
        {
            assert(opt.parsed);
            return last_match(opt.option);
        }

        template <typename t1, typename iterator_t>
        iterator_t last_match(reference<t1, iterator_t>& ref)
        {
            assert(ref.parsed);
            return ref.ptr.get() == nullptr ? ref.start : last_match(*ref.ptr);
        }

        template <typename t1, typename iterator_t>
        iterator_t last_match(repetition<t1, iterator_t>& rep)
        {
            assert(rep.parsed);
            if (rep.partial.parsed) return last_match(rep.partial);
            else if (rep.matches.size() > 0) return last_match(rep.matches.back());
            else return rep.start;
        }
        /*
        template <typename branch_t>
        struct branch_iterator
        {
            typedef typename branch_iterator<typename branch_t::left_type>::type type;
        };

        template <size_t i, typename value_t>
        struct branch_iterator<leaf<i, value_t> >
        {
            typedef typename value_t::iterator type;
        };

        template <typename t1, typename t2>
        typename branch_iterator<t1>::type last_match(branch<t1, t2>& b)
        {
            return max(last_match(b.left()), last_match(b.right()));
        }

        template <size_t i, typename t1>
        typename branch_iterator<leaf<i, t1> >::type last_match(leaf<i, t1>& b)
        {
            return last_match(b.value);
        }
        
        template <typename iterator_t, typename base_t>
        iterator_t last_match(base<iterator_t, base_t>& b)
        {
            return b.matched ? b.end : b.start;
        }
        */
        template <typename parser_t, typename stream_t>
		typename parser_t::template get_ast< typename stream_t::iterator >::type make_ast(parser_t& p, stream_t& s)
		{
			return typename parser_t::get_ast<typename stream_t::iterator>::type();
		}

	}

}