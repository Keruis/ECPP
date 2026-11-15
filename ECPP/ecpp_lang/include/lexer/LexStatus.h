#pragma once

#include <cstdint>

namespace LANG::LEXER {

enum class LexStatus : std::uint8_t {
    NOT_MATCHED,
    INVALID,
    SUCCESS
};

}