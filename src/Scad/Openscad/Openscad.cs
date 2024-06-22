namespace Scad.Openscad;

public class Openscad : ExecutionContext
{
    /// <summary>
    /// Log writing delegate.
    /// </summary>
    public delegate void LogDelegate(string message);

    /// <summary>
    /// Log event. This event is activated when context class needs to send some message for uses.
    /// </summary>
    public event LogDelegate? LogEvent;

    /// <summary>
    /// List of paths to search for files.
    /// </summary>
    public List<String> SearchPath = new();

    /// <summary>
    /// Renderer to use for render.
    /// </summary>
    public Renderer Renderer = new();

    struct FileCacheItem
    {
        public Grammar.Prog Prog;
        public DateTime Mtime;

        public FileCacheItem(Grammar.Prog prog, DateTime mtime)
        {
            Prog = prog;
            Mtime = mtime;
        }
    }

    private Dictionary<string, FileCacheItem> _files = new();

    /// Log message (for errors)
    public override void Log(string msg)
    {
        if (LogEvent != null) {
            LogEvent(msg);
        } else {
            Console.WriteLine($"OpenSCAD: {msg}");
        }
    }

    /// <summary>
    /// Create new context and register standard modules and functions
    /// </summary>
    public Openscad()
    {
        SearchPath.Add(".");

        SetVariable("undef", Value.Undef);
        SetVariable("PI", new Value.Number(Math.PI));
        SetVariable("$fa", new Value.Number(12.0f));
        SetVariable("$fs", new Value.Number(2.0f));
        SetVariable("$fn", new Value.Number(0.0f));

        SetModule("cube", new Modules.Cube());
        SetModule("sphere", new Modules.Sphere());
        SetModule("cylinder", new Modules.Cylinder());
        SetModule("union", new Modules.Union());
        SetModule("difference", new Modules.Difference());
        SetModule("intersection", new Modules.Intersection());
        SetModule("multmatrix", new Modules.MultMatrix());
        SetModule("translate", new Modules.Translate());
        SetModule("rotate", new Modules.Rotate());
        SetModule("scale", new Modules.Scale());
        SetModule("mirror", new Modules.Mirror());
        SetModule("hull", new Modules.Hull());
        SetModule("minkowski", new Modules.Minkowski());
        SetModule("echo", new Modules.Echo());
        SetModule("circle", new Modules.Circle());
        SetModule("square", new Modules.Square());
        SetModule("polygon", new Modules.Polygon());
        SetModule("polyhedron", new Modules.Polyhedron());
        SetModule("linear_extrude", new Modules.LinearExtrude());
        SetModule("color", new Modules.Color());

        SetFunction("abs", new Functions.Abs());
        SetFunction("sign", new Functions.Sign());
        SetFunction("sin", new Functions.Sin());
        SetFunction("cos", new Functions.Cos());
        SetFunction("tan", new Functions.Tan());
        SetFunction("acos", new Functions.Acos());
        SetFunction("asin", new Functions.Asin());
        SetFunction("atan", new Functions.Atan());
        SetFunction("floor", new Functions.Floor());
        SetFunction("round", new Functions.Round());
        SetFunction("ceil", new Functions.Ceil());
        SetFunction("ln", new Functions.Ln());
        SetFunction("log", new Functions.Log());
        SetFunction("sqrt", new Functions.Sqrt());
        SetFunction("exp", new Functions.Exp());
        SetFunction("min", new Functions.Min());
        SetFunction("max", new Functions.Max());
        SetFunction("rands", new Functions.Rands());
        SetFunction("concat", new Functions.Concat());
        SetFunction("len", new Functions.Len());
        SetFunction("version", new Functions.Version());
        SetFunction("version_num", new Functions.VersionNum());
        SetFunction("pow", new Functions.Pow());
        SetFunction("is_undef", new Functions.TypeCheck<Value.Undefined>());
        SetFunction("is_bool", new Functions.TypeCheck<Value.Bool>());
        SetFunction("is_num", new Functions.TypeCheck<Value.Number>());
        SetFunction("is_string", new Functions.TypeCheck<Value.String>());
        SetFunction("is_list", new Functions.TypeCheck<Value.List>());
        SetFunction("is_function", new Functions.TypeCheck<Value.Function>());

    }

    /// <summary>
    /// This is the main function that loads file, parses it, and generates 3D model.
    /// </summary>
    public Scad.Model ProcessFile(string path)
    {
        var prog = LoadProgram(path);

        // Generate Scene Tree
        var mods = prog.Execute(this);

        // Now we can render the scene:
        if (mods.Count == 0) {
            return new();
        }

        var res = Renderer.RenderTree(mods[0]);
        for (int i = 1; i < mods.Count; ++i) {
            res = res.Join(Renderer.RenderTree(mods[i]));
        }

        return res;
    }

    /// <summary>
    /// Load program and all includes.
    /// </summary>
    public Grammar.Prog LoadProgram(string path)
    {
        var prog = LoadFile(path);
        return prog;
    }

    private string? LocateFile(string filename)
    {
        if (Path.IsPathRooted(filename)) {
            if (!Path.Exists(filename)) {
                Log($"ERROR: Can't find file {filename}");
                return null;
            }

            return filename;
        }

        foreach (string dir in SearchPath) {
            var fnm = Path.GetFullPath(Path.Join(dir, filename));
            if (Path.Exists(fnm)) {
                return fnm;
            }
        }

        Log($"ERROR: Can't find file {filename}");

        return null;
    }

    private static string ReadFile(string filename)
    {
        using (var f = System.IO.File.OpenText(filename))
        {
            return f.ReadToEnd();
        }
    }

    private Grammar.Prog LoadFile(string path)
    {
        var savePaths = SearchPath;
        try {
            var abs = Path.GetFullPath(path);
            var info = new FileInfo(abs);

            if (_files.ContainsKey(abs)) {
                if (info.LastWriteTime <= _files[abs].Mtime) {
                    Log($"DEBUG: Loading file from cache {abs}");
                    return _files[abs].Prog;
                }
            }
            Log($"DEBUG: Loading file {abs}");

            SearchPath = new();
            var dname = Path.GetDirectoryName(abs);
            SearchPath.Add(dname == null? ".": dname);
            SearchPath.AddRange(savePaths);

            var src = ReadFile(abs);

            // Actually read and parse file:
            var ctx = new Parser.Context(src, path, Parser.Whitespace.Skip.WhiteChars | Parser.Whitespace.Skip.CStyleComment | Parser.Whitespace.Skip.CppStyleComment);
            var prog = new Grammar.Prog();
            try {
                ctx.Parse(prog);
                _files[abs] = new FileCacheItem(prog, info.LastWriteTime);

                LoadIncludes(prog);

                return prog;
            } catch (Parser.ParseException exc) {
                Log($"ERROR: Error in {path}");
                Log($"{exc.ParseError()}");
            }

            return new();
        } finally {
            SearchPath = savePaths;
        }
    }

    void LoadIncludes(Grammar.Prog prog)
    {
        foreach (var expr in prog.Statements) {
            var use = expr.GetUse();
            if (use != null) {
                var useName = LocateFile(use.Filename);
                if (useName == null) {
                    Log($"ERROR: Can't locate file {use.Filename}");
                    // But we are still optimists:
                    use.Prog = new();
                    continue;
                }
                use.Filename = useName;

                Log($"DEBUG: Use <{useName}>");
                use.Prog = LoadFile(useName);

                continue;
            }

            var inc = expr.GetInclude();
            if (inc != null) {
                var incName = LocateFile(inc.Filename);
                if (incName == null) {
                    Log($"ERROR: Can't locate file {inc.Filename}");
                    // But we are still optimists:
                    inc.Prog = new();
                    continue;
                }
                inc.Filename = incName;

                Log($"DEBUG: Include <{incName}>");

                inc.Prog = LoadFile(incName);

                continue;
            }
        }
    }

};

