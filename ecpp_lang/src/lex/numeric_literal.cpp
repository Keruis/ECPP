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

bool NumericLiteralParser::IsNondigit(char32_t cp) const noexcept {
    return BASIC::IsLetter(cp) || cp == U'_';
}

std::size_t NumericLiteralParser::ScanPPNumber() {
    const char8_t* const start = m_source.CurPtr();
    /*
        pp-number:
            digit
            . digit
    */
    if (BASIC::IsDigit(Peek(), 10)) {
        m_source.AdvanceBytes(1);
    } else if (Peek() == U'.' && m_source.HasRemaining(2) && BASIC::IsDigit(PeekNext(), 10)) {
        m_source.AdvanceBytes(2);
    } else return 0;

    while (!m_source.AtEnd()) {
        const BASIC::UTFChar c = m_source.PeekAt(0);

        if (UTILITY::IsAnyOf(c.codepoint, U'e', U'E', U'p', U'P')) {
            if (m_source.HasRemaining(2) && UTILITY::IsAnyOf(PeekNext(), U'+', U'-')) {
                /*
                    pp-number:
                        pp-number e sign
                        pp-number E sign
                        pp-number p sign
                        pp-number P sign
                */
                m_source.AdvanceBytes(2);
                continue;
            }
        }

        if (BASIC::IsXidContinue(c.codepoint)) {
            /*
                pp-number:
                    pp-number identifier-continue
            */
            m_source.AdvanceBytes(c.length);
            continue;
        }

        if (c.codepoint == U'\'' && m_source.HasRemaining(2)) {
            if (BASIC::IsDigit(PeekNext(), 10) || IsNondigit(PeekNext())) {
                /*
                    pp-number:
                        pp-number ' digit
                        pp-number ' nondigit
                */
                m_source.AdvanceBytes(2);
                continue;
            }
        }

        if (c.codepoint == U'.') {
            /*
                pp-number:
                    pp-number.
            */
            m_source.AdvanceBytes(1);
            continue;
        }

        break;
    }
    
    return m_source.CurPtr() - start;
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
            /*
                radix-digit ' radix-digit
            */
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
                /*
                    binary literal prefix:
                        0b or 0B
                */
                return BIN;
            case U'\'':
            case U'0' ... U'9':
                /*
                    leading-zero literal candidate:
                        0' or 0+digit

                    Check for a fractional part or exponent while scanning decimal digits
                */
                return OCT;
            case U'x': case U'X':
                /*
                    hexadecimal literal prefix:
                        0x or 0X
                */
                return HEX;
            default:
                /*
                    decimal floating point literal:
                        0. or 0e or 0E
                */
                break;
        }
    }
    /*
        nonzero decimal digit:
            one of
            1 2 3 4 5 6 7 8 9
    */
    return DEC;
}

void NumericLiteralParser::ScanBinaryLiteral() {
    if (!ScanDigitSequence(BIN))
        BASIC::Fail("Invalid binary literal: expected at least one binary digit after the prefix", m_loc);
    /*
        binary-literal:
            0b binary-digit
            0B binary-digit
            binary-literal '_opt binary-digit

        binary-digit:
            one of
            0 1
    */
    if (!m_source.AtEnd() && BASIC::IsDigit(Peek(), DEC))
        BASIC::Fail("Invalid digit in binary literal: expected 0 or 1", m_loc);
}

void NumericLiteralParser::ScanLeadingZeroLiteral() {
    const std::size_t oct_len = ScanDigitSequence(DEC);
    /*
        octal-literal:
            0
            octal-literal '_opt octal-digit

        oct-digit:
            one of
            0 1 2 3 4 5 6 7
    */
    if (!m_source.AtEnd() && UTILITY::IsAnyOf(Peek(), U'.', U'e', U'E')) {
        /*
            HasAny( . e E ) -> decimal-floating-point-literal
        */
        ScanDecimaLiteral();
        return;
    }
    
    if (BASIC::ScanWhile<[](const char8_t c){
        return BASIC::IsDigit(static_cast<char32_t>(c), 8) || c == U'\'';
    }>(m_source.CurPtr() - oct_len, m_source.CurPtr()) != oct_len)
        BASIC::Fail("Invalid octal literal: expected digits between 0 and 7", m_loc);
}

/*
    floating-point-literal:
        decimal-floating-point-literal
        hexadecimal-floating-point-literal

    sign:
        one of
        + -
*/

void NumericLiteralParser::ScanDecimaLiteral() {
    ScanDigitSequence(DEC);
    /*
        decimal-literal integer-suffix_opt

        decimal-literal:
            nonzero-digit
            decimal-literal '_opt digit

        nonzero-literal:
            one of
            1 2 3 4 5 6 7 8 9
    */
    const bool has_point = !m_source.AtEnd() && Peek() == U'.';
    /*
        decimal-floating-point-literal:
            fractional-constant exponent-part_opt floating-point-suffix_opt
            digit-sequence exponent-part floating-point-suffix_opt

        fractional-constant:
            digit-sequence_opt . digit-sequence
            digit-sequence .

        exponent-part:
            e sign_opt digit-sequence
            E sign_opt digit-sequence

        digit-sequence:
            digit
            digit-sequence '_opt digit
    */
    m_source.AdvanceBytes(has_point);
    ScanDigitSequence(DEC);
    
    const bool has_e = !m_source.AtEnd() && ((Peek() | 0x20) == U'e');
    if (has_e) {
        m_source.AdvanceBytes(has_e + (!m_source.AtEnd() && UTILITY::IsAnyOf(Peek(), U'+', U'-')));
        
        if (!ScanDigitSequence(DEC))
            BASIC::Fail("Invalid floating-point literal: expected at least one decimal digit in the exponent", m_loc);
    }
    
    m_type = (has_point || has_e) ? TokenType::LITERALS_FLOAT : TokenType::LITERALS_INT;
}

void NumericLiteralParser::ScanHexadecimalLiteral() {
    std::size_t hex_len = ScanDigitSequence(HEX);
    /*
        hexadecimal-literal integer-suffix_opt

        hexadecimal-literal:
            hexadecimal-prefix hexadecimal-digit-sequence

        hexadecimal-prefix:
            one of
            0x 0X

        hexadecimal-digit-sequence:
            hexadecimal-digit
            hexadecimal-digit-sequence '_opt hexadecimal-digit

        hexadecimal-digit:
            one of
            0 1 2 3 4 5 6 7 8 9
            a b c d e f
            A B C D E F
    */
    const bool has_point = !m_source.AtEnd() && Peek() == U'.';
    /*
        hexadecimal-floating-point-literal:
            hexadecimal-prefix hadecimal-fractional-constant binary-exponent-part floating-point-suffix
            hexadecimal-prefix hexadecimal-digit-sequence binary-exponent-part floating-point-suffix

        binary-exponent-part:
            p sign_opt digit-sequence
            P sign_opt digit-sequence
    */
    m_source.AdvanceBytes(has_point);
    std::size_t len = ScanDigitSequence(HEX);
    const bool has_p = !m_source.AtEnd() && (Peek() | 0x20) == U'p';
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
            /*
                integer-suffix::
                    integer-width-prefix numeric-width-sequence

                integer-width-prefix:
                    one of
                    u U i I

                numeric-width-sequence:
                    numeric-width
                    0 numeric-width-sequence

                numeric-width:
                    one of
                    8 16 32 64
            */
            if (m_type == TokenType::LITERALS_FLOAT)
                BASIC::Fail("Invalid numeric suffix: integer suffix cannot be applied to a floating-point literal", m_loc);
            
            m_source.AdvanceBytes(1);
            m_type = TokenType::LITERALS_INT_SUF;
            break;
        case U'f': case U'F':
            /*
                floating-point-suffix:
                    floating-point-width-suffix

                floating-point-width-suffix:
                    floating-point-width-prefix numeric-width

                floating-point-width-prefix:
                    one of
                    f F
            */
            if (m_type != TokenType::LITERALS_FLOAT)
                BASIC::Fail("Invalid numeric suffix: floating-point suffix cannot be applied to a integr literal", m_loc);
            
            m_source.AdvanceBytes(1);
            m_type = TokenType::LITERALS_FLOAT_SUF;
            break;
        default:
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
}

}