#include <lexer/TriviaSkipper.h>

#include <basic/ascii/CharClass.h>
#include <basic/diagnostic/DiagnosticEngine.h>

namespace LANG::LEXER {

::BASIC::UTF::DecodedCodePoint TriviaSkipper::SkipTrivia() {
    while (!m_cursor.AtEnd()) {
        ::BASIC::UTF::DecodedCodePoint c1 = m_cursor.Peek();
        if (::BASIC::ASCII::IsHorizontalWhitespace(c1.codepoint) || ::BASIC::ASCII::IsVerticalWhitespace(c1.codepoint)) {
            m_cursor.Advance(c1);
            continue;
        }

        if (c1.codepoint != U'/' || !m_cursor.HasRemaining(2))
            return c1;

        ::BASIC::UTF::DecodedCodePoint c2 = m_cursor.PeekNext();
        if (c2.codepoint == U'/') {
            m_cursor.Advance(c1, c2);
            SkipLineComment();
            continue;
        }

        if (c2.codepoint == U'*') {
            m_cursor.UpdateStartPosition();
            m_cursor.Advance(c1, c2);
            SkipBlockComment();
            continue;
        }

        return c1;
    }

    return {};
}

void TriviaSkipper::SkipLineComment() {
    while (!m_cursor.AtEnd()) {
        ::BASIC::UTF::DecodedCodePoint c = m_cursor.Peek();
        if (::BASIC::ASCII::IsVerticalWhitespace(c.codepoint))
            return;
        m_cursor.Advance(c);
    }
}

void TriviaSkipper::SkipBlockComment() {
    bool closed = false;

    while (!m_cursor.AtEnd()) {
        ::BASIC::UTF::DecodedCodePoint c1 = m_cursor.Peek();
        if (c1.codepoint == U'*' && m_cursor.HasRemaining(2)) {
            ::BASIC::UTF::DecodedCodePoint c2 = m_cursor.PeekNext();
            if (c2.codepoint == U'/') {
                m_cursor.Advance(c1, c2);
                closed = true;
                break;
            }
        }

        m_cursor.Advance(c1);
    }

    if (!closed) {
        m_cursor.UpdateEndPosition();
        BASIC::DIAGNOSTIC::DiagnosticEngine::Report<
            BASIC::DIAGNOSTIC::DiagnosticID::UNTERMINATED_BLOCK_COMMENT
        >(m_cursor.Range());
    }
}

}