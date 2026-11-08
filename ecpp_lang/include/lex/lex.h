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
        const BASIC::Location Location() const noexcept;

    private:
        bool AtEnd() const noexcept;
        BASIC::UTFChar Peek() const;
        BASIC::UTFChar PeekNext() const;
        void Advance(const BASIC::UTFChar) noexcept;
        void AdvanceCursor(const std::size_t) noexcept;
        void AdvanceColumn(const std::size_t) noexcept;
        void UpdatePostion() noexcept;
        void UpdateToken(const TokenClass, const TokenType, const std::u8string_view) noexcept;

        void SkipLineComment(BASIC::UTFChar);
        void SkipBlockComment(BASIC::UTFChar, BASIC::UTFChar);
        BASIC::UTFChar SkipTrivia();
        void EqualsSuffixOperator(const BASIC::UTFChar) noexcept;
        void RepeatedCharOperator(const BASIC::UTFChar) noexcept;
        void Punctuation(const BASIC::UTFChar) noexcept;
        void Identifier(BASIC::UTFChar);
        TokenType FindKeyword(const std::u8string_view) const noexcept;
        void StringLiteral();
        void CharLiteral();
        void NumericLiteral(const BASIC::UTFChar);

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