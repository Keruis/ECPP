#pragma once

#include <basic/utf/utf_view.h>
#include <basic/location.h>

namespace ECPP::LEXER {

class StringLiteralParser {
    public:
        StringLiteralParser(const BASIC::UTFCursor<char8_t>& source, BASIC::Location loc, std::u8string& str)
            : m_str(str), m_loc(loc), m_source(source)
        {}

        std::size_t ParseEscape();
        static char32_t GetEscapeChar(const char32_t, const BASIC::Location);

    private:
        char32_t Peek() const;
        char32_t PeekNext() const;

        void AppendCodepoint(const char32_t);
        std::size_t GetEsacpeRadix(const char32_t) const noexcept;
        char32_t UnicodeNameToCodepoint(const std::u8string_view) const;
        std::size_t ProcessCharEscape(char32_t&);
        std::size_t ProcessNumericEscape(char32_t&, const std::size_t);
        std::size_t ProcessNumericUCNEscape(char32_t&);
        std::size_t ProcessNameUCNEscape(char32_t&);

    private:
        std::u8string& m_str;
        BASIC::Location m_loc;
        const BASIC::UTFCursor<char8_t>& m_source;
};

}