namespace Scad;

using System.Text.Json;
using System.Text.Json.Serialization;
using Vec3 = Scad.Linalg.Vec3;

public class Phoria
{
    public class Point
    {
        [JsonPropertyName("x")]
        public float X { get; set; } = 0.0f;
        [JsonPropertyName("y")]
        public float Y { get; set; } = 0.0f;
        [JsonPropertyName("z")]
        public float Z { get; set; } = 0.0f;
    }

    public class Polygon
    {
        [JsonPropertyName("vertices")]
        public List<int> Vertices { get; set; } = new();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("texture")]
        public int? Texture { get; set; } = null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("color")]
        public List<int>? Color { get; set; } = null;

        [JsonPropertyName("uvs")]
        public List<float> UV { get; set; } = new();
    }

    public class Style
    {
        /// RGB colour of the object surface
        [JsonPropertyName("color")]
        public List<int> Color { get; set; } = new(){255, 128, 128};

        /// if not zero, specifies specular shinyness power - e.g. values like 16 or 64
        [JsonPropertyName("specular")]
        public float Specular { get; set; } = 0.0f;

        /// material diffusion generally ranges from 0-1
        [JsonPropertyName("diffuse")]
        public float Diffuse { get; set; } = 1.0f;

        /// material emission (glow) 0-1
        [JsonPropertyName("emit")]
        public float Emit { get; set; } = 0.0f;

        /// material opacity 0-1
        [JsonPropertyName("opacity")]
        public float Opacity { get; set; } = 1.0f;

        /// one of "point", "wireframe", "solid"
        [JsonPropertyName("drawmode")]
        public string DrawMode { get; set; } = "solid";

        /// one of "plain", "lightsource", "sprite", "callback" (only for point rendering)
        [JsonPropertyName("shademode")]
        public string ShadeMode { get; set; } = "lightsource";

        /// one of "fill", "filltwice", "inflate", "fillstroke", "hiddenline"
        [JsonPropertyName("fillmode")]
        public string FillMode { get; set; } = "inflate";

        /// coarse object sort - one of "sorted", "front", "back"
        [JsonPropertyName("objectsortmode")]
        public string ObjectSortMode { get; set; } = "sorted";

        /// point, edge or polygon sorting mode - one of "sorted", "automatic", "none"
        [JsonPropertyName("geometrysortmode")]
        public string GeometrySortMode { get; set; } = "automatic";

        /// wireframe line thickness
        [JsonPropertyName("linewidth")]
        public float LineWidth { get; set; } = 1.0f;

        /// depth based scaling factor for wireframes - can be zero for no scaling
        [JsonPropertyName("linescale")]
        public float LineScale { get; set; } = 0.0f;

        /// true to always render polygons - i.e. do not perform hidden surface test
        [JsonPropertyName("doublesided")]
        public bool DoubleSided { get; set; } = false;

        /// default texture index to use for polygons if not specified - e.g. when UVs are used
        //[JsonPropertyName("texture")]
        //public int Texture { get; set; } = 0;
    }

    public class JsonModel
    {
        [JsonPropertyName("points")]
        public List<Point> Points { get; set; } = new();

        // TODO: edges

        [JsonPropertyName("polygons")]
        public List<Polygon> Polygons { get; set; } = new();

        [JsonPropertyName("style")]
        public Style Style { get; set; } = new();
    }

    public static void Save(Model m, System.IO.TextWriter w)
    {
        w.WriteLine(JsonSerializer.Serialize(ToJson(m)));
    }

    public static JsonModel ToJson(Model model)
    {
        var res = new JsonModel();
        Vec3Index index = new();

        int VecIndex(Vec3 v)
        {
            int idx = index.Add(v, out bool added);
            if (added) {
                var p = new Point();
                p.X = (float)v.X;
                p.Y = (float)v.Y;
                p.Z = (float)v.Z;
                res.Points.Add(p);
            }

            return idx;
        }

        // Process all faces:
        if (model.Count() == 0) {
            return res;
        }

        for (int i = 0; i < model.Count(); ++i) {
            var f = model.GetFace(i);
            var p = new Polygon();
            for (int j = 0; j < 3; ++j) {
                p.Vertices.Add(VecIndex(f.Vertices[j]));
                p.UV.Add(f.TexCoordinates[j].X);
                p.UV.Add(f.TexCoordinates[j].Y);
            }

            // TODO: textures and so on
            p.Color = new List<int>();
            p.Color.Add(127);
            p.Color.Add(127);
            p.Color.Add(127);

            res.Polygons.Add(p);
        }

        return res;
    }

}
