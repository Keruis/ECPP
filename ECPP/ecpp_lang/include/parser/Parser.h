#pragma once

#include <sema/Sema.h>
#include <lexer/Lexer.h>
#include <AST/expr/Expr.h>
#include <AST/ASTResource.h>
#include <AST/TranslationUnit.h>
#include <AST/top_level/stmt/Stmt.h>

namespace LANG::PARSER {

class Parser {
    public:
        explicit Parser(LEXER::Lexer& lexer, SEMA::Sema& sema)
            : m_lexer(lexer), m_sema(sema)
        {}

        AST::TranslationUnit Parse();

    private:
        bool IsKind(BASIC::TokenKind) const noexcept;
        bool IsType(BASIC::TokenType) const noexcept;

        AST::TopLevelItemRef ParseTopLevelItem();

        AST::StmtRef ParseStatement();
        AST::StmtRef ParseBlockStatement();

        AST::ExprRef ParseExpression();
        AST::ExprRef ParsePrefixExpression();
        AST::ExprRef ParsePostfixExpression();
        AST::ExprRef ParsePrimaryExpression();
        AST::ExprRef ParseLiteralExpression();

    private:
        LEXER::Lexer& m_lexer;
        SEMA::Sema& m_sema;
};

}