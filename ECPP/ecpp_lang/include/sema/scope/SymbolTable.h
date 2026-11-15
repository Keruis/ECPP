#pragma once

#include <sema/scope/Symbol.h>

#include <string_view>
#include <unordered_map>

namespace LANG::SEMA {

class SymbolTable {
    public:
        const Symbol* FindSymbol(std::u8string_view);
        void CreateSymbol(std::u8string_view, const Symbol&);

    private:
        std::unordered_map<std::u8string_view, Symbol> m_symbols;
};

}