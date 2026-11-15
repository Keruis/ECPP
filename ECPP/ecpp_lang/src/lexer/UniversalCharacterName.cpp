#include <lexer/UniversalCharacterName.h>

#include <basic/PP.h>
#include <utility/Predicate.h>
#include <basic/unicode/UnicodeClass.h>
#include <basic/string/StringAlgorithms.h>
#include <basic/unicode/UnicodeNameToCodepoint.h>
#include <basic/diagnostic/DiagnosticEngine.h>

namespace LANG::LEXER {

LexStatus UCNParser::ParseIdentifierUCN(::BASIC::UTF::DecodedCodePoint& c) {    
    const char8_t* const start = m_cursor.CurPtr();
    
    if (!m_cursor.HasRemaining(2))
        return LexStatus::NOT_MATCHED;

    const ::BASIC::UTF::DecodedCodePoint c1 = m_cursor.Peek();
    const ::BASIC::UTF::DecodedCodePoint c2 = m_cursor.PeekNext();

    if (c1.codepoint != U'\\')
        return LexStatus::NOT_MATCHED;

    if (!UTILITY::IsAnyOf(c2.codepoint, U'u', U'U', U'N'))
        return LexStatus::NOT_MATCHED;

    m_cursor.Advance(c1, c2);

    if (m_cursor.AtEnd()) {
        ReportInvalidUCN();
        return m_status;
    }

    char32_t cp = 0;
    std::size_t length = 8;
    switch (c2.codepoint) {
        case U'u': {
            if (m_cursor.Peek().codepoint == U'{') {
                ParseDelimitedHexCodepoint(cp);
                break;
            }
            
            length = 4;
        case U'U':
            if (!m_cursor.HasRemaining(length)) {
                ReportInvalidUCN();
                return m_status;
            }

            cp = ParseHexCodepoint(length);
            break;
        }

        case U'N':
            if (m_cursor.Peek().codepoint != U'{') {
                ReportInvalidUCN();
                return m_status;
            }

            ParseNamedCodepoint(cp);
            break;
    }

    ValidateCodepoint(cp);

    c.codepoint = cp;
    c.uint_length = m_cursor.CurPtr() - start;

    return m_status;
}

char32_t UCNParser::ParseHexCodepoint(std::size_t length) {
    const char8_t* const first = m_cursor.CurPtr();
    const char8_t* const last = first + length;

    const std::size_t digit_length = ::BASIC::STRING::ScanNumeric(first, last, 16);

    char32_t cp = 0;
    bool is_overflow = false;

    if (digit_length != 0)
        cp = ::BASIC::STRING::IntegerStringToNumeric(first, first + digit_length, is_overflow, 0x10FFFF, 16 );

    m_cursor.AdvanceBytes(length);

    if (digit_length == 0 || digit_length != length || is_overflow || !BASIC::IsUnicodeScalarValue(cp))
        ReportInvalidUCN();

    return cp;
}

void UCNParser::ParseDelimitedHexCodepoint(char32_t& cp) {
    m_cursor.Advance(m_cursor.Peek());

    const std::u8string_view digits{m_cursor.CurPtr(), m_cursor.EndPtr()};

    const std::size_t close_pos = digits.find(u8'}');
    if (close_pos == std::u8string_view::npos) {
        const std::size_t digit_length = ::BASIC::STRING::ScanNumeric(digits.begin(), digits.end(), 16);

        m_cursor.AdvanceBytes(digit_length);

        ReportInvalidUCN();
        return;
    }

    cp = ParseHexCodepoint(close_pos);

    m_cursor.AdvanceBytes(1);
}

void UCNParser::ParseNamedCodepoint(char32_t& cp) {
    m_cursor.Advance(m_cursor.Peek());

    const std::u8string_view name{m_cursor.CurPtr(), m_cursor.EndPtr()};

    const std::size_t close_pos = name.find(u8'}');
    if (close_pos == std::u8string_view::npos) {
        const std::size_t name_length =
            ::BASIC::STRING::ScanWhile<
                [](const char8_t c) {
                    return BASIC::IsUnicodeNameCharacter(
                        static_cast<char32_t>(c)
                    );
                }
            >(name.begin(), name.end());

        m_cursor.AdvanceBytes(name_length);

        ReportInvalidUCN();
        return;
    }

    const bool found = ::BASIC::UNICODE::UnicodeNameToCodepoint::Lookup({m_cursor.CurPtr(), close_pos}, cp);

    m_cursor.AdvanceBytes(close_pos + 1);

    if (!found)
        ReportInvalidUCN();
}

void UCNParser::ValidateCodepoint(char32_t cp) {
    if (m_status == LexStatus::INVALID)
        return;

    if (BASIC::IsControlCharacter(cp)) {
        m_status = LexStatus::INVALID;

        BASIC::DIAGNOSTIC::DiagnosticEngine::Report<
            BASIC::DIAGNOSTIC::DiagnosticID::UCN_DESIGNATES_CONTROL_CHARACTER
        >(m_cursor.Range());

        return;
    }

    if (BASIC::IsBasicCharacter(cp)) {
        m_status = LexStatus::INVALID;

        BASIC::DIAGNOSTIC::DiagnosticEngine::Report<
            BASIC::DIAGNOSTIC::DiagnosticID::UCN_DESIGNATES_BASIC_CHARACTER
        >(m_cursor.Range());

        return;
    }
}

void UCNParser::ReportInvalidUCN() {
    m_status = LexStatus::INVALID;

    BASIC::DIAGNOSTIC::DiagnosticEngine::Report<
        BASIC::DIAGNOSTIC::DiagnosticID::INVALID_UNIVERSAL_CHARACTER_NAME
    >(m_cursor.Range());
}

}