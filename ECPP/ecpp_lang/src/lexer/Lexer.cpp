#include <lexer/Lexer.h>

#include <lexer/TriviaSkipper.h>
#include <basic/ascii/CharClass.h>
#include <lexer/IdentifierScanner.h>
#include <lexer/StringLiteralScanner.h>
#include <basic/unicode/UnicodeClass.h>
#include <lexer/NumericLiteralScanner.h>
#include <lexer/CharacterLiteralScanner.h>
#include <basic/diagnostic/DiagnosticEngine.h>
#include <lexer/OperatorOrPunctuatorScanner.h>

namespace LANG::LEXER {

void Lexer::UpdateToken(const BASIC::TokenKind kind, const BASIC::TokenType type, const std::u8string_view text = {}) noexcept {
    m_tok.kind = kind;
    m_tok.type = type;
    m_tok.text = text;
    m_cursor.UpdateEndPosition();
}

BASIC::TokenType Lexer::FindKeyword() const noexcept {
    using enum BASIC::TokenType;
    
    const std::u8string_view text = m_tok.text;
    if (text == u8"if")
        return KEYWORD_IF;
    if (text == u8"else")
        return KEYWORD_ELSE;
    if (text == u8"true")
        return KEYWORD_TRUE;
    if (text == u8"false")
        return KEYWORD_FALSE;

    return m_tok.type;
}

void Lexer::NextToken() {
    const ::BASIC::UTF::DecodedCodePoint c = TriviaSkipper{m_cursor}.SkipTrivia();
    m_cursor.UpdateStartPosition();

    if (m_cursor.AtEnd()) {
        UpdateToken(
            BASIC::TokenKind::END,
            BASIC::TokenType::END
        );

        return;
    }

    if (NumericLiteralScanner{m_cursor, m_tok}.Scan() != LexStatus::NOT_MATCHED)
        return;

    switch (IdentifierScanner{m_cursor, m_tok}.Scan()) {
        case LexStatus::INVALID:
            return;

        case LexStatus::SUCCESS:
            m_tok.type = FindKeyword();
            return;

        case LexStatus::NOT_MATCHED:
    }

    if (StringLiteralScanner{m_cursor, m_tok}.Scan() != LexStatus::NOT_MATCHED)
        return;

    if (CharacterLiteralScanner{m_cursor, m_tok}.Scan() != LexStatus::NOT_MATCHED)
        return;

    if (OperatorOrPunctuatorScanner{m_cursor, m_tok}.Scan() != LexStatus::NOT_MATCHED)
        return;

    BASIC::DIAGNOSTIC::DiagnosticEngine::Report<
        BASIC::DIAGNOSTIC::DiagnosticID::INVALID_CHARACTER
    >(m_cursor.Range(), std::string_view{reinterpret_cast<const char*>(m_cursor.CurPtr()), c.uint_length});

    m_cursor.Advance(c);
    m_cursor.UpdateEndPosition();
}

const BASIC::Token& Lexer::RefToken() const noexcept {
    return m_tok;
}

const ::BASIC::SOURCE::SourceRange Lexer::RefRange() const noexcept {
    return m_cursor.Range();
}

}