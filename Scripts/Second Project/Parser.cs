using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class Parser
{
    // Lista de tokens a analizar.
    private List<Token> tokens;
    // Posici�n actual en la lista de tokens.
    private int position;
    // L�nea actual en el c�digo fuente.
    private int line;
    // Columna actual en el c�digo fuente.
    private int column;
    // Diccionario para almacenar los efectos definidos.
    private Dictionary<string, EffectNode> definedEffects = new Dictionary<string, EffectNode>();

    // Constructor que inicializa el parser con la lista de tokens.
    public Parser(List<Token> tokens)
    {
        // Asigna la lista de tokens al campo tokens.
        this.tokens = tokens;
        // Inicializa la posici�n a 0.
        position = 0;
        // Inicializa la l�nea a 1.
        line = 1;
        // Inicializa la columna a 1.
        column = 1;
    }

    // M�todo principal para analizar el c�digo fuente y generar el AST.
    public List<NodeAST> Parse()
    {
        // Lista para almacenar los nodos del AST.
        List<NodeAST> nodes = new List<NodeAST>();

        // Mientras no se haya alcanzado el final de la lista de tokens.
        while (position < tokens.Count)
        {
            // Si el token actual es una palabra clave.
            if (MatchToken(Compiler.TokenType.KeyWord))
            {
                // Obtener el valor de la palabra clave.
                var keyword = CurrentToken().Value;
                // Si la palabra clave es "effect".
                if (keyword == "effect")
                {
                    // Analizar el nodo de efecto.
                    var effectNode = ParseEffect();

                    // Agregar el nodo de efecto a la lista de nodos.
                    nodes.Add(effectNode);
                }
                // Si la palabra clave es "card".
                else if (keyword == "card")
                {
                    // Analizar el nodo de carta y agregarlo a la lista de nodos.
                    nodes.Add(ParseCard());
                }
                // Si la palabra clave no es reconocida, lanzar una excepci�n.
                else
                {
                    throw new Exception($"Token inesperado: {CurrentToken().Value} en la l�nea {line}, columna {column}");
                }
            }
            // Si el token actual no es una palabra clave, lanzar una excepci�n.
            else
            {
                throw new Exception($"Token inesperado: {CurrentToken().Value} en la l�nea {line}, columna {column}");
            }
        }

        // Devolver la lista de nodos del AST.
        return nodes;
    }

    // Obtener el token actual.
    private Token CurrentToken()
    {
        // Devuelve el token en la posici�n actual.
        return tokens[position];
    }

    // Obtener el siguiente token y actualizar la posici�n.
    private Token NextToken()
    {
        // Obtener el token actual.
        var token = tokens[position];
        // Incrementar la posici�n.
        position++;

        // Actualizar la columna y la l�nea correctamente.
        int newLines = token.Value.Count(c => c == '\n');
        if (newLines > 0)
        {
            // Si el token contiene saltos de l�nea, incrementar la l�nea y ajustar la columna.
            line += newLines;
            column = token.Value.Length - token.Value.LastIndexOf('\n');
        }
        else
        {
            // Si no contiene saltos de l�nea, incrementar la columna.
            column += token.Value.Length;
        }

        // Mostrar un mensaje de depuraci�n con el siguiente token.
        Debug.Log($"Siguiente Token: {token.Value}, Type: {token.Type}, Line: {line}, Column: {column}");
        return token;
    }

    // Verificar si el token actual coincide con el tipo esperado.
    private bool MatchToken(Compiler.TokenType type)
    {
        // Devuelve true si el tipo del token actual coincide con el tipo esperado.
        return CurrentToken().Type == type;
    }

    // Lanzar una excepci�n si el token actual no coincide con el tipo esperado.
    private void TokenExpected(Compiler.TokenType type)
    {
        // Si el token actual no coincide con el tipo esperado, lanzar una excepci�n.
        if (!MatchToken(type))
        {
            throw new Exception($"Token inesperado en la l�nea {line}, columna {column}. Se esperaba {type} y se encontr� {CurrentToken().Value}");
        }
        // Obtener el siguiente token.
        NextToken();
    }

    // Analizar un nodo de efecto.
    private EffectNode ParseEffect()
    {
        // Esperar y consumir una palabra clave (effect).
        TokenExpected(Compiler.TokenType.KeyWord);
        if (CurrentToken().Value == ":")
        {
            // Ignorar el car�cter ':' si est� presente.
            NextToken();
        }

        // Manejar la sintaxis abreviada effect: "Draw".
        if (CurrentToken().Type == Compiler.TokenType.String)
        {
            // Obtener el nombre del efecto.
            var effectName = ParseValue();
            // Si el efecto est� definido, devolverlo.
            if (definedEffects.ContainsKey(effectName))
            {
                return definedEffects[effectName];
            }
            else
            {
                // Si el efecto no est� definido, lanzar una excepci�n.
                throw new Exception($"Efecto no definido: {effectName}");
            }
        }

        // Esperar y consumir un car�cter especial (abrir llave '{').
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        // Crear un nuevo nodo de efecto detallado.
        var detailedEffectNode = new EffectNode();
        // Mientras no se encuentre el car�cter especial (cerrar llave '}').
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            // Obtener la palabra clave.
            var keyword = NextToken().Value;
            // Esperar y consumir un car�cter especial (':').
            TokenExpected(Compiler.TokenType.SpecialCharacter);

            // Analizar diferentes componentes del efecto seg�n la palabra clave.
            if (keyword == "Action")
            {
                detailedEffectNode.Action = ParseAction();
            }
            else if (keyword == "Params")
            {
                detailedEffectNode.Params = ParseParams();
            }
            else if (keyword == "Selector")
            {
                detailedEffectNode.Selector = ParseSelector();
            }
            else if (keyword == "PostAction")
            {
                detailedEffectNode.PostAction = ParsePostAction();
            }
            else
            {
                // Obtener el valor del componente.
                var value = ParseValue();
                if (keyword == "Name")
                {
                    // Asignar el nombre del efecto.
                    detailedEffectNode.Name = value;
                    // Guardar el efecto en definedEffects.
                    if (!definedEffects.ContainsKey(value))
                    {
                        definedEffects[value] = detailedEffectNode;
                    }
                }
                else if (keyword == "Amount")
                {
                    // Si Params es nulo, inicializarlo.
                    if (detailedEffectNode.Params == null)
                    {
                        detailedEffectNode.Params = new ParamsNode();
                    }
                    // Agregar un nuevo par�metro Amount.
                    detailedEffectNode.Params.Params.Add(new ParamNode { Name = "Amount", Type = "Number" });
                }
                else
                {
                    // Si la palabra clave no es reconocida, lanzar una excepci�n.
                    throw new Exception($"Token inesperado: {keyword} en la l�nea {line}, columna {column}");
                }
            }

            // Si el siguiente token es una coma, consumirlo.
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        // Esperar y consumir un car�cter especial (cerrar llave '}').
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        // Devolver el nodo de efecto detallado.
        return detailedEffectNode;
    }

    // Analiza el nodo PostAction.
    private EffectNode ParsePostAction()
    {
        // Esperar y consumir un car�cter especial (abrir llave '{').
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        // Crear un nuevo nodo de efecto para PostAction.
        var postActionNode = new EffectNode();
        // Mientras no se encuentre el car�cter especial (cerrar llave '}').
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            // Obtener la palabra clave.
            var keyword = NextToken().Value;
            // Esperar y consumir un car�cter especial (':').
            TokenExpected(Compiler.TokenType.SpecialCharacter);

            // Analizar diferentes componentes del PostAction seg�n la palabra clave.
            if (keyword == "Type")
            {
                postActionNode.Name = ParseValue();
            }
            else if (keyword == "Selector")
            {
                postActionNode.Selector = ParseSelector();
            }
            else
            {
                // Si la palabra clave no es reconocida, lanzar una excepci�n.
                throw new Exception($"Token inesperado: {keyword} en la l�nea {line}, columna {column}");
            }

            // Si el siguiente token es una coma, consumirlo.
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        // Esperar y consumir un car�cter especial (cerrar llave '}').
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        // Devolver el nodo de PostAction.
        return postActionNode;
    }

    // Analizar un valor.
    private string ParseValue()
    {
        // Obtener el token actual.
        var token = CurrentToken();
        // Si el token es de tipo String, Identifier o Number.
        if (token.Type == Compiler.TokenType.String || token.Type == Compiler.TokenType.Identifier || token.Type == Compiler.TokenType.Number)
        {
            // Obtener el siguiente token.
            NextToken();
            // Devolver el valor del token.
            return token.Value;
        }
        // Si el token no es de un tipo esperado, lanzar una excepci�n.
        throw new Exception($"Token inesperado: {token.Type} con valor '{token.Value}' en la l�nea {line}, columna {column}");
    }

    // Analizar un nodo de par�metros.
    private ParamsNode ParseParams()
    {
        // Esperar y consumir un car�cter especial (abrir llave '{').
        TokenExpected(Compiler.TokenType.SpecialCharacter);

        // Crear un nuevo nodo de par�metros.
        var paramsNode = new ParamsNode();

        // Mientras no se encuentre el car�cter especial (cerrar llave '}').
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            // Crear un nuevo nodo de par�metro.
            var paramNode = new ParamNode
            {
                // Asignar el nombre del par�metro.
                Name = NextToken().Value
            };
            // Esperar y consumir un car�cter especial (':').
            TokenExpected(Compiler.TokenType.SpecialCharacter);
            // Asignar el tipo del par�metro.
            paramNode.Type = NextToken().Value;
            // Agregar el nodo de par�metro a la lista de par�metros.
            paramsNode.Params.Add(paramNode);
            // Si el siguiente token es una coma, consumirlo.
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        // Esperar y consumir un car�cter especial (cerrar llave '}').
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        // Devolver el nodo de par�metros.
        return paramsNode;
    }

    // Analizar un nodo de acci�n.
    private ActionNode ParseAction()
    {
        // Mostrar un mensaje de depuraci�n.
        Debug.Log("Iniciando parseo de la accion");
        // Esperar y consumir un car�cter especial (abrir par�ntesis '(').
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        // Crear un nuevo nodo de acci�n.
        var actionNode = new ActionNode();

        // Lista para almacenar los par�metros de la acci�n.
        var parameters = new List<string>();
        // Mientras no se encuentre el car�cter especial (cerrar par�ntesis ')').
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != ")")
        {
            // Si el token actual es un identificador.
            if (MatchToken(Compiler.TokenType.Identifier))
            {
                // Agregar el valor del token a la lista de par�metros.
                parameters.Add(CurrentToken().Value);
                // Mostrar un mensaje de depuraci�n con el par�metro encontrado.
                Debug.Log($"Par�metro encontrado: {CurrentToken().Value}");
                // Obtener el siguiente token.
                NextToken();
            }
            // Si el siguiente token es una coma, consumirlo.
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken(); // Ignorar la coma y continuar.
            }
        }
        // Esperar y consumir un car�cter especial (cerrar par�ntesis ')').
        TokenExpected(Compiler.TokenType.SpecialCharacter);

        // Procesar el cuerpo de la acci�n.
        // Esperar y consumir un operador ('=>').
        TokenExpected(Compiler.TokenType.Operator);
        // Esperar y consumir un car�cter especial (abrir llave '{').
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        // Mientras no se encuentre el car�cter especial (cerrar llave '}').
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            // Mostrar un mensaje de depuraci�n con el token actual.
            Debug.Log($"ParseAction: CurrentToken = {CurrentToken().Value}, Type = {CurrentToken().Type}, Line = {line}, Column = {column}");
            // Analizar una declaraci�n y agregarla a la lista de declaraciones de la acci�n.
            actionNode.Declarations.Add(ParseStatement());
            // Si el siguiente token es un punto y coma, consumirlo.
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ";")
            {
                NextToken(); // Ignorar el punto y coma y continuar.
            }
        }
        // Esperar y consumir un car�cter especial (cerrar llave '}').
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        // Mostrar un mensaje de depuraci�n.
        Debug.Log("Terminando Parseo de la Accion");
        // Devolver el nodo de acci�n.
        return actionNode;
    }

   
    // Analizar una declaraci�n
    private NodeAST ParseStatement()
    {
        // Mostrar un mensaje de depuraci�n con el token actual.
        Debug.Log($"ParseStatement: CurrentToken = {CurrentToken().Value}, Type = {CurrentToken().Type}, Line = {line}, Column = {column}");

        // Si el token actual es una palabra clave.
        if (MatchToken(Compiler.TokenType.KeyWord))
        {
            // Obtener el valor de la palabra clave.
            string keyword = CurrentToken().Value;
            // Si la palabra clave es "for".
            if (keyword == "for")
            {
                Debug.Log("Keyword found: for");
                // Analizar un bucle for.
                return ParseForLoop();
            }
            // Si la palabra clave es "while".
            else if (keyword == "while")
            {
                Debug.Log("Keyword found: while");
                // Analizar un bucle while.
                return ParseWhileLoop();
            }
            // Si la palabra clave es "if".
            else if (keyword == "if")
            {
                Debug.Log("Keyword found: if");
                // Analizar una declaraci�n if.
                return ParseIfStatement();
            }
            // Si la palabra clave no es reconocida, lanzar una excepci�n.
            else
            {
                throw new Exception($"Token inesperado en la l�nea {line}, columna {column}. Se esperaba una palabra clave y se encontr� {CurrentToken().Value}");
            }
        }
        // Si el token actual es un identificador.
        else if (MatchToken(Compiler.TokenType.Identifier))
        {
            // Parsear una asignaci�n o llamada a m�todo.
            string identifier = CurrentToken().Value;
            // Obtener el siguiente token.
            NextToken();

            // Manejar propiedades de objetos.
            while (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ".")
            {
                Debug.Log("Entr� en el bucle de propiedades de objetos");
                // Consumir el punto.
                NextToken();
                // Obtener el token de la propiedad.
                var propertyToken = CurrentToken();
                // Si el token de la propiedad es un identificador o una palabra clave.
                if (propertyToken.Type == Compiler.TokenType.Identifier || propertyToken.Type == Compiler.TokenType.KeyWord)
                {
                    Debug.Log($"Propiedad encontrada: {propertyToken.Value}");
                    // Obtener el siguiente token.
                    NextToken();
                    // Actualizar el identificador con la propiedad.
                    identifier += "." + propertyToken.Value;
                }
                // Si el token de la propiedad no es v�lido, lanzar una excepci�n.
                else
                {
                    throw new Exception($"Token inesperado: {propertyToken.Value} en la l�nea {line}, columna {column}");
                }
            }

            // Verificar si es una llamada a m�todo.
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == "(")
            {
                Debug.Log("Se encontr� llamada a un m�todo");
                // Consumir el par�ntesis de apertura.
                NextToken();
                // Analizar los argumentos del m�todo.
                var arguments = ParseArguments();
                // Esperar y consumir el punto y coma.
                TokenExpected(Compiler.TokenType.SpecialCharacter);
                // Devolver un nuevo nodo de llamada a m�todo con el nombre del m�todo y los argumentos.
                return new MethodCallNode { MethodName = identifier, Arguments = arguments };
            }
            // Verificar si es una asignaci�n.
            else if (MatchToken(Compiler.TokenType.Operator) && (CurrentToken().Value == "=" || CurrentToken().Value == "+=" || CurrentToken().Value == "-="))
            {
                Debug.Log("Se encontr� asignaci�n");
                // Obtener el valor del operador.
                string operatorToken = CurrentToken().Value;
                // Obtener el siguiente token.
                NextToken();
                // Analizar una expresi�n y asignarla al valor.
                var value = ParseExpression();
                // Esperar y consumir el punto y coma.
                TokenExpected(Compiler.TokenType.SpecialCharacter);
                // Devolver un nuevo nodo de asignaci�n con el identificador, el valor y el operador.
                return new AssignmentNode { Identifier = identifier, Value = value.ToString(), Operator = operatorToken };
            }
            // Si el token no es v�lido, lanzar una excepci�n.
            else
            {
                throw new Exception($"Token inesperado en la l�nea {line}, columna {column}. Se esperaba '=' o '(' y se encontr� {CurrentToken().Value}");
            }
        }
        // Si el token actual es un car�cter especial y su valor es "{".
        else if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == "{")
        {
            // Manejar bloques de c�digo.
            // Consumir el car�cter de apertura.
            NextToken();
            // Lista para almacenar las declaraciones del bloque.
            List<NodeAST> statements = new List<NodeAST>();
            // Mientras no se encuentre el car�cter especial (cerrar llave '}').
            while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
            {
                // Analizar una declaraci�n y agregarla a la lista de declaraciones.
                statements.Add(ParseStatement());
            }
            // Esperar y consumir el car�cter de cierre.
            TokenExpected(Compiler.TokenType.SpecialCharacter);
            // Devolver un nuevo nodo de bloque con las declaraciones.
            return new BlockNode { Statements = statements };
        }
        // Si el token no es v�lido, lanzar una excepci�n.
        else
        {
            throw new Exception($"Token inesperado en la l�nea {line}, columna {column}. Se esperaba una declaraci�n y se encontr� {CurrentToken().Value}");
        }
    }

    // Analizar un bucle while.
    private NodeAST ParseWhileLoop()
    {
        // Consumir la palabra clave 'while'.
        TokenExpected(Compiler.TokenType.KeyWord);
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Consumir el par�ntesis de apertura '('.
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Analizar la condici�n del bucle.
        var condition = ParseExpression();
        Debug.Log($"Condition: {condition}");

        // Consumir el par�ntesis de cierre ')'.
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Lista para almacenar las declaraciones del cuerpo del bucle.
        List<NodeAST> body = new List<NodeAST>();
        // Consumir el car�cter de apertura '{'.
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        // Mientras no se encuentre el car�cter especial (cerrar llave '}').
        while (CurrentToken().Type != Compiler.TokenType.SpecialCharacter || CurrentToken().Value != "}")
        {
            // Analizar una declaraci�n y agregarla a la lista de declaraciones.
            body.Add(ParseStatement());
            // Si el siguiente token es un punto y coma, consumirlo.
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ";")
            {
                NextToken(); // Consumir el punto y coma.
            }
        }
        // Consumir el car�cter de cierre '}'.
        TokenExpected(Compiler.TokenType.SpecialCharacter);

        // Devolver un nuevo nodo de bucle while con la condici�n y el cuerpo.
        return new WhileLoopNode
        {
            Condition = condition,
            Body = body
        };
    }

    // Analizar un bucle for.
    private NodeAST ParseForLoop()
    {
        // Consumir la palabra clave 'for'.
        NextToken();
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Consumir la variable del bucle.
        string variable = CurrentToken().Value;
        TokenExpected(Compiler.TokenType.Identifier);
        Debug.Log($"Variable del bucle: {variable}");

        // Consumir la palabra clave 'in'.
        TokenExpected(Compiler.TokenType.KeyWord);
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Consumir la colecci�n sobre la que itera el bucle.
        string collection = CurrentToken().Value;
        TokenExpected(Compiler.TokenType.Identifier);
        Debug.Log($"Colecci�n del bucle: {collection}");

        // Consumir el car�cter de apertura '{'.
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Lista para almacenar las declaraciones del cuerpo del bucle.
        List<NodeAST> body = new List<NodeAST>();
        // Mientras no se encuentre el car�cter especial (cerrar llave '}').
        while (CurrentToken().Type != Compiler.TokenType.SpecialCharacter || CurrentToken().Value != "}")
        {
            // Analizar una declaraci�n y agregarla a la lista de declaraciones.
            body.Add(ParseStatement());
            // Si el siguiente token es un punto y coma, consumirlo.
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ";")
            {
                NextToken(); // Consumir el punto y coma.
            }
        }
        // Consumir el car�cter de cierre '}'.
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Devolver un nuevo nodo de bucle for con la variable, la colecci�n y el cuerpo
        return new ForLoopNode
        {
            Variable = variable,
            Collection = collection,
            Body = body
        };
    }

    // Analizar una declaraci�n if
    private NodeAST ParseIfStatement()
    {
        // Esperar y consumir la palabra clave 'if'
        TokenExpected(Compiler.TokenType.KeyWord); // if
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Esperar y consumir el car�cter especial '('
        TokenExpected(Compiler.TokenType.SpecialCharacter); // (
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Analizar la condici�n del if
        var condition = ParseExpression();
        Debug.Log($"Condition: {condition}");

        // Esperar y consumir el car�cter especial ')'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // )
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Esperar y consumir el car�cter especial '{'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // {
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Lista para almacenar las declaraciones del cuerpo del if
        List<NodeAST> body = new List<NodeAST>();
        // Mientras no se encuentre el car�cter especial (cerrar llave '}')
        while (CurrentToken().Type != Compiler.TokenType.SpecialCharacter || CurrentToken().Value != "}")
        {
            // Analizar una declaraci�n y agregarla a la lista de declaraciones
            body.Add(ParseStatement());
        }

        // Esperar y consumir el car�cter especial '}'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // }
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Devolver un nuevo nodo de declaraci�n if con la condici�n y el cuerpo
        return new IfStatementNode
        {
            Condition = condition,
            Body = body
        };
    }

    // Analizar argumentos de un m�todo
    private List<NodeAST> ParseArguments()
    {
        Debug.Log("ParseArguments: Iniciando an�lisis de argumentos");
        // Lista para almacenar los argumentos
        var arguments = new List<NodeAST>();
        // Mientras no se encuentre el car�cter especial ')'
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != ")")
        {
            // Analizar una expresi�n y agregarla a la lista de argumentos
            arguments.Add(ParseExpression());
            Debug.Log($"Argumento encontrado: {arguments.Last()}");

            // Si el siguiente token es una coma, consumirlo
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        // Esperar y consumir el car�cter especial ')'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // Consumir ')'
        Debug.Log("ParseArguments: Finalizando an�lisis de argumentos");
        // Devolver la lista de argumentos
        return arguments;
    }


    // Analizar un selector
    private SelectorNode ParseSelector()
    {
        // Esperar y consumir el car�cter especial '{'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // {
                                                            // Crear un nuevo nodo de selector
        var selectorNode = new SelectorNode();
        // Mientras no se encuentre el car�cter especial (cerrar llave '}')
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            // Obtener la palabra clave
            var keyword = NextToken().Value;
            // Esperar y consumir el car�cter especial ':'
            TokenExpected(Compiler.TokenType.SpecialCharacter); // :

            // Analizar diferentes componentes del selector seg�n la palabra clave
            if (keyword == "Source")
            {
                selectorNode.Source = ParseValue();
            }
            else if (keyword == "Single")
            {
                selectorNode.Single = bool.Parse(ParseValue());
            }
            else if (keyword == "Predicate")
            {
                selectorNode.Predicate = ParseExpression();
            }
            else
            {
                // Si la palabra clave no es reconocida, lanzar una excepci�n
                throw new Exception($"Token inesperado: {keyword} en la l�nea {line}, columna {column}");
            }

            // Si el siguiente token es una coma, consumirlo
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        // Esperar y consumir el car�cter especial '}'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // }
                                                            // Devolver el nodo de selector
        return selectorNode;
    }
    // Analizar una expresi�n.
    private NodeAST ParseExpression()
    {
        // Analizar una expresi�n primaria y asignarla al nodo izquierdo.
        var left = ParsePrimaryExpression();

        // Mientras el token actual sea un operador.
        while (MatchToken(Compiler.TokenType.Operator))
        {
            // Obtener el valor del operador.
            var operatorToken = NextToken().Value;
            // Analizar una expresi�n primaria y asignarla al nodo derecho.
            var right = ParsePrimaryExpression();

            // Si el nodo derecho es nulo, lanzar una excepci�n.
            if (right == null)
            {
                throw new Exception($"La expresi�n binaria debe tener operandos izquierdo y derecho. Operador: {operatorToken}, Operando izquierdo: {left}, Token actual: {CurrentToken().Value}, L�nea: {line}, Columna: {column}");
            }

            // Crear un nuevo nodo de expresi�n binaria con los nodos izquierdo y derecho y el operador.
            left = new BinaryExpressionNode { Left = left, Operator = operatorToken, Right = right };
        }

        // Devolver el nodo izquierdo.
        return left;
    }

    // Analizar una expresi�n primaria.
    private NodeAST ParsePrimaryExpression()
    {
        // Obtener el token actual.
        var token = CurrentToken();

        // Si el token es de tipo Number.
        if (token.Type == Compiler.TokenType.Number)
        {
            // Obtener el siguiente token.
            NextToken();
            // Devolver un nuevo nodo de n�mero con el valor del token.
            return new NumberNode { Value = token.Value };
        }
        // Si el token es de tipo String o Identifier.
        else if (token.Type == Compiler.TokenType.String || token.Type == Compiler.TokenType.Identifier)
        {
            // Si el valor del token es "context".
            if (token.Value == "context")
            {
                // Analizar la propiedad del contexto.
                return ParseContextProperty();
            }

            // Obtener el siguiente token.
            NextToken();
            // Crear un nuevo nodo de identificador con el nombre del token.
            var identifierNode = new IdentifierNode { Name = token.Value };

            // Manejar propiedades de objetos.
            while (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ".")
            {
                // Consumir el punto.
                NextToken();
                // Obtener el token de la propiedad.
                var propertyToken = CurrentToken();
                // Si el token de la propiedad es un identificador o una palabra clave.
                if (propertyToken.Type == Compiler.TokenType.Identifier || propertyToken.Type == Compiler.TokenType.KeyWord)
                {
                    // Obtener el siguiente token.
                    NextToken();
                    // Actualizar el nombre del nodo de identificador con la propiedad.
                    identifierNode = new IdentifierNode { Name = identifierNode.Name + "." + propertyToken.Value };
                }
                // Si el token de la propiedad no es v�lido, lanzar una excepci�n.
                else
                {
                    throw new Exception($"Token inesperado: {propertyToken.Value} en la l�nea {line}, columna {column}");
                }
            }

            // Manejar llamadas a m�todos.
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == "(")
            {
                // Consumir el par�ntesis de apertura.
                NextToken();
                // Analizar los argumentos del m�todo.
                var arguments = ParseArguments();
                // Esperar y consumir el par�ntesis de cierre.
                TokenExpected(Compiler.TokenType.SpecialCharacter);
                // Devolver un nuevo nodo de llamada a m�todo con el nombre del m�todo y los argumentos.
                return new MethodCallNode { MethodName = identifierNode.Name, Arguments = arguments };
            }

            // Manejar operadores de incremento y decremento.
            if (MatchToken(Compiler.TokenType.Operator) && (CurrentToken().Value == "++" || CurrentToken().Value == "--"))
            {
                // Obtener el valor del operador.
                var operatorToken = NextToken().Value;
                // Devolver un nuevo nodo de expresi�n unaria con el operando y el operador.
                return new UnaryExpressionNode { Operand = identifierNode, Operator = operatorToken };
            }

            // Devolver el nodo de identificador.
            return identifierNode;
        }
        // Si el token es un car�cter especial y su valor es "(".
        else if (token.Type == Compiler.TokenType.SpecialCharacter && token.Value == "(")
        {
            // Consumir el par�ntesis de apertura.
            NextToken();
            // Analizar una expresi�n.
            var expression = ParseExpression();
            // Esperar y consumir el par�ntesis de cierre.
            TokenExpected(Compiler.TokenType.SpecialCharacter);
            // Devolver la expresi�n.
            return expression;
        }
        // Si el token no es de un tipo esperado, lanzar una excepci�n.
        throw new Exception($"Tipo de token inesperado: {token.Type} con valor '{token.Value}' en la l�nea {line}, columna{column}");
    }


    // Analizar un nodo de carta
    private CardNode ParseCard()
    {
        // Esperar y consumir la palabra clave 'card'
        TokenExpected(Compiler.TokenType.KeyWord); // 'card'
        // Esperar y consumir el car�cter especial '{'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // '{'
        // Crear un nuevo nodo de carta
        var cardNode = new CardNode();

        // Mientras no se encuentre el car�cter especial (cerrar llave '}')
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            Debug.Log($"ParseCard: CurrentToken = {CurrentToken().Value}, Type = {CurrentToken().Type}, Line = {line}, Column = {column}");
            // Obtener la clave
            var key = NextToken().Value;
            // Esperar y consumir el car�cter especial ':'
            TokenExpected(Compiler.TokenType.SpecialCharacter); // :

            // Analizar diferentes componentes de la carta seg�n la clave
            if (key == "Type")
            {
                // Verificar que el valor sea una cadena de texto
                if (CurrentToken().Type != Compiler.TokenType.String)
                {
                    throw new Exception($"El campo 'Type' debe ser una cadena de texto en la l�nea {line}, columna {column}");
                }
                cardNode.Type = ParseValue();
            }
            else if (key == "Name")
            {
                // Verificar que el valor sea una cadena de texto
                if (CurrentToken().Type != Compiler.TokenType.String)
                {
                    throw new Exception($"El campo 'Name' debe ser una cadena de texto en la l�nea {line}, columna {column}");
                }
                cardNode.Name = ParseValue();
            }
            else if (key == "Faction")
            {
                // Verificar que el valor sea una cadena de texto
                if (CurrentToken().Type != Compiler.TokenType.String)
                {
                    throw new Exception($"El campo 'Faction' debe ser una cadena de texto en la l�nea {line}, columna {column}");
                }
                cardNode.Faction = ParseValue();
            }
            else if (key == "Power")
            {
                cardNode.Power = int.Parse(ParseValue());
            }
            else if (key == "Range")
            {
                // Esperar y consumir el car�cter especial '['
                TokenExpected(Compiler.TokenType.SpecialCharacter); // [
                cardNode.Range = ParseRange();
            }
            else if (key == "OnActivation")
            {
                cardNode.OnActivation = ParseEffects();
            }
            else
            {
                // Si la clave no es reconocida, lanzar una excepci�n
                throw new Exception($"Clave inesperada: {key} en la l�nea {line}, columna {column}");
            }
            // Si el siguiente token es una coma, consumirlo
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        // Esperar y consumir el car�cter especial '}'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // }
        // Devolver el nodo de carta
        return cardNode;
    }

    // Analizar rangos
    private List<string> ParseRange()
    {
        // Lista para almacenar los rangos
        var range = new List<string>();

        // Mientras no se encuentre el car�cter especial ']'
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "]")
        {
            // Analizar un valor y agregarlo a la lista de rangos
            range.Add(ParseValue());

            // Si el siguiente token es una coma, consumirlo
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        // Esperar y consumir el car�cter especial ']'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // ]
        // Devolver la lista de rangos
        return range;
    }

    // Analizar una lista de efectos
    private List<EffectNode> ParseEffects()
    {
        // Esperar y consumir el car�cter especial '['
        TokenExpected(Compiler.TokenType.SpecialCharacter); // [
                                                            // Crear una lista para almacenar los efectos
        List<EffectNode> effects = new List<EffectNode>();

        // Mientras no se encuentre el car�cter especial ']'
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "]")
        {
            // Esperar y consumir el car�cter especial '{'
            TokenExpected(Compiler.TokenType.SpecialCharacter); // {

            // Si el token actual es una palabra clave y su valor es "effect"
            if (MatchToken(Compiler.TokenType.KeyWord) && CurrentToken().Value == "effect")
            {
                // Manejar la sintaxis abreviada effect: "Draw"
                if (CurrentToken().Type == Compiler.TokenType.String)
                {
                    // Obtener el nombre del efecto
                    var effectName = ParseValue();
                    // Si el efecto est� definido, agregarlo a la lista de efectos
                    if (definedEffects.ContainsKey(effectName))
                    {
                        effects.Add(definedEffects[effectName]);
                    }
                    else
                    {
                        // Si el efecto no est� definido, lanzar una excepci�n
                        throw new Exception($"Efecto no definido: {effectName}");
                    }
                }
                else
                {
                    // Crear un nuevo nodo de efecto
                    var effectNode = ParseEffect();
                    // Si el nombre del efecto no est� vac�o y el efecto est� definido, reutilizar el nodo de efecto definido previamente
                    if (!string.IsNullOrEmpty(effectNode.Name) && definedEffects.ContainsKey(effectNode.Name))
                    {
                        effects.Add(definedEffects[effectNode.Name]);
                    }
                    else
                    {
                        // Agregar el nuevo nodo de efecto a la lista de efectos
                        effects.Add(effectNode);
                    }
                }
            }
            else
            {
                // Si el token no es v�lido, lanzar una excepci�n
                throw new Exception($"Token inesperado: {CurrentToken().Value} en la l�nea {line}, columna {column}");
            }
            // Esperar y consumir el car�cter especial '}'
            TokenExpected(Compiler.TokenType.SpecialCharacter); // }

            // Si el siguiente token es una coma, consumirlo
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        // Esperar y consumir el car�cter especial ']'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // ]
                                                            // Devolver la lista de efectos
        return effects;
    }

    // Analizar una propiedad de contexto
    private NodeAST ParseContextProperty()
    {
        // Crear un nuevo nodo de identificador con el nombre del token actual
        var identifierNode = new IdentifierNode { Name = CurrentToken().Value };
        // Consumir el token 'context'
        NextToken();
        Debug.Log($"ParseContextProperty: CurrentToken = {CurrentToken().Value}, Type = {CurrentToken().Type}, Line = {line}, Column = {column}");

        // Si el siguiente token es un punto, consumirlo
        if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ".")
        {
            NextToken(); // Consumir '.'
                         // Obtener el token de la propiedad
            var propertyToken = CurrentToken();
            Debug.Log($"ParseContextProperty: Propiedad encontrada = {propertyToken.Value}, Type = {propertyToken.Type}, Line = {line}, Column = {column}");

            // Verificar si el token de la propiedad es un identificador o una palabra clave
            if (propertyToken.Type == Compiler.TokenType.Identifier || propertyToken.Type == Compiler.TokenType.KeyWord)
            {
                // Consumir el token de la propiedad
                NextToken();
                // Guardar el nombre de la propiedad
                var propertyName = propertyToken.Value;

                // Manejar az�car sint�ctica para propiedades comunes
                if (propertyName == "Hand")
                {
                    // Devolver un nodo de llamada a m�todo para obtener la mano del jugador
                    return new MethodCallNode
                    {
                        MethodName = "HandOfPlayer",
                        Arguments = new List<NodeAST> { new IdentifierNode { Name = "context.TriggerPlayer" } }
                    };
                }
                else if (propertyName == "Deck")
                {
                    // Devolver un nodo de llamada a m�todo para obtener el mazo del jugador
                    return new MethodCallNode
                    {
                        MethodName = "DeckOfPlayer",
                        Arguments = new List<NodeAST> { new IdentifierNode { Name = "context.TriggerPlayer" } }
                    };
                }
                else if (propertyName == "Field")
                {
                    // Devolver un nodo de llamada a m�todo para obtener el campo del jugador
                    return new MethodCallNode
                    {
                        MethodName = "FieldOfPlayer",
                        Arguments = new List<NodeAST> { new IdentifierNode { Name = "context.TriggerPlayer" } }
                    };
                }
                else if (propertyName == "Graveyard")
                {
                    // Devolver un nodo de llamada a m�todo para obtener el cementerio del jugador
                    return new MethodCallNode
                    {
                        MethodName = "GraveyardOfPlayer",
                        Arguments = new List<NodeAST> { new IdentifierNode { Name = "context.TriggerPlayer" } }
                    };
                }
                else if (propertyName == "DeckOfPlayer")
                {
                    Debug.Log("ParseContextProperty: Llamada a m�todo DeckOfPlayer encontrada");
                    // Consumir el par�ntesis de apertura '('
                    TokenExpected(Compiler.TokenType.SpecialCharacter); // Consumir '('
                                                                        // Analizar los argumentos del m�todo
                    var arguments = ParseArguments();
                    // Devolver un nodo de llamada a m�todo con los argumentos
                    return new MethodCallNode { MethodName = "DeckOfPlayer", Arguments = arguments };
                }
                else
                {
                    // Si no es una propiedad especial, actualizar el nombre del nodo de identificador con la propiedad
                    identifierNode.Name += "." + propertyName;
                    return identifierNode;
                }
            }
            else
            {
                // Si el token de la propiedad no es v�lido, lanzar una excepci�n
                throw new Exception($"Token inesperado: {propertyToken.Value} en la l�nea {line}, columna {column}");
            }
        }
        else
        {
            // Si el token no es v�lido, lanzar una excepci�n
            throw new Exception($"Token inesperado: {CurrentToken().Value} en la l�nea {line}, columna {column}");
        }
    }
    
}
