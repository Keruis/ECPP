#include <lexer/CharacterLiteralScanner.h>

#include <basic/ascii/CharClass.h>
#include <basic/diagnostic/DiagnosticEngine.h>

namespace LANG::LEXER {

LexStatus CharacterLiteralScanner::Scan() {
    const char8_t* const start = m_cursor.CurPtr();

    const ::BASIC::UTF::DecodedCodePoint c = m_cursor.Peek();
    if (c.codepoint != U'\'')
        return LexStatus::NOT_MATCHED;

    const LexStatus status = ScanCharacter();

    m_tok.kind = BASIC::TokenKind::LITERAL;
    m_tok.type = BASIC::TokenType::LITERALS_CHARACTER;
    m_tok.text = {start, m_cursor.CurPtr()};
    m_cursor.UpdateEndPosition();
    
    return status;
}

LexStatus CharacterLiteralScanner::ScanCharacter() {
    m_cursor.Advance(m_cursor.Peek());

    if (!m_cursor.AtEnd()) {
        ::BASIC::UTF::DecodedCodePoint c = m_cursor.Peek();

        if (c.codepoint == U'\'') {
            m_cursor.Advance(c);
            BASIC::DIAGNOSTIC::DiagnosticEngine::Report<
                BASIC::DIAGNOSTIC::DiagnosticID::EMPTY_CHARACTER_LITERAL
            >(m_cursor.Range());
            return LexStatus::INVALID;
        }

        while (!m_cursor.AtEnd()) {
            c = m_cursor.Peek();
            if (c.codepoint == U'\'') {
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
    }

    BASIC::DIAGNOSTIC::DiagnosticEngine::Report<
        BASIC::DIAGNOSTIC::DiagnosticID::UNTERMINATED_CHARACTER_LITERAL
    >(m_cursor.Range());
    return LexStatus::INVALID;
}

}