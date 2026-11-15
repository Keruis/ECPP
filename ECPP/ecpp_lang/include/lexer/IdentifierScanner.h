#pragma once

#include <basic/Token.h>
#include <lexer/LexStatus.h>
#include <basic/source/SourceCursor.h>

namespace LANG::LEXER {

class IdentifierScanner {
    public:
        explicit IdentifierScanner(::BASIC::SOURCE::SourceCursor& cursor, BASIC::Token& tok)
            : m_cursor(cursor), m_tok(tok)
        {}

        LexStatus Scan();

    private:
        LexStatus ScanIdentifierStart();
        LexStatus ScanIdentifierContinue();

    private:
        bool m_has_ucn{};
        ::BASIC::SOURCE::SourceCursor& m_cursor;
        BASIC::Token& m_tok;
};
    
}