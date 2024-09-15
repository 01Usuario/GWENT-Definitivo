using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Compiler
{
    public enum TokenType { KeyWord, SpecialCharacter, String, Number, Operator, Identifier }
}
public class Token
{
    public Compiler.TokenType Type { get; private set; }
    public string Value { get; private set; }
    public Token(Compiler.TokenType type, string value)
    {
        Type = type;
        Value = value;
    }
}

public class Lexer
{
    private readonly string source;
    private int line;
    private int column;
    private int position;
    private static readonly HashSet<string> KeywordsList = new HashSet<string>
    {
        "effect", "card", "Name", "Params", "Action", "Type", "Faction", "Power", "Range",
        "OnActivation", "Selector", "Source", "Single", "Predicate", "PostAction", "for", "in", "while", "if"
    };
    private static readonly HashSet<char> SpecialCharactersList = new HashSet<char>
    {
        '(', ')', '{', '}', ',', ':', ';', '[', ']', '.'
    };

    private int openParentheses = 0;
    private int openBraces = 0;

    public Lexer(string source)
    {
        this.source = source;
        column = 1;
        line = 1;
        position = 0;
    }

    public List<Token> Tokenize()
    {
        List<Token> tokens = new List<Token>();
        while (position < source.Length)
        {
            SkipWhiteSpaces();
            if (position >= source.Length)
                break;

            char currentChar = source[position];
            if (char.IsLetter(currentChar))
                tokens.Add(IdentifierOrKeyword());
            else if (char.IsDigit(currentChar))
                tokens.Add(Number());
            else if (currentChar == '"')
                tokens.Add(String());
            else if (IsOperator(currentChar))
                tokens.Add(Operator());
            else if (SpecialCharactersList.Contains(currentChar))
            {
                tokens.Add(SpecialCharacter());
            }
            else
                throw new Exception($"Token inesperado: {currentChar} en la línea {line}, columna {column}");
        }

        if (openBraces != 0)
            throw new Exception($"Llaves desbalanceadas en la línea {line}, columna {column}");
        if (openParentheses != 0)
            throw new Exception($"Paréntesis desbalanceados en la línea {line}, columna {column}");

        // Agregar mensajes de depuración para verificar los tokens
        foreach (var token in tokens)
        {
            Debug.Log($"Token: {token.Value}, Tipo: {token.Type}");
        }

        return tokens;
    }

    private void SkipWhiteSpaces()
    {
        while (position < source.Length && char.IsWhiteSpace(source[position]))
        {
            if (source[position] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
            position++;
        }
    }

    private Token IdentifierOrKeyword()
    {
        int startPos = position;
        while (position < source.Length && (char.IsLetterOrDigit(source[position]) || source[position] == '_'))
        {
            position++;
            column++;
        }
        string value = source.Substring(startPos, position - startPos);

        // Si el siguiente carácter es un punto, tokenizarlo por separado
        if (position < source.Length && source[position] == '.')
        {
            return new Token(Compiler.TokenType.Identifier, value);
        }

        if (KeywordsList.Contains(value))
            return new Token(Compiler.TokenType.KeyWord, value);
        else
            return new Token(Compiler.TokenType.Identifier, value);
    }

    private Token Number()
    {
        int startPos = position;
        while (position < source.Length && char.IsDigit(source[position]))
        {
            position++;
            column++;
        }
        string value = source.Substring(startPos, position - startPos);
        return new Token(Compiler.TokenType.Number, value);
    }

    private Token String()
    {
        int startPos = position;
        position++;
        column++;
        while (position < source.Length && source[position] != '"')
        {
            position++;
            column++;
        }
        if (position >= source.Length)
            throw new Exception($"Literal de cadena sin cerrar en la línea {line}, columna {column}");
        string value = source.Substring(startPos + 1, position - startPos - 1);
        position++;
        column++;
        return new Token(Compiler.TokenType.String, value);
    }

    private Token Operator()
    {
        string twoCharOp = position + 1 < source.Length ? source.Substring(position, 2) : null;
        if (twoCharOp != null && new HashSet<string> { "++", "--", "&&", "||", "==", "!=", ">=", "<=", "@@", "=>", "-=" }.Contains(twoCharOp))
        {
            position += 2;
            column += 2;
            return new Token(Compiler.TokenType.Operator, twoCharOp);
        }

        char currentChar = source[position];
        position++;
        column++;
        return new Token(Compiler.TokenType.Operator, currentChar.ToString());
    }

    private bool IsOperator(char character)
    {
        return "+-*/^<>=!&|@".Contains(character);
    }

    private Token SpecialCharacter()
    {
        char currentChar = source[position];
        position++;
        column++;
        if (currentChar == '(')
            openParentheses++;
        else if (currentChar == ')')
            openParentheses--;
        else if (currentChar == '{')
            openBraces++;
        else if (currentChar == '}')
            openBraces--;
        return new Token(Compiler.TokenType.SpecialCharacter, currentChar.ToString());
    }
}
