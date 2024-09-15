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
    private List<Token> tokens;
    private int position;
    private int line;
    private int column;
    private Dictionary<string, EffectNode> definedEffects = new Dictionary<string, EffectNode>();

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        position = 0;
        line = 1;
        column = 1;
    }

    public NodeAST Parse()
    {
        List<NodeAST> nodes = new List<NodeAST>();

        while (position < tokens.Count)
        {
            if (MatchToken(Compiler.TokenType.KeyWord))
            {
                var keyword = CurrentToken().Value;
                if (keyword == "effect")
                {
                    var effectNode = ParseEffect();
                    definedEffects[effectNode.Name] = effectNode;
                    nodes.Add(effectNode);
                }
                else if (keyword == "card")
                {
                    nodes.Add(ParseCard());
                }
                else
                {
                    throw new Exception($"Token inesperado: {CurrentToken().Value} en la línea {line}, columna {column}");
                }
            }
            else
            {
                throw new Exception($"Token inesperado: {CurrentToken().Value} en la línea {line}, columna {column}");
            }
        }

        return new BlockNode { Statements = nodes };
    }
    // Obtener el token actual
    private Token CurrentToken()
    {
        return tokens[position];
    }

    private Token NextToken()
    {
        var token = tokens[position];
        position++;

        // Actualizar la columna y la línea correctamente
        int newLines = token.Value.Count(c => c == '\n');
        if (newLines > 0)
        {
            line += newLines;
            column = token.Value.Length - token.Value.LastIndexOf('\n');
        }
        else
        {
            column += token.Value.Length;
        }

        Debug.Log($"Siguiente Token: {token.Value}, Type: {token.Type}, Line: {line}, Column: {column}");
        return token;
    }


    // Verificar si el token actual coincide con el tipo esperado
    private bool MatchToken(Compiler.TokenType type)
    {
        return CurrentToken().Type == type;
    }

    // Lanzar una excepción si el token actual no coincide con el tipo esperado
    private void TokenExpected(Compiler.TokenType type)
    {
        if (!MatchToken(type))
            throw new Exception($"Token inesperado en la línea {line}, columna {column}. Se esperaba {type} y se encontró {CurrentToken().Value}");
        NextToken();
    }

    // Analizar un nodo de efecto
    private EffectNode ParseEffect()
    {
        TokenExpected(Compiler.TokenType.KeyWord); // effect
        if (CurrentToken().Value == ":") NextToken(); // ignorar el : si sale

        // Manejar la sintaxis abreviada effect: "Draw"
        if (CurrentToken().Type == Compiler.TokenType.String)
        {
            var effectName = ParseValue();
            Debug.Log($"Encontrado efecto abreviado: {effectName}");
            if (definedEffects.ContainsKey(effectName))
            {
                Debug.Log($"Reutilizando efecto definido: {effectName}");
                return definedEffects[effectName];
            }
            else
            {
                throw new Exception($"Efecto no definido: {effectName}");
            }
        }

        TokenExpected(Compiler.TokenType.SpecialCharacter); // {
        var detailedEffectNode = new EffectNode();
        Debug.Log($"Creando nuevo nodo de efecto: {CurrentToken().Value}");
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            var keyword = NextToken().Value;
            TokenExpected(Compiler.TokenType.SpecialCharacter); // :

            if (keyword == "Action")
            {
                detailedEffectNode.Action = ParseAction();
                Debug.Log($"Asignando acción al efecto: {detailedEffectNode.Name}");
            }
            else if (keyword == "Params")
            {
                detailedEffectNode.Params = ParseParams();
                Debug.Log($"Asignando parámetros al efecto: {detailedEffectNode.Name}");
            }
            else if (keyword == "Selector")
            {
                detailedEffectNode.Selector = ParseSelector();
                Debug.Log($"Asignando selector al efecto: {detailedEffectNode.Name}");
            }
            else if (keyword == "PostAction")
            {
                detailedEffectNode.PostAction = ParsePostAction();
                Debug.Log($"Asignando post-acción al efecto: {detailedEffectNode.Name}");
            }
            else
            {
                var value = ParseValue();
                if (keyword == "Name")
                {
                    detailedEffectNode.Name = value;
                    Debug.Log($"Asignando nombre al efecto: {detailedEffectNode.Name}");
                }
                else if (keyword == "Amount")
                {
                    detailedEffectNode.Params = detailedEffectNode.Params ?? new ParamsNode();
                    detailedEffectNode.Params.Params.Add(new ParamNode { Name = "Amount", Type = "Number" });
                    Debug.Log($"Asignando cantidad al efecto: {detailedEffectNode.Name}");
                }
                else
                {
                    throw new Exception($"Token inesperado: {keyword} en la línea {line}, columna {column}");
                }
            }

            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // }
        return detailedEffectNode;
    }

    // Analiza el nodo PostAction
    private EffectNode ParsePostAction()
    {
        TokenExpected(Compiler.TokenType.SpecialCharacter); // {
        var postActionNode = new EffectNode();
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            var keyword = NextToken().Value;
            TokenExpected(Compiler.TokenType.SpecialCharacter); // :

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
                throw new Exception($"Token inesperado: {keyword} en la línea {line}, columna {column}");
            }

            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // }
        return postActionNode;
    }
    private string ParseValue()
    {
        var token = CurrentToken();
        if (token.Type == Compiler.TokenType.String || token.Type == Compiler.TokenType.Identifier || token.Type == Compiler.TokenType.Number)
        {
            NextToken();
            return token.Value;
        }
        throw new Exception($"Token inesperado: {token.Type} con valor '{token.Value}' en la línea {line}, columna {column}");
    }
    // Analizar un nodo de parámetros
    private ParamsNode ParseParams()
    {
        TokenExpected(Compiler.TokenType.SpecialCharacter); // {

        var paramsNode = new ParamsNode();

        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            var paramNode = new ParamNode
            {
                Name = NextToken().Value
            };
            TokenExpected(Compiler.TokenType.SpecialCharacter); // :
            paramNode.Type = NextToken().Value;
            paramsNode.Params.Add(paramNode);
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // }
        return paramsNode;
    }

    private ActionNode ParseAction()
    {
        Debug.Log("Iniciando parseo de la accion");
        TokenExpected(Compiler.TokenType.SpecialCharacter); // (
        var actionNode = new ActionNode();

        // Procesar los parámetros de la acción
        var parameters = new List<string>();
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != ")")
        {
            if (MatchToken(Compiler.TokenType.Identifier))
            {
                parameters.Add(CurrentToken().Value);
                Debug.Log($"Parámetro encontrado: {CurrentToken().Value}");
                NextToken();
            }
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken(); // Ignorar la coma y continuar
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // )

        // Procesar el cuerpo de la acción
        TokenExpected(Compiler.TokenType.Operator); // =>
        TokenExpected(Compiler.TokenType.SpecialCharacter); // {
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            Debug.Log($"ParseAction: CurrentToken = {CurrentToken().Value}, Type = {CurrentToken().Type}, Line = {line}, Column = {column}");
            actionNode.Declarations.Add(ParseStatement());
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ";")
            {
                NextToken(); // Ignorar el punto y coma y continuar
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // }
        Debug.Log("Terminando Parseo de la Accion");
        return actionNode;
    }

    private NodeAST ParseExpression()
    {
        var left = ParsePrimaryExpression();

        while (MatchToken(Compiler.TokenType.Operator))
        {
            var operatorToken = NextToken().Value;
            var right = ParsePrimaryExpression();

            if (right == null)
            {
                throw new Exception($"La expresión binaria debe tener operandos izquierdo y derecho. Operador: {operatorToken}, Operando izquierdo: {left}, Token actual: {CurrentToken().Value}, Línea: {line}, Columna: {column}");
            }

            left = new BinaryExpressionNode { Left = left, Operator = operatorToken, Right = right };
        }

        return left;
    }

    private NodeAST ParsePrimaryExpression()
    {
        var token = CurrentToken();

        if (token.Type == Compiler.TokenType.Number)
        {
            NextToken();
            return new NumberNode { Value = token.Value };
        }
        else if (token.Type == Compiler.TokenType.String || token.Type == Compiler.TokenType.Identifier)
        {
            if (token.Value == "context")
            {
                return ParseContextProperty();
            }

            NextToken();
            var identifierNode = new IdentifierNode { Name = token.Value };

            // Manejar propiedades de objetos
            while (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ".")
            {
                NextToken(); // Consumir el punto
                var propertyToken = CurrentToken();
                if (propertyToken.Type == Compiler.TokenType.Identifier || propertyToken.Type == Compiler.TokenType.KeyWord)
                {
                    NextToken();
                    identifierNode = new IdentifierNode { Name = identifierNode.Name + "." + propertyToken.Value };
                }
                else
                {
                    throw new Exception($"Token inesperado: {propertyToken.Value} en la línea {line}, columna {column}");
                }
            }

            // Manejar llamadas a métodos
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == "(")
            {
                NextToken(); // Consumir '('
                var arguments = ParseArguments();
                TokenExpected(Compiler.TokenType.SpecialCharacter); // Consumir ')'
                return new MethodCallNode { MethodName = identifierNode.Name, Arguments = arguments };
            }

            // Manejar operadores de incremento y decremento
            if (MatchToken(Compiler.TokenType.Operator) && (CurrentToken().Value == "++" || CurrentToken().Value == "--"))
            {
                var operatorToken = NextToken().Value;
                return new UnaryExpressionNode { Operand = identifierNode, Operator = operatorToken };
            }

            return identifierNode;
        }
        else if (token.Type == Compiler.TokenType.SpecialCharacter && token.Value == "(")
        {
            // Manejar expresiones entre paréntesis
            NextToken(); // Consumir '('
            var expression = ParseExpression();
            TokenExpected(Compiler.TokenType.SpecialCharacter); // Consumir ')'
            return expression;
        }
        throw new Exception($"Tipo de token inesperado: {token.Type} con valor '{token.Value}' en la línea {line}, columna {column}");
    }

    private NodeAST ParseStatement()
    {
        Debug.Log($"ParseStatement: CurrentToken = {CurrentToken().Value}, Type = {CurrentToken().Type}, Line = {line}, Column = {column}");

        if (MatchToken(Compiler.TokenType.KeyWord))
        {
            switch (CurrentToken().Value)
            {
                case "for":
                    Debug.Log("Keyword found: for");
                    return ParseForLoop();
                case "while":
                    Debug.Log("Keyword found: while");
                    return ParseWhileLoop();
                case "if":
                    Debug.Log("Keyword found: if");
                    return ParseIfStatement();
                default:
                    throw new Exception($"Token inesperado en la línea {line}, columna {column}. Se esperaba una palabra clave y se encontró {CurrentToken().Value}");
            }
        }
        else if (MatchToken(Compiler.TokenType.Identifier))
        {
            // Parsear una asignación o llamada a método
            string identifier = CurrentToken().Value;
            NextToken();

            // Manejar propiedades de objetos
            while (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ".")
            {
                Debug.Log("Entró en el bucle de propiedades de objetos");
                NextToken(); // Consumir el punto
                var propertyToken = CurrentToken();
                if (propertyToken.Type == Compiler.TokenType.Identifier || propertyToken.Type == Compiler.TokenType.KeyWord)
                {
                    Debug.Log($"Propiedad encontrada: {propertyToken.Value}");
                    NextToken();
                    identifier += "." + propertyToken.Value;
                }
                else
                {
                    throw new Exception($"Token inesperado: {propertyToken.Value} en la línea {line}, columna {column}");
                }
            }

            // Verificar si es una llamada a método
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == "(")
            {
                Debug.Log("Se encontro llamada a un metodo");
                NextToken();
                var arguments = ParseArguments();
                TokenExpected(Compiler.TokenType.SpecialCharacter); // Consumir el punto y coma
                return new MethodCallNode { MethodName = identifier, Arguments = arguments };
            }
            // Verificar si es una asignación
            else if (MatchToken(Compiler.TokenType.Operator) && (CurrentToken().Value == "=" || CurrentToken().Value == "+=" || CurrentToken().Value == "-="))
            {
                Debug.Log("Se encontro asignacion");
                string operatorToken = CurrentToken().Value;
                NextToken();
                var value = ParseExpression(); // Cambiado para manejar cualquier expresión, no solo identificadores
                TokenExpected(Compiler.TokenType.SpecialCharacter); // Consumir el punto y coma
                return new AssignmentNode { Identifier = identifier, Value = value.ToString(), Operator = operatorToken };
            }
            else
            {
                throw new Exception($"Token inesperado en la línea {line}, columna {column}. Se esperaba '=' o '(' y se encontró {CurrentToken().Value}");
            }
        }
        else if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == "{")
        {
            // Manejar bloques de código
            NextToken(); // Consumir '{'
            List<NodeAST> statements = new List<NodeAST>();
            while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
            {
                statements.Add(ParseStatement());
            }
            TokenExpected(Compiler.TokenType.SpecialCharacter); // Consumir '}'
            return new BlockNode { Statements = statements };
        }
        else
        {
            throw new Exception($"Token inesperado en la línea {line}, columna {column}. Se esperaba una declaración y se encontró {CurrentToken().Value}");
        }
    }

    // Analizar un bucle while
    private NodeAST ParseWhileLoop()
    {
        // Consumir 'while'
        TokenExpected(Compiler.TokenType.KeyWord); // while
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Consumir '('
        TokenExpected(Compiler.TokenType.SpecialCharacter); // (
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Consumir condición
        var condition = ParseExpression();
        Debug.Log($"Condition: {condition}");

        // Consumir ')'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // )
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Parsear el cuerpo del while
        List<NodeAST> body = new List<NodeAST>();
        TokenExpected(Compiler.TokenType.SpecialCharacter); // {
        while (CurrentToken().Type != Compiler.TokenType.SpecialCharacter || CurrentToken().Value != "}")
        {
            body.Add(ParseStatement());
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ";")
            {
                NextToken(); // Consumir el punto y coma
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // }

        return new WhileLoopNode
        {
            Condition = condition,
            Body = body
        };
    }

    // Analizar un bucle for
    private NodeAST ParseForLoop()
    {
        // Consumir 'for'
        NextToken();
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Consumir variable del bucle
        string variable = CurrentToken().Value;
        TokenExpected(Compiler.TokenType.Identifier);
        Debug.Log($"Variable del bucle: {variable}");

        // Consumir 'in'
        TokenExpected(Compiler.TokenType.KeyWord);
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Consumir colección
        string collection = CurrentToken().Value;
        TokenExpected(Compiler.TokenType.Identifier);
        Debug.Log($"Colección del bucle: {collection}");

        // Consumir '{'
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        // Parsear el cuerpo del bucle
        List<NodeAST> body = new List<NodeAST>();
        while (CurrentToken().Type != Compiler.TokenType.SpecialCharacter || CurrentToken().Value != "}")
        {
            body.Add(ParseStatement());
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ";")
            {
                NextToken(); // Consumir el punto y coma
            }
        }
        // Consumir '}'
        TokenExpected(Compiler.TokenType.SpecialCharacter);
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        return new ForLoopNode
        {
            Variable = variable,
            Collection = collection,
            Body = body
        };
    }

    //Analiza los if
    private NodeAST ParseIfStatement()
    {
        TokenExpected(Compiler.TokenType.KeyWord); // if
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");
        TokenExpected(Compiler.TokenType.SpecialCharacter); // (
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");
        var condition = ParseExpression();
        Debug.Log($"Condition: {condition}");

        TokenExpected(Compiler.TokenType.SpecialCharacter); // )
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        TokenExpected(Compiler.TokenType.SpecialCharacter); // {
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        List<NodeAST> body = new List<NodeAST>();
        while (CurrentToken().Type != Compiler.TokenType.SpecialCharacter || CurrentToken().Value != "}")
        {
            body.Add(ParseStatement());
        }

        TokenExpected(Compiler.TokenType.SpecialCharacter); // }
        Debug.Log($"NextToken: {CurrentToken().Value}, Type: {CurrentToken().Type}, Line: {line}, Column: {column}");

        return new IfStatementNode
        {
            Condition = condition,
            Body = body
        };
    }
    // Analizar argumentos de un método
    private List<NodeAST> ParseArguments()
    {
        Debug.Log("ParseArguments: Iniciando análisis de argumentos");
        var arguments = new List<NodeAST>();
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != ")")
        {
            arguments.Add(ParseExpression());
            Debug.Log($"Argumento encontrado: {arguments.Last()}");

            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // Consumir ')'
        Debug.Log("ParseArguments: Finalizando análisis de argumentos");
        return arguments;
    }

    private SelectorNode ParseSelector()
    {
        TokenExpected(Compiler.TokenType.SpecialCharacter); // {
        var selectorNode = new SelectorNode();
        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            var keyword = NextToken().Value;
            TokenExpected(Compiler.TokenType.SpecialCharacter); // :

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
                throw new Exception($"Token inesperado: {keyword} en la línea {line}, columna {column}");
            }

            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // }
        return selectorNode;
    }
    // Analizar un nodo de carta
    private CardNode ParseCard()
    {
        TokenExpected(Compiler.TokenType.KeyWord); // 'card'
        TokenExpected(Compiler.TokenType.SpecialCharacter); // '{'
        var cardNode = new CardNode();

        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "}")
        {
            Debug.Log($"ParseCard: CurrentToken = {CurrentToken().Value}, Type = {CurrentToken().Type}, Line = {line}, Column = {column}");
            var key = NextToken().Value;
            TokenExpected(Compiler.TokenType.SpecialCharacter); // :

            if (key == "Type")
            {
                cardNode.Type = ParseValue();
            }
            else if (key == "Name")
            {
                cardNode.Name = ParseValue();
            }
            else if (key == "Faction")
            {
                cardNode.Faction = ParseValue();
            }
            else if (key == "Power")
            {
                cardNode.Power = int.Parse(ParseValue());
            }
            else if (key == "Range")
            {
                TokenExpected(Compiler.TokenType.SpecialCharacter); // [
                cardNode.Range = ParseRange();
            }
            else if (key == "OnActivation")
            {
               
                cardNode.OnActivation = ParseEffects();
            }
            else
            {
                throw new Exception($"Clave inesperada: {key} en la línea {line}, columna {column}");
            }
            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // }
        return cardNode;
    }
    //Parsea los Rangos
    private List<string> ParseRange()
    {
        var range = new List<string>();

        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "]")
        {
            range.Add(ParseValue());

            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // ]
        return range;
    }
    private List<EffectNode> ParseEffects()
    {
        var effects = new List<EffectNode>();
        TokenExpected(Compiler.TokenType.SpecialCharacter); // [

        while (!MatchToken(Compiler.TokenType.SpecialCharacter) || CurrentToken().Value != "]")
        {
            Debug.Log("Analizando efecto en la carta");
            TokenExpected(Compiler.TokenType.SpecialCharacter); // {
            Debug.Log("Paso la llave");

            if (MatchToken(Compiler.TokenType.KeyWord) && CurrentToken().Value == "effect")
            {
                Debug.Log("Encontrado efecto");
                effects.Add(ParseEffect());
            }
            else
            {
                throw new Exception($"Token inesperado: {CurrentToken().Value} en la línea {line}, columna {column}");
            }
            TokenExpected(Compiler.TokenType.SpecialCharacter); // }

            if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ",")
            {
                NextToken();
            }
        }
        TokenExpected(Compiler.TokenType.SpecialCharacter); // ]
        return effects;
    }

    private NodeAST ParseContextProperty()
    {
        var identifierNode = new IdentifierNode { Name = CurrentToken().Value };
        NextToken(); // Consumir 'context'
        Debug.Log($"ParseContextProperty: CurrentToken = {CurrentToken().Value}, Type = {CurrentToken().Type}, Line = {line}, Column = {column}");

        if (MatchToken(Compiler.TokenType.SpecialCharacter) && CurrentToken().Value == ".")
        {
            NextToken(); // Consumir '.'
            var propertyToken = CurrentToken();
            Debug.Log($"ParseContextProperty: Propiedad encontrada = {propertyToken.Value}, Type = {propertyToken.Type}, Line = {line}, Column = {column}");
            if (propertyToken.Type == Compiler.TokenType.Identifier || propertyToken.Type == Compiler.TokenType.KeyWord)
            {
                NextToken();
                var propertyName = propertyToken.Value;

                // Manejar azúcar sintáctica
                if (propertyName == "Hand")
                {
                    return new MethodCallNode
                    {
                        MethodName = "HandOfPlayer",
                        Arguments = new List<NodeAST> { new IdentifierNode { Name = "context.TriggerPlayer" } }
                    };
                }
                else if (propertyName == "Deck")
                {
                    return new MethodCallNode
                    {
                        MethodName = "DeckOfPlayer",
                        Arguments = new List<NodeAST> { new IdentifierNode { Name = "context.TriggerPlayer" } }
                    };
                }
                else if (propertyName == "Field")
                {
                    return new MethodCallNode
                    {
                        MethodName = "FieldOfPlayer",
                        Arguments = new List<NodeAST> { new IdentifierNode { Name = "context.TriggerPlayer" } }
                    };
                }
                else if (propertyName == "Graveyard")
                {
                    return new MethodCallNode
                    {
                        MethodName = "GraveyardOfPlayer",
                        Arguments = new List<NodeAST> { new IdentifierNode { Name = "context.TriggerPlayer" } }
                    };
                }
                else if (propertyName == "DeckOfPlayer")
                {
                    Debug.Log("ParseContextProperty: Llamada a método DeckOfPlayer encontrada");
                    TokenExpected(Compiler.TokenType.SpecialCharacter); // Consumir '('
                    var arguments = ParseArguments();
                    return new MethodCallNode { MethodName = "DeckOfPlayer", Arguments = arguments };
                }
                else
                {
                    identifierNode.Name += "." + propertyName;
                    return identifierNode;
                }
            }
            else
            {
                throw new Exception($"Token inesperado: {propertyToken.Value} en la línea {line}, columna {column}");
            }
        }
        else
        {
            throw new Exception($"Token inesperado: {CurrentToken().Value} en la línea {line}, columna {column}");
        }
    }


}
