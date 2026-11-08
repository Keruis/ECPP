#pragma once

#include <lex/lex.h>
#include <AST/decl/decl.h>
#include <AST/ast_resource.h>
#include <AST/translation_unit.h>

namespace ECPP::PARSER {

class Parser {
    public:
        Parser(LEXER::Lexer& lex, AST::ASTResource& resource)
            : m_lex(lex), m_resource(resource)
        {}

        AST::TranslationUnit Parse();
    
    private:
        const LEXER::Token& RefTok() const noexcept;
        BASIC::Location Location() const noexcept;
        const LEXER::Token& AdvanceRefTok();

        bool IsEofToken() const noexcept;
        bool Is(LEXER::TokenType) const noexcept;
        bool IsCls(LEXER::TokenClass cls) const noexcept;

        AST::DeclRef ParseDeclaration();
        AST::DeclRef ParseVariableDeclaration();
        AST::DeclRef ParseFunctionDeclaration();

    private:
        LEXER::Lexer& m_lex;
        AST::ASTResource& m_resource;
};
    
}