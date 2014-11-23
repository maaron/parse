#pragma once

#include "list.h"
#include <memory>
#include <vector>
#include <memory>

namespace parse
{

    namespace tree
    {

        // AST "spec" definitions.  These classes are used to describe the 
        // structure of an AST, but don't themselves have any data/function 
        // members.  Together with the iterator type, the spec completely 
        // defines an AST.

        // Branch spec.  This describes a binary tree structure with left 
        // and right branches.
        template <typename left_t, typename right_t> struct b {};

        // Leaf spec.  This describes a terminal node in a binary tree.  It 
        // can be a left/right piece of a branch spec.
        template <size_t i> struct l {};

        // Root spec.  This is the same as a leaf spec, but also specifies 
        // an additional spec type.  This allows a leaf to contain a 
        // sub-tree with indeces that are independent of the parent.
        template <size_t i, typename root_t> struct r {};

        // Dynamic root spec.  This is like the root spec, but indicates 
        // that the leaf contains a dynamic list of sub-trees of the given 
        // spec type.
        template <typename root_t> struct d {};

        // Reference spec.  This is similar to the root spec, except that 
        // the parser type is given, instead of an AST spec.  This is used 
        // when creating recursive parsers.  In those cases, the root spec 
        // can't be used because the parser type is be incomplete, and thus 
        // the associated AST type is not defined.
        template <typename parser_t> struct ref {};

        // Empty spec.  This is used to indicate when an AST is "void".  It 
        // may be possible to just replace the usage of e with void...
        struct e {};

        // This meta-function returns an AST type given an iterator and spec 
        //type.
        template <typename iterator_t, typename spec>
        struct from_spec;

        // This structure represents a possible match.  This is used by leaf 
        // nodes to indicate where in the input data a match occurred.
        template <typename iterator_t>
        struct match
        {
            iterator_t start, end;
            bool matched;

            match() : matched(false) {}
        };

        // This is similar to match, but used by root specs to create a type 
        // that includes match information and the sub-tree.
        template <typename iterator_t, typename base_t>
        struct root_match : base_t, match<iterator_t> {};

        template <typename t>
        struct always_false { enum { value = false }; };

        template <typename iterator_t, typename spec>
        struct ast
        {
            static_assert(always_false<iterator_t>::value, "Unknown AST specification");
        };

        template <typename iterator_t>
        struct ast<iterator_t, e>
        {
            static_assert(always_false<iterator_t>::value, "AST is empty");
        };

        // AST branch implementation.  This structure represents an AST 
        // composed of two branches that are themselves either additional 
        // branches or leaves.  The tree is composed using inheritance such 
        // that obtaining the left or right branch is just a cast.  All 
        // leaves of the tree are accessible via this class by calling the 
        // appropriate operator[] overload.
        template <typename iterator_t, typename left_t, typename right_t>
        struct ast<iterator_t, b<left_t, right_t> >
            : ast<iterator_t, left_t>, ast<iterator_t, right_t>
        {
            typedef ast<iterator_t, left_t> left_type;
            typedef ast<iterator_t, right_t> right_type;
            typedef ast<iterator_t, b<left_t, right_t> > self_type;
            typedef self_type value_type;

            // Meta-function that returns true if the tree contains a leaf 
            // with the specified index (either left or right branches).
            template <size_t i>
            struct has_key
            {
                static const bool value =
                left_type::template has_key<i>::type ||
                right_type::template has_key<i>::value;
            };

            // Meta-function that returns the type of the branch/leaf given 
            // its index.  If neither branch contains the index, void is returned.
            template <size_t i>
            struct get_leaf_type
            {
                typedef typename left_type::template get_leaf_type<i>::type left_base;
                typedef typename right_type::template get_leaf_type<i>::type right_base;
                typedef typename std::conditional<std::is_void<left_base>::value, right_base, left_base>::type type;
            };

            // Returns the leaf at the specified index, or throws a 
            // static_assert if the index doesn't exist.
            template <size_t i>
            typename get_leaf_type<i>::type::value_type& operator[] (const placeholders::index<i>& ph)
            {
                typedef typename get_leaf_type<i>::type leaf_type;
                static_assert(!std::is_void<leaf_type>::value, "Element index out of range");
                return static_cast<leaf_type&>(*this).value;
            }

            // Returns the left/right branch.
            right_type& right() { return static_cast<right_type&>(*this); }
            left_type& left() { return static_cast<left_type&>(*this); }
        };

        // AST leaf implementation.
        template <typename iterator_t, size_t i>
        struct ast<iterator_t, l<i> >
        {
            typedef ast<iterator_t, l<i> > self_t;
            typedef match<iterator_t> value_type;
            
            // The match structure associated with the leaf.
            value_type value;

            // Unique (within the AST tree) index of this leaf.
            static const size_t idx = i;

            // Meta-function that returns true if a leaf with the given 
            // index exists.
            template <size_t i>
            struct has_key
            {
                static const bool value = i == idx;
            };

            template <size_t i> struct get_leaf_type { typedef void type; };
            template <> struct get_leaf_type<idx> { typedef self_t type; };

            value_type& operator[] (const placeholders::index<idx>&)
            {
                return value;
            }
        };

        // AST root implementation.  This is very similar to the leaf 
        // implementation above, and has some identical members, as 
        // accessing a root or leaf from an AST tree works the same way.  
        // Normally, this could be factored out into a base class, but 
        // because inheritance is used to compose AST branches, this would 
        // introduce hidden data members.
        template <typename iterator_t, size_t i, typename root_t>
        struct ast<iterator_t, r<i, root_t> >
        {
            typedef ast<iterator_t, r<i, root_t> > self_t;
            typedef root_match<iterator_t, ast<iterator_t, root_t> > value_type;

            // Match and root structure associated with this root node.
            value_type value;

            static const size_t idx = i;

            template <size_t i>
            struct has_key
            {
                static const bool value = i == idx;
            };

            template <size_t i> struct get_leaf_type { typedef void type; };
            template <> struct get_leaf_type<idx> { typedef self_t type; };

            value_type& operator[] (const placeholders::index<idx>&)
            {
                return value;
            }
        };

        template <typename iterator_t, typename root_t>
        struct ast<iterator_t, d<root_t> >
        {
            ast<iterator_t, root_t> partial;
            std::vector<ast<iterator_t, root_t> > matches;
        };

        template <typename iterator_t, typename parser_t>
        struct ast<iterator_t, ref<parser_t> >
        {
            typedef typename from_spec<iterator_t, typename parser_t::ast_spec>::type ast_t;

            std::shared_ptr<ast_t> ptr;

            ast_t& get()
            {
                if (ptr.get() == nullptr) ptr.reset(new ast_t());
                return *ptr;
            }
        };

        template <size_t i, typename spec>
        struct make_leaf
        {
            typedef r<i, spec> type;
        };

        template <size_t i>
        struct make_leaf<i, e>
        {
            typedef l<i> type;
        };

        template <typename iterator_t>
        struct make_empty
        {
            typedef ast<iterator_t, e> type;
        };

        template <typename t>
        struct is_branch_or_leaf
        {
            static const bool value = false;
        };
        template <size_t i> struct is_branch_or_leaf<l<i> > { static const bool value = true; };
        template <size_t i, typename root_t> struct is_branch_or_leaf<r<i, root_t> > { static const bool value = true; };
        template <typename left_t, typename right_t> struct is_branch_or_leaf<b<left_t, right_t> > { static const bool value = true; };

        template <typename left_t, typename right_t>
        struct make_branch
        {
            static const bool left = is_branch_or_leaf<left_t>::value;
            static const bool right = is_branch_or_leaf<right_t>::value;

            typedef typename std::conditional<left,
                typename std::conditional<right, b<left_t, right_t>, left_t>::type,
                typename std::conditional<right, right_t, e>::type>::type type;
        };

        template <typename spec>
        struct make_dynamic
        {
            typedef d<spec> type;
        };

        template <>
        struct make_dynamic<e>
        {
            typedef e type;
        };

        template <typename parser_t>
        struct make_reference
        {
            typedef ref<parser_t> type;
        };

        template <typename iterator_t, typename spec>
        struct from_spec
        {
            typedef spec type;
        };

        template <typename iterator_t, typename left_t, typename right_t>
        struct from_spec<iterator_t, b<left_t, right_t> >
        {
            typedef ast<iterator_t, b<left_t, right_t> > type;
        };

        template <typename iterator_t, size_t i, typename root_t>
        struct from_spec<iterator_t, r<i, root_t> >
        {
            typedef ast<iterator_t, r<i, root_t> > type;
        };

        template <typename iterator_t, size_t i>
        struct from_spec<iterator_t, l<i> >
        {
            typedef ast<iterator_t, l<i> > type;
        };

        template <typename iterator_t, typename spec>
        struct from_spec<iterator_t, d<spec> >
        {
            typedef ast<iterator_t, d<spec> > type;
        };

        template <typename iterator_t>
        struct from_spec<iterator_t, e>
        {
            typedef void type;
        };

        template <typename iterator_t, typename ast_t>
        struct from_spec<iterator_t, ref<ast_t> >
        {
            typedef ast<iterator_t, ref<ast_t> > type;
        };

        template <typename spec> struct is_empty { static const bool value = false; };
        template <> struct is_empty<e> { static const bool value = true; };

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

        template <typename parser_t, typename stream_t>
		typename parser_t::template get_ast< typename stream_t::iterator >::type make_ast(parser_t& p, stream_t& s)
		{
			return typename parser_t::get_ast<typename stream_t::iterator>::type();
		}

	}

}