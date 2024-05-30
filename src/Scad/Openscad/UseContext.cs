namespace Scad.Openscad;

/// <summary>
/// Default context implementation
/// </summary>
public class UseContext : ExecutionContext {
    /// <summary>
    /// Create context with specified parent.
    /// </summary>
    public UseContext(ExecutionContext parent) : base(parent)
    {
    }

    /// Get function implementation
    public override Value.Function? GetFunction(string name)
    {
        return Parent?.GetFunction(name);
    }

    /// Get module implementation
    public override IModule? GetModule(string name)
    {
        return Parent?.GetModule(name);
    }

    /// Define function
    public override void SetFunction(string name, Value.Function fcn)
    {
        Parent?.SetFunction(name, fcn);
    }

    /// Define module
    public override void SetModule(string name, IModule mod)
    {
        Parent?.SetModule(name, mod);
    }
}
