#pragma once

#include <AST/expr/expr.h>
#include <AST/stmt/stmt.h>

namespace ECPP::AST {

class ExprStmt : public Stmt {
    public:
        ExprStmt(BASIC::Location loc, BASIC::SourceRange range, ExprRef expr)
            : Stmt(loc, range), m_expr(expr)
        {}

        StmtKind Kind() const noexcept override {
            return StmtKind::EXPR_STMT;
        }

        const ExprRef Expr() const noexcept {
            return m_expr;
        }
    
    private:
        ExprRef m_expr;
};

}