namespace MTRAN.Semantic
{
    public class SymbolTable
{
    private readonly Dictionary<string, Dictionary<string, string>> _scopes = new Dictionary<string, Dictionary<string, string>>();
    private string _currentScope = "Block"; // Default scope is Block

    public void EnterScope(string scopeName)
    {
        _currentScope = scopeName;
        if (!_scopes.ContainsKey(scopeName))
        {
            _scopes[scopeName] = new Dictionary<string, string>();
        }
        Console.WriteLine($"[DEBUG] Entered scope: {scopeName}");
    }

    public void ExitScope()
    {
        Console.WriteLine($"[DEBUG] Exited scope: {_currentScope}");
        _currentScope = "Block"; // Reset to default Block scope after exiting
    }

    public void DeclareVariable(string variable, string type)
    {
        if (_scopes[_currentScope].ContainsKey(variable))
        {
            throw new Exception($"Semantic Error: Variable '{variable}' is already declared in scope '{_currentScope}'.");
        }

        _scopes[_currentScope][variable] = type;
        Console.WriteLine($"[DEBUG] Declared variable '{variable}' of type '{type}' in scope '{_currentScope}'.");
    }

    public string GetVariableType(string variable)
    {
        if (_scopes[_currentScope].ContainsKey(variable))
        {
            return _scopes[_currentScope][variable];
        }

        if (_scopes.ContainsKey("Block") && _scopes["Block"].ContainsKey(variable))
        {
            return _scopes["Block"][variable];
        }

        throw new Exception($"Semantic Error: Variable '{variable}' is not declared in scope '{_currentScope}' or default 'Block' scope.");
    }

    public bool IsVariableDeclared(string variable)
    {
        return _scopes[_currentScope].ContainsKey(variable) || 
               (_scopes.ContainsKey("Block") && _scopes["Block"].ContainsKey(variable));
    }

    public bool IsCurrentScope(string scopeName)
    {
        return _currentScope == scopeName;
    }

    public void PrintSymbolTable()
    {
        Console.WriteLine("Symbol Table:");
        foreach (var scope in _scopes)
        {
            Console.WriteLine($"Scope: {scope.Key}");
            foreach (var variable in scope.Value)
            {
                Console.WriteLine($"  {variable.Key}: {variable.Value}");
            }
        }
    }
}
}