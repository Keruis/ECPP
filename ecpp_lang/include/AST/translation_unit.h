#pragma once

#include <AST/decl/decl.h>

#include <vector>

namespace ECPP::AST {

class TranslationUnit {
    public:
        using DeclarationList = std::vector<Decl*>;

        DeclarationList& Declarations() noexcept {
            return m_declarations;
        }

    private:
        DeclarationList m_declarations;
};

}