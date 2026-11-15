#pragma once

#include <basic/Token.h>
#include <lexer/LexStatus.h>
#include <basic/source/SourceCursor.h>

namespace LANG::LEXER {

class CharacterLiteralScanner {
    public:
        CharacterLiteralScanner(::BASIC::SOURCE::SourceCursor& cursor, BASIC::Token& tok)
            : m_cursor(cursor), m_tok(tok)
        {}

        LexStatus Scan();

    private:
        LexStatus ScanCharacter();

    private:
        ::BASIC::SOURCE::SourceCursor& m_cursor;
        BASIC::Token& m_tok;
};

}