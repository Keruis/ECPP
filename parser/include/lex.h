#pragma once

#include "utf8_view.h"
#include "token.h"

#include <cassert>

namespace ECPP::PARSER {

class Lexer {
    public:
        Lexer(std::string_view source)
            : m_source(source) 
        {}

        void NextToken();
        const Token& RefTok() const noexcept;

    private:
        bool AtEnd() const noexcept;
        BASIC::UTFChar Peek() const;
        BASIC::UTFChar PeekNext() const;
        void Advance(BASIC::UTFChar) noexcept;
        void AdvanceCursor(std::size_t) noexcept;
        void AdvanceColumn(std::size_t) noexcept;
        void UpdatePostion() noexcept;
        void UpdateToken(TokenClass, TokenType, std::string_view) noexcept;

        void SingleLineComment(BASIC::UTFChar);
        void MultiLineComment(BASIC::UTFChar, BASIC::UTFChar);
        BASIC::UTFChar SkipTrivia();
        void EqualsSuffixOperator(BASIC::UTFChar) noexcept;
        void RepeatedCharOperator(BASIC::UTFChar) noexcept;
        void Punctuation(BASIC::UTFChar) noexcept;
        void Identifier(BASIC::UTFChar);
        TokenType FindKeyword(std::string_view) const noexcept;
        void Escape(BASIC::UTFChar) noexcept;
        void StringLiteral();
        bool RadixPrefix();
        void FloatLiteral(TokenType&);
        void NumericSuffix(TokenType&);
        void NumericLiteral(BASIC::UTFChar);

    private:
        BASIC::UTF8View m_source;

        Token m_tok{};

        const char* m_start{};

        std::size_t m_line_offset{};
        std::size_t m_column_offset{};
        bool m_last_was_cr{};
};

}