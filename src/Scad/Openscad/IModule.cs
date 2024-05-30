namespace Scad.Openscad;

/// Module implementation
public interface IModule
{
    public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments);
}
