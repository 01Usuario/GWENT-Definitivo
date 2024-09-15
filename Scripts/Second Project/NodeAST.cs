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
    public abstract void Accept(IVisitor visitor);
}

public interface IVisitor
{
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
    public string Name { get; set; }
    public ActionNode Action { get; set; }
    public ParamsNode Params { get; set; }
    public SelectorNode Selector { get; set; } 
    public EffectNode PostAction { get; set; } 

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SelectorNode : NodeAST
{
    public string Source { get; set; }
    public bool Single { get; set; }
    public NodeAST Predicate { get; set; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ActionNode : NodeAST
{
    public List<NodeAST> Declarations { get; set; } = new List<NodeAST>();

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ParamsNode : NodeAST
{
    public List<ParamNode> Params { get; set; } = new List<ParamNode>();

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
    
}


public class ParamNode : NodeAST
{
    public string Name { get; set; }
    public string Type { get; set; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class CardNode : NodeAST
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Faction { get; set; }
    public int Power { get; set; }
    public List<string> Range { get; set; } = new List<string>();
    public List<EffectNode> OnActivation { get; set; } = new List<EffectNode>();

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class AssignmentNode : NodeAST
{
    public string Identifier { get; set; }
    public string Value { get; set; }
    public string Operator { get; set; } 

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}


public class MethodCallNode : NodeAST
{
    public string MethodName { get; set; }
    public List<NodeAST> Arguments { get; set; } = new List<NodeAST>();

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ForLoopNode : NodeAST
{
    public string Variable { get; set; }
    public string Collection { get; set; }
    public List<NodeAST> Body { get; set; } = new List<NodeAST>();

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class IfStatementNode : NodeAST
{
    public NodeAST Condition { get; set; }
    public List<NodeAST> Body { get; set; } = new List<NodeAST>();

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class NumberNode : NodeAST
{
    public string Value { get; set; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class IdentifierNode : NodeAST
{
    public string Name { get; set; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class WhileLoopNode : NodeAST
{
    public NodeAST Condition { get; set; }
    public List<NodeAST> Body { get; set; } = new List<NodeAST>();

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class BinaryExpressionNode : NodeAST
{
    public NodeAST Left { get; set; }
    public string Operator { get; set; }
    public NodeAST Right { get; set; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
public class UnaryExpressionNode : NodeAST
{
    public NodeAST Operand { get; set; }
    public string Operator { get; set; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class BlockNode : NodeAST
{
    public List<NodeAST> Statements { get; set; } = new List<NodeAST>();

    public override void Accept(IVisitor visitor)
    {
        foreach (var statement in Statements)
        {
            statement.Accept(visitor);
        }
    }
}
