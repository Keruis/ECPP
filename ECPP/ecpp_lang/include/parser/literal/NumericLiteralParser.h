#pragma once

#include <basic/Token.h>
#include <AST/expr/LiteralExpr.h>

namespace LANG::PARSER {

class NumericLiteralParser {
    public:
        explicit NumericLiteralParser(const BASIC::Token& tok)
            : m_tok(tok)
        {}

        AST::NumericLiteralData Parse();

    private:
        const BASIC::Token& m_tok;
};

}