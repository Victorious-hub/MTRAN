namespace MTRAN.Interpreter
{
    public class SymbolTable
    {
        private readonly Stack<Dictionary<string, string>> _scopes = new Stack<Dictionary<string, string>>();
        private readonly Stack<string> _scopeNames = new Stack<string>(); // Track scope names
        private readonly Dictionary<string, FunctionInfo> _functions = new();
        private readonly Dictionary<string, string> _symbols = new Dictionary<string, string>(); // Global symbols (e.g., classes)
        private readonly List<(string ScopeName, Dictionary<string, string> Variables)> _reservedScopes = new List<(string, Dictionary<string, string>)>();

        public void DeclareClass(string className)
        {
            if (!_symbols.ContainsKey(className))
            {
                _symbols[className] = "class";
            }
        }
        public bool IsClassDeclared(string className)
        {
            return _symbols.ContainsKey(className) && _symbols[className] == "class";
        }

        public SymbolTable()
        {
            // Initialize with a global scope
            _scopes.Push(new Dictionary<string, string>());
            _scopeNames.Push("Global");
        }

        public void DeclareFunction(string name, List<string> parameterTypes, string returnType)
        {
            _functions[name] = new FunctionInfo(parameterTypes, returnType);
        }

        public bool IsFunctionDeclared(string functionName)
        {
            return _functions.ContainsKey(functionName);
        }

        public int GetFunctionParameterCount(string functionName)
        {
            if (!_functions.ContainsKey(functionName))
            {
                throw new Exception($"Semantic Error: Function '{functionName}' is not declared.");
            }

            // Access the ParameterTypes.Count property of the FunctionInfo object
            return _functions[functionName].ParameterTypes.Count;
        }

        public FunctionInfo GetFunctionInfo(string name)
        {
            return _functions[name];
        }

        public void EnterScope(string scopeName)
        {
            // Save the current scope to _reservedScopes before entering a new scope
            if (_scopes.Count > 0)
            {
                string currentScopeName = _scopeNames.Peek();
                var currentScopeVariables = new Dictionary<string, string>(_scopes.Peek());
                _reservedScopes.Add((currentScopeName, currentScopeVariables));
            }

            // Enter the new scope
            _scopes.Push(new Dictionary<string, string>());
            _scopeNames.Push(scopeName);
            Console.WriteLine($"[DEBUG] Entered scope: {scopeName}");
        }

        public void PrintAllScopes()
        {
            Console.WriteLine("[DEBUG] Current Symbol Table:");
            int scopeLevel = _scopeNames.Count;

            var scopeNamesArray = _scopeNames.ToArray();
            var scopesArray = _scopes.ToArray();

            for (int i = scopeLevel - 1; i >= 0; i--)
            {
                Console.WriteLine($"Scope Level {scopeLevel - i}: {scopeNamesArray[i]}");
                foreach (var entry in scopesArray[i])
                {
                    Console.WriteLine($"    {entry.Key}: {entry.Value}");
                }
            }
        }

        public void ExitScope()
        {
            if (_scopes.Count > 0)
            {
                // Save the exited scope to _reservedScopes
                string exitedScopeName = _scopeNames.Pop();
                var exitedScopeVariables = _scopes.Pop();
                _reservedScopes.Add((exitedScopeName, new Dictionary<string, string>(exitedScopeVariables)));

                Console.WriteLine($"[DEBUG] Exited scope: {exitedScopeName}");
            }
            else
            {
                throw new InvalidOperationException("Cannot exit the global scope.");
            }
        }

        public bool IsCurrentScope(string scopeName)
        {
            return _scopeNames.Peek() == scopeName;
        }

        public void DeclareVariable(string variable, string type)
        {
            if (_scopes.Peek().ContainsKey(variable))
            {
                throw new Exception($"Semantic Error: Variable '{variable}' is already declared in the current scope.");
            }

            _scopes.Peek()[variable] = type;
            Console.WriteLine($"[DEBUG] Declared variable '{variable}' of type '{type}' in the current scope.");
        }

        public void PrintReservedScopes()
        {
            Console.WriteLine("[DEBUG] Reserved Scopes:");
            foreach (var (scopeName, variables) in _reservedScopes)
            {
                Console.WriteLine($"Scope: {scopeName}");
                foreach (var (key, value) in variables)
                {
                    Console.WriteLine($"    {key}: {value}");
                }
            }
        }

        public string GetVariableType(string variable)
        {
            foreach (var scope in _scopes)
            {
                if (scope.ContainsKey(variable))
                {
                    return scope[variable];
                }
            }

            throw new Exception($"Semantic Error: Variable '{variable}' is not declared in any accessible scope.");
        }

        public bool IsVariableDeclared(string variable)
        {
            return _scopes.Any(scope => scope.ContainsKey(variable));
        }

        public void PrintSymbolTable()
        {
            Console.WriteLine("Symbol Table:");
            int scopeLevel = _scopes.Count;
            foreach (var scope in _scopes.Reverse())
            {
                Console.WriteLine($"Scope Level {scopeLevel--}:");
                foreach (var variable in scope)
                {
                    Console.WriteLine($"  {variable.Key}: {variable.Value}");
                }
            }
        }
    }
}

public class FunctionInfo
{
    public List<string> ParameterTypes { get; }
    public string ReturnType { get; }

    public FunctionInfo(List<string> parameterTypes, string returnType)
    {
        ParameterTypes = parameterTypes;
        ReturnType = returnType;
    }
}