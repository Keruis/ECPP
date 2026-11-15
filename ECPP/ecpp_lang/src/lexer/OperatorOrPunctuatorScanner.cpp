#include <lexer/OperatorOrPunctuatorScanner.h>

namespace LANG::LEXER {

LexStatus OperatorOrPunctuatorScanner::Scan() {
    const ::BASIC::UTF::DecodedCodePoint c = m_cursor.Peek();
    
    switch (c.codepoint) {
        case U'!': case U'=': 
        case U'*': case U'/': 
        case U'%':
            ScanEqualsSuffixOperator();
            break;

        case U'>': case U'<':
        case U'+': case U'-':
            ScanRepeatedCharOperator();
            break;

        case U'(': case U')':
        case U'{': case U'}':
        case U'[': case U']':
        case U',': case U':':
        case U';':
            ScanPunctuation();
            break;

        default:
            return LexStatus::NOT_MATCHED;
    }

    return LexStatus::SUCCESS;
}

void OperatorOrPunctuatorScanner::ScanEqualsSuffixOperator() {
    const ::BASIC::UTF::DecodedCodePoint c = m_cursor.Peek();
    const bool equals_suffix = m_cursor.HasRemaining(2) && m_cursor.PeekNext().codepoint == U'=';
    const std::size_t offset = 1 + equals_suffix;
    m_cursor.AdvanceBytes(offset);

    m_tok.kind = BASIC::TokenKind::OPERATOR;
    m_tok.type = static_cast<BASIC::TokenType>(c.codepoint + equals_suffix * BASIC::EqualsSuffixOperatorOffset);
    
    m_cursor.UpdateEndPosition();
}

void OperatorOrPunctuatorScanner::ScanRepeatedCharOperator() {
    const ::BASIC::UTF::DecodedCodePoint c = m_cursor.Peek();
    char32_t next_cp = 0;
    if (m_cursor.HasRemaining(2))
        next_cp = m_cursor.PeekNext().codepoint;
    
    const bool repeat = next_cp == c.codepoint;
    const bool equals_suffix = next_cp == U'=';
    const std::size_t offset = 1 + (repeat || equals_suffix);
    m_cursor.AdvanceBytes(offset);
    
    m_tok.kind = BASIC::TokenKind::OPERATOR;
    m_tok.type = static_cast<BASIC::TokenType>(c.codepoint + repeat * BASIC::RepeatedCharOperatorOffset + equals_suffix * BASIC::EqualsSuffixOperatorOffset);

    m_cursor.UpdateEndPosition();
}

void OperatorOrPunctuatorScanner::ScanPunctuation() {
    const ::BASIC::UTF::DecodedCodePoint c = m_cursor.Peek();

    m_cursor.Advance(c);

    m_tok.kind = BASIC::TokenKind::PUNCTUATOR;
    m_tok.type = static_cast<BASIC::TokenType>(c.codepoint);

    m_cursor.UpdateEndPosition();
}

}