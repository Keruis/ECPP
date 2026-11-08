#pragma once

#include <AST/expr/expr.h>

#include <string>
#include <cstdint>
#include <variant>
#include <string_view>

namespace ECPP::AST {

enum class NumericType {
    INTEGER,
    FLOAT_POINT
};

enum class NumericSuffixType {
    NONE,
    UNSIGNED,
    SIGNED,
    FLOAT_POINT
};

struct NumericLiteralData {
    NumericType type;
    NumericSuffixType s_type;
    std::uint8_t width;
    std::u8string_view data;
};

struct StringLiteralData {
    std::pmr::u8string data;

    StringLiteralData(std::u8string_view value, std::pmr::memory_resource* resource)
        : data(value, std::pmr::polymorphic_allocator<char8_t>{resource})
    {}
};

struct StringViewLiteralData {
    std::u8string_view data;
};

struct BooleanLiteralData {
    bool data;
};

using LiteralValue = std::variant<
    NumericLiteralData,
    StringLiteralData,
    StringViewLiteralData,
    BooleanLiteralData
>;

class LiteralExpr final : public Expr {
    public:
        LiteralExpr(BASIC::Location loc, BASIC::SourceRange range, LiteralValue&& value)
            : Expr(loc, range), m_data(std::move(value))
        {}

        ExprKind Kind() const noexcept override {
            return ExprKind::LITERAL_EXPR;
        }

        const LiteralValue& Value() const noexcept {
            return m_data;
        }

    private:
        LiteralValue m_data;
};

}