#pragma once

#include <AST/expr/Expr.h>
#include <AST/type/QualType.h>
#include <AST/top_level/decl/Decl.h>

#include <string_view>

namespace LANG::AST {

class VariableDecl final : public Decl {
    public:
        VariableDecl(BASIC::SOURCE::SourceLocation loc, BASIC::SOURCE::SourceRange range, std::u8string_view name, QualType type, ExprRef initializer)
            : Decl(loc, range), m_name(name), m_type(type), m_initializer(initializer)
        {}

        DeclKind Kind() const noexcept override {
            return DeclKind::VARIABLE_DECL;
        }

        std::u8string_view Name() const noexcept {
            return m_name;
        }

        QualType Type() const noexcept {
            return m_type;
        }

        const ExprRef Initializer() const noexcept {
            return m_initializer;
        }

        bool HasInitializer() const noexcept {
            return m_initializer;
        }

    private:
        std::u8string_view m_name;
        QualType m_type;
        ExprRef m_initializer;
};

}