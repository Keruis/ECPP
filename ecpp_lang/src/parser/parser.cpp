#include "parser/parser.h"

#include <basic/fail.h>

namespace ECPP::PARSER {

using namespace LEXER;

const LEXER::Token& Parser::RefTok() const noexcept {
    return m_lex.RefTok();
}

BASIC::Location Parser::Location() const noexcept {
    return m_lex.Location();
}

const Token& Parser::AdvanceRefTok() {
    m_lex.NextToken();
    return m_lex.RefTok();
}
    
bool Parser::IsEofToken() const noexcept {
    return RefTok().cls == TokenClass::END;
}

bool Parser::Is(TokenType type) const noexcept {
    return RefTok().type == type;
}

bool Parser::IsCls(TokenClass cls) const noexcept {
    return RefTok().cls == cls;
}

AST::DeclRef Parser::ParseDeclaration() {
    if (!IsCls(TokenClass::IDENTIFIER))
        BASIC::Fail("", Location());

}

AST::DeclRef Parser::ParseVariableDeclaration() {

}

AST::DeclRef Parser::ParseFunctionDeclaration() {

}

AST::TranslationUnit Parser::Parse() {
    AST::TranslationUnit uint;

    while (!IsEofToken()) {
        uint.Declarations().push_back(ParseDeclaration());
    }

    return uint;
}

}