namespace ScadView;

using System.Text.Json.Serialization;

public class ScadApi
{
    public string Filename { get; set; } = string.Empty;

    public SaveResponse Save(Content content)
    {
        string filename = Filename != ""? Filename : "/tmp/xxx.scad";
        using (var f = System.IO.File.CreateText(filename)) {
            f.Write(content.Text);
        }
        var resp = new SaveResponse();

        try {
            var scad = new Scad.Openscad.Openscad();
            scad.LogEvent += (s) => {
                Console.WriteLine($"#> {s}");
                resp.Log.Add(s);
            };

            var stl = scad.ProcessFile(filename);
            resp.Model = Scad.ThreeJs.ToJson(stl);

            return resp;
        } catch (Parser.ParseException exc) {
            Console.WriteLine($"{exc.ParseError()}");
            resp.Error = exc.ParseError();
            return resp;
        }
    }

    public Content Load()
    {
        var res = new Content();

        if (Filename == "") {
            return res;
        }

        res.Text = System.IO.File.ReadAllText(Filename);

        return res;
    }

    public class Content
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = String.Empty;
    }

    public class SaveResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("model")]
        public Scad.ThreeJs.JsonModel? Model { get; set; } = null;

        [JsonPropertyName("log")]
        public List<string> Log { get; set; } = new();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("error")]
        public string? Error { get; set; } = null;
    }
}
