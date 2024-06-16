namespace Scad.Openscad;

/// <summary>
/// Default context implementation
/// </summary>
public class ExecutionContext {
    private ExecutionContext? _parent = null;
    private int _level = 0;
    private Dictionary<string, Value> _vars = new();
    private Dictionary<string, Value.Function> _funcs = new();
    private Dictionary<string, IModule> _mods = new();
    private MaterialDB? _mdb = null;
    private Grammar.Ast? _ast = null;

    //public const int MaxLevel = 1024;
    public const int MaxLevel = 32;
    public ExecutionContext? Parent { get { return _parent; } }
    public int Level { get { return _level; } }
    public MaterialDB MaterialDB {
        get {
            if (_parent != null) {
                return _parent.MaterialDB;
            }

            if (_mdb == null) {
                _mdb = new MaterialDB();
            }

            return _mdb;
        }
    }

    public class RecursionLevelException : Exception { }

    /// <summary>
    /// Create context without parent. This context will not contain any standard modules of functions.
    /// </summary>
    public ExecutionContext() {}

    /// <summary>
    /// Create context with specified parent.
    /// </summary>
    public ExecutionContext(ExecutionContext parent)
    {
        _parent = parent;
        _level = parent.Level + 1;
        if (_level >= MaxLevel) {
            throw new RecursionLevelException();
        }
    }

    /// Get variable value
    public virtual Value GetVariable(string name)
    {
        if (_vars.ContainsKey(name)) {
            return _vars[name];
        }

        if (_funcs.ContainsKey(name)) {
            return _funcs[name];
        }

        if (_parent != null) {
            return _parent.GetVariable(name);
        }

        return new Value.Undefined();
    }

    public virtual Value? GetLocalVariable(string name)
    {
        if (_vars.ContainsKey(name)) {
            return _vars[name];
        }

        return null;
    }

    /// Get function implementation
    public virtual Value.Function? GetFunction(string name)
    {
        if (_vars.ContainsKey(name)) {
            var f = _vars[name];
            if (f is Value.Function) {
                return ((Value.Function)f);
            }
        }

        if (_funcs.ContainsKey(name)) {
            return _funcs[name];
        }

        if (_parent != null) {
            return _parent.GetFunction(name);
        }

        return null;
    }

    /// Get module implementation
    public virtual IModule? GetModule(string name)
    {
        if (_mods.ContainsKey(name)) {
            return _mods[name];
        }

        if (_parent != null) {
            return _parent.GetModule(name);
        }

        return null;
    }

    /// Enter new level of context
    public virtual ExecutionContext Enter(Grammar.Ast? ast = null)
    {
        var res = new ExecutionContext(this);
        res._ast = ast;
        return res;
    }

    /// Set variable value
    public virtual void SetVariable(string name, Value val)
    {
        _vars[name] = val;
    }

    /// Define function
    public virtual void SetFunction(string name, Value.Function fcn)
    {
        _funcs[name] = fcn;
    }

    /// Define module
    public virtual void SetModule(string name, IModule mod)
    {
        _mods[name] = mod;
    }

    /// Log message (for errors)
    public virtual void Log(string msg)
    {
        if (_ast != null) {
            int line = _ast.LineNo();
            if (line > 0) {
                if (_ast.PegFilename != null) {
                    msg = $"at {_ast.PegFilename}:{line}\n{msg}";
                } else {
                    msg = $"at line {line}\n{msg}";
                }
            }
        }

        if (_parent != null) {
            _parent.Log(msg);
        } else {
            Console.WriteLine(msg);
        }
    }
}
