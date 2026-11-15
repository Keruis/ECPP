#pragma once

#include <basic/Token.h>
#include <lexer/LexStatus.h>
#include <basic/source/SourceCursor.h>

namespace LANG::LEXER {

class NumericLiteralScanner {
    public:
        explicit NumericLiteralScanner(::BASIC::SOURCE::SourceCursor& cursor, BASIC::Token& tok)
            : m_cursor(cursor), m_tok(tok)
        {}

        LexStatus Scan();

    private:
        const char8_t* ScanPPNumber();

    private:
        ::BASIC::SOURCE::SourceCursor& m_cursor;
        BASIC::Token& m_tok;
};

}