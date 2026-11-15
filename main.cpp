#include "ECPP/ecpp_lang/include/lexer/Lexer.h"
#include <string_view>
#include <print>

int main() {
    std::u8string_view text = u8"123466_4\\u+12";
    BASIC::SOURCE::SourceCursor cur(text);
    LANG::LEXER::Lexer lex(cur);
    lex.NextToken();
    std::println("{}", std::string_view{reinterpret_cast<const char*>(lex.RefToken().text.data()), lex.RefToken().text.size()});
}