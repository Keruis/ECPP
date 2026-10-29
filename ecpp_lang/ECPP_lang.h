#pragma once

#include <lex/lex.h>

#include <print>
#include <string_view>

namespace ECPP {

class ECPP {
    public:
        void Test() {
            std::u8string_view text = u8"\"123\\d{97}\"";
            BASIC::UTFCursor<char8_t> cursor{text};
            BASIC::Location location{};
            LEXER::Lexer lex{cursor, location};
            try {
                lex.NextToken();
            } catch (const char* p) {
                std::println("{}", p);
            }
            std::println("{}", std::string_view{reinterpret_cast<const char*>(lex.RefTok().str.data()), lex.RefTok().str.size()});
        }

    private:

    
};

}