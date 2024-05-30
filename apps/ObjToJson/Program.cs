namespace ObjToJson;

using CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;

class Program
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Wavefront.obj file to load.")]
        public string Input { get; set; } = "";

        [Option('o', "output", Default = "model.json", HelpText = "Output file to write JSON model into.")]
        public string Output { get; set; } = "";

        [Option('v', "verbose", HelpText = "Be more verbose")]
        public bool Verbose { get; set; }
    }

    static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunProgram)
            .WithNotParsed(OptionsError);
    }

    class Point
    {
        [JsonPropertyName("x")]
        public float X { get; set; } = 0.0f;
        [JsonPropertyName("y")]
        public float Y { get; set; } = 0.0f;
        [JsonPropertyName("z")]
        public float Z { get; set; } = 0.0f;
    }

    class Polygon
    {
        [JsonPropertyName("vertices")]
        public List<int> Vertices { get; set; } = new();

        [JsonPropertyName("texture")]
        public int Texture { get; set; } = 0;

        [JsonPropertyName("uvs")]
        public List<float> UV { get; set; } = new();
    }

    class Style
    {
        /// RGB colour of the object surface
        [JsonPropertyName("color")]
        public List<int> Color { get; set; } = new(){128, 128, 128};

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

    class Model
    {
        [JsonPropertyName("points")]
        public List<Point> Points { get; set; } = new();

        // TODO: edges

        [JsonPropertyName("polygons")]
        public List<Polygon> Polygons { get; set; } = new();

        [JsonPropertyName("style")]
        public Style Style { get; set; } = new();
    }

    static void RunProgram(Options opts)
    {
        Console.WriteLine($"Processing file {opts.Input}");
        var obj = new ObjParser.Obj();
        obj.LoadObj(opts.Input);

        // OK: now we can generate the model:
        if (obj.FaceList.Count == 0) {
            Console.WriteLine("ERROR: no faces in the model");
            return;
        }

        var model = new Model();
        for (int i = 0; i < obj.VertexList.Count; ++i) {
            var p = new Point();
            p.X = (float)obj.VertexList[i].X;
            p.Y = (float)obj.VertexList[i].Y;
            p.Z = (float)obj.VertexList[i].Z;
            model.Points.Add(p);
        }

        foreach (var pol in obj.FaceList) {
            var p = new Polygon();
            p.Vertices = pol.VertexIndexList.ToList();
            if (pol.TextureVertexIndexList.Count() != 0) {
                foreach (int i in pol.TextureVertexIndexList) {
                    if (i < obj.TextureList.Count()) {
                        p.UV.Add((float)obj.TextureList[i].X);
                        p.UV.Add((float)obj.TextureList[i].Y);
                    } else {
                        p.UV.Add(0.0f);
                        p.UV.Add(0.0f);
                    }
                }
            }
            model.Polygons.Add(p);
        }

        using (var output = System.IO.File.CreateText(opts.Output)) {
            output.WriteLine(JsonSerializer.Serialize(model));
        }
    }

    static void OptionsError(IEnumerable<Error> errors)
    {
        Console.WriteLine("Error while parsing command line arguments.");
        foreach (var err in errors) {
            Console.WriteLine($"Error: {err}");
        }
    }
}
