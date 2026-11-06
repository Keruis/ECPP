#include <lex/numeric_literal.h>

#include <basic/fail.h>
#include <utility/algorithms.h>
#include <basic/string_extras.h>
#include <basic/unicode/unicode_extras.h>

#include <format>

namespace ECPP::LEXER {

char32_t NumericLiteralParser::Peek() const {
    return m_source.PeekAt(0).codepoint;
}
    
char32_t NumericLiteralParser::PeekNext() const {
    return m_source.PeekAt(1).codepoint;
}

std::size_t NumericLiteralParser::ScanDigitSequence(NumericRadix radix) {
    const char8_t* start = m_source.CurPtr();
    bool previous_was_digit = false;

    while (!m_source.AtEnd()) {
        const char32_t cp = Peek();
        if (BASIC::IsDigit(cp, radix)) {
            m_source.AdvanceBytes(1);
            previous_was_digit = true;
            continue;
        }

        if (cp == U'\'') {
            const bool next_is_digit = m_source.HasRemaining(2) && BASIC::IsDigit(PeekNext(), radix);
            if (!previous_was_digit || !next_is_digit) 
                BASIC::Fail("Invalid digit separator: expected a digit on both sides", m_loc);
            m_source.AdvanceBytes(1);
            previous_was_digit = false;
            continue;
        }
        break;
    }

    return m_source.CurPtr() - start;
}

NumericLiteralParser::NumericRadix NumericLiteralParser::DetectNumericRadix(const char32_t cp) {
    using enum NumericRadix;
    if (cp == U'0' && m_source.HasRemaining(2)) {
        switch (PeekNext()) {
            case U'b': case U'B':
                return BIN;
            case U'\'':
            case U'0' ... U'9':
                return OCT;
            case U'x': case U'X':
                return HEX;
            default:
                break;
        }
    }
    return DEC;
}

void NumericLiteralParser::ScanBinaryLiteral() {
    if (!ScanDigitSequence(BIN))
        BASIC::Fail("Invalid binary literal: expected at least one binary digit after the prefix", m_loc);
    if (!m_source.AtEnd() && BASIC::IsDigit(Peek(), DEC))
        BASIC::Fail("Invalid digit in binary literal: expected 0 or 1", m_loc);
}

void NumericLiteralParser::ScanLeadingZeroLiteral() {
    const std::size_t oct_len = ScanDigitSequence(DEC);
    if (!m_source.AtEnd() && UTILITY::IsAnyOf(Peek(), U'.', U'e', U'E')) {
        ScanDecimaLiteral();
        return;
    }
    if (BASIC::ScanWhile<[](const char8_t c){
        return BASIC::IsDigit(c, 8) || c == U'\'';
    }>(m_source.CurPtr() - oct_len, m_source.CurPtr()) != oct_len)
        BASIC::Fail("Invalid octal literal: expected digits between 0 and 7", m_loc);
}

void NumericLiteralParser::ScanDecimaLiteral() {
    ScanDigitSequence(DEC);
    const bool has_point = !m_source.AtEnd() && Peek() == U'.';
    m_source.AdvanceBytes(has_point);
    ScanDigitSequence(DEC);
    const bool has_e = !m_source.AtEnd() && ((Peek() | 0x20) == U'e');
    if (has_e) {
        m_source.AdvanceBytes(has_e + !m_source.AtEnd() && UTILITY::IsAnyOf(Peek(), U'+', U'-'));
        if (!ScanDigitSequence(DEC))
            BASIC::Fail("Invalid floating-point literal: expected at least one decimal digit in the exponent", m_loc);
    }
    m_type = (has_point || has_e) ? TokenType::LITERALS_FLOAT : TokenType::LITERALS_INT;
}

void NumericLiteralParser::ScanHexadecimalLiteral() {
    std::size_t hex_len = ScanDigitSequence(HEX);
    const bool has_point = Peek() == U'.';
    m_source.AdvanceBytes(has_point);
    std::size_t len = ScanDigitSequence(HEX);
    const bool has_p = (Peek() | 0x20) == U'p';
    if (has_p) {
        if (!hex_len && !len)
            BASIC::Fail("Invalid hexadecimal floating-point literal: expected at least one hexadecimal digit in the significand", m_loc);
        m_source.AdvanceBytes(1);
        if (!m_source.AtEnd() && UTILITY::IsAnyOf(Peek(), U'+', U'-'))
            m_source.AdvanceBytes(1);
        if (!ScanDigitSequence(DEC))
            BASIC::Fail("Invalid hexadecimal floating-point literal: expected at least one decimal digit in the exponent", m_loc);
        m_type = TokenType::LITERALS_FLOAT;
    }
    if (has_point && !has_p)
        BASIC::Fail("Invalid hexadecimal floating-point literal: expected a binary exponent introduced by 'p' or 'P'", m_loc);
    if (!has_point && !has_p && !hex_len)
        BASIC::Fail("Invalid hexadecimal literal: expected at least one hexadecimal digit after the prefix", m_loc);
}

void NumericLiteralParser::ScanDigits(NumericLiteralParser::NumericRadix radix) {
    using enum NumericRadix;
    switch (radix) {
        case BIN:
            m_source.AdvanceBytes(2);
            return ScanBinaryLiteral();
        case OCT:
            return ScanLeadingZeroLiteral();
        case DEC:
            return ScanDecimaLiteral();
        case HEX:
            m_source.AdvanceBytes(2);
            return ScanHexadecimalLiteral();
    }
}

void NumericLiteralParser::ScanNumericSuffix() {
    switch (Peek()) {
        case U'u': case U'U':
        case U'i': case U'I':
            if (m_type == TokenType::LITERALS_FLOAT)
                BASIC::Fail("Invalid numeric suffix: integer suffix cannot be applied to a floating-point literal", m_loc);
            m_source.AdvanceBytes(1);
            m_type = TokenType::LITERALS_INT_SUF;
            break;
        case U'f': case U'F':
            m_source.AdvanceBytes(1);
            m_type = TokenType::LITERALS_FLOAT_SUF;
            break;
        default:
            if (Peek() == U'_' || BASIC::IsXidStart(Peek()) || BASIC::IsXidContinue(Peek()))
                BASIC::Fail("Invalid numeric suffix: unexpected identifier character after the numeric literal", m_loc);
            return;
    }
    
    const std::size_t length = BASIC::ScanNumeric(m_source.CurPtr(), m_source.EndPtr(), 10);
    if (!length)
        BASIC::Fail("Incomplete numeric suffix: expected a width after the suffix specifier", m_loc);
    bool is_overflow = false;
    m_suffix = BASIC::IntegerStringToNumeric(m_source.CurPtr(), m_source.CurPtr() + length, is_overflow, std::numeric_limits<std::uint8_t>::max(), 10);
    if (is_overflow)
        BASIC::Fail("Numeric width suffix is out of range: expected 8, 16, 32, or 64", m_loc);
    if (!UTILITY::IsAnyOf(m_suffix, 8, 16, 32, 64))
        BASIC::Fail(std::format("Invalid numeric suffix '{}': expected one of 8, 16, 32, or 64", m_suffix), m_loc);
    m_source.AdvanceBytes(length);
    if (!m_source.AtEnd() && (Peek() == U'_' || BASIC::IsXidStart(Peek()) || BASIC::IsXidContinue(Peek())))
        BASIC::Fail("Invalid numeric suffix: unexpected identifier character after the width", m_loc);
}

std::size_t NumericLiteralParser::ParseNumericLiteral(char32_t cp) {
    m_type = TokenType::LITERALS_INT;
    m_suffix = 0;
    const char8_t* numeric_start = m_source.CurPtr();
    const NumericLiteralParser::NumericRadix radix = DetectNumericRadix(cp);
    ScanDigits(radix);
    m_str = {numeric_start, m_source.CurPtr()};
    ScanNumericSuffix();
    return m_source.CurPtr() - numeric_start;
}
    
}