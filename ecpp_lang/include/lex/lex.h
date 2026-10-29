#pragma once

#include <lex/token.h>
#include <basic/utf/utf_view.h>

namespace ECPP::LEXER {

class Lexer {
    public:
        Lexer(BASIC::UTFCursor<char8_t>& source, BASIC::Location& location)
            : m_source(source), m_location(location)
        {}

        void NextToken();
        const Token& RefTok() const noexcept;
        const BASIC::Location& Location() const noexcept;

    private:
        bool AtEnd() const noexcept;
        BASIC::UTFChar Peek() const;
        BASIC::UTFChar PeekNext() const;
        void Advance(BASIC::UTFChar) noexcept;
        void AdvanceCursor(std::size_t) noexcept;
        void AdvanceColumn(std::size_t) noexcept;
        void UpdatePostion() noexcept;
        void UpdateToken(TokenClass, TokenType, std::u8string_view) noexcept;

        void SkipLineComment(BASIC::UTFChar);
        void SkipBlockComment(BASIC::UTFChar, BASIC::UTFChar);
        BASIC::UTFChar SkipTrivia();
        void EqualsSuffixOperator(BASIC::UTFChar) noexcept;
        void RepeatedCharOperator(BASIC::UTFChar) noexcept;
        void Punctuation(BASIC::UTFChar) noexcept;
        void Identifier(BASIC::UTFChar);
        TokenType FindKeyword(std::u8string_view) const noexcept;
        void Escape(BASIC::UTFChar) noexcept;
        void StringLiteral();
        bool RadixPrefix();
        void FloatLiteral(TokenType&);
        void NumericSuffix(TokenType&);
        void NumericLiteral(BASIC::UTFChar);

    private:
        BASIC::UTFCursor<char8_t>& m_source;

        Token m_tok{};

        const char8_t* m_start{};

        std::size_t m_length{};
        std::size_t m_line_offset{};
        std::size_t m_column_offset{};

        BASIC::Location& m_location;

        bool m_last_was_cr{};
};

}