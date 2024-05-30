namespace Scad.Openscad;

/// <summary>
/// Cache implementation that allows to update old cache with new values
/// </summary>
public class CacheUpdater : IRenderCache
{
    private IRenderCache _old;
    private IRenderCache _new;

    public CacheUpdater(IRenderCache cache)
    {
        _old = cache;
        _new = cache.Clone();
    }

    public Scad.Model? Lookup(string key)
    {
        var res = _old.Lookup(key);
        if (res != null) {
            _new.Set(key, res);
        }
        return res;
    }

    public void Set(string key, Scad.Model model)
    {
        _new.Set(key, model);
    }

    public IRenderCache Clone()
    {
        return _new;
    }

    public void Dispose()
    {
        _old.Dispose();
    }
}
