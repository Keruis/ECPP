#include <lexer/IdentifierScanner.h>

#include <basic/PP.h>
#include <utility/Predicate.h>
#include <basic/unicode/UnicodeClass.h>
#include <lexer/UniversalCharacterName.h>
#include <basic/diagnostic/DiagnosticEngine.h>

namespace LANG::LEXER {

LexStatus IdentifierScanner::Scan() {
    const char8_t* const start = m_cursor.CurPtr();

    const LexStatus start_status = ScanIdentifierStart();

    if (start_status == LexStatus::NOT_MATCHED)
        return LexStatus::NOT_MATCHED;

    const LexStatus continue_status = ScanIdentifierContinue();

    m_tok.kind = BASIC::TokenKind::IDENTIFIER;
    m_tok.type = m_has_ucn ? BASIC::TokenType::IDENTIFIER_UCN : BASIC::TokenType::IDENTIFIER;
    m_tok.text = {start, m_cursor.CurPtr()};
    m_cursor.UpdateEndPosition();

    return UTILITY::IsAnyOf(LexStatus::INVALID, start_status, continue_status)
            ? LexStatus::INVALID
            : LexStatus::SUCCESS;
}

LexStatus IdentifierScanner::ScanIdentifierStart() {
    ::BASIC::UTF::DecodedCodePoint c{};
    switch (UCNParser{m_cursor}.ParseIdentifierUCN(c)) {
        case LexStatus::INVALID:
            return LexStatus::INVALID;

        case LexStatus::SUCCESS:
            if (!BASIC::IsIdentifierStart(c.codepoint)) {
                BASIC::DIAGNOSTIC::DiagnosticEngine::Report<
                    BASIC::DIAGNOSTIC::DiagnosticID::INVALID_IDENTIFIER_CHARACTER
                >(m_cursor.Range(), static_cast<std::uint32_t>(c.codepoint));
                return LexStatus::INVALID;
            }

            m_has_ucn = true;
            return LexStatus::SUCCESS;

        case LexStatus::NOT_MATCHED:
    }

    c = m_cursor.Peek();
    if (BASIC::IsIdentifierStart(c.codepoint)) {
        m_cursor.Advance(c);
        return LexStatus::SUCCESS;
    }

    return LexStatus::NOT_MATCHED;
}

LexStatus IdentifierScanner::ScanIdentifierContinue() {
    LexStatus status = LexStatus::SUCCESS;
    
    while (!m_cursor.AtEnd()) {
        ::BASIC::UTF::DecodedCodePoint c{};
        switch (UCNParser{m_cursor}.ParseIdentifierUCN(c)) {
            case LexStatus::INVALID:
                status = LexStatus::INVALID;
                continue;

            case LexStatus::SUCCESS:
                if (!BASIC::IsIdentifierContinue(c.codepoint)) {
                    status = LexStatus::INVALID;

                    BASIC::DIAGNOSTIC::DiagnosticEngine::Report<
                        BASIC::DIAGNOSTIC::DiagnosticID::INVALID_IDENTIFIER_CHARACTER
                    >(m_cursor.Range(), static_cast<std::uint32_t>(c.codepoint));
                    continue;;
                }

                m_has_ucn = true;
                continue;

            case LexStatus::NOT_MATCHED:
        }

        c = m_cursor.Peek();
        if (BASIC::IsIdentifierContinue(c.codepoint)) {
            m_cursor.Advance(c);
            continue;
        }

        break;
    }

    return status;
}

}