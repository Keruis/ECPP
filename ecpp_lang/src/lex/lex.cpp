#include <cstddef>
#include <lex/lex.h>

#include <config.h>
#include <lex/string_literal.h>
#include <basic/string_extras.h>
#include <basic/unicode/unicode_extras.h>

namespace ECPP::LEXER {

const Token& Lexer::RefTok() const noexcept {
    return m_tok;
}

const BASIC::Location& Lexer::Location() const noexcept {
    return m_location;
}

bool Lexer::AtEnd() const noexcept {
    return m_source.AtEnd();
}

BASIC::UTFChar Lexer::Peek() const {
    return m_source.PeekAt(0);
}

BASIC::UTFChar Lexer::PeekNext() const {
    return m_source.PeekAt(1);
}

void Lexer::Advance(BASIC::UTFChar c) noexcept {
    AdvanceCursor(c.length);
    switch (c.codepoint) {
        case U'\n':
            if (m_last_was_cr)
                break;
        case U'\r':
            m_line_offset++;
            m_column_offset = 0;
            break;
        default:
            m_column_offset++;
    }
    m_last_was_cr = c.codepoint == U'\r';
}

void Lexer::AdvanceCursor(std::size_t offset) noexcept {
    m_source.AdvanceBytes(offset);
}

void Lexer::AdvanceColumn(std::size_t offset) noexcept {
    m_column_offset += offset;
    m_last_was_cr = false;
}

void Lexer::UpdatePostion() noexcept {
    m_location.line += m_line_offset;
    m_location.column = m_line_offset 
        ? 1 + m_column_offset
        : m_location.column + m_column_offset;
    m_line_offset = 0;
    m_column_offset = 0;
}

void Lexer::UpdateToken(TokenClass cls, TokenType type, std::u8string_view text = {}) noexcept {
    m_tok.cls = cls;
    m_tok.type = type;
    m_tok.text = text;
    m_length = m_source.CurPtr() - m_start;
}

void Lexer::SkipLineComment(BASIC::UTFChar c) {
    do {
        Advance(c);
        c = Peek();
    } while (!AtEnd() && !BASIC::IsHorizontalWhitespace(c.codepoint)); 
    UpdatePostion();
}

void Lexer::SkipBlockComment(BASIC::UTFChar c1, BASIC::UTFChar c2) {
    bool closed{};
    while (!AtEnd()) {
        c1 = Peek();
        if (c1.codepoint == U'*' && (c2 = PeekNext(), c2.codepoint == U'/')) {
            Advance(c1);
            Advance(c2);
            closed = true;
            break;
        }
        Advance(c1);
    }
    if (!closed) [[unlikely]]
        throw "ECPP::PARSER::Lexer::SkipTrivia";
    UpdatePostion();
}

BASIC::UTFChar Lexer::SkipTrivia() {
    while (!AtEnd()) {
        BASIC::UTFChar c1 = Peek();
        if (BASIC::IsHorizontalWhitespace(c1.codepoint) || BASIC::IsVerticalWhitespace(c1.codepoint)) {
            Advance(c1);
            continue;
        }
        if (c1.codepoint != U'/')
            return c1;
        BASIC::UTFChar c2 = PeekNext();
        if (c2.codepoint == U'/') {
            Advance(c1);
            SkipLineComment(c2);
            continue;
        }
        if (c2.codepoint == U'*') {
            Advance(c1);
            Advance(c2);
            SkipBlockComment(c1, c2);
            continue;
        }
        return c1;
    }
    UpdatePostion();
    return {0, 0};
}

void Lexer::EqualsSuffixOperator(BASIC::UTFChar c) noexcept {
    const bool equals_suffix = ((m_source.EndPtr() - m_source.CurPtr()) >= 2) && m_source.CurPtr()[1] == U'=';
    const std::size_t offset = 1 + equals_suffix;
    AdvanceCursor(offset);
    AdvanceColumn(offset);
    UpdateToken(
        TokenClass::OPERATOR, 
        static_cast<TokenType>(
            c.codepoint + 
            equals_suffix * equals_suffix_operator_offset
        )
    );
}

void Lexer::RepeatedCharOperator(BASIC::UTFChar c) noexcept {
    char next_char = 0;
    if (m_source.EndPtr() - m_source.CurPtr() >= 2)
        next_char = m_source.CurPtr()[1];
    const bool repeat = next_char == c.codepoint;
    const bool equals_suffix = next_char == U'=';
    const std::size_t offset = 1 + (repeat || equals_suffix);
    AdvanceCursor(offset);
    AdvanceColumn(offset);
    UpdateToken(
        TokenClass::OPERATOR, 
        static_cast<TokenType>(
            c.codepoint + 
            repeat * repeated_char_operator_offset + 
            equals_suffix * equals_suffix_operator_offset
        )
    );
}

void Lexer::Punctuation(BASIC::UTFChar c) noexcept {
    Advance(c);
    UpdateToken(
        TokenClass::PUNCTUATOR, 
        static_cast<TokenType>(c.codepoint)
    );
}

TokenType Lexer::FindKeyword(std::u8string_view text) const noexcept {
    if (text == u8"if")
        return TokenType::KEYWORD_IF;
    if (text == u8"else")
        return TokenType::KEYWORD_ELSE;
    if (text == u8"true")
        return TokenType::KEYWORD_TRUE;
    if (text == u8"false")
        return TokenType::KEYWORD_FALSE;
    return TokenType::IDENTIFIER;
}

void Lexer::Identifier(BASIC::UTFChar c) {
    do {
        Advance(c);
        c = Peek();
    } while (c.codepoint == U'_' || BASIC::IsXidStart(c.codepoint) || BASIC::IsXidContinue(c.codepoint));
    std::u8string_view text{m_start, m_source.CurPtr()};
    auto type = FindKeyword(text);
    UpdateToken(
        type != TokenType::IDENTIFIER ? TokenClass::KEYWORD : TokenClass::IDENTIFIER, 
        type, 
        text
    );
}

void Lexer::StringLiteral() {
    auto& character = m_tok.character;
    auto& str = m_tok.str;
    TokenType type = TokenType::LITERALS_STRING_VIEW;
    BASIC::UTFChar c{U'\"', 1};
    Advance(c);
    while (!AtEnd()) {
        c = Peek();
        if (BASIC::IsVerticalWhitespace(c.codepoint)) [[unlikely]]
            throw "ECPP::PARSER::Lexer::StringLiteral_1";
        if (c.codepoint == U'"') {
            Advance(c);
            UpdateToken(
                TokenClass::LITERAL,
                type
            );
            if (type == TokenType::LITERALS_STRING_VIEW)
                m_tok.text = {m_start + 1, m_source.CurPtr() - 1};
            return;
        }
        if (c.codepoint == U'\\') {
            if (type == TokenType::LITERALS_STRING_VIEW)
                str.assign(m_start + 1, m_source.CurPtr());
            type = TokenType::LITERALS_STRING;
            Advance(c);
            StringLiteralParser par{m_source, m_tok.str};
            const std::size_t length = par.ParserEscape();
            AdvanceCursor(length);
            AdvanceColumn(length);
        }
        Advance(c);
    }
    throw "ECPP::PARSER::Lexer::StringLiteral_2";
}

bool Lexer::RadixPrefix() {
    if (AtEnd() || ((Peek().codepoint | 0x20) != U'x'))
        return false;
    AdvanceCursor(1);
    const std::size_t length = BASIC::ScanNumeric(m_source.CurPtr(), m_source.EndPtr(), 16);
    if (!length)
        throw "ECPP::PARSER::Lexer::RadixPrefix";
    AdvanceCursor(length);
    return true;
}

void Lexer::FloatLiteral(TokenType& type) {
    if (AtEnd())
        return;

    if (Peek().codepoint == U'.') {
        AdvanceCursor(1);
        AdvanceCursor(BASIC::ScanNumeric(m_source.CurPtr(), m_source.EndPtr(), 10));
        type = TokenType::LITERALS_FLOAT;
    }

    if (AtEnd() || ((Peek().codepoint | 0x20) != U'e'))
        return;

    AdvanceCursor(1);

    const char cp = Peek().codepoint;
    if (!AtEnd() && (cp == U'+' || cp == U'-'))
        AdvanceCursor(1);

    const char8_t* start = m_source.CurPtr();
    const std::size_t length = BASIC::ScanNumeric(start, m_source.EndPtr(), 10);
    if (!length)
        throw "ECPP::PARSER::Lexer::FloatLiteral";
    AdvanceCursor(length);
    type = TokenType::LITERALS_FLOAT;
}

void Lexer::NumericSuffix(TokenType& type) {
    m_tok.suffix = 0;
    BASIC::UTFChar c = Peek();
    if (AtEnd())
        return;

    switch (c.codepoint) {
        case U'u': case U'U':
        case U'i': case U'I':
            if (type == TokenType::LITERALS_FLOAT)
                throw "ECPP::PARSER::Lexer::NumericSuffix";
            AdvanceCursor(1);
            type = TokenType::LITERALS_INT_SUF;
            break;
        case U'f': case U'F':
            AdvanceCursor(1);
            type = TokenType::LITERALS_FLOAT_SUF;
            break;
        default:
            if (c.codepoint == U'_' || BASIC::IsXidStart(c.codepoint) || BASIC::IsXidContinue(c.codepoint))
                throw "ECPP::PARSER::Lexer::NumericSuffix";
            return;
    }
    
    c = Peek();
    while (!AtEnd() && BASIC::IsDigit(c.codepoint, 10)) {
        const std::uint8_t digit = c.codepoint - U'0';
        if (m_tok.suffix > (255 - digit) / 10)
            throw "ECPP::PARSER::Lexer::NumericSuffix";
        m_tok.suffix = m_tok.suffix * 10 + digit;
        AdvanceCursor(1);
        c = Peek();
    }

    if (!UTILITY::IsAnyOf(m_tok.suffix, 8, 16, 32, 64))
        throw "ECPP::PARSER::Lexer::NumericSuffix";
}

void Lexer::NumericLiteral(BASIC::UTFChar c) {
    TokenType type = TokenType::LITERALS_INT;
    m_tok.suffix = 0;
    
    AdvanceCursor(1);
    const bool is_hex = c.codepoint == U'0' && RadixPrefix();
    if (!is_hex) {
        AdvanceCursor(BASIC::ScanNumeric(m_source.CurPtr(), m_source.EndPtr(), 10));
        FloatLiteral(type);
    }

    NumericSuffix(type);
    const std::size_t consumed = m_source.CurPtr() - m_start;
    const std::size_t suffix_length = m_tok.suffix == 0 ? 0 : (m_tok.suffix >= 10 ? 3 : 2);
    UpdateToken(
        TokenClass::LITERAL,
        type,
        {m_start, consumed - suffix_length}
    );
    AdvanceColumn(consumed);
}

void Lexer::NextToken() {
    UpdatePostion();
    BASIC::UTFChar c = SkipTrivia();
    UpdatePostion();
    if (AtEnd()) {
        UpdateToken( 
            TokenClass::UNEXPECTED_END,
            TokenType::END
        );
        m_length = 0;
        return;
    }

    m_start = m_source.CurPtr();

    switch (c.codepoint) {
        case U'0' ... U'9':
            return NumericLiteral(c);

        case U'"':
            return StringLiteral();

        case U'!': case U'=':
        case U'*': case U'/':
        case U'%':
            return EqualsSuffixOperator(c);

        case U'>': case U'<': 
        case U'+': case U'-':
            return RepeatedCharOperator(c);

        case U'(': case U')':
        case U'{': case U'}':
        case U'[': case U']':
        case U',': case U':':
        case U';':
            return Punctuation(c);

        default:
            if (c.codepoint == U'_' || BASIC::IsXidStart(c.codepoint))
                return Identifier(c);
    }
    if ((m_source.EndPtr() - m_source.CurPtr()) >= ScriptEndDelimiter.size()) {
        if (std::string_view{reinterpret_cast<const char*>(m_source.CurPtr()), ScriptEndDelimiter.size()} == ScriptEndDelimiter) {
            UpdateToken( 
                TokenClass::END,
                TokenType::END
            );
            m_length = 0;
            return;
        }
    }
    throw "ECPP::PARSER::Lexer::NextToken";
}

}