#pragma once

#include <AST/operator.h>
#include <AST/expr/expr.h>

namespace ECPP::AST {

class UnaryExpr : public Expr {
    public:
        UnaryExpr(BASIC::Location loc, BASIC::SourceRange range, UnaryOperator op, ExprRef operand)
            : Expr(loc, range), m_op(op), m_operand(operand)
        {}

        ExprKind Kind() const noexcept override {
            return ExprKind::UNARY_EXPR;
        }

        UnaryOperator Op() const noexcept {
            return m_op;
        }

        const ExprRef Operand() const noexcept {
            return m_operand;
        }

    private:
        UnaryOperator m_op;
        ExprRef m_operand;
};

}