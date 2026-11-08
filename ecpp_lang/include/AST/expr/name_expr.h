#pragma once

#include <AST/expr/expr.h>

#include <string_view>

namespace ECPP::AST {

class NameExpr final : public Expr {
    public:
        NameExpr(BASIC::Location loc, BASIC::SourceRange range, std::u8string_view name)
            : Expr(loc, range), m_name(name)
        {}

        ExprKind Kind() const noexcept override {
            return ExprKind::NAME_EXPR;
        }

        std::u8string_view Name() const noexcept {
            return m_name;
        }

    private:
        std::u8string_view m_name;
};

}