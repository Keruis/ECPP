#include <cstddef>
#include <lex/string_literal.h>

#include <basic/char_extras.h>
#include <basic/string_extras.h>

namespace ECPP::LEXER {

char32_t StringLiteralParser::Peek() const {
    return m_source.PeekAt(0).codepoint;
}

char32_t StringLiteralParser::PeekNext() const {
    return m_source.PeekAt(1).codepoint;
}

char32_t StringLiteralParser::PeekAt(std::size_t offset) const {
    return m_source.PeekAt(offset).codepoint;
}

void StringLiteralParser::AppendCodepoint(char32_t cp) {

}

std::size_t StringLiteralParser::ProcessNumericEscape() {

}

std::size_t StringLiteralParser::ProcessNumericUCNEscape() {

}

std::size_t StringLiteralParser::ProcessNameUCNEscape() {

}

std::size_t StringLiteralParser::ProcessCharEscape() {
    std::size_t length = 1;
    std::size_t radix = 0;
    BASIC::UTFChar c = m_source.PeekAt(0);
    char character{};
    switch (c.codepoint) {
        case U'\\': case U'\'':
        case U'\"':
            character = c.codepoint;
            break;
        case U'a': character = '\a';
            break;
        case U'e':
        case U'E': character = '\e';
            break;
        case U'f': character = '\f';
            break;
        case U'n': character = '\n';
            break;
        case U'r': character = '\r';
            break;
        case U't': character = '\t';
            break;
        case U'b': case U'B':
            radix = 2;
            break;
        case U'o': case U'O':
            radix = 8;
            break;
        case U'd': case U'D':
            radix = 10;
            break;
        case U'x': case U'X':
            radix = 16;
            break;
        default:
            throw "ECPP::LEXER::StringLiteralParser::ProcessCharEscape1";
    }
    if (radix) {
        if (!m_source.AtEnd() && PeekNext() == U'{') {
            length++;
            const char8_t* start = m_source.CurPtr() + 2;
            std::size_t num_length = BASIC::ScanNumeric(start, m_source.EndPtr(), radix);
            if (!num_length) 
                throw "ECPP::LEXER::StringLiteralParser::ProcessCharEscape2";
            length += num_length;
            if (PeekAt(length) != U'}')
                throw "ECPP::LEXER::StringLiteralParser::ProcessCharEscape3";
            bool is_overflow = false;
            std::size_t cp = BASIC::IntegerStringToNumeric(start, start + num_length, is_overflow, std::numeric_limits<char32_t>::max(), radix);
            if (is_overflow)
                throw "ECPP::LEXER::StringLiteralParser::ProcessCharEscape4";
            if (!BASIC::Encode(cp, m_str))
                throw "ECPP::LEXER::StringLiteralParser::ProcessCharEscape5";
            return length;
        }
        if (c.codepoint != U'b')
            throw "ECPP::LEXER::StringLiteralParser::ProcessCharEscape6";
        character = '\b';
    }
    m_str.push_back(character);
    return length;
}

std::size_t StringLiteralParser::ParserEscape() {
    return ProcessCharEscape();
}

}