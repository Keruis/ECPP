#pragma once

#include <AST/expr/Expr.h>
#include <AST/top_level/stmt/Stmt.h>

namespace LANG::AST {

class ExprStmt final : public Stmt {
    public:
        ExprStmt(::BASIC::SOURCE::SourceLocation loc, ::BASIC::SOURCE::SourceRange range, ExprRef expr)
            : Stmt(loc, range), m_expr(expr)
        {}

        StmtKind Kind() const noexcept override {
            return StmtKind::EXPR_STMT;
        }

        const ExprRef Expression() const noexcept {
            return m_expr;
        }
    
    private:
        ExprRef m_expr;
};

}