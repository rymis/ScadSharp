namespace Scad.Openscad;

public interface IRenderCache : IDisposable
{
    public Scad.Model? Lookup(string key);
    public void Set(string key, Scad.Model model);
    public IRenderCache Clone();
}
