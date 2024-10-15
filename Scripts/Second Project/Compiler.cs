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
    // Enum que define los tipos de tokens que se pueden encontrar en el código fuente.
    public enum TokenType { KeyWord, SpecialCharacter, String, Number, Operator, Identifier }
}

public class Token
{
    // Propiedad que almacena el tipo de token.
    public Compiler.TokenType Type { get; private set; }
    // Propiedad que almacena el valor del token.
    public string Value { get; private set; }

    // Constructor que inicializa un token con su tipo y valor.
    public Token(Compiler.TokenType type, string value)
    {
        Type = type;
        Value = value;
    }
}

public class Lexer
{
    // Código fuente que se va a analizar.
    private readonly string source;
    // Línea actual en el código fuente.
    private int line;
    // Columna actual en el código fuente.
    private int column;
    // Posición actual en el código fuente.
    private int position;

    // Conjunto de palabras clave reconocidas por el lexer.
    private static readonly HashSet<string> KeywordsList = new HashSet<string>
    {
        "effect", "card", "Name", "Params", "Action", "Type", "Faction", "Power", "Range",
        "OnActivation", "Selector", "Source", "Single", "Predicate", "PostAction", "for", "in", "while", "if"
    };

    // Conjunto de caracteres especiales reconocidos por el lexer.
    private static readonly HashSet<char> SpecialCharactersList = new HashSet<char>
    {
        '(', ')', '{', '}', ',', ':', ';', '[', ']', '.'
    };

    // Contador de paréntesis abiertos.
    private int openParentheses = 0;
    // Contador de llaves abiertas.
    private int openBraces = 0;

    // Constructor que inicializa el lexer con el código fuente.
    public Lexer(string source)
    {
        this.source = source;
        column = 1;
        line = 1;
        position = 0;
    }

    // Método que tokeniza el código fuente.
    public List<Token> Tokenize()
    {
        // Lista para almacenar los tokens generados.
        List<Token> tokens = new List<Token>();
        // Mientras no se haya alcanzado el final del código fuente.
        while (position < source.Length)
        {
            // Omitir espacios en blanco.
            SkipWhiteSpaces();
            // Si se alcanzó el final del código fuente, salir del bucle.
            if (position >= source.Length)
                break;

            // Obtener el carácter actual.
            char currentChar = source[position];
            // Si el carácter es una letra, identificarlo como un identificador o palabra clave.
            if (char.IsLetter(currentChar))
                tokens.Add(IdentifierOrKeyword());
            // Si el carácter es un dígito, identificarlo como un número.
            else if (char.IsDigit(currentChar))
                tokens.Add(Number());
            // Si el carácter es una comilla doble, identificarlo como una cadena.
            else if (currentChar == '"')
                tokens.Add(String());
            // Si el carácter es un operador, identificarlo como un operador.
            else if (IsOperator(currentChar))
                tokens.Add(Operator());
            // Si el carácter es un carácter especial, identificarlo como un carácter especial.
            else if (SpecialCharactersList.Contains(currentChar))
            {
                tokens.Add(SpecialCharacter());
            }
            // Si el carácter no es reconocido, lanzar una excepción.
            else
                throw new Exception($"Token inesperado: {currentChar} en la línea {line}, columna {column}");
        }

        // Verificar si hay llaves desbalanceadas.
        if (openBraces != 0)
            throw new Exception($"Llaves desbalanceadas en la línea {line}, columna {column}");
        // Verificar si hay paréntesis desbalanceados.
        if (openParentheses != 0)
            throw new Exception($"Paréntesis desbalanceados en la línea {line}, columna {column}");

        // Agregar mensajes de depuración para verificar los tokens.
        foreach (var token in tokens)
        {
            Debug.Log($"Token: {token.Value}, Tipo: {token.Type}");
        }

        // Devolver la lista de tokens generados.
        return tokens;
    }

    // Método que omite los espacios en blanco en el código fuente.
    private void SkipWhiteSpaces()
    {
        // Mientras no se haya alcanzado el final del código fuente y el carácter actual sea un espacio en blanco.
        while (position < source.Length && char.IsWhiteSpace(source[position]))
        {
            // Si el carácter actual es un salto de línea, incrementar la línea y reiniciar la columna.
            if (source[position] == '\n')
            {
                line++;
                column = 1;
            }
            // Si el carácter actual no es un salto de línea, incrementar la columna.
            else
            {
                column++;
            }
            // Incrementar la posición.
            position++;
        }
    }

    // Método que identifica si una secuencia de caracteres es un identificador o una palabra clave.
    private Token IdentifierOrKeyword()
    {
        // Guardar la posición inicial.
        int startPos = position;
        // Mientras no se haya alcanzado el final del código fuente y el carácter actual sea una letra, dígito o guion bajo.
        while (position < source.Length && char.IsLetterOrDigit(source[position]) )
        {
            position++;
            column++;
        }
        // Obtener el valor del identificador o palabra clave.
        string value = source.Substring(startPos, position - startPos);

        // Si el siguiente carácter es un punto, tokenizarlo por separado.
        if (position < source.Length && source[position] == '.')
        {
            return new Token(Compiler.TokenType.Identifier, value);
        }

        // Si el valor es una palabra clave, devolver un token de tipo palabra clave.
        if (KeywordsList.Contains(value))
            return new Token(Compiler.TokenType.KeyWord, value);
        // Si no, devolver un token de tipo identificador.
        else
            return new Token(Compiler.TokenType.Identifier, value);
    }

    // Método que tokeniza un número.
    private Token Number()
    {
        // Guardar la posición inicial.
        int startPos = position;
        // Mientras no se haya alcanzado el final del código fuente y el carácter actual sea un dígito.
        while (position < source.Length && char.IsDigit(source[position]))
        {
            position++;
            column++;
        }
        // Obtener el valor del número.
        string value = source.Substring(startPos, position - startPos);
        // Devolver un token de tipo número.
        return new Token(Compiler.TokenType.Number, value);
    }

    // Método que tokeniza una cadena de texto.
    private Token String()
    {
        // Guardar la posición inicial.
        int startPos = position;
        // Incrementar la posición y la columna para omitir la comilla inicial.
        position++;
        column++;
        // Mientras no se haya alcanzado el final del código fuente y el carácter actual no sea una comilla.
        while (position < source.Length && source[position] != '"')
        {
            position++;
            column++;
        }
        // Si se alcanzó el final del código fuente sin encontrar una comilla de cierre, lanzar una excepción.
        if (position >= source.Length)
            throw new Exception($"Literal de cadena sin cerrar en la línea {line}, columna {column}");
        // Obtener el valor de la cadena.
        string value = source.Substring(startPos + 1, position - startPos - 1);
        // Incrementar la posición y la columna para omitir la comilla de cierre.
        position++;
        column++;
        // Devolver un token de tipo cadena.
        return new Token(Compiler.TokenType.String, value);
    }

    // Método que tokeniza un operador.
    private Token Operator()
    {
        // Obtener un posible operador de dos caracteres.
        string twoCharOp = position + 1 < source.Length ? source.Substring(position, 2) : null;
        // Si el operador de dos caracteres es válido, devolver un token de tipo operador.
        if (twoCharOp != null && new HashSet<string> { "++", "--", "&&", "||", "==", "!=", ">=", "<=", "@@", "=>", "-=" }.Contains(twoCharOp))
        {
            position += 2;
            column += 2;
            return new Token(Compiler.TokenType.Operator, twoCharOp);
        }

        // Obtener el carácter actual.
        char currentChar = source[position];
        // Incrementar la posición y la columna.
        position++;
        column++;
        // Devolver un token de tipo operador.
        return new Token(Compiler.TokenType.Operator, currentChar.ToString());
    }

    // Método que verifica si un carácter es un operador.
    private bool IsOperator(char character)
    {
        // Verificar si el carácter está en la lista de operadores.
        return "+-*/^<>=!&|@".Contains(character);
    }

    // Método que tokeniza un carácter especial.
    private Token SpecialCharacter()
    {
        // Obtener el carácter actual.
        char currentChar = source[position];
        // Incrementar la posición y la columna.
        position++;
        column++;
        // Incrementar o decrementar los contadores de paréntesis y llaves según el carácter.
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