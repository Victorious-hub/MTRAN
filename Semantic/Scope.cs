namespace MTRAN.Semantic
{
    public class ScopeSnapshot
    {
        public string ScopeName { get; }
        public Dictionary<string, string> Variables { get; }

        public ScopeSnapshot(string scopeName, Dictionary<string, string> variables)
        {
            ScopeName = scopeName;
            Variables = new Dictionary<string, string>(variables); // Create a copy to preserve state
        }
    }

    public class Scope
    {
        private readonly Stack<Dictionary<string, string>> _scopes = new Stack<Dictionary<string, string>>();
        private readonly Stack<string> _scopeNames = new Stack<string>();

        public void EnterScope(string scopeName)
        {
            _scopes.Push(new Dictionary<string, string>());
            _scopeNames.Push(scopeName);
            Console.WriteLine($"[DEBUG] Entering scope: {scopeName}");
        }

        public void ExitScope()
        {
            if (_scopes.Count == 0 || _scopeNames.Count == 0)
            {
                throw new Exception("Semantic Error: Attempted to exit a scope when no scope exists. Ensure scopes are properly entered and exited.");
            }

            string scopeName = _scopeNames.Pop();
            _scopes.Pop();
            Console.WriteLine($"[DEBUG] Exiting scope: {scopeName}");
        }

        public bool IsVariableDeclared(string variable)
        {
            foreach (var scope in _scopes)
            {
                if (scope.ContainsKey(variable))
                {
                    return true;
                }
            }
            return false;
        }

        public void DeclareVariable(string variable, string type)
        {
            if (_scopes.Count == 0)
            {
                throw new Exception("Semantic Error: No active scope to declare a variable.");
            }

            if (_scopes.Peek().ContainsKey(variable))
            {
                throw new Exception($"Semantic Error: Variable '{variable}' is already declared in this scope.");
            }

            _scopes.Peek()[variable] = type;
            Console.WriteLine($"[DEBUG] Declared variable '{variable}' of type '{type}' in scope '{GetCurrentScopeName()}'.");
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
            throw new Exception($"Semantic Error: Variable '{variable}' is not declared.");
        }

        public string GetCurrentScopeName()
        {
            if (_scopeNames.Count == 0)
            {
                throw new Exception("Semantic Error: No active scope.");
            }
            return _scopeNames.Peek();
        }

        public bool HasActiveScope()
        {
            return _scopes.Count > 0;
        }

        public ScopeSnapshot CaptureSnapshot()
        {
            if (_scopes.Count == 0 || _scopeNames.Count == 0)
            {
                throw new Exception("Semantic Error: No active scope to capture.");
            }

            string currentScopeName = _scopeNames.Peek();
            Dictionary<string, string> currentVariables = _scopes.Peek();

            return new ScopeSnapshot(currentScopeName, currentVariables);
        }
    }
}