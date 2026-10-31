#include <lex/string_literal.h>

#include <basic/char_extras.h>
#include <basic/string_extras.h>
#include <basic/unicode/unicode_extras.h>

#include <limits>

namespace ECPP::LEXER {

char32_t StringLiteralParser::Peek() const {
    return m_source.PeekAt(0).codepoint;
}

char32_t StringLiteralParser::PeekNext() const {
    return m_source.PeekAt(1).codepoint;
}

char32_t StringLiteralParser::PeekAt(const std::size_t offset) const {
    return m_source.PeekAt(offset).codepoint;
}

void StringLiteralParser::AppendCodepoint(const char32_t cp) {
    if (!BASIC::Encode(cp, m_str)) [[unlikely]]
        throw "ECPP::LEXER::StringLiteralParser::AppendCodepoint";
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

char32_t StringLiteralParser::GetEscapeChar(const char32_t cp) const {
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
            throw "ECPP::LEXER::StringLiteralParser::GetEscapeChar";
    }
}

std::size_t StringLiteralParser::ProcessNumericEscape(char32_t& cp, const std::size_t radix) {
    if ((m_source.HasRemaining(2)) && PeekNext() == U'{') {
        const char8_t* start = m_source.CurPtr() + 2;
        const std::size_t num_length = BASIC::ScanNumeric(start, m_source.EndPtr(), radix);
        const char8_t* close = start + num_length;
        if (close >= m_source.EndPtr() || *close != U'}') [[unlikely]]
            throw "ECPP::LEXER::StringLiteralParser::ProcessCharEscape3";
        if (!num_length) [[unlikely]]
            throw "ECPP::LEXER::StringLiteralParser::ProcessCharEscape2";
        bool is_overflow = false;
        cp = BASIC::IntegerStringToNumeric(start, close, is_overflow, std::numeric_limits<char32_t>::max(), radix);
        if (is_overflow) [[unlikely]]
            throw "ECPP::LEXER::StringLiteralParser::ProcessCharEscape4";
        return num_length + 3;
    }

    if (cp != U'b') [[unlikely]]
        throw "ECPP::LEXER::StringLiteralParser::ProcessCharEscape6";
    cp = '\b';
    
    return 1;
}

std::size_t StringLiteralParser::ProcessNumericUCNEscape(char32_t& cp) {
    if (!m_source.HasRemaining(2) || PeekNext() != U'{') [[unlikely]]
        throw "ECPP::LEXER::StringLiteralParser::ProcessNumericUCNEscape1";
    const char8_t* start = m_source.CurPtr() + 2;
    const std::size_t hex_length = BASIC::ScanNumeric(start, m_source.EndPtr(), 16);
    const char8_t* close = start + hex_length;
    if (close >= m_source.EndPtr() || *close != U'}') [[unlikely]]
        throw "ECPP::LEXER::StringLiteralParser::ProcessNumericUCNEscape2";
    if (!hex_length) [[unlikely]]
        throw "ECPP::LEXER::StringLiteralParser::ProcessNumericUCNEscape3";
    bool is_overflow = false;
    cp = BASIC::IntegerStringToNumeric(start, close, is_overflow, std::numeric_limits<char32_t>::max(), 16);
    return hex_length + 3;
}

char32_t StringLiteralParser::UnicodeNameToCodepoint(std::u8string_view name) const {
    if (name.empty()) [[unlikely]]
        throw "ECPP::LEXER::StringLiteralParser::UnicodeNameToCodepoint";
    
}

std::size_t StringLiteralParser::ProcessNameUCNEscape(char32_t& cp) {
    if (!m_source.HasRemaining(2) || PeekNext() != U'{') [[unlikely]] 
        throw "ECPP::LEXER::StringLiteralParser::ProcessNameUCNEscape";
    const char8_t* start = m_source.CurPtr() + 2;
    std::size_t name_length = BASIC::ScanWhile<[](const char8_t c){
        return BASIC::IsUnicodeNameChar(static_cast<char32_t>(c));
    }>(start, m_source.EndPtr());
    const char8_t* close = start + name_length;
    if (close >= m_source.EndPtr() || *close != U'}') [[unlikely]]
        throw "ECPP::LEXER::StringLiteralParser::ProcessNumericUCNEscape2";
    std::u8string_view name{start, close};
    cp = UnicodeNameToCodepoint(name);
    return name_length + 3;
}

std::size_t StringLiteralParser::ProcessCharEscape(char32_t& cp) {
    cp = GetEscapeChar(cp);
    return 1;
}

std::size_t StringLiteralParser::ParserEscape() {
    std::size_t length = 0;
    char32_t cp = Peek(); 
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