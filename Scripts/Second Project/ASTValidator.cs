using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASTValidator : IVisitor
{
    private List<string> definedEffects = new List<string>();

    public void Validate(NodeAST root)
    {
        root.Accept(this);
    }

    public void Visit(EffectNode effectNode)
    {
        if (string.IsNullOrEmpty(effectNode.Name))
        {
            throw new Exception("El efecto no tiene nombre.");
        }
        if (effectNode.Action == null)
        {
            throw new Exception($"El efecto {effectNode.Name} no tiene una acción.");
        }

        definedEffects.Add(effectNode.Name);
        effectNode.Action.Accept(this);

        if (effectNode.Params != null)
        {
            effectNode.Params.Accept(this);
        }
        if (effectNode.Selector != null)
        {
            effectNode.Selector.Accept(this);
        }
        if (effectNode.PostAction != null)
        {
            effectNode.PostAction.Accept(this);
        }
    }

    public void Visit(CardNode cardNode)
    {
        if (string.IsNullOrEmpty(cardNode.Name))
        {
            throw new Exception("La carta no tiene nombre.");
        }
        if (cardNode.Power < 0)
        {
            throw new Exception($"La carta {cardNode.Name} no puede tener un poder menor que 0.");
        }

        foreach (var effect in cardNode.OnActivation)
        {
            if (!definedEffects.Contains(effect.Name))
            {
                throw new Exception($"El efecto {effect.Name} de la carta {cardNode.Name} no se ha definido.");
            }
            effect.Accept(this);
        }
    }
    public void Visit(SelectorNode selectorNode)
    {
        if (string.IsNullOrEmpty(selectorNode.Source))
        {
            throw new Exception("El selector debe tener una fuente.");
        }
        if (selectorNode.Predicate != null)
        {
            selectorNode.Predicate.Accept(this);
        }
    }

    public void Visit(ActionNode actionNode)
    {
        foreach (var statement in actionNode.Declarations)
        {
            statement.Accept(this);
        }
    }

    public void Visit(ParamsNode paramsNode)
    {
        foreach (var param in paramsNode.Params)
        {
            param.Accept(this);
        }
    }

    public void Visit(ParamNode paramNode)
    {
        if (string.IsNullOrEmpty(paramNode.Name) || string.IsNullOrEmpty(paramNode.Type))
        {
            throw new Exception("El parámetro debe tener un nombre y un tipo.");
        }
    }

    public void Visit(AssignmentNode assignmentNode)
    {
        if (string.IsNullOrEmpty(assignmentNode.Identifier) || string.IsNullOrEmpty(assignmentNode.Value))
        {
            throw new Exception("La asignación debe tener un identificador y un valor.");
        }
    }

    public void Visit(MethodCallNode callNode)
    {
        if (string.IsNullOrEmpty(callNode.MethodName))
        {
            throw new Exception("El método no tiene un nombre.");
        }

        foreach (var argument in callNode.Arguments)
        {
            argument.Accept(this);
        }
    }

    public void Visit(IfStatementNode ifStatementNode)
    {
        if (ifStatementNode.Condition == null)
        {
            throw new Exception("El if no tiene una condición.");
        }

        ifStatementNode.Condition.Accept(this);

        foreach (var statement in ifStatementNode.Body)
        {
            statement.Accept(this);
        }
    }

    public void Visit(ForLoopNode forLoopNode)
    {
        if (string.IsNullOrEmpty(forLoopNode.Variable) || string.IsNullOrEmpty(forLoopNode.Collection))
        {
            throw new Exception("El bucle for debe tener una variable y una colección para iterar.");
        }

        foreach (var statement in forLoopNode.Body)
        {
            statement.Accept(this);
        }
    }

    public void Visit(WhileLoopNode whileLoopNode)
    {
        if (whileLoopNode.Condition == null)
        {
            throw new Exception("El bucle while debe tener una condición.");
        }

        whileLoopNode.Condition.Accept(this);

        foreach (var statement in whileLoopNode.Body)
        {
            statement.Accept(this);
        }
    }

    public void Visit(NumberNode numberNode)
    {
        if (string.IsNullOrEmpty(numberNode.Value))
        {
            throw new Exception("El nodo de número debe tener un valor.");
        }
    }

    public void Visit(IdentifierNode identifierNode)
    {
        if (string.IsNullOrEmpty(identifierNode.Name))
        {
            throw new Exception("El nodo de identificador debe tener un nombre.");
        }
    }

    public void Visit(BinaryExpressionNode binaryExpressionNode)
    {
        if (binaryExpressionNode.Left == null || binaryExpressionNode.Right == null)
        {
            throw new Exception($"La expresión binaria debe tener operandos izquierdo y derecho. Operador: {binaryExpressionNode.Operator}, Operando izquierdo: {binaryExpressionNode.Left}, Operando derecho: {binaryExpressionNode.Right}");
        }

        binaryExpressionNode.Left.Accept(this);
        binaryExpressionNode.Right.Accept(this);
    }
    public void Visit(UnaryExpressionNode unaryExpressionNode)
    {
        if (unaryExpressionNode.Operand == null)
        {
            throw new Exception($"La expresión unaria debe tener un operando. Operador: {unaryExpressionNode.Operator}");
        }

        unaryExpressionNode.Operand.Accept(this);
    }
    public void Visit(BlockNode blockNode)
    {
        foreach (var statement in blockNode.Statements)
        {
            statement.Accept(this);
        }
    }


}

public class SemanticValidator : IVisitor
{
    private HashSet<string> declaredVariables = new HashSet<string>();

    public void Validate(NodeAST root)
    {
        root.Accept(this);
    }

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
    public void Visit(SelectorNode selectorNode)
    {
        if (selectorNode.Predicate != null)
        {
            selectorNode.Predicate.Accept(this);
        }
    }
    public void Visit(ParamsNode paramsNode)
    {
        foreach (var param in paramsNode.Params)
        {
            declaredVariables.Add(param.Name);
        }
    }

    public void Visit(ParamNode paramNode)
    {}
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

    public void Visit(CardNode cardNode)
    {
        foreach (var effect in cardNode.OnActivation)
        {
            effect.Accept(this);
        }
    }
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

    public void Visit(NumberNode numberNode)
    {
        // No se necesita validación adicional para NumberNode
    }

    public void Visit(BinaryExpressionNode binaryExpressionNode)
    {
        binaryExpressionNode.Left.Accept(this);
        binaryExpressionNode.Right.Accept(this);
    }

    public void Visit(UnaryExpressionNode unaryExpressionNode)
    {
        if (unaryExpressionNode.Operand == null)
        {
            throw new Exception($"La expresión unaria debe tener un operando. Operador: {unaryExpressionNode.Operator}");
        }

        unaryExpressionNode.Operand.Accept(this);
    }
    public void Visit(BlockNode blockNode)
    {
        // Crear un nuevo conjunto de variables declaradas para el bloque
        var previousDeclaredVariables = new HashSet<string>(declaredVariables);

        foreach (var statement in blockNode.Statements)
        {
            statement.Accept(this);
        }

        declaredVariables = previousDeclaredVariables;
    }

}

