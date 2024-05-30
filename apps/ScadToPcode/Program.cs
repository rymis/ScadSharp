namespace ScadToPcode;

using Parser;
using CommandLine;

class Program
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input files to be processed.")]
        public string Input { get; set; } = "";

        [Option('o', "openscad", HelpText = "File to write output into.")]
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

    static string ReadFile(string filename)
    {
        using (var f = System.IO.File.OpenText(filename))
        {
            return f.ReadToEnd();
        }
    }

    static void RunProgram(Options opts)
    {
        Console.WriteLine($"Processing file {opts.Input}");
        string src = ReadFile(opts.Input);

        var parser = new Scad.Openscad.Grammar.Prog();
        var ctx = new Context(src, Whitespace.Skip.WhiteChars | Whitespace.Skip.CStyleComment | Whitespace.Skip.CppStyleComment);

        try {
            var scad = new Scad.Openscad.Openscad();
            scad.LogEvent += (s) => { Console.WriteLine($"#> {s}"); };

            var prog = scad.LoadProgram(opts.Input);
            var mods = prog.Execute(scad);

            if (opts.Output != "") {
                using var output = System.IO.File.CreateText(opts.Output);
                foreach (var mod in mods) {
                    mod.ToOpenscad(output, 0);
                }
            }

            var doc = new System.Xml.XmlDocument();
            var root = doc.CreateElement("scad");
            doc.AppendChild(root);

            foreach (var mod in mods) {
                root.AppendChild(mod.ToXml(doc));
            }

            var f = new System.IO.StringWriter();
            doc.Save(f);

            Console.WriteLine(f.ToString());
        } catch (ParseException exc) {
            Console.WriteLine($"Error in {opts.Input}");
            Console.WriteLine($"{exc.ParseError()}");
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
