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
    // Enum que define los tipos de tokens que se pueden encontrar en el c�digo fuente.
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
    // C�digo fuente que se va a analizar.
    private readonly string source;
    // L�nea actual en el c�digo fuente.
    private int line;
    // Columna actual en el c�digo fuente.
    private int column;
    // Posici�n actual en el c�digo fuente.
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

    // Contador de par�ntesis abiertos.
    private int openParentheses = 0;
    // Contador de llaves abiertas.
    private int openBraces = 0;

    // Constructor que inicializa el lexer con el c�digo fuente.
    public Lexer(string source)
    {
        this.source = source;
        column = 1;
        line = 1;
        position = 0;
    }

    // M�todo que tokeniza el c�digo fuente.
    public List<Token> Tokenize()
    {
        // Lista para almacenar los tokens generados.
        List<Token> tokens = new List<Token>();
        // Mientras no se haya alcanzado el final del c�digo fuente.
        while (position < source.Length)
        {
            // Omitir espacios en blanco.
            SkipWhiteSpaces();
            // Si se alcanz� el final del c�digo fuente, salir del bucle.
            if (position >= source.Length)
                break;

            // Obtener el car�cter actual.
            char currentChar = source[position];
            // Si el car�cter es una letra, identificarlo como un identificador o palabra clave.
            if (char.IsLetter(currentChar))
                tokens.Add(IdentifierOrKeyword());
            // Si el car�cter es un d�gito, identificarlo como un n�mero.
            else if (char.IsDigit(currentChar))
                tokens.Add(Number());
            // Si el car�cter es una comilla doble, identificarlo como una cadena.
            else if (currentChar == '"')
                tokens.Add(String());
            // Si el car�cter es un operador, identificarlo como un operador.
            else if (IsOperator(currentChar))
                tokens.Add(Operator());
            // Si el car�cter es un car�cter especial, identificarlo como un car�cter especial.
            else if (SpecialCharactersList.Contains(currentChar))
            {
                tokens.Add(SpecialCharacter());
            }
            // Si el car�cter no es reconocido, lanzar una excepci�n.
            else
                throw new Exception($"Token inesperado: {currentChar} en la l�nea {line}, columna {column}");
        }

        // Verificar si hay llaves desbalanceadas.
        if (openBraces != 0)
            throw new Exception($"Llaves desbalanceadas en la l�nea {line}, columna {column}");
        // Verificar si hay par�ntesis desbalanceados.
        if (openParentheses != 0)
            throw new Exception($"Par�ntesis desbalanceados en la l�nea {line}, columna {column}");

        // Agregar mensajes de depuraci�n para verificar los tokens.
        foreach (var token in tokens)
        {
            Debug.Log($"Token: {token.Value}, Tipo: {token.Type}");
        }

        // Devolver la lista de tokens generados.
        return tokens;
    }

    // M�todo que omite los espacios en blanco en el c�digo fuente.
    private void SkipWhiteSpaces()
    {
        // Mientras no se haya alcanzado el final del c�digo fuente y el car�cter actual sea un espacio en blanco.
        while (position < source.Length && char.IsWhiteSpace(source[position]))
        {
            // Si el car�cter actual es un salto de l�nea, incrementar la l�nea y reiniciar la columna.
            if (source[position] == '\n')
            {
                line++;
                column = 1;
            }
            // Si el car�cter actual no es un salto de l�nea, incrementar la columna.
            else
            {
                column++;
            }
            // Incrementar la posici�n.
            position++;
        }
    }

    // M�todo que identifica si una secuencia de caracteres es un identificador o una palabra clave.
    private Token IdentifierOrKeyword()
    {
        // Guardar la posici�n inicial.
        int startPos = position;
        // Mientras no se haya alcanzado el final del c�digo fuente y el car�cter actual sea una letra, d�gito o guion bajo.
        while (position < source.Length && char.IsLetterOrDigit(source[position]) )
        {
            position++;
            column++;
        }
        // Obtener el valor del identificador o palabra clave.
        string value = source.Substring(startPos, position - startPos);

        // Si el siguiente car�cter es un punto, tokenizarlo por separado.
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

    // M�todo que tokeniza un n�mero.
    private Token Number()
    {
        // Guardar la posici�n inicial.
        int startPos = position;
        // Mientras no se haya alcanzado el final del c�digo fuente y el car�cter actual sea un d�gito.
        while (position < source.Length && char.IsDigit(source[position]))
        {
            position++;
            column++;
        }
        // Obtener el valor del n�mero.
        string value = source.Substring(startPos, position - startPos);
        // Devolver un token de tipo n�mero.
        return new Token(Compiler.TokenType.Number, value);
    }

    // M�todo que tokeniza una cadena de texto.
    private Token String()
    {
        // Guardar la posici�n inicial.
        int startPos = position;
        // Incrementar la posici�n y la columna para omitir la comilla inicial.
        position++;
        column++;
        // Mientras no se haya alcanzado el final del c�digo fuente y el car�cter actual no sea una comilla.
        while (position < source.Length && source[position] != '"')
        {
            position++;
            column++;
        }
        // Si se alcanz� el final del c�digo fuente sin encontrar una comilla de cierre, lanzar una excepci�n.
        if (position >= source.Length)
            throw new Exception($"Literal de cadena sin cerrar en la l�nea {line}, columna {column}");
        // Obtener el valor de la cadena.
        string value = source.Substring(startPos + 1, position - startPos - 1);
        // Incrementar la posici�n y la columna para omitir la comilla de cierre.
        position++;
        column++;
        // Devolver un token de tipo cadena.
        return new Token(Compiler.TokenType.String, value);
    }

    // M�todo que tokeniza un operador.
    private Token Operator()
    {
        // Obtener un posible operador de dos caracteres.
        string twoCharOp = position + 1 < source.Length ? source.Substring(position, 2) : null;
        // Si el operador de dos caracteres es v�lido, devolver un token de tipo operador.
        if (twoCharOp != null && new HashSet<string> { "++", "--", "&&", "||", "==", "!=", ">=", "<=", "@@", "=>", "-=" }.Contains(twoCharOp))
        {
            position += 2;
            column += 2;
            return new Token(Compiler.TokenType.Operator, twoCharOp);
        }

        // Obtener el car�cter actual.
        char currentChar = source[position];
        // Incrementar la posici�n y la columna.
        position++;
        column++;
        // Devolver un token de tipo operador.
        return new Token(Compiler.TokenType.Operator, currentChar.ToString());
    }

    // M�todo que verifica si un car�cter es un operador.
    private bool IsOperator(char character)
    {
        // Verificar si el car�cter est� en la lista de operadores.
        return "+-*/^<>=!&|@".Contains(character);
    }

    // M�todo que tokeniza un car�cter especial.
    private Token SpecialCharacter()
    {
        // Obtener el car�cter actual.
        char currentChar = source[position];
        // Incrementar la posici�n y la columna.
        position++;
        column++;
        // Incrementar o decrementar los contadores de par�ntesis y llaves seg�n el car�cter.
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