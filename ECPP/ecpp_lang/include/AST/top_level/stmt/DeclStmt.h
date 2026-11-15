#pragma once

#include <AST/top_level/stmt/Stmt.h>
#include <AST/top_level/decl/Decl.h>

namespace LANG::AST {

class DeclStmt final : public Stmt {
    public:
        DeclStmt(::BASIC::SOURCE::SourceLocation loc, ::BASIC::SOURCE::SourceRange range, DeclRef decl)
            : Stmt(loc, range), m_decl(decl)
        {}

        StmtKind Kind() const noexcept override {
            return StmtKind::DECL_STMT;
        }

        const DeclRef Declaration() const noexcept {
            return m_decl;
        }

    private:
        DeclRef m_decl;
};

}