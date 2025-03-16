namespace MTRAN.LR3
{
    public abstract class ASTNode
    {
    }

    public class AssignmentNode : ASTNode
    {
        public string Variable { get; }
        public ASTNode Value { get; }

        public AssignmentNode(string variable, ASTNode value)
        {
            Variable = variable;
            Value = value;
        }
    }

    public class IfNode : ASTNode
    {
        public ASTNode Condition { get; }
        public ASTNode ThenBranch { get; }
        public List<ElseIfNode> ElseIfBranches { get; }
        public ASTNode ElseBranch { get; }

        public IfNode(ASTNode condition, ASTNode thenBranch, List<ElseIfNode> elseIfBranches, ASTNode elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseIfBranches = elseIfBranches;
            ElseBranch = elseBranch;
        }
    }

    public class ParameterDeclarationNode : ASTNode
    {
        public List<VariableNode> Parameters { get; }

        public ParameterDeclarationNode(List<VariableNode> parameters)
        {
            Parameters = parameters;
        }
    }

    public class BlockNode : ASTNode
    {
        public List<ASTNode> Statements { get; }

        public BlockNode(List<ASTNode> statements)
        {
            Statements = statements;
        }
    }

    public class ElseIfNode : ASTNode
    {
        public ASTNode Condition { get; }
        public ASTNode ThenBranch { get; }

        public ElseIfNode(ASTNode condition, ASTNode thenBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
        }
    }

    public class ElseNode : ASTNode
    {
        public ASTNode ThenBranch { get; }

        public ElseNode(ASTNode thenBranch)
        {
            ThenBranch = thenBranch;
        }
    }

    public class BinaryOperationNode : ASTNode
    {
        public ASTNode Left { get; }
        public string Operator { get; }
        public ASTNode Right { get; }

        public BinaryOperationNode(ASTNode left, string op, ASTNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }

    public class IntNode : ASTNode
    {
        public int Value { get; }

        public IntNode(int value)
        {
            Value = value;
        }
    }

    public class NumberNode : ASTNode
    {
        public float Value { get; }

        public NumberNode(float value)
        {
            Value = value;
        }
    }

    public class StatementListNode : ASTNode
    {
        public List<ASTNode> Statements { get; }

        public StatementListNode(List<ASTNode> statements)
        {
            Statements = statements;
        }
    }

    public class StringNode : ASTNode
    {
        public string Value { get; }

        public StringNode(string value)
        {
            Value = value;
        }
    }

    public class VariableNode : ASTNode
    {
        public string Name { get; }

        public VariableNode(string name)
        {
            Name = name;
        }
    }

    public class VariableDeclarationNode : ASTNode
    {
        public string Keyword { get; set; }
        public string Variable { get; set; }
        public ASTNode Value { get; set; }

        public VariableDeclarationNode(string keyword, string variable, ASTNode value)
        {
            Keyword = keyword;
            Variable = variable;
            Value = value;
        }
    }

    public class ForNode : ASTNode
    {
        public ASTNode Initialization { get; }
        public ASTNode Condition { get; }
        public ASTNode Increment { get; }
        public ASTNode Body { get; }

        public ForNode(ASTNode initialization, ASTNode condition, ASTNode increment, ASTNode body)
        {
            Initialization = initialization;
            Condition = condition;
            Increment = increment;
            Body = body;
        }
    }

    public class UnaryOperationNode : ASTNode
    {
        public string Variable { get; }
        public string Operator { get; }

        public UnaryOperationNode(string variable, string op)
        {
            Variable = variable;
            Operator = op;
        }
    }

    public class ForeachNode : ASTNode
    {
        public string Variable { get; }
        public ASTNode Collection { get; }
        public ASTNode Body { get; }

        public ForeachNode(string variable, ASTNode collection, ASTNode body)
        {
            Variable = variable;
            Collection = collection;
            Body = body;
        }
    }

    public class WhileNode : ASTNode
    {
        public ASTNode Condition { get; }
        public ASTNode Body { get; }

        public WhileNode(ASTNode condition, ASTNode body)
        {
            Condition = condition;
            Body = body;
        }
    }

    public class ReturnNode : ASTNode
    {
        public ASTNode Value { get; }

        public ReturnNode(ASTNode value)
        {
            Value = value;
        }
    }

    public class FunctionNode : ASTNode
    {
        public string Name { get; }
        public List<string> Parameters { get; }
        public ASTNode Body { get; }

        public FunctionNode(string name, List<string> parameters, ASTNode body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }

    public class ArrayNode : ASTNode
    {
        public string Name { get; }
        public List<ASTNode> Elements { get; }

        public ArrayNode(string name, List<ASTNode> elements)
        {
            Name = name;
            Elements = elements;
        }
    }

    public class PunctuationNode : ASTNode
    {
        public string Punctuation { get; }

        public PunctuationNode(string punctuation)
        {
            Punctuation = punctuation;
        }
    }


    public class ParenthesizedExpression : ASTNode
    {
        public ASTNode Expression { get; }

        public ParenthesizedExpression(ASTNode expression)
        {
            Expression = expression;
        }
    }

    public class HashNode : ASTNode
    {
        public string Name { get; }
        public List<(ASTNode Key, ASTNode Value)> Elements { get; }

        public HashNode(string name, List<(ASTNode Key, ASTNode Value)> elements)
        {
            Name = name;
            Elements = elements;
        }
    }

}