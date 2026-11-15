#pragma once

namespace BASIC::UTF {

struct DecodedCodePoint {
    char32_t codepoint : 24;
    char32_t uint_length : 8;
};

}