#pragma once

#include <sema/scope/ScopeKind.h>
#include <sema/scope/SymbolTable.h>

namespace LANG::SEMA {

class Scope {
    public:
        explicit Scope(ScopeKind kind, std::size_t depth)
            : m_kind(kind), m_depth(depth)
        {}

        SymbolTable& Table() noexcept;
        ScopeKind Kind() const noexcept;
        std::size_t Depth() const noexcept;

    private:
        SymbolTable m_table;
        ScopeKind m_kind;
        std::size_t m_depth;
};

}