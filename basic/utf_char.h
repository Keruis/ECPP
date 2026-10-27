#pragma once

namespace ECPP::BASIC {

struct UTFChar {
    char32_t codepoint : 24;
    char32_t length : 8;
};

}