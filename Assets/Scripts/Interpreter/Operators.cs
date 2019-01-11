﻿public static class Operators {
    public const string EMPTY = "",
        EQUALS = "=",
        INCREMENT = "++",
        DECREMENT = "--",
        ADDITION = "+=",
        SUBTRACTION = "-=",
        MULTIPLICATION = "*=",
        DIVISION = "/=",
        MODULO = "%=",
        EQUAL_TO = "==",
        NOT_EQUAL = "!=",
        GREATER_THAN = ">",
        GREATER_THAN_EQUAL = ">=",
        LESS_THAN = "<",
        LESS_THAN_EQUAL = "<=",
        AND = "&&",
        OR = "||",
        MODULUS = "%",
        TIMES = "*",
        DIVIDE = "/",
        ADD = "+",
        SUBTRACT = "-",
        OPENING_BRACKET = "{",
        CLOSING_BRACKET = "}",
        OPENING_PARENTHESIS = "(",
        CLOSING_PARENTHESIS = ")",
        END_LINE = ";",

        BREAK = "break;",
        CONTINUE = "continue;",
        IF = "if",
        WHILE = "while",
        FOR = "for",
        LIBRARY_IMPORT = "using";

    public const char END_LINE_CHAR = ';';


    public static readonly string[][] PEMDAS = {
        new string[] { "%" },
        new string[] { "*", "/" },
        new string[] { "+", "-" },
        new string[] { "==", "!=", ">", ">=", "<", "<=" },
        new string[] { "&&" },
        new string[] { "||" }
    };

}