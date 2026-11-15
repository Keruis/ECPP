#pragma once

#include <AST/Operator.h>
#include <AST/expr/Expr.h>

namespace LANG::AST {

class BinaryExpr final : public Expr {
    public:
        BinaryExpr(::BASIC::SOURCE::SourceLocation loc, ::BASIC::SOURCE::SourceRange range, BinaryOperator op, ExprRef lhs, ExprRef rhs)
            : Expr(loc, range), m_op(op), m_lhs(lhs), m_rhs(rhs)
        {}

        ExprKind Kind() const noexcept override {
            return ExprKind::BINARY_EXPR;
        }

        BinaryOperator Op() const noexcept {
            return m_op;
        }

        const ExprRef Lhs() const noexcept {
            return m_lhs;
        }

        const ExprRef Rhs() const noexcept {
            return m_rhs;
        }

    private:
        BinaryOperator m_op;
        ExprRef m_lhs;
        ExprRef m_rhs;
};

}