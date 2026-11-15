#pragma once

#include <basic/Token.h>
#include <lexer/LexStatus.h>
#include <basic/source/SourceCursor.h>

namespace LANG::LEXER {

class OperatorOrPunctuatorScanner {
    public:
        OperatorOrPunctuatorScanner(::BASIC::SOURCE::SourceCursor& cursor, BASIC::Token& tok)
            : m_cursor(cursor), m_tok(tok)
        {}

        LexStatus Scan();

    private:
        void ScanEqualsSuffixOperator();
        void ScanRepeatedCharOperator();
        void ScanPunctuation();

    private:
        ::BASIC::SOURCE::SourceCursor& m_cursor;
        BASIC::Token& m_tok;
};
    
}