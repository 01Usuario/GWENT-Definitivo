using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASTValidator : IVisitor
{
    // Lista de efectos definidos.
    private List<string> definedEffects = new List<string>();

    // Método para validar el nodo raíz del AST.
    public void Validate(NodeAST root)
    {
        // Aceptar el visitante en el nodo raíz.
        root.Accept(this);
    }

    // Visitar un nodo de efecto.
    public void Visit(EffectNode effectNode)
    {
        // Verificar si el efecto tiene nombre.
        if (string.IsNullOrEmpty(effectNode.Name))
        {
            throw new Exception("El efecto no tiene nombre.");
        }
        Debug.Log("Se encontró un efecto: " + effectNode.Name);

        // Verificar si el efecto tiene una acción.
        if (effectNode.Action == null)
        {
            throw new Exception($"El efecto {effectNode.Name} no tiene una acción.");
        }

        // Agregar el nombre del efecto a la lista de efectos definidos.
        definedEffects.Add(effectNode.Name);
        // Aceptar el visitante en la acción del efecto.
        effectNode.Action.Accept(this);

        // Si el efecto tiene parámetros, aceptar el visitante en los parámetros.
        if (effectNode.Params != null)
        {
            effectNode.Params.Accept(this);
        }
        // Si el efecto tiene un selector, aceptar el visitante en el selector.
        if (effectNode.Selector != null)
        {
            effectNode.Selector.Accept(this);
        }
        // Si el efecto tiene una acción posterior, aceptar el visitante en la acción posterior.
        if (effectNode.PostAction != null)
        {
            effectNode.PostAction.Accept(this);
        }
    }

    // Visitar un nodo de carta.
    public void Visit(CardNode cardNode)
    {
        // Verificar si la carta tiene nombre.
        if (string.IsNullOrEmpty(cardNode.Name))
        {
            throw new Exception("La carta no tiene nombre.");
        }
        // Verificar si el poder de la carta es menor que 0.
        if (cardNode.Power < 0)
        {
            throw new Exception($"La carta {cardNode.Name} no puede tener un poder menor que 0.");
        }

        // Para cada efecto en la activación de la carta.
        foreach (var effect in cardNode.OnActivation)
        {
            // Verificar si el efecto está definido.
            if (!definedEffects.Contains(effect.Name))
            {
                throw new Exception($"El efecto {effect.Name} de la carta {cardNode.Name} no se ha definido.");
            }
            // Aceptar el visitante en el efecto.
            effect.Accept(this);
        }
    }

    // Visitar un nodo de selector.
    public void Visit(SelectorNode selectorNode)
    {
        // Verificar si el selector tiene una fuente.
        if (string.IsNullOrEmpty(selectorNode.Source))
        {
            throw new Exception("El selector debe tener una fuente.");
        }
        // Si el selector tiene un predicado, aceptar el visitante en el predicado.
        if (selectorNode.Predicate != null)
        {
            selectorNode.Predicate.Accept(this);
        }
    }

    // Visitar un nodo de acción.
    public void Visit(ActionNode actionNode)
    {
        // Para cada declaración en la acción.
        foreach (var statement in actionNode.Declarations)
        {
            // Aceptar el visitante en la declaración.
            statement.Accept(this);
        }
    }

    // Visitar un nodo de parámetros.
    public void Visit(ParamsNode paramsNode)
    {
        // Para cada parámetro en los parámetros.
        foreach (var param in paramsNode.Params)
        {
            // Aceptar el visitante en el parámetro.
            param.Accept(this);
        }
    }

    // Visitar un nodo de parámetro.
    public void Visit(ParamNode paramNode)
    {
        // Verificar si el parámetro tiene nombre y tipo.
        if (string.IsNullOrEmpty(paramNode.Name) || string.IsNullOrEmpty(paramNode.Type))
        {
            throw new Exception("El parámetro debe tener un nombre y un tipo.");
        }
    }

    // Visitar un nodo de asignación.
    public void Visit(AssignmentNode assignmentNode)
    {
        // Verificar si la asignación tiene identificador y valor.
        if (string.IsNullOrEmpty(assignmentNode.Identifier) || string.IsNullOrEmpty(assignmentNode.Value))
        {
            throw new Exception("La asignación debe tener un identificador y un valor.");
        }
    }

    // Visitar un nodo de llamada a método.
    public void Visit(MethodCallNode callNode)
    {
        // Verificar si el método tiene nombre.
        if (string.IsNullOrEmpty(callNode.MethodName))
        {
            throw new Exception("El método no tiene un nombre.");
        }

        // Para cada argumento en la llamada al método.
        foreach (var argument in callNode.Arguments)
        {
            // Aceptar el visitante en el argumento.
            argument.Accept(this);
        }
    }

    // Visitar un nodo de declaración if.
    public void Visit(IfStatementNode ifStatementNode)
    {
        // Verificar si el if tiene una condición.
        if (ifStatementNode.Condition == null)
        {
            throw new Exception("El if no tiene una condición.");
        }

        // Aceptar el visitante en la condición del if.
        ifStatementNode.Condition.Accept(this);

        // Para cada declaración en el cuerpo del if.
        foreach (var statement in ifStatementNode.Body)
        {
            // Aceptar el visitante en la declaración.
            statement.Accept(this);
        }
    }

    // Visitar un nodo de bucle for.
    public void Visit(ForLoopNode forLoopNode)
    {
        // Verificar si el bucle for tiene una variable y una colección.
        if (string.IsNullOrEmpty(forLoopNode.Variable) || string.IsNullOrEmpty(forLoopNode.Collection))
        {
            throw new Exception("El bucle for debe tener una variable y una colección para iterar.");
        }

        // Para cada declaración en el cuerpo del bucle for.
        foreach (var statement in forLoopNode.Body)
        {
            // Aceptar el visitante en la declaración.
            statement.Accept(this);
        }
    }

    // Visitar un nodo de bucle while.
    public void Visit(WhileLoopNode whileLoopNode)
    {
        // Verificar si el bucle while tiene una condición.
        if (whileLoopNode.Condition == null)
        {
            throw new Exception("El bucle while debe tener una condición.");
        }

        // Aceptar el visitante en la condición del bucle while.
        whileLoopNode.Condition.Accept(this);

        // Para cada declaración en el cuerpo del bucle while.
        foreach (var statement in whileLoopNode.Body)
        {
            // Aceptar el visitante en la declaración.
            statement.Accept(this);
        }
    }

    // Visitar un nodo de número.
    public void Visit(NumberNode numberNode)
    {
        // Verificar si el nodo de número tiene un valor.
        if (string.IsNullOrEmpty(numberNode.Value))
        {
            throw new Exception("El nodo de número debe tener un valor.");
        }
    }

    // Visitar un nodo de identificador.
    public void Visit(IdentifierNode identifierNode)
    {
        // Verificar si el nodo de identificador tiene un nombre.
        if (string.IsNullOrEmpty(identifierNode.Name))
        {
            throw new Exception("El nodo de identificador debe tener un nombre.");
        }
    }

    // Visitar un nodo de expresión binaria.
    public void Visit(BinaryExpressionNode binaryExpressionNode)
    {
        // Verificar si la expresión binaria tiene operandos izquierdo y derecho.
        if (binaryExpressionNode.Left == null || binaryExpressionNode.Right == null)
        {
            throw new Exception($"La expresión binaria debe tener operandos izquierdo y derecho. Operador: {binaryExpressionNode.Operator}, Operando izquierdo: {binaryExpressionNode.Left}, Operando derecho: {binaryExpressionNode.Right}");
        }

        // Aceptar el visitante en el operando izquierdo.
        binaryExpressionNode.Left.Accept(this);
        // Aceptar el visitante en el operando derecho.
        binaryExpressionNode.Right.Accept(this);
    }

    // Visitar un nodo de expresión unaria.
    public void Visit(UnaryExpressionNode unaryExpressionNode)
    {
        // Verificar si la expresión unaria tiene un operando.
        if (unaryExpressionNode.Operand == null)
        {
            throw new Exception($"La expresión unaria debe tener un operando. Operador: {unaryExpressionNode.Operator}");
        }

        // Aceptar el visitante en el operando.
        unaryExpressionNode.Operand.Accept(this);
    }

    // Visitar un nodo de bloque.
    public void Visit(BlockNode blockNode)
    {
        // Para cada declaración en el bloque.
        foreach (var statement in blockNode.Statements)
        {
            // Aceptar el visitante en la declaración.
            statement.Accept(this);
        }
    }
}

public class SemanticValidator : IVisitor
{
    // Conjunto de variables declaradas.
    private HashSet<string> declaredVariables = new HashSet<string>();

    // Método para validar el nodo raíz del AST.
    public void Validate(NodeAST root)
    {
        root.Accept(this);
    }

    // Visitar un nodo de efecto.
    public void Visit(EffectNode effectNode)
    {
        if (effectNode.Params != null)
        {
            foreach (var param in effectNode.Params.Params)
            {
                declaredVariables.Add(param.Name);
            }
        }

        effectNode.Action.Accept(this);

        if (effectNode.Selector != null)
        {
            effectNode.Selector.Accept(this);
        }
        if (effectNode.PostAction != null)
        {
            effectNode.PostAction.Accept(this);
        }
    }

    // Visitar un nodo de selector.
    public void Visit(SelectorNode selectorNode)
    {
        if (selectorNode.Predicate != null)
        {
            selectorNode.Predicate.Accept(this);
        }
    }

    // Visitar un nodo de parámetros.
    public void Visit(ParamsNode paramsNode)
    {
        foreach (var param in paramsNode.Params)
        {
            declaredVariables.Add(param.Name);
        }
    }

    // Visitar un nodo de parámetro.
    public void Visit(ParamNode paramNode)
    {
        // No se necesita validación adicional para ParamNode
    }

    // Visitar un nodo de acción.
    public void Visit(ActionNode actionNode)
    {
        // Crear un nuevo conjunto de variables declaradas para el cuerpo de la acción
        var previousDeclaredVariables = new HashSet<string>(declaredVariables);

        foreach (var declaration in actionNode.Declarations)
        {
            if (declaration is AssignmentNode assignmentNode && assignmentNode.Operator == "=")
            {
                declaredVariables.Add(assignmentNode.Identifier);
            }
            declaration.Accept(this);
        }

        // Restaurar el conjunto de variables declaradas previo a la acción
        declaredVariables = previousDeclaredVariables;
    }

    // Visitar un nodo de carta.
    public void Visit(CardNode cardNode)
    {
        foreach (var effect in cardNode.OnActivation)
        {
            effect.Accept(this);
        }
    }

    // Visitar un nodo de asignación.
    public void Visit(AssignmentNode assignmentNode)
    {
        // Verificar si el identificador completo está declarado
        if (!declaredVariables.Contains(assignmentNode.Identifier))
        {
            // Si no está declarado, verificar cada parte del identificador
            var parts = assignmentNode.Identifier.Split('.');
            bool isValid = true;
            foreach (var part in parts)
            {
                // Verificar si la parte es una propiedad válida de un objeto conocido
                if (!declaredVariables.Contains(part) && !IsValidProperty(part))
                {
                    isValid = false;
                    break;
                }
            }
            if (!isValid)
            {
                throw new Exception($"Variable no declarada: {assignmentNode.Identifier}");
            }
        }
    }

    // Visitar un nodo de identificador.
    public void Visit(IdentifierNode identifierNode)
    {
        // Verificar si el identificador completo está declarado
        if (!declaredVariables.Contains(identifierNode.Name))
        {
            // Si no está declarado, verificar cada parte del identificador
            var parts = identifierNode.Name.Split('.');
            bool isValid = true;
            foreach (var part in parts)
            {
                // Verificar si la parte es una propiedad válida de un objeto conocido
                if (!declaredVariables.Contains(part) && !IsValidProperty(part))
                {
                    isValid = false;
                    break;
                }
            }
            if (!isValid)
            {
                throw new Exception($"Variable no declarada: {identifierNode.Name}");
            }
        }
    }

    // Método auxiliar para verificar si una parte es una propiedad válida
    private bool IsValidProperty(string part)
    {
        // Lista de propiedades válidas de objetos conocidos
        var validProperties = new HashSet<string> { "Power", "Owner", "Faction", "Type", "Name", "Range" };
        return validProperties.Contains(part);
    }

    // Visitar un nodo de llamada a método.
    public void Visit(MethodCallNode callNode)
    {
        if (callNode.MethodName == "HandOfPlayer" || callNode.MethodName == "DeckOfPlayer" ||
            callNode.MethodName == "FieldOfPlayer" || callNode.MethodName == "GraveyardOfPlayer")
        {
            if (callNode.Arguments.Count != 1 || !(callNode.Arguments[0] is IdentifierNode identifierNode) ||
                identifierNode.Name != "context.TriggerPlayer")
            {
                throw new Exception($"Argumento inválido para {callNode.MethodName}. Se esperaba context.TriggerPlayer.");
            }
        }

        foreach (var argument in callNode.Arguments)
        {
            argument.Accept(this);
        }
    }

    // Visitar un nodo de declaración if.
    public void Visit(IfStatementNode ifStatementNode)
    {
        ifStatementNode.Condition.Accept(this);

        // Crear un nuevo conjunto de variables declaradas para el cuerpo del if
        var previousDeclaredVariables = new HashSet<string>(declaredVariables);

        foreach (var statement in ifStatementNode.Body)
        {
            if (statement is AssignmentNode assignmentNode && assignmentNode.Operator == "=")
            {
                declaredVariables.Add(assignmentNode.Identifier);
            }
            statement.Accept(this);
        }

        // Restaurar el conjunto de variables declaradas previo al if
        declaredVariables = previousDeclaredVariables;
    }

    // Visitar un nodo de bucle for.
    public void Visit(ForLoopNode forLoopNode)
    {
        declaredVariables.Add("targets");
        declaredVariables.Add(forLoopNode.Variable);

        if (!declaredVariables.Contains(forLoopNode.Collection))
        {
            throw new Exception($"Colección no declarada: {forLoopNode.Collection}");
        }

        // Crear un nuevo conjunto de variables declaradas para el cuerpo del bucle
        var previousDeclaredVariables = new HashSet<string>(declaredVariables);

        foreach (var statement in forLoopNode.Body)
        {
            if (statement is AssignmentNode assignmentNode && assignmentNode.Operator == "=")
            {
                declaredVariables.Add(assignmentNode.Identifier);
            }
            statement.Accept(this);
        }

        // Restaurar el conjunto de variables declaradas previo al bucle
        declaredVariables = previousDeclaredVariables;
    }

    // Visitar un nodo de bucle while.
    public void Visit(WhileLoopNode whileLoopNode)
    {
        whileLoopNode.Condition.Accept(this);

        // Crear un nuevo conjunto de variables declaradas para el cuerpo del bucle
        var previousDeclaredVariables = new HashSet<string>(declaredVariables);

        foreach (var statement in whileLoopNode.Body)
        {
            if (statement is AssignmentNode assignmentNode && assignmentNode.Operator == "=")
            {
                declaredVariables.Add(assignmentNode.Identifier);
            }
            statement.Accept(this);
        }

        // Restaurar el conjunto de variables declaradas previo al bucle
        declaredVariables = previousDeclaredVariables;
    }

    // Visitar un nodo de número.
    public void Visit(NumberNode numberNode)
    {
    }

    // Visitar un nodo de expresión binaria.
    public void Visit(BinaryExpressionNode binaryExpressionNode)
    {
        binaryExpressionNode.Left.Accept(this);
        binaryExpressionNode.Right.Accept(this);
    }

    // Visitar un nodo de expresión unaria.
    public void Visit(UnaryExpressionNode unaryExpressionNode)
    {
        if (unaryExpressionNode.Operand == null)
        {
            throw new Exception($"La expresión unaria debe tener un operando. Operador: {unaryExpressionNode.Operator}");
        }

        unaryExpressionNode.Operand.Accept(this);
    }

    // Visitar un nodo de bloque.
    public void Visit(BlockNode blockNode)
    {
        var previousDeclaredVariables = new HashSet<string>(declaredVariables);

        foreach (var statement in blockNode.Statements)
        {
            statement.Accept(this);
        }

        declaredVariables = previousDeclaredVariables;
    }
}


