using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using static Unity.VisualScripting.Antlr3.Runtime.Tree.TreeWizard;
public abstract class NodeAST
{
    // Método abstracto que acepta un visitante.
    public abstract void Accept(IVisitor visitor);
}

public interface IVisitor
{
    // Métodos que visitan diferentes tipos de nodos.
    void Visit(EffectNode effectNode);
    void Visit(ParamsNode paramsNode);
    void Visit(ParamNode paramNode);
    void Visit(ActionNode actionNode);
    void Visit(CardNode cardNode);
    void Visit(AssignmentNode assignmentNode);
    void Visit(MethodCallNode callNode);
    void Visit(IfStatementNode ifStatementNode);
    void Visit(ForLoopNode forLoopNode);
    void Visit(WhileLoopNode whileLoopNode);
    void Visit(NumberNode numberNode);
    void Visit(IdentifierNode identifierNode);
    void Visit(BinaryExpressionNode binaryExpressionNode);
    void Visit(UnaryExpressionNode unaryExpressionNode);
    void Visit(SelectorNode selectorNode);
    void Visit(BlockNode blockNode);
}

public class EffectNode : NodeAST
{
    // Nombre del efecto.
    public string Name { get; set; }
    // Acción asociada al efecto.
    public ActionNode Action { get; set; }
    // Parámetros del efecto.
    public ParamsNode Params { get; set; }
    // Selector del efecto.
    public SelectorNode Selector { get; set; }
    // Acción posterior al efecto.
    public EffectNode PostAction { get; set; }

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SelectorNode : NodeAST
{
    // Fuente del selector.
    public string Source { get; set; }
    // Indica si es un selector único.
    public bool Single { get; set; }
    // Predicado del selector.
    public NodeAST Predicate { get; set; }

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ActionNode : NodeAST
{
    // Lista de declaraciones en la acción.
    public List<NodeAST> Declarations { get; set; } = new List<NodeAST>();

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ParamsNode : NodeAST
{
    // Lista de parámetros.
    public List<ParamNode> Params { get; set; } = new List<ParamNode>();

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ParamNode : NodeAST
{
    // Nombre del parámetro.
    public string Name { get; set; }
    // Tipo del parámetro.
    public string Type { get; set; }

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class CardNode : NodeAST
{
    // Tipo de la carta.
    public string Type { get; set; }
    // Nombre de la carta.
    public string Name { get; set; }
    // Facción de la carta.
    public string Faction { get; set; }
    // Poder de la carta.
    public int Power { get; set; }
    // Rango de la carta.
    public List<string> Range { get; set; } = new List<string>();
    // Efectos al activar la carta.
    public List<EffectNode> OnActivation { get; set; } = new List<EffectNode>();

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class AssignmentNode : NodeAST
{
    // Identificador de la asignación.
    public string Identifier { get; set; }
    // Valor de la asignación.
    public string Value { get; set; }
    // Operador de la asignación.
    public string Operator { get; set; }

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class MethodCallNode : NodeAST
{
    // Nombre del método.
    public string MethodName { get; set; }
    // Argumentos del método.
    public List<NodeAST> Arguments { get; set; } = new List<NodeAST>();

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ForLoopNode : NodeAST
{
    // Variable del bucle.
    public string Variable { get; set; }
    // Colección sobre la que itera el bucle.
    public string Collection { get; set; }
    // Cuerpo del bucle.
    public List<NodeAST> Body { get; set; } = new List<NodeAST>();

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class IfStatementNode : NodeAST
{
    // Condición del if.
    public NodeAST Condition { get; set; }
    // Cuerpo del if.
    public List<NodeAST> Body { get; set; } = new List<NodeAST>();

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class NumberNode : NodeAST
{
    // Valor del número.
    public string Value { get; set; }

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class IdentifierNode : NodeAST
{
    // Nombre del identificador.
    public string Name { get; set; }

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class WhileLoopNode : NodeAST
{
    // Condición del bucle while.
    public NodeAST Condition { get; set; }
    // Cuerpo del bucle while.
    public List<NodeAST> Body { get; set; } = new List<NodeAST>();

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class BinaryExpressionNode : NodeAST
{
    // Nodo izquierdo de la expresión binaria.
    public NodeAST Left { get; set; }
    // Operador de la expresión binaria.
    public string Operator { get; set; }
    // Nodo derecho de la expresión binaria.
    public NodeAST Right { get; set; }

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class UnaryExpressionNode : NodeAST
{
    // Operando de la expresión unaria.
    public NodeAST Operand { get; set; }
    // Operador de la expresión unaria.
    public string Operator { get; set; }

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class BlockNode : NodeAST
{
    // Lista de declaraciones en el bloque.
    public List<NodeAST> Statements { get; set; } = new List<NodeAST>();

    // Método que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        // Para cada declaración en el bloque, aceptar el visitante.
        foreach (var statement in Statements)
        {
            statement.Accept(visitor);
        }
    }
}
