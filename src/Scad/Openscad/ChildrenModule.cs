namespace Scad.Openscad;

public class ChildrenModule : IModule
{
    private List<Tree.Node> _children;

    public ChildrenModule(List<Tree.Node> children)
    {
        _children = children;
    }

    public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
    {
        if (arguments.Count() > 0) {
            var n = arguments[0].Val;
            if (n is Value.Number) {
                int idx = (int)(((Value.Number)n).Val);
                if (idx < 0 || idx > _children.Count) {
                    return new();
                }

                var res = new List<Tree.Node>();
                res.Add(_children[idx]);

                return res;
            }

            if (n is Value.List) {
                var lst = ((Value.List)n).Val;
                var res = new List<Tree.Node>();

                foreach (var n2 in lst) {
                    if (n2 is Value.Number) {
                        int idx = (int)(((Value.Number)n2).Val);
                        if (idx < 0 || idx > _children.Count) {
                            return new();
                        }

                        res.Add(_children[idx]);
                    }
                }

                return res;
            }

            context.Log("Invalid arguments for children");
            return new();
        }

        return _children;
    }
}
