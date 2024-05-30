namespace Scad.Openscad;

/// <summary>
/// Model cacher that does nothing
/// </summary>
public class NullCache : IRenderCache
{
    public Scad.Model? Lookup(string key)
    {
        return null;
    }

    public void Set(string key, Scad.Model model)
    {
    }

    public IRenderCache Clone()
    {
        return new NullCache();
    }

    public void Dispose()
    {
    }
}
