namespace MTRAN.Parser
{
    public abstract class ASTNode
    {
    }

    public class ParameterNode : ASTNode
    {
        public string Name { get; }

        public ParameterNode(string name)
        {
            Name = name;
        }
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

    public class ArrayAccessNode : ASTNode
    {
        public string ArrayName { get; }
        public ASTNode Index { get; }

        public ArrayAccessNode(string arrayName, ASTNode index)
        {
            ArrayName = arrayName;
            Index = index;
        }
    }

    public class HashAccessNode : ASTNode
    {
        public string HashName { get; }
        public ASTNode Key { get; }

        public HashAccessNode(string hashName, ASTNode key)
        {
            HashName = hashName;
            Key = key;
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

    public class InterpolatedStringNode : ASTNode
    {
        public List<ASTNode> Parts { get; }

        public InterpolatedStringNode(List<ASTNode> parts)
        {
            Parts = parts;
        }
    }

    public class UseNode : ASTNode
    {
        public string ConstantName { get; }
        public ASTNode Value { get; }

        public UseNode(string constantName, ASTNode value)
        {
            ConstantName = constantName;
            Value = value;
        }
    }

    public class ArrayElementAssignmentNode : ASTNode
    {
        public string ArrayName { get; }
        public ASTNode Index { get; }
        public ASTNode Value { get; }

        public ArrayElementAssignmentNode(string arrayName, ASTNode index, ASTNode value)
        {
            ArrayName = arrayName;
            Index = index;
            Value = value;
        }
    }

    public class HashElementAssignmentNode : ASTNode
    {
        public string HashName { get; }
        public ASTNode Key { get; }
        public ASTNode Value { get; }

        public HashElementAssignmentNode(string hashName, ASTNode key, ASTNode value)
        {
            HashName = hashName;
            Key = key;
            Value = value;
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

    public class CompoundAssignmentNode : ASTNode
    {
        public string Variable { get; }
        public string OperatorSymbol { get; }
        public ASTNode Value { get; }

        public CompoundAssignmentNode(string variable, string operatorSymbol, ASTNode value)
        {
            Variable = variable;
            OperatorSymbol = operatorSymbol;
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
        public ASTNode VariableDeclaration { get; }
        public ASTNode Array { get; }
        public ASTNode Body { get; }

        public ForeachNode(ASTNode variableDeclaration, ASTNode array, ASTNode body)
        {
            VariableDeclaration = variableDeclaration;
            Array = array;
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
    public List<ASTNode> Parameters { get; }
    public ASTNode Body { get; }

    public FunctionNode(string name, List<ASTNode> parameters, ASTNode body)
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

    public class GroupedAssignmentNode : ASTNode
    {
        public List<ASTNode> Variables { get; }
        public ASTNode Value { get; }

        public GroupedAssignmentNode(List<ASTNode> variables, ASTNode value)
        {
            Variables = variables;
            Value = value;
        }
    }

    public class PrintStatementNode : ASTNode
    {
        public List<ASTNode> Arguments { get; }

        public PrintStatementNode(List<ASTNode> arguments)
        {
            Arguments = arguments;
        }
    }

    public class FunctionCallNode : ASTNode
    {
        public string FunctionName { get; }
        public List<ASTNode> Arguments { get; }

        public FunctionCallNode(string functionName, List<ASTNode> arguments)
        {
            FunctionName = functionName;
            Arguments = arguments;
        }
    }

    public class ClassNode : ASTNode
    {
        public string ClassName { get; }
        public List<ASTNode> Methods { get; }

        public ClassNode(string className, List<ASTNode> methods)
        {
            ClassName = className;
            Methods = methods;
        }

    }

    public class ObjectNode : ASTNode
    {
        public string Name { get; }
        public List<(ASTNode Key, ASTNode Value)> Properties { get; }

        public ObjectNode(string name, List<(ASTNode Key, ASTNode Value)> properties)
        {
            Name = name;
            Properties = properties;
        }
    }

    public class ErrorNode : ASTNode
    {
        public string ErrorMessage { get; }

        public ErrorNode(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public override string ToString()
        {
            return $"Error: {ErrorMessage}";
        }
    }

    public class ProgramNode : ASTNode
    {
        public List<ASTNode> Children { get; }

        public ProgramNode(List<ASTNode> children)
        {
            Children = children;
        }
    }

    public class BlessNode : ASTNode
    {
        public ASTNode Object { get; }
        public ASTNode Class { get; }

        public BlessNode(ASTNode obj, ASTNode cls)
        {
            Object = obj;
            Class = cls;
        }

    }

    public class KeywordNode : ASTNode
    {
        public string Keyword { get; }

        public KeywordNode(string keyword)
        {
            Keyword = keyword;
        }

    }
}