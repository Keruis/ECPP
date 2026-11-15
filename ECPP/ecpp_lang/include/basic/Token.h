#pragma once

#include <basic/TokenKind.h>

#include <string>
#include <string_view>

namespace LANG::BASIC {

struct Token {
    TokenKind kind = TokenKind::END;
    TokenType type = TokenType::END;

    std::u8string str;
    std::u8string_view text;
};

}