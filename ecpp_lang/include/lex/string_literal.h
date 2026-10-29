#pragma once

#include <basic/utf/utf_view.h>
#include <basic/location.h>

#include <string>

namespace ECPP::LEXER {

class StringLiteralParser {
    public:
        StringLiteralParser(const BASIC::UTFCursor<char8_t>& source, std::u8string& str)
            : m_str(str), m_source(source)
        {}

        std::size_t ParserEscape();

    private:
        char32_t Peek() const;
        char32_t PeekNext() const;
        char32_t PeekAt(std::size_t) const;

        void AppendCodepoint(char32_t);
        std::size_t ProcessCharEscape();
        std::size_t ProcessNumericEscape();
        std::size_t ProcessNumericUCNEscape();
        std::size_t ProcessNameUCNEscape();

    private:
        std::u8string& m_str;
        const BASIC::UTFCursor<char8_t>& m_source;
};

}