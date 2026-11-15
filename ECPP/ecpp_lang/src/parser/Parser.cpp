#include <parser/Parser.h>

namespace LANG::PARSER {

bool Parser::IsKind(BASIC::TokenKind kind) const noexcept {
    return m_lexer.RefToken().kind == kind;
}

bool Parser::IsType(BASIC::TokenType type) const noexcept {
    return m_lexer.RefToken().type == type;
}

AST::TranslationUnit Parser::Parse() {
    AST::TranslationUnit unit;

    while (!IsKind(BASIC::TokenKind::END)) {
        unit.Items().push_back(ParseTopLevelItem());
    }

    return unit;
}

AST::TopLevelItemRef Parser::ParseTopLevelItem() {
    
}

}