#pragma once

#include <lexer/LexStatus.h>
#include <basic/source/SourceCursor.h>

namespace LANG::LEXER {

class UCNParser {
    public:
        explicit UCNParser(::BASIC::SOURCE::SourceCursor& cursor)
            : m_cursor(cursor)
        {}

        LexStatus ParseIdentifierUCN(::BASIC::UTF::DecodedCodePoint&);

    private:
        char32_t ParseHexCodepoint(std::size_t);

        void ParseDelimitedHexCodepoint(char32_t&);
        void ParseNamedCodepoint(char32_t&);
        void ValidateCodepoint(char32_t);
        void ReportInvalidUCN();

    private:
        ::BASIC::SOURCE::SourceCursor& m_cursor;
        LexStatus m_status = LexStatus::SUCCESS;
};

}