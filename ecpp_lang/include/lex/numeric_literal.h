#pragma omce

#include <lex/token.h>
#include <basic/location.h>
#include <basic/utf/utf_view.h>

namespace ECPP::LEXER {

class NumericLiteralParser {
    public:
        enum NumericRadix {
            BIN = 2,
            OCT = 8,
            DEC = 10,
            HEX = 16
        };

    public:
        NumericLiteralParser(BASIC::UTFCursor<char8_t>& source, BASIC::Location loc, std::u8string_view& numeric_str, std::uint8_t& suffix, TokenType& type)
            : m_source(source), m_loc(loc), m_str(numeric_str), m_suffix(suffix), m_type(type)
        {}

        std::size_t ParseNumericLiteral(char32_t);

    private:
        char32_t Peek() const;
        char32_t PeekNext() const;

        std::size_t ScanDigitSequence(NumericRadix);
        NumericRadix DetectNumericRadix(const char32_t);
        void ScanDigits(const NumericRadix);
        void ScanBinaryLiteral();
        void ScanLeadingZeroLiteral();
        void ScanDecimaLiteral();
        void ScanHexadecimalLiteral();
        void ScanNumericSuffix();

    private:
        BASIC::Location m_loc;
        std::uint8_t& m_suffix;
        TokenType& m_type;
        std::u8string_view& m_str;
        BASIC::UTFCursor<char8_t>& m_source;
};

}