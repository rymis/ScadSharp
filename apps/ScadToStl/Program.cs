namespace ScadToStl;

using Parser;
using CommandLine;

class Program
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input file to be processed.")]
        public string Input { get; set; } = "";

        [Option('o', "output", Default = "a.stl", HelpText = "Output file to write model into.")]
        public string Output { get; set; } = "";

        [Option('O', "obj", HelpText = "Use obj format")]
        public bool Obj { get; set; }

        [Option('v', "verbose", HelpText = "Be more verbose")]
        public bool Verbose { get; set; }
    }

    static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunProgram)
            .WithNotParsed(OptionsError);
    }

    static void RunProgram(Options opts)
    {
        Console.WriteLine($"Processing file {opts.Input}");

        try {
            var scad = new Scad.Openscad.Openscad();
            scad.LogEvent += (s) => { Console.WriteLine($"#> {s}"); };

            var stl = scad.ProcessFile(opts.Input);

            using (var f = System.IO.File.CreateText(opts.Output)) {
                if (opts.Obj) {
                    Scad.WaveFrontObj.Save(stl, f);
                } else {
                    Scad.Stl.Save(stl, f);
                }
            }
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
