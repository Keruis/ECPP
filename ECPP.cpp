#include <print>
#include <string_view>

#include "lex.h"

int main() {
    std::string_view text = R"(
        10u16
    )";
    ECPP::PARSER::Lexer lex{text};
    try {
        lex.NextToken();
    } catch (const char* p) {
        std::println("{}", p);
    }
    std::println("{}", lex.RefTok().text);
}