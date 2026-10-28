#pragma once

#include <lex/lex.h>

#include <print>
#include <string_view>

namespace ECPP {

class ECPP {
    public:
        void Test() {
            std::u8string_view text = u8"@ScriptEnd";
            BASIC::UTFCursor<char8_t> cursor{text};
            PARSER::Lexer lex{cursor};
            try {
                lex.NextToken();
            } catch (const char* p) {
                std::println("{}", p);
            }
            std::println("{}", static_cast<std::uint8_t>(lex.RefTok().cls));
        }

    private:

    
};

}