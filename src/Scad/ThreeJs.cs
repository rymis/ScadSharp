namespace Scad;

using System.Text.Json;
using System.Text.Json.Serialization;
using Vec3 = Scad.Linalg.Vec3;
using Vec2 = Scad.Linalg.Vec2;

public class ThreeJs
{
    public class JsonModel
    {
        [JsonPropertyName("meshes")]
        public List<Mesh> Meshes { get; set; } = new();

        [JsonPropertyName("images")]
        public List<Image> Images { get; set; } = new();
    }

    public class Mesh
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; } = new();

        [JsonPropertyName("material")]
        public Material Material { get; set; } = new();
    }

    public class Color
    {
        [JsonPropertyName("r")]
        public float R { get; set; } = 1.0f;

        [JsonPropertyName("g")]
        public float G { get; set; } = 1.0f;

        [JsonPropertyName("b")]
        public float B { get; set; } = 1.0f;

        public Color(float r = 1.0f, float g = 1.0f, float b = 1.0f)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    public class Material
    {
        [JsonPropertyName("color")]
        public Color Color { get; set; } = new();

        [JsonPropertyName("ambient")]
        public Color Ambient { get; set; } = new();

        [JsonPropertyName("emissive")]
        public Color Emissive { get; set; } = new(0.0f, 0.0f, 0.0f);

        [JsonPropertyName("specular")]
        public Color Specular { get; set; } = new(0.06f, 0.06f, 0.06f);

        [JsonPropertyName("shininess")]
        public float Shininess { get; set; } = 30.0f;

        [JsonPropertyName("opacity")]
        public float Opacity { get; set; } = 1.0f;

        [JsonPropertyName("transparent")]
        public bool Transparent { get; set; } = false;

        [JsonPropertyName("wireframe")]
        public bool Wireframe { get; set; } = false;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("map")]
        public string? Map { get; set; } = null;
    }

    public class Image
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = System.Guid.NewGuid().ToString();

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
    }

    public class Texture
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = System.Guid.NewGuid().ToString();

        [JsonPropertyName("image")]
        public string Image { get; set; } = "";

        [JsonPropertyName("wrap")]
        public List<string> Wrap { get; set; } = new(){"repeat", "repeat"};

        [JsonPropertyName("repeat")]
        public List<float> Repeat { get; set; } = new(){1.0f, 1.0f};
    }

    public class Geometry
    {
        [JsonPropertyName("position")]
        public Float32Array Position { get; set; } = new(3);

        [JsonPropertyName("normal")]
        public Float32Array Normal { get; set; } = new(3);

        [JsonPropertyName("uv")]
        public Float32Array Uv { get; set; } = new(2);

        [JsonPropertyName("boundingSphere")]
        public BoundingSphere Sphere { get { return this.GetBoundingSphere(); } }

        private BoundingSphere GetBoundingSphere()
        {
            if (Position.Count() == 0) {
                return new();
            }

            var res = new BoundingSphere();
            if (Position.Count() == 1) {
                res.SetPosition(Position.GetVec3(0));
                res.Radius = 0.001f;

                return res;
            }

            if (Position.Count() == 2) {
                Vec3 p1 = Position.GetVec3(0);
                Vec3 p2 = Position.GetVec3(1);

                res.SetPosition((p1 + p2) / 2.0f);
                res.Radius = p1.Distance(p2) / 2.0f;

                return res;
            }

            // Ritter's bounding sphere implementation. It is not optimal algorithm but who cares?

            // List of points that are not in sphere
            Vec3 a = Position.GetVec3(0);
            int bIdx = 1;
            float bestDist = a.Distance2(Position.GetVec3(1));
            for (int i = 2; i < Position.Count(); ++i) {
                float d = a.Distance2(Position.GetVec3(i));
                if (d > bestDist) {
                    bIdx = i;
                    bestDist = d;
                }
            }
            Vec3 b = Position.GetVec3(bIdx);
            int cIdx = 0;
            bestDist = b.Distance2(a);
            for (int i = 1; i < Position.Count(); ++i) {
                if (i == bIdx) {
                    continue;
                }

                float d = b.Distance2(Position.GetVec3(i));
                if (d > bestDist) {
                    cIdx = i;
                    bestDist = d;
                }
            }
            Vec3 center = (b + Position.GetVec3(cIdx)) / 2.0f;
            float r = MathF.Sqrt(bestDist) / 2.0f;

            // Find all out points:
            for (int i = 0; i < Position.Count(); ++i) {
                if (i == bIdx || i == cIdx) {
                    continue;
                }
                var x = Position.GetVec3(i);

                float d = center.Distance(x);
                if (d > r) {
                    // Correct center and distance:
                    Vec3 n = (x - center).Unit();
                    float dx = (d - r) / 2.0f;
                    r = (d + r) / 2.0f;
                    center += n * dx;
                }
            }

            res.SetPosition(center);
            res.Radius = r;

            return res;
        }

        public class BoundingSphere
        {
            [JsonPropertyName("position")]
            public List<float> Position { get; set; } = new();

            public void SetPosition(Vec3 v)
            {
                Position.Clear();
                Position.Add(v.X);
                Position.Add(v.Y);
                Position.Add(v.Z);
            }

            [JsonPropertyName("radius")]
            public float Radius { get; set; } = 0.0f;
        }
    }

    public class Float32Array
    {
        public Float32Array(int itemSize)
        {
            ItemSize = itemSize;
            if (ItemSize < 2 || ItemSize > 3) {
                throw new System.IndexOutOfRangeException("Only 2/3 dimmensional arrays are supported");
            }
        }

        [JsonPropertyName("itemSize")]
        public int ItemSize { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; init; } = "Float32Array";

        [JsonPropertyName("array")]
        public List<float> Array { get; set; } = new();

        public void Append(Vec3 v)
        {
            Array.Add(v.X);
            Array.Add(v.Y);

            if (ItemSize == 3) {
                Array.Add(v.Z);
            }
        }

        public void Append(Vec2 v)
        {
            Array.Add(v.X);
            Array.Add(v.Y);

            if (ItemSize == 3) {
                Array.Add(0.0f);
            }
        }

        public void Get(int idx, out Vec2 v)
        {
            v = new Vec2(Array[idx * ItemSize], Array[idx * ItemSize + 1]);
        }

        public void Get(int idx, out Vec3 v)
        {
            if (ItemSize == 2) {
                v = new Vec3(Array[idx * ItemSize], Array[idx * ItemSize + 1]);
            } else {
                v = new Vec3(Array[idx * ItemSize], Array[idx * ItemSize + 1], Array[idx * ItemSize + 2]);
            }
        }

        public Vec3 GetVec3(int idx)
        {
            Get(idx, out Vec3 res);
            return res;
        }

        public int Count()
        {
            return Array.Count / ItemSize;
        }
    }

    public static void Save(Model m, System.IO.TextWriter w)
    {
        w.WriteLine(JsonSerializer.Serialize(ToJson(m)));
    }

    public static JsonModel ToJson(Model model)
    {
        MaterialDB db = new();
        return ToJson(db, model);
    }

    public static JsonModel ToJson(MaterialDB materials, Model model)
    {
        var res = new JsonModel();

        // Process all faces:
        if (model.Count() == 0) {
            return res;
        }

        foreach (var (matId, mat, faces) in materials.SplitFaces(model)) {
            var mesh = new Mesh();
            var geometry = mesh.Geometry;

            mesh.Material.Color = new Color(mat.Color.R, mat.Color.G, mat.Color.B);

            foreach (var f in faces) {
                for (int j = 0; j < 3; ++j) {
                    geometry.Position.Append(f.Vertices[j]);
                    geometry.Normal.Append(f.Normals[j]);
                    geometry.Uv.Append(f.TexCoordinates[j]);
                }
            }

            res.Meshes.Add(mesh);
        }

        return res;
    }

}
