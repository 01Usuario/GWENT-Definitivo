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
    // M�todo abstracto que acepta un visitante.
    public abstract void Accept(IVisitor visitor);
}

public interface IVisitor
{
    // M�todos que visitan diferentes tipos de nodos.
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
    // Acci�n asociada al efecto.
    public ActionNode Action { get; set; }
    // Par�metros del efecto.
    public ParamsNode Params { get; set; }
    // Selector del efecto.
    public SelectorNode Selector { get; set; }
    // Acci�n posterior al efecto.
    public EffectNode PostAction { get; set; }

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SelectorNode : NodeAST
{
    // Fuente del selector.
    public string Source { get; set; }
    // Indica si es un selector �nico.
    public bool Single { get; set; }
    // Predicado del selector.
    public NodeAST Predicate { get; set; }

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ActionNode : NodeAST
{
    // Lista de declaraciones en la acci�n.
    public List<NodeAST> Declarations { get; set; } = new List<NodeAST>();

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ParamsNode : NodeAST
{
    // Lista de par�metros.
    public List<ParamNode> Params { get; set; } = new List<ParamNode>();

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ParamNode : NodeAST
{
    // Nombre del par�metro.
    public string Name { get; set; }
    // Tipo del par�metro.
    public string Type { get; set; }

    // M�todo que acepta un visitante.
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
    // Facci�n de la carta.
    public string Faction { get; set; }
    // Poder de la carta.
    public int Power { get; set; }
    // Rango de la carta.
    public List<string> Range { get; set; } = new List<string>();
    // Efectos al activar la carta.
    public List<EffectNode> OnActivation { get; set; } = new List<EffectNode>();

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class AssignmentNode : NodeAST
{
    // Identificador de la asignaci�n.
    public string Identifier { get; set; }
    // Valor de la asignaci�n.
    public string Value { get; set; }
    // Operador de la asignaci�n.
    public string Operator { get; set; }

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class MethodCallNode : NodeAST
{
    // Nombre del m�todo.
    public string MethodName { get; set; }
    // Argumentos del m�todo.
    public List<NodeAST> Arguments { get; set; } = new List<NodeAST>();

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ForLoopNode : NodeAST
{
    // Variable del bucle.
    public string Variable { get; set; }
    // Colecci�n sobre la que itera el bucle.
    public string Collection { get; set; }
    // Cuerpo del bucle.
    public List<NodeAST> Body { get; set; } = new List<NodeAST>();

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class IfStatementNode : NodeAST
{
    // Condici�n del if.
    public NodeAST Condition { get; set; }
    // Cuerpo del if.
    public List<NodeAST> Body { get; set; } = new List<NodeAST>();

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class NumberNode : NodeAST
{
    // Valor del n�mero.
    public string Value { get; set; }

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class IdentifierNode : NodeAST
{
    // Nombre del identificador.
    public string Name { get; set; }

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class WhileLoopNode : NodeAST
{
    // Condici�n del bucle while.
    public NodeAST Condition { get; set; }
    // Cuerpo del bucle while.
    public List<NodeAST> Body { get; set; } = new List<NodeAST>();

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class BinaryExpressionNode : NodeAST
{
    // Nodo izquierdo de la expresi�n binaria.
    public NodeAST Left { get; set; }
    // Operador de la expresi�n binaria.
    public string Operator { get; set; }
    // Nodo derecho de la expresi�n binaria.
    public NodeAST Right { get; set; }

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class UnaryExpressionNode : NodeAST
{
    // Operando de la expresi�n unaria.
    public NodeAST Operand { get; set; }
    // Operador de la expresi�n unaria.
    public string Operator { get; set; }

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class BlockNode : NodeAST
{
    // Lista de declaraciones en el bloque.
    public List<NodeAST> Statements { get; set; } = new List<NodeAST>();

    // M�todo que acepta un visitante.
    public override void Accept(IVisitor visitor)
    {
        // Para cada declaraci�n en el bloque, aceptar el visitante.
        foreach (var statement in Statements)
        {
            statement.Accept(visitor);
        }
    }
}
