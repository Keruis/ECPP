#pragma once

#include <basic/Token.h>
#include <basic/source/SourceCursor.h>

namespace LANG::LEXER {

class Lexer {
    public:
        explicit Lexer(::BASIC::SOURCE::SourceCursor& cursor)
            : m_cursor(cursor)
        {}

        void NextToken();
        const BASIC::Token& RefToken() const noexcept;
        const ::BASIC::SOURCE::SourceRange RefRange() const noexcept;

    private:
        void UpdateToken(const BASIC::TokenKind, const BASIC::TokenType, const std::u8string_view) noexcept;

        BASIC::TokenType FindKeyword() const noexcept;

    private:
        ::BASIC::SOURCE::SourceCursor& m_cursor;
        BASIC::Token m_tok;
};
    
}