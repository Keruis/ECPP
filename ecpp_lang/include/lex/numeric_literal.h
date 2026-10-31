#pragma omce

#include <basic/utf/utf_view.h>

namespace ECPP::LEXER {

class NumericLiteralParser {
    public:
        NumericLiteralParser(const BASIC::UTFCursor<char8_t>& source)
            : m_source(source)
        {}

        
    private:


    private:
        const BASIC::UTFCursor<char8_t>& m_source; 
};

}