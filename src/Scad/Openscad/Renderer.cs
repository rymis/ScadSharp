namespace Scad.Openscad;

/// <summary>
/// Renderer allows to render construction tree into 3D Model.
/// </summary>
public class Renderer
{
    public IRenderCache Cache = new NullCache();

    public Renderer()
    {
    }

    public Renderer(IRenderCache cache)
    {
        Cache = cache;
    }

    public Scad.Model RenderTree(Tree.Node node)
    {
        using(var updater = new CacheUpdater(Cache)) {
            var res = node.Render(updater);
            Cache = updater.Clone();
            return res;
        }
    }

}
