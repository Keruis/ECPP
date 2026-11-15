#pragma once

#include <basic/source/SourceCursor.h>

namespace LANG::LEXER {

class TriviaSkipper {
    public:
        explicit TriviaSkipper(::BASIC::SOURCE::SourceCursor& cursor)
            : m_cursor(cursor)
        {}

        ::BASIC::UTF::DecodedCodePoint SkipTrivia();

    private:
        void SkipLineComment();
        void SkipBlockComment();

    private:
        ::BASIC::SOURCE::SourceCursor& m_cursor;
};

}