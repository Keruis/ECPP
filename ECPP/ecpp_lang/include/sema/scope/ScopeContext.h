#pragma once

#include <sema/scope/Scope.h>

#include <vector>

namespace LANG::SEMA {

class ScopeContext {
    public:
        ScopeContext();

        void CreateScope(ScopeKind);
        void DeleteScope();

        const Symbol* LookupSymbol(std::u8string_view);
        const Symbol* LookupLocalSymbol(std::u8string_view);

        void DeclareSymbol(std::u8string_view, const Symbol&);

    private:
        std::vector<Scope> m_scopes;
};
    
}