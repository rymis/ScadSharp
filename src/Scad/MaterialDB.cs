namespace Scad;

public class MaterialDB
{
    private Dictionary<string, Material> _materials = new();
    private static Material _defaultMaterial = GenDefaultMaterial();

    public Material this[string name] {
        get {
            if (_materials.ContainsKey(name)) {
                return _materials[name];
            }

            if (name.StartsWith("#") && name.Length == 7) {
                try {
                    uint val = Convert.ToUInt32(name.Substring(1), 16);
                    float r = (float)((val >> 16) & 0xff);
                    float g = (float)((val >> 8) & 0xff);
                    float b = (float)(val & 0xff);

                    var m = new Material();
                    m.Color.R = r / 255.0f;
                    m.Color.G = g / 255.0f;
                    m.Color.B = b / 255.0f;

                    return m;
                } catch (Exception) {
                }
            }

            return _defaultMaterial;
        }

        set {
            _materials[name] = value;
        }
    }

    public IEnumerable<(string Name, Material Material)> GetMaterials()
    {
        foreach (var m in _materials) {
            yield return (m.Key, m.Value);
        }
    }

    static Material GenDefaultMaterial()
    {
        var m = new Material();
        m.Color.R = 0.6f;
        m.Color.G = 0.5f;
        m.Color.B = 0.6f;

        return m;
    }

    static System.Text.RegularExpressions.Regex _nameReplacer = new(@"[^a-zA-Z0-9]");
    string SafeName(string name)
    {
        return _nameReplacer.Replace(name, "_");
    }

    public IEnumerable<(string materialId, Material material, IEnumerable<Face>)> SplitFaces(Model model)
    {
        // Prepare list of materials:
        var materials = new Dictionary<string, (string materialId, Material material)>();
        for (int i = 0; i < model.Count(); ++i) {
            var f = model.GetFace(i);
            if (!materials.ContainsKey(f.MaterialId)) {
                var m = this[f.MaterialId];
                var mKey = SafeName(f.MaterialId);
                if (materials.ContainsKey(mKey)) {
                    for (int xxx = 0;; ++xxx) {
                        var s = mKey + "-" + xxx.ToString();
                        if (!materials.ContainsKey(s)) {
                            mKey = s;
                            break;
                        }
                    }
                }

                materials[f.MaterialId] = (mKey, m);
            }
        }

        foreach (var item in materials) {
            yield return (item.Value.materialId, item.Value.material, model.GetFacesByMaterial(item.Key));
        }
    }

}
