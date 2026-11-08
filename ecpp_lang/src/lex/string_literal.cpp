#include <lex/string_literal.h>

#include <basic/fail.h>
#include <basic/char_extras.h>
#include <basic/string_extras.h>
#include <basic/unicode/unicode_extras.h>
#include <basic/unicode/unicode_name_to_codepoint.h>

namespace ECPP::LEXER {

char32_t StringLiteralParser::Peek() const {
    return m_source.PeekAt(0).codepoint;
}

char32_t StringLiteralParser::PeekNext() const {
    return m_source.PeekAt(1).codepoint;
}

void StringLiteralParser::AppendCodepoint(const char32_t cp) {
    if (!BASIC::Encode(cp, m_str)) [[unlikely]]
        BASIC::Fail("Invalid Unicode code point: cannot encode the value as UTF-8", m_loc);
}

std::size_t StringLiteralParser::GetEsacpeRadix(const char32_t cp) const noexcept {
    switch (cp) {
        case U'b': case U'B':
            return 2;
        case U'o': case U'O':
            return 8;
        case U'd': case U'D':
            return 10;
        case U'x': case U'X':
            return 16;
        default:
            return 0;
    }
}

char32_t StringLiteralParser::GetEscapeChar(const char32_t cp, const BASIC::Location loc) {
    /*
        simple-escape-sequence:
            one of
            \\ \' \" \a \e \E \f \n \r \t
    */
    switch (cp) {
        case U'\\': case U'\'':
        case U'\"':
            return cp;
        case U'a': return U'\a';
        case U'e':
        case U'E': return U'\e';
        case U'f': return U'\f';
        case U'n': return U'\n';
        case U'r': return U'\r';
        case U't': return U'\t';
        default: [[unlikely]]
            BASIC::Fail("Invalid escape sequence: unsupported escape character.", loc);
    }
}

std::size_t StringLiteralParser::ProcessNumericEscape(char32_t& cp, const std::size_t radix) {
    if ((m_source.HasRemaining(2)) && PeekNext() == U'{') {
        /*
            numeric-escape-sequence:
                binary-escape-sequence
                octal-escape-sequence
                decimal-escape-sequence
                hexadecimal-escape-sequence
                backspace-escape-sequence

            binary-escape-sequence:
                \b{ binary-digit-sequence }
                \B{ binary-digit-sequence }

            octal-escape-sequence:
                \o{ octal-digit-sequence }
                \O{ octal-digit-sequence }

            decimal-escape-sequence:
                \d{ decimal-digit-sequence }
                \D{ decimal-digit-sequence }

            hexadecimal-escape-sequence:
                \x{ hexadecimal-digit-sequence }
                \X{ hexadecimal-digit-sequence }

            backspace-escape-sequence:
                \b
        */
        const char8_t* start = m_source.CurPtr() + 2;
        const std::size_t num_length = BASIC::ScanNumeric(start, m_source.EndPtr(), radix);
        const char8_t* close = start + num_length;
        
        if (close >= m_source.EndPtr() || *close != U'}') [[unlikely]]
            BASIC::Fail("Unterminated numeric escape: expected '}' after the digits", m_loc);
        if (!num_length) [[unlikely]]
            BASIC::Fail("Invalid numeric escape: expected at least one digit after the radix prefix", m_loc);
            
        bool is_overflow = false;
        cp = BASIC::IntegerStringToNumeric(start, close, is_overflow, 0x10FFFF, radix);
        if (is_overflow) [[unlikely]]
            BASIC::Fail("Numeric escape value exceeds the maximum supported Unicode code point", m_loc);
            
        return num_length + 3;
    }

    if (cp != U'b') [[unlikely]]
        BASIC::Fail("Invalid numeric escape: expected '{' after the radix prefix", m_loc);
    cp = '\b';
    
    return 1;
}

std::size_t StringLiteralParser::ProcessNumericUCNEscape(char32_t& cp) {
    if (!m_source.HasRemaining(2) || PeekNext() != U'{') [[unlikely]]
        BASIC::Fail("Invalid Unicode escape: expected '{' after '\\u'", m_loc);
    /*
        numeric-universal-character-name:
            \u{ hexadecimal-digit-sequence }

        hexadecimal-digit-sequence:
            hexadecimal-digit
            hexadecimal-digit-sequence hexadecimal-digit
    */
    const char8_t* start = m_source.CurPtr() + 2;
    const std::size_t hex_length = BASIC::ScanNumeric(start, m_source.EndPtr(), 16);
    
    const char8_t* close = start + hex_length;
    if (close >= m_source.EndPtr() || *close != U'}') [[unlikely]]
        BASIC::Fail("Unterminated Unicode escape: expected '}' after hexadecimal digits", m_loc);
    if (!hex_length) [[unlikely]]
        BASIC::Fail("Invalid Unicode escape: expected at least one hexadecimal digit", m_loc);
        
    bool is_overflow = false;
    cp = BASIC::IntegerStringToNumeric(start, close, is_overflow, 0x10FFFF, 16);
    if (is_overflow)
        BASIC::Fail("Unicode escape value exceeds the maximum supported Unicode code point", m_loc);
    
    return hex_length + 3;
}

char32_t StringLiteralParser::UnicodeNameToCodepoint(const std::u8string_view name) const {
    if (name.empty()) [[unlikely]]
        BASIC::Fail("Empty Unicode character name in escape sequence", m_loc);
        
    char32_t cp = 0;
    if (BASIC::UnicodeNameToCodepoint(name, cp))
        return cp;
    
    BASIC::Fail("Unknown Unicode character name", m_loc);
}

std::size_t StringLiteralParser::ProcessNameUCNEscape(char32_t& cp) {
    if (!m_source.HasRemaining(2) || PeekNext() != U'{') [[unlikely]] 
        BASIC::Fail("Invalid Unicode name escape: expected '{' after '\\N'", m_loc);
    /*
        named-universal-character-name:
            \N{ unicode-character-name }

        unicode-character-name:
            unicode-name-character
            unicode-character-name unicode-name-character
    */
    const char8_t* start = m_source.CurPtr() + 2;
    std::size_t name_length = BASIC::ScanWhile<[](const char8_t c){
        return BASIC::IsUnicodeNameChar(static_cast<char32_t>(c));
    }>(start, m_source.EndPtr());
    
    const char8_t* close = start + name_length;
    if (close >= m_source.EndPtr() || *close != U'}') [[unlikely]]
        BASIC::Fail("Unterminated Unicode name escape: expected '}' after the character name", m_loc);
        
    std::u8string_view name{start, close};
    cp = UnicodeNameToCodepoint(name);
    return name_length + 3;
}

std::size_t StringLiteralParser::ProcessCharEscape(char32_t& cp) {
    cp = GetEscapeChar(cp, m_loc);
    return 1;
}

std::size_t StringLiteralParser::ParseEscape() {
    std::size_t length = 0;
    char32_t cp = Peek();
    /*
        escape-sequence:
            simple-escape-sequence
            numeric-escape-sequence
            numeric-universal-character-name
            named-universal-character-name

        escape-sequence dispatch:
            u       -> numeric-universal-character-name
            N       -> named-universal-character-name
            b B     -> binary-escape-sequence
            o O     -> octal-escape-sequence
            d D     -> decimal-escape-sequence
            x X     -> hexadecimal-escape-sequence
            other   -> simple-escape-sequence
    */
    switch (cp) {
        case U'u':
            length = ProcessNumericUCNEscape(cp);
            break;
        case U'N':
            length =  ProcessNameUCNEscape(cp);
            break;;
        default:
            std::size_t radix = GetEsacpeRadix(cp);
            if (radix) {
                length = ProcessNumericEscape(cp, radix); 
            } else length = ProcessCharEscape(cp);
    }
    
    AppendCodepoint(cp);
    return length;
}

}