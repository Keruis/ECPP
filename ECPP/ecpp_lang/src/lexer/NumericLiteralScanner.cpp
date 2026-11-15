#include <lexer/NumericLiteralScanner.h>

#include <basic/PP.h>
#include <basic/ascii/CharClass.h>
#include <basic/unicode/UnicodeClass.h>
#include <lexer/UniversalCharacterName.h>

namespace LANG::LEXER {

LexStatus NumericLiteralScanner::Scan() {
    const char8_t* const start = m_cursor.CurPtr();

    ::BASIC::UTF::DecodedCodePoint c1 = m_cursor.Peek();
    ::BASIC::UTF::DecodedCodePoint c2;

    if (::BASIC::ASCII::IsDigit10(c1.codepoint)) {
        m_cursor.Advance(c1);
    } else if (c1.codepoint == U'.' && m_cursor.HasRemaining(2)) {
        c2 = m_cursor.PeekNext();
        if (!::BASIC::ASCII::IsDigit10(c2.codepoint))
            return LexStatus::NOT_MATCHED;

        m_cursor.Advance(c1, c2);
    } else return LexStatus::NOT_MATCHED;

    m_tok.kind = BASIC::TokenKind::LITERAL;
    m_tok.type = BASIC::TokenType::LITERALS_NUMERIC;
    m_tok.text = {start, ScanPPNumber()};
    m_cursor.UpdateEndPosition();

    return LexStatus::SUCCESS;
}

const char8_t* NumericLiteralScanner::ScanPPNumber() {
    ::BASIC::UTF::DecodedCodePoint c1;
    ::BASIC::UTF::DecodedCodePoint c2;

    while (!m_cursor.AtEnd()) {
        c1 = m_cursor.Peek();

        if (UTILITY::IsAnyOf(c1.codepoint, U'e', U'E', U'p', U'P')) {
            if (m_cursor.HasRemaining(2)) {
                c2 = m_cursor.PeekNext();
                if (UTILITY::IsAnyOf(c2.codepoint, U'+', U'-')) {
                    m_cursor.Advance(c1, c2);
                    continue;
                }
            }
        }

        ::BASIC::UTF::DecodedCodePoint c{};
        switch (UCNParser{m_cursor}.ParseIdentifierUCN(c)) {
            case LexStatus::SUCCESS:
                if (BASIC::IsIdentifierContinue(c.codepoint))
                    continue;
                
                m_cursor.Retreat(c);

                break;

            case LexStatus::INVALID:
                continue;

            case LexStatus::NOT_MATCHED:
        }

        if (BASIC::IsIdentifierContinue(c1.codepoint)) {
            m_cursor.Advance(c1);
            continue;
        }

        if (c1.codepoint == U'\'' && m_cursor.HasRemaining(2)) {
            c2 = m_cursor.PeekNext();
            if (::BASIC::ASCII::IsDigit10(c2.codepoint) || BASIC::IsNondigit(c2.codepoint)) {
                m_cursor.Advance(c1, c2);
                continue;
            }
        }

        if (c1.codepoint == U'.') {
            m_cursor.Advance(c1);
            continue;
        }

        break;
    }

    return m_cursor.CurPtr();
}

}