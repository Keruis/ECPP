#include <lex/lex.h>

#include <config.h>
#include <basic/fail.h>
#include <lex/string_literal.h>
#include <lex/numeric_literal.h>
#include <basic/string_extras.h>
#include <basic/unicode/unicode_extras.h>

#include <format>

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

void Lexer::Advance(const BASIC::UTFChar c) noexcept {
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

void Lexer::AdvanceCursor(const std::size_t offset) noexcept {
    m_source.AdvanceBytes(offset);
}

void Lexer::AdvanceColumn(const std::size_t offset) noexcept {
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

void Lexer::UpdateToken(const TokenClass cls, const TokenType type, const std::u8string_view text = {}) noexcept {
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
        BASIC::Fail("Unterminated block comment: expected '*/' before end of input", m_location);
        
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

void Lexer::EqualsSuffixOperator(const BASIC::UTFChar c) noexcept {
    const bool equals_suffix = m_source.HasRemaining(2) && m_source.CurPtr()[1] == U'=';
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

void Lexer::RepeatedCharOperator(const BASIC::UTFChar c) noexcept {
    char32_t next_char = 0;
    if (m_source.HasRemaining(2))
        next_char = PeekNext().codepoint;
    
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

void Lexer::Punctuation(const BASIC::UTFChar c) noexcept {
    Advance(c);
    UpdateToken(
        TokenClass::PUNCTUATOR, 
        static_cast<TokenType>(c.codepoint)
    );
}

TokenType Lexer::FindKeyword(const std::u8string_view text) const noexcept {
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
    auto& str = m_tok.str;
    TokenType type = TokenType::LITERALS_STRING_VIEW;
    BASIC::UTFChar c{U'\"', 1};
    Advance(c);
    
    while (!AtEnd()) {
        c = Peek();
        if (BASIC::IsVerticalWhitespace(c.codepoint)) [[unlikely]]
            BASIC::Fail("Unescaped newline in string literal", m_location);

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
            if (AtEnd())
                BASIC::Fail("Unterminated escape sequence in string literal", m_location);
            StringLiteralParser par{m_source, m_location, m_tok.str};
            
            const std::size_t length = par.ParseEscape();
            AdvanceCursor(length);
            AdvanceColumn(length);
            continue;
        }
        
        if (type == TokenType::LITERALS_STRING)
            str.append(m_source.CurPtr(), c.length);
        Advance(c);
    }
    
    BASIC::Fail("Unterminated string literal: expected '\"' before end of input", m_location);
}

void Lexer::CharLiteral() {
    Advance({'\'', 1});
    if (AtEnd())
        BASIC::Fail("Unterminated character literal: expected a character before end of input", m_location);
    
    BASIC::UTFChar c = Peek();
    if (c.codepoint == U'\'')
        BASIC::Fail("Empty character literal: expected exactly one character", m_location);
    
    if (BASIC::IsVerticalWhitespace(c.codepoint))
        BASIC::Fail("Unescaped newline in character literal", m_location);
    
    if (c.codepoint == U'\\') {
        Advance(c);
        if (AtEnd())
            BASIC::Fail("Unterminated escape sequence in character literal", m_location);
        c = Peek();
        m_tok.character = StringLiteralParser::GetEscapeChar(c.codepoint, m_location);
    } else m_tok.character = c.codepoint;
    
    Advance(c);
    if (AtEnd())
        BASIC::Fail("Unterminated character literal: expected a closing quote before end of input", m_location);
    
    c = Peek();
    if (c.codepoint != U'\'')
        BASIC::Fail("Invalid character literal: expected a closing quote after exactly one character", m_location);
    
    Advance(c);
    UpdateToken(
        TokenClass::LITERAL,
        TokenType::LITERALS_CHARACTER
    );
}

void Lexer::NumericLiteral(const BASIC::UTFChar c) {
    m_tok.text = {};
    m_tok.suffix = 0;
    m_tok.type = TokenType::LITERALS_INT;

    NumericLiteralParser parser{
        m_source,
        m_location,
        m_tok.suffix,
        m_tok.type
    };

    const char8_t* const start = m_source.CurPtr();

    const std::size_t pp_number_length = parser.ScanPPNumber();
    if (!pp_number_length)
        BASIC::Fail("Invalid preprocessing number", m_location);

    const char8_t* const end = start + pp_number_length;

    m_source.RetreatBytes(pp_number_length);

    const auto radix = parser.DetectNumericRadix(c.codepoint);
    parser.ScanDigits(radix);

    m_tok.text = {start, m_source.CurPtr()};

    parser.ScanNumericSuffix();

    if (m_source.CurPtr() != end)
        BASIC::Fail("Invalid numeric literal: unexpected characters", m_location);

    AdvanceColumn(pp_number_length);
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
        case U'.':
            if (m_source.HasRemaining(2) && BASIC::IsDigit(PeekNext().codepoint, 10)) {
        case U'0' ... U'9':
            return NumericLiteral(c);
            }
            break;

        case U'"':
            return StringLiteral();
        
        case U'\'':
            return CharLiteral();

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
    
    if (m_source.HasRemaining(ScriptEndDelimiter.size())) {
        if (std::u8string_view{m_source.CurPtr(), ScriptEndDelimiter.size()} == ScriptEndDelimiter) {
            UpdateToken( 
                TokenClass::END,
                TokenType::END
            );
            m_length = 0;
            return;
        }
    }

    BASIC::Fail(std::format("Unexpected character U+{:04X}", static_cast<std::uint32_t>(c.codepoint)), m_location);
}

}