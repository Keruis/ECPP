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
        NumericLiteralParser(BASIC::UTFCursor<char8_t>& source, BASIC::Location loc, std::uint8_t& suffix, TokenType& type)
            : m_source(source), m_loc(loc), m_suffix(suffix), m_type(type)
        {}

        void ScanNumericSuffix();
        std::size_t ScanPPNumber();
        void ScanDigits(const NumericRadix);
        NumericRadix DetectNumericRadix(const char32_t);

    private:
        char32_t Peek() const;
        char32_t PeekNext() const;

        bool IsNondigit(char32_t) const noexcept;
        std::size_t ScanDigitSequence(NumericRadix);
        void ScanBinaryLiteral();
        void ScanLeadingZeroLiteral();
        void ScanDecimaLiteral();
        void ScanHexadecimalLiteral();

    private:
        TokenType& m_type;
        BASIC::Location m_loc;
        std::uint8_t& m_suffix;
        BASIC::UTFCursor<char8_t>& m_source;
};

}