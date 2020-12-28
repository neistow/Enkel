namespace Enkel.Core.Lexer
{
    public enum TokenType
    {
        Class,
        If,
        Else,
        And,
        Or,
        True,
        False,
        Func,
        For,
        While,
        None,
        Return,
        Parent,
        This,
        Var,


        LeftParen,
        RightParen,
        LeftBrace,
        RightBrace,
        Comma,
        Dot,
        Minus,
        Plus,
        Semicolon,
        Slash,
        Star,


        BangEqual,
        DoubleEqual,
        Bang,
        Equal,
        GreaterEqual,
        LessEqual,
        Greater,
        Less,


        Identifier,
        String,
        Number,
        
        Eof
    }
}