#pragma once

#include <AST/Operator.h>
#include <AST/expr/Expr.h>

namespace LANG::AST {

class UnaryExpr final : public Expr {
    public:
        UnaryExpr(::BASIC::SOURCE::SourceLocation loc, ::BASIC::SOURCE::SourceRange range, UnaryOperator op, ExprRef operand)
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