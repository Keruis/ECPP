#pragma once

#include <AST/top_level/stmt/Stmt.h>
#include <AST/top_level/decl/Decl.h>

#include <span>
#include <vector>

namespace LANG::AST {

class BlockStmt final : public Stmt {
    public:
        BlockStmt(::BASIC::SOURCE::SourceLocation loc, ::BASIC::SOURCE::SourceRange range, std::pmr::vector<StmtRef>&& stmts)
            : Stmt(loc, range), m_stmts(stmts)
        {}

        StmtKind Kind() const noexcept override {
            return StmtKind::BLOCK_STMT;
        }

        std::span<const StmtRef> Stmts() const noexcept {
            return m_stmts;
        }

    private:
        std::pmr::vector<StmtRef> m_stmts;
};

}