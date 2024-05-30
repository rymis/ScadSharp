namespace ScadView;
using SharpWebview;
using SharpWebview.Content;
using CommandLine;

class Program
{
    public class Options
    {
        [Value(0, Required = false, HelpText = "Input file to process", MetaName = "SOURCE")]
        public string Source { get; set; } = "";

        [Option('p', "port", Required = false, HelpText = "Port to listen on. If not specified use random port.")]
        public int Port { get; set; } = 0;

        [Option('v', "verbose", HelpText = "Be more verbose")]
        public bool Verbose { get; set; }
    }

    [STAThread]
    static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunProgram)
            .WithNotParsed(OptionsError);
    }

    static void RunProgram(Options opts)
    {
        using var webview = new Webview(true);

        webview
            .SetTitle("Scad#")
            .SetSize(1024, 768, WebviewHint.None)
            .SetSize(800, 600, WebviewHint.Min)
            .Navigate(new ScadContent(opts.Port))
            .Run();
    }

    static void OptionsError(IEnumerable<Error> errors)
    {
        Console.WriteLine("Error while parsing command line arguments.");
        foreach (var err in errors) {
            Console.WriteLine($"Error: {err}");
        }
    }
}
