#pragma once

#include <AST/operator.h>
#include <AST/expr/expr.h>

namespace ECPP::AST {

class BinnaryExpr : public Expr {
    public:
        BinnaryExpr(BASIC::Location loc, BASIC::SourceRange range, BinaryOperator op, ExprRef lhs, ExprRef rhs)
            : Expr(loc, range), m_op(op), m_lhs(lhs), m_rhs(rhs)
        {}

        ExprKind Kind() const noexcept override {
            return ExprKind::UNARY_EXPR;
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