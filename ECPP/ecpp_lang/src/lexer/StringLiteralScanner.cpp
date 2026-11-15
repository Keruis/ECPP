#include <lexer/StringLiteralScanner.h>

#include <basic/ascii/CharClass.h>
#include <basic/diagnostic/DiagnosticEngine.h>

namespace LANG::LEXER {

LexStatus StringLiteralScanner::Scan() {
    const char8_t* const start = m_cursor.CurPtr();

    const ::BASIC::UTF::DecodedCodePoint c = m_cursor.Peek();
    if (c.codepoint != U'"')
        return LexStatus::NOT_MATCHED;

    const LexStatus status = ScanString();

    m_tok.kind = BASIC::TokenKind::LITERAL;
    m_tok.type = BASIC::TokenType::LITERALS_STRING;
    m_tok.text = {start, m_cursor.CurPtr()};
    m_cursor.UpdateEndPosition();
    
    return status;
}

LexStatus StringLiteralScanner::ScanString() {    
    m_cursor.Advance(m_cursor.Peek());

    while (!m_cursor.AtEnd()) {
        const ::BASIC::UTF::DecodedCodePoint c = m_cursor.Peek();
        if (c.codepoint == U'"') {
            m_cursor.Advance(c);
            return LexStatus::SUCCESS;
        }

        if (::BASIC::ASCII::IsVerticalWhitespace(c.codepoint))
            break;

        if (c.codepoint == U'\\') {
            if (m_cursor.HasRemaining(2)) {
                m_cursor.Advance(c);
                m_cursor.Advance(m_cursor.Peek());
                continue;
            }

            break;
        }

        m_cursor.Advance(c);
    }

    BASIC::DIAGNOSTIC::DiagnosticEngine::Report<
        BASIC::DIAGNOSTIC::DiagnosticID::UNTERMINATED_STRING
    >(m_cursor.Range());

    return LexStatus::INVALID;
}

}