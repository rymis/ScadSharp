namespace Scad.Openscad;

using Scad.Linalg;

/// <summary>
/// Standard modules implementation for OpenScad
/// </summary>
public class Modules
{
    public static Dictionary<string, Value> ParseArgs(ExecutionContext context, string name, Value.Argument[] arguments, params string[] names)
    {
        var res = new Dictionary<string, Value>();
        int idx = 0;
        foreach (var arg in arguments) {
            if (arg.Name == null || arg.Name == "") {
                if (idx >= names.Count()) {
                    context.Log($"ERROR: Too many arguments for {name}");
                } else {
                    res[names[idx]] = arg.Val;
                    ++idx;
                }
            } else {
                if (arg.Name[0] == '$') {
                    // Just set local variable:
                    context.SetVariable(arg.Name, arg.Val);
                } else {
                    bool found = false;
                    foreach (var nm in names) {
                        if (nm == arg.Name) {
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        context.Log($"ERROR: unknown parameter {arg.Name} for {name}");
                    } else {
                        res[arg.Name] = arg.Val;
                    }
                }
            }
        }

        foreach (var nm in names) {
            if (!res.ContainsKey(nm)) {
                res[nm] = Value.Undef;
            }
        }

        return res;
    }

    public static Vec3? AsVec3(ExecutionContext context, Value x)
    {
        if (x is Value.Number) {
            float v = (float)((Value.Number)x).Val;
            return new Vec3(v, v, v);
        }

        if (x is Value.List) {
            var l = ((Value.List)x).Val;
            if (l.Count < 3) {
                context.Log($"ERROR: Invalid value for vector {x.ToString()}");
                return null;
            }

            for (int i = 0; i < 3; ++i) {
                if (!(l[i] is Value.Number)) {
                    context.Log($"ERROR: Invalid value for vector {x.ToString()}");
                    return null;
                }
            }

            return new Vec3((float)((Value.Number)l[0]).Val, (float)((Value.Number)l[1]).Val, (float)((Value.Number)l[2]).Val);
        }

        return null;
    }

    public static Vec3? AsVec2(ExecutionContext context, Value x)
    {
        if (x is Value.Number) {
            float v = (float)((Value.Number)x).Val;
            return new Vec3(v, v, 0.0f);
        }

        if (x is Value.List) {
            var l = ((Value.List)x).Val;
            if (l.Count < 2) {
                context.Log($"ERROR: Invalid value for vector {x.ToString()}");
                return null;
            }

            for (int i = 0; i < 2; ++i) {
                if (!(l[i] is Value.Number)) {
                    context.Log($"ERROR: Invalid value for vector {x.ToString()}");
                    return null;
                }
            }

            return new Vec3((float)((Value.Number)l[0]).Val, (float)((Value.Number)l[1]).Val, 0.0f);
        }

        return null;
    }

    static uint ParseHexChar(char c)
    {
        switch (c) {
            case '0': return 0;
            case '1': return 1;
            case '2': return 2;
            case '3': return 3;
            case '4': return 4;
            case '5': return 5;
            case '6': return 6;
            case '7': return 7;
            case '8': return 8;
            case '9': return 9;
            case 'a': return 10;
            case 'b': return 11;
            case 'c': return 12;
            case 'd': return 13;
            case 'e': return 14;
            case 'f': return 15;
            case 'A': return 10;
            case 'B': return 11;
            case 'C': return 12;
            case 'D': return 13;
            case 'E': return 14;
            case 'F': return 15;
            default:
                      throw new System.FormatException("Invalid digits in number");
        }
    }

    static (float R, float G, float B, float A) ParseColor(ExecutionContext ctx, string s)
    {
        uint r = 0, g = 0, b = 0, a = 255;

        try {
            if (s.StartsWith("#")) {
                if (s.Length == 4 || s.Length == 5) { // #rgb(a)
                    r = ParseHexChar(s[1]);
                    g = ParseHexChar(s[2]);
                    b = ParseHexChar(s[3]);
                    r += r * 16;
                    g += g * 16;
                    b += b * 16;

                    if (s.Length == 5) {
                        a = ParseHexChar(s[4]);
                        a += a * 16;
                    }
                } else if (s.Length == 7 || s.Length == 9) { // #rrggbb(aa)
                    r = ParseHexChar(s[1]) * 16 + ParseHexChar(s[2]);
                    g = ParseHexChar(s[3]) * 16 + ParseHexChar(s[4]);
                    b = ParseHexChar(s[5]) * 16 + ParseHexChar(s[6]);
                    if (s.Length == 9) {
                        a = ParseHexChar(s[7]) * 16 + ParseHexChar(s[8]);
                    }
                } else {
                    ctx.Log($"WARNING: invalid color {s}: unknown format");
                    return (1.0f, 0.1f, 0.1f, 1.0f);
                }

                return ((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, (float)a / 255.0f);
            }

            var col = (int r, int g, int b) => ((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 1.0f);

            switch (s.ToLowerInvariant()) {
                case "aliceblue":	return col(240,248,255);
                case "antiquewhite":	return col(250,235,215);
                case "aqua":	return col(0,255,255);
                case "aquamarine":	return col(127,255,212);
                case "azure":	return col(240,255,255);
                case "beige":	return col(245,245,220);
                case "bisque":	return col(255,228,196);
                case "black":	return col(0,0,0);
                case "blanchedalmond":	return col(255,235,205);
                case "blue":	return col(0,0,255);
                case "blueviolet":	return col(138,43,226);
                case "brown":	return col(165,42,42);
                case "burlywood":	return col(222,184,135);
                case "cadetblue":	return col(95,158,160);
                case "chartreuse":	return col(127,255,0);
                case "chocolate":	return col(210,105,30);
                case "coral":	return col(255,127,80);
                case "cornflowerblue":	return col(100,149,237);
                case "cornsilk":	return col(255,248,220);
                case "crimson":	return col(220,20,60);
                case "cyan":	return col(0,255,255);
                case "darkblue":	return col(0,0,139);
                case "darkcyan":	return col(0,139,139);
                case "darkgoldenrod":	return col(184,134,11);
                case "darkgray":	return col(169,169,169);
                case "darkgreen":	return col(0,100,0);
                case "darkgrey":	return col(169,169,169);
                case "darkkhaki":	return col(189,183,107);
                case "darkmagenta":	return col(139,0,139);
                case "darkolivegreen":	return col(85,107,47);
                case "darkorange":	return col(255,140,0);
                case "darkorchid":	return col(153,50,204);
                case "darkred":	return col(139,0,0);
                case "darksalmon":	return col(233,150,122);
                case "darkseagreen":	return col(143,188,143);
                case "darkslateblue":	return col(72,61,139);
                case "darkslategray":	return col(47,79,79);
                case "darkslategrey":	return col(47,79,79);
                case "darkturquoise":	return col(0,206,209);
                case "darkviolet":	return col(148,0,211);
                case "deeppink":	return col(255,20,147);
                case "deepskyblue":	return col(0,191,255);
                case "dimgray":	return col(105,105,105);
                case "dimgrey":	return col(105,105,105);
                case "dodgerblue":	return col(30,144,255);
                case "firebrick":	return col(178,34,34);
                case "floralwhite":	return col(255,250,240);
                case "forestgreen":	return col(34,139,34);
                case "fuchsia":	return col(255,0,255);
                case "gainsboro":	return col(220,220,220);
                case "ghostwhite":	return col(248,248,255);
                case "gold":	return col(255,215,0);
                case "goldenrod":	return col(218,165,32);
                case "gray":	return col(128,128,128);
                case "green":	return col(0,128,0);
                case "greenyellow":	return col(173,255,47);
                case "grey":	return col(128,128,128);
                case "honeydew":	return col(240,255,240);
                case "hotpink":	return col(255,105,180);
                case "indianred":	return col(205,92,92);
                case "indigo":	return col(75,0,130);
                case "ivory":	return col(255,255,240);
                case "khaki":	return col(240,230,140);
                case "lavender":	return col(230,230,250);
                case "lavenderblush":	return col(255,240,245);
                case "lawngreen":	return col(124,252,0);
                case "lemonchiffon":	return col(255,250,205);
                case "lightblue":	return col(173,216,230);
                case "lightcoral":	return col(240,128,128);
                case "lightcyan":	return col(224,255,255);
                case "lightgoldenrodyellow":	return col(250,250,210);
                case "lightgray":	return col(211,211,211);
                case "lightgreen":	return col(144,238,144);
                case "lightgrey":	return col(211,211,211);
                case "lightpink":	return col(255,182,193);
                case "lightsalmon":	return col(255,160,122);
                case "lightseagreen":	return col(32,178,170);
                case "lightskyblue":	return col(135,206,250);
                case "lightslategray":	return col(119,136,153);
                case "lightslategrey":	return col(119,136,153);
                case "lightsteelblue":	return col(176,196,222);
                case "lightyellow":	return col(255,255,224);
                case "lime":	return col(0,255,0);
                case "limegreen":	return col(50,205,50);
                case "linen":	return col(250,240,230);
                case "magenta":	return col(255,0,255);
                case "maroon":	return col(128,0,0);
                case "mediumaquamarine":	return col(102,205,170);
                case "mediumblue":	return col(0,0,205);
                case "mediumorchid":	return col(186,85,211);
                case "mediumpurple":	return col(147,112,219);
                case "mediumseagreen":	return col(60,179,113);
                case "mediumslateblue":	return col(123,104,238);
                case "mediumspringgreen":	return col(0,250,154);
                case "mediumturquoise":	return col(72,209,204);
                case "mediumvioletred":	return col(199,21,133);
                case "midnightblue":	return col(25,25,112);
                case "mintcream":	return col(245,255,250);
                case "mistyrose":	return col(255,228,225);
                case "moccasin":	return col(255,228,181);
                case "navajowhite":	return col(255,222,173);
                case "navy":	return col(0,0,128);
                case "oldlace":	return col(253,245,230);
                case "olive":	return col(128,128,0);
                case "olivedrab":	return col(107,142,35);
                case "orange":	return col(255,165,0);
                case "orangered":	return col(255,69,0);
                case "orchid":	return col(218,112,214);
                case "palegoldenrod":	return col(238,232,170);
                case "palegreen":	return col(152,251,152);
                case "paleturquoise":	return col(175,238,238);
                case "palevioletred":	return col(219,112,147);
                case "papayawhip":	return col(255,239,213);
                case "peachpuff":	return col(255,218,185);
                case "peru":	return col(205,133,63);
                case "pink":	return col(255,192,203);
                case "plum":	return col(221,160,221);
                case "powderblue":	return col(176,224,230);
                case "purple":	return col(128,0,128);
                case "red":	return col(255,0,0);
                case "rosybrown":	return col(188,143,143);
                case "royalblue":	return col(65,105,225);
                case "saddlebrown":	return col(139,69,19);
                case "salmon":	return col(250,128,114);
                case "sandybrown":	return col(244,164,96);
                case "seagreen":	return col(46,139,87);
                case "seashell":	return col(255,245,238);
                case "sienna":	return col(160,82,45);
                case "silver":	return col(192,192,192);
                case "skyblue":	return col(135,206,235);
                case "slateblue":	return col(106,90,205);
                case "slategray":	return col(112,128,144);
                case "slategrey":	return col(112,128,144);
                case "snow":	return col(255,250,250);
                case "springgreen":	return col(0,255,127);
                case "steelblue":	return col(70,130,180);
                case "tan":	return col(210,180,140);
                case "teal":	return col(0,128,128);
                case "thistle":	return col(216,191,216);
                case "tomato":	return col(255,99,71);
                case "turquoise":	return col(64,224,208);
                case "violet":	return col(238,130,238);
                case "wheat":	return col(245,222,179);
                case "white":	return col(255,255,255);
                case "whitesmoke":	return col(245,245,245);
                case "yellow":	return col(255,255,0);
                case "yellowgreen":	return col(154,205,50);
            }

            ctx.Log($"WARNING: invalid color {s}: unknown color name");
            return (1.0f, 0.1f, 0.1f, 1.0f);
        } catch (Exception exc) {
            ctx.Log($"WARNING: invalid color {s}: {exc}");
            return (1.0f, 0.1f, 0.1f, 1.0f);
        }
    }

    public static List<int>? AsIntList(Value val)
    {
        if (val is not Value.List) {
            return null;
        }

        var lst = ((Value.List)val).Val;
        var res = new List<int>();

        foreach (var v in lst) {
            if (v is not Value.Number) {
                return null;
            }

            res.Add((int)((Value.Number)v).Val);
        }

        return res;
    }

    public static (float fa, float fs, float fn) Precision(ExecutionContext context)
    {
        float fa = 12;
        float fs = 2;
        float fn = 0;

        var va = context.GetVariable("$fa");
        if (va is Value.Number) {
            fa = (float)((Value.Number)va).Val;
        }

        var vs = context.GetVariable("$fs");
        if (vs is Value.Number) {
            fs = (float)((Value.Number)vs).Val;
        }

        var vn = context.GetVariable("$fn");
        if (vn is Value.Number) {
            fn = (float)((Value.Number)vn).Val;
        }

        return (fa, fs, fn);
    }

    /// <summary>
    /// Get current context children.
    /// </summary>
    public static List<Tree.Node> Children(ExecutionContext context)
    {
        var mod = context.GetModule("children");
        if (mod == null) {
            return new();
        }

        return mod.Execute(context);
    }

    public class Cube : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var args = ParseArgs(context, "cube", arguments, "size", "center");
            var size = args["size"];
            var center = Value.AsBool(args["center"]);
            Vec3? sz = AsVec3(context, size);

            if (sz == null) {
                return new();
            }

            Tree.Node c = new Tree.Cube(sz);
            if (!center) {
                c = Tree.Translate(sz * 0.5f, c);
            }
            var res = new List<Tree.Node>();
            res.Add(c);

            return res;
        }
    }

    public class Sphere : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var args = ParseArgs(context, "sphere", arguments, "r", "d");
            var r = args["r"];
            float radius = 1.0f;
            if (r is Value.Undefined) {
                var d = args["d"];
                if (d is Value.Number) {
                    radius = (float)((Value.Number)d).Val / 2.0f;
                }
            } else if (r is Value.Number) {
                radius = (float)((Value.Number)r).Val;
            }

            if (radius <= 0.0f) {
                radius = 1.0f;
            }

            var p = Precision(context);
            int n = 20;

            if (p.fn >= 3.0f) {
                n = (int)p.fn;
            } else {
                int na = -1;
                if (p.fa > 0.1f) {
                    na = (int)(360.0f / p.fa);
                }

                int ns = -1;
                if (p.fs >= 0.01f) {
                    ns = (int)((MathF.PI * radius * 2.0f) / p.fs);
                }

                int mn = na > ns? na : ns;

                if (mn > 0) {
                    n = mn;
                }
            }

            Tree.Node c = new Tree.Sphere(radius, n);
            var res = new List<Tree.Node>();
            res.Add(c);

            return res;
        }
    }

    public class Cylinder : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var args = ParseArgs(context, "cylinder", arguments, "h", "r", "r1", "r2", "d", "d1", "d2", "center");
            float h = 1.0f;
            float r1 = 1.0f;
            float r2 = 1.0f;

            if (args["h"] is Value.Number) {
                h = (float)((Value.Number)args["h"]).Val;
            }

            if (args["r"] is Value.Number) {
                r1 = (float)((Value.Number)args["r"]).Val;
                r2 = r1;
            } else if (args["d"] is Value.Number) {
                r1 = (float)((Value.Number)args["r"]).Val / 2.0f;
                r2 = r1;
            } else if (args["r1"] is Value.Number) {
                r1 = (float)((Value.Number)args["r1"]).Val;
            } else if (args["r2"] is Value.Number) {
                r2 = (float)((Value.Number)args["r2"]).Val;
            } else if (args["d1"] is Value.Number) {
                r1 = (float)((Value.Number)args["d1"]).Val / 2.0f;
            } else if (args["d2"] is Value.Number) {
                r2 = (float)((Value.Number)args["d2"]).Val / 2.0f;
            }

            var p = Precision(context);
            int n = 20;

            if (p.fn >= 3.0f) {
                n = (int)p.fn;
            } else {
                int na = -1;
                if (p.fa > 0.1f) {
                    na = (int)(360.0f / p.fa);
                }

                int ns = -1;
                if (p.fs >= 0.01f) {
                    ns = (int)((MathF.PI * MathF.Max(r1, r2) * 2.0f) / p.fs);
                }

                int mn = na > ns? na : ns;

                if (mn > 0) {
                    n = mn;
                }
            }

            Tree.Node c = new Tree.Cylinder(h, r1, r2, n);
            var res = new List<Tree.Node>();
            res.Add(c);

            return res;
        }
    }

    // TODO: Polyhedron
    // TODO: Import
    public class LinearExtrude : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var args = ParseArgs(context, "linear_extrude", arguments, "height", "v", "center", "convexity", "twist", "slices", "scale");
            float height = 1.0f;
            if (args["height"] is Value.Number) {
                height = (float)((Value.Number)args["height"]).Val;
            }
            float twist = 0.0f;
            if (args["twist"] is Value.Number) {
                twist = (float)((Value.Number)args["twist"]).Val;
            }
            int slices = 20;
            if (args["slices"] is Value.Number) {
                slices = (int)((Value.Number)args["slices"]).Val;
            }
            float scale = 1.0f;
            if (args["scale"] is Value.Number) {
                scale = (float)((Value.Number)args["scale"]).Val;
            }

            var node = new Tree.LinearExtrude(Children(context));
            node.H = height;
            node.Slices = slices;
            node.Twist = twist * MathF.PI / 180.0f;
            node.Scale = scale;

            var res = new List<Tree.Node>();
            res.Add(node);
            return res;
        }
    }

    // TODO: RotateExtrude
    // TODO: Surface
    //
    public class Circle : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            float r = 1.0f;
            var args = ParseArgs(context, "circle", arguments, "r", "d");
            if (args["r"] is Value.Number) {
                r = (float)((Value.Number)args["r"]).Val;
            } else if (args["d"] is Value.Number) {
                r = (float)((Value.Number)args["d"]).Val / 2.0f;
            } else {
                context.Log("ERROR: Invalid arguments for circle");
            }

            var p = Precision(context);
            int n = 20;

            if (p.fn >= 3.0f) {
                n = (int)p.fn;
            } else {
                int na = -1;
                if (p.fa > 0.1f) {
                    na = (int)(360.0f / p.fa);
                }

                int ns = -1;
                if (p.fs >= 0.01f) {
                    ns = (int)((MathF.PI * r * 2.0f) / p.fs);
                }

                int mn = na > ns? na : ns;

                if (mn > 0) {
                    n = mn;
                }
            }

            Tree.Node c = new Tree.Cylinder(1.0f, r, r, n);
            var res = new List<Tree.Node>();
            res.Add(c);

            return res;
        }
    }

    public class Square : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var args = ParseArgs(context, "square", arguments, "size", "center");
            var size = args["size"];
            var center = Value.AsBool(args["center"]);
            Vec3? sz = AsVec2(context, size);

            if (sz == null) {
                return new();
            }
            sz.Z = 1.0f;

            Tree.Node c = new Tree.Cube(sz);
            if (center) {
                c = Tree.Translate(new Vec3(0.0f, 0.0f, 0.5f), c);
            } else {
                c = Tree.Translate(new Vec3(-sz.X / 2.0f, -sz.X / 2.0f, 0.5f), c);
            }
            var res = new List<Tree.Node>();
            res.Add(c);

            return res;
        }
    }

    // TODO: Polygon
    // TODO: Text
    // TODO: Projection
    //

    public class Translate : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            if (arguments.Count() == 0) {
                context.Log("Invalid argument for translate");
                return new();
            }

            var delta = AsVec3(context, arguments[0].Val);
            if (delta == null) {
                return new();
            }

            return new(){Tree.Translate(delta, Children(context))};
        }
    }

    public class Rotate : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            if (arguments.Count() == 0) {
                context.Log("Invalid argument for rotate");
                return new();
            }

            if (arguments.Count() == 1) {
                var angles = AsVec3(context, arguments[0].Val);
                if (angles == null) {
                    return new();
                }

                return new(){new Tree.Affine(RotateVec(angles * MathF.PI / 180.0f), Children(context))};
            }

            var args = ParseArgs(context, "rotate", arguments, "a", "rotate");
            float a = 0.0f;
            if (args["a"] is Value.Number) {
                a = (float)((Value.Number)args["a"]).Val;
            } else {
                context.Log("Invalid argument for rotate: a is not a number");
                return new();
            }
            var rotate = AsVec3(context, args["rotate"]);
            if (rotate == null) {
                context.Log("Invalid argument for rotate: rotate is not a vector");
                return new();
            }

            // See: https://en.wikipedia.org/wiki/Rotation_matrix for details of the matrix
            var m = new Mat3();
            var axe = rotate.Unit();
            a = a * MathF.PI / 180.0f;
            float s = MathF.Sin(a);
            float c = MathF.Cos(a);

            m.A11 = c + (1.0f - c) * axe.X * axe.X;
            m.A12 = axe.X * axe.Y * (1.0f - c) - axe.Z * s;
            m.A13 = axe.X * axe.Z * (1.0f - c) + axe.Y * s;
            m.A21 = axe.Y * axe.Z * (1.0f - c) + axe.Z * s;
            m.A22 = c + axe.Y * axe.Y * (1.0f - c);
            m.A23 = axe.Y * axe.Z * (1.0f - c) - axe.X * s;
            m.A31 = axe.X * axe.Z * (1.0f - c) - axe.Y * s;
            m.A32 = axe.Z * axe.Y * (1.0f - c) + axe.X * s;
            m.A33 = c + axe.Z * axe.Z * (1.0f - c);

            return new(){new Tree.Affine(m, Children(context))};
        }

        private static Mat3 RotateVec(Vec3 v)
        {
            return RotateAxe(v.Z, 2) * RotateAxe(v.Y, 1) * RotateAxe(v.X, 0);
        }

        private static Mat3 RotateAxe(float a, int axe)
        {
            float s = MathF.Sin(a);
            float c = MathF.Cos(a);
            Mat3 res = new();

            if (axe == 0) { // X
                res.A22 = c;
                res.A23 = -s;
                res.A32 = s;
                res.A33 = c;
            } else if (axe == 1) { // Y
                res.A11 = c;
                res.A13 = -s;
                res.A31 = s;
                res.A33 = c;
            } else { // Z
                res.A11 = c;
                res.A12 = -s;
                res.A21 = s;
                res.A22 = c;
            }

            return res;
        }
    }

    public class Scale : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            if (arguments.Count() == 0) {
                context.Log("Invalid argument for scale");
                return new();
            }

            var s = AsVec3(context, arguments[0].Val);
            if (s == null) {
                context.Log("Invalid argument for scale");
                return new();
            }

            Mat3 m = new();
            m.A11 = s.X;
            m.A22 = s.Y;
            m.A33 = s.Z;

            return new(){new Tree.Affine(m, Children(context))};
        }
    }

    // TODO: Resize
    public class Mirror : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            if (arguments.Count() == 0) {
                context.Log("Invalid argument for mirror");
                return new();
            }

            var s = AsVec3(context, arguments[0].Val);
            if (s == null) {
                context.Log("Invalid argument for mirror");
                return new();
            }

            Mat3 m = new();
            if (MathF.Abs(s.X) > 0.00001) {
                m.A11 = -1.0f;
            }
            if (MathF.Abs(s.Y) > 0.00001) {
                m.A22 = -1.0f;
            }
            if (MathF.Abs(s.Z) > 0.00001) {
                m.A33 = -1.0f;
            }

            return new(){new Tree.Affine(m, Children(context))};
        }
    }

    public class MultMatrix : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            if (arguments.Count() == 0) {
                context.Log("Invalid argument for mult_matrix");
                return new();
            }

            var m = arguments[0].Val;
            var data = new float[12];

            if (m is Value.List) {
                var lst = ((Value.List)m).Val;
                for (int i = 0; i < 3 && i < lst.Count; ++i) {
                    var row = lst[i];
                    if (!(row is Value.List)) {
                        context.Log("Invalid argument for mult_matrix");
                        return new();
                    }

                    var r = ((Value.List)row).Val;
                    for (int j = 0; j < 4 && j < r.Count; ++j) {
                        var x = r[j];

                        if (x is Value.Number) {
                            data[i * 4 + j] = (float)((Value.Number)x).Val;
                        }
                    }
                }

                var a = new Mat3();
                var b = new Vec3();
                a.A11 = data[0]; a.A12 = data[1]; a.A13 = data[2]; b.X = data[3];
                a.A21 = data[4]; a.A22 = data[5]; a.A23 = data[6]; b.Y = data[7];
                a.A31 = data[8]; a.A32 = data[9]; a.A33 = data[10]; b.Z = data[11];

                return new List<Tree.Node>{new Tree.Affine(a, b, Children(context))};
            }

            context.Log("Invalid argument for mult_matrix");
            return new();
        }
    }

    // TODO: Color
    // TODO: Offset

    public class Hull : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var children = Children(context);
            if (children.Count == 0) {
                context.Log("ERROR: no children for hull");
                return new();
            }

            var res = new List<Tree.Node>();
            res.Add(new Tree.Hull(children));
            return res;
        }
    }

    public class Minkowski : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var children = Children(context);
            if (children.Count != 2) {
                context.Log("ERROR: invalid children count for minkowski");
                return new();
            }
            var res = new List<Tree.Node>();
            res.Add(new Tree.Minkowski(children));
            return res;
        }
    }

    public class Union : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var children = Children(context);
            var res = new List<Tree.Node>();
            res.Add(new Tree.Union(children));
            return res;
        }
    }

    public class Difference : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var children = Children(context);
            var res = new List<Tree.Node>();
            res.Add(new Tree.Difference(children));
            return res;
        }
    }

    public class Intersection : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var children = Children(context);
            var res = new List<Tree.Node>();
            res.Add(new Tree.Intersection(children));
            return res;
        }
    }

    public class Echo : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var b = new System.Text.StringBuilder();
            b.Append("ECHO:");
            foreach (var arg in arguments) {
                b.Append(" ");
                b.Append(arg.ToString());
            }

            context.Log(b.ToString());

            return Children(context);
        }
    }

    public class Polygon : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            if (arguments.Count() == 0) {
                context.Log("Invalid arguments for polygon");
                return new();
            }

            var args = ParseArgs(context, "polygon", arguments, "points", "paths");
            if (args["points"] is not Value.List) {
                context.Log("Invalid arguments for polygon: first argument should be a list");
                return new();
            }
            List<Vec2> points = new();
            var lst = ((Value.List)args["points"]).Val;

            foreach (var p in lst) {
                var v = AsVec2(context, p);
                if (v == null) {
                    context.Log("Invalid point in polygon");
                    return new();
                }
                points.Add(v.XY());
            }

            if (args["paths"] is Value.List) {
                var paths = ((Value.List)args["paths"]).Val;
                if (paths.Count == 0) {
                    context.Log("Empty paths in polygon");
                    return new();
                }

                var rawPaths = new List<List<int>>();
                if (paths[0] is Value.List) {
                    var pp = AsIntList(paths[0]);
                    if (pp == null) {
                        context.Log("Invalid paths for polygon");
                        return new();
                    }

                    rawPaths.Add(pp);

                    for (int i = 1; i < paths.Count; ++i) {
                        pp = AsIntList(paths[i]);
                        if (pp != null) {
                            rawPaths.Add(pp);
                        }
                    }
                } else {
                    var sp = AsIntList(args["paths"]);
                    if (sp == null) {
                        context.Log("Invalid paths for polygon");
                        return new();
                    }

                    rawPaths.Add(sp);
                }

                var polygonPoints = (List<int> pts) => {
                    List<Vec2> res = new List<Vec2>();
                    foreach (int p in pts) {
                        if (p < 0 || p >= points.Count) {
                            context.Log("Invalid points index in polygon");
                            return null;
                        }

                        res.Add(points[p]);
                    }

                    if (res.Count < 3) {
                        context.Log("Too short path for polygon");
                        return null;
                    }

                    return res;
                };

                var polygons = new List<List<Vec2>>();
                foreach (var pts in rawPaths) {
                    var pol = polygonPoints(pts);
                    if (pol == null) { 
                        if (polygons.Count == 0) {
                            return new();
                        }
                        continue;
                    }

                    polygons.Add(pol);
                }

                if (polygons.Count == 1) {
                    return new List<Tree.Node>(){new Tree.Polygon(polygons[0])};
                } else {
                    var children = new List<Tree.Node>();
                    foreach (var p in polygons) {
                        children.Add(new Tree.Polygon(p));
                    }

                    return new List<Tree.Node>(){new Tree.Difference(children)};
                }
            }

            return new List<Tree.Node>(){new Tree.Polygon(points)};
        }
    }

    public class Color : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var args = ParseArgs(context, "color", arguments, "c", "alpha", "material", "mleft", "mright", "mtop", "mbottom", "mfront", "mback");

            var col = args["c"];
            var alpha = args["alpha"];
            float r = 0.0f;
            float g = 0.0f;
            float b = 0.0f;
            float a = 1.0f;
            if (alpha is Value.Number n) {
                a = (float)n.Val;
            }

            if (col is Value.List lst) {
                for (int i = 0; i < lst.Val.Count; ++i) {
                    if (lst.Val[i] is Value.Number x) {
                        switch (i) {
                            case 0:
                                r = (float)x.Val;
                                break;
                            case 1:
                                g = (float)x.Val;
                                break;
                            case 2:
                                b = (float)x.Val;
                                break;
                            default:
                                a = (float)x.Val;
                                break;
                        }
                    }
                }
            } else if (col is Value.String s) {
                (r, g, b, a) = ParseColor(context, s.Val);
            } else {
                // Ignore the module
                return Children(context);
            }

            r *= 255.0f;
            g *= 255.0f;
            b *= 255.0f;
            a *= 255.0f;
            string colName = $"#{(int)r:x2}{(int)g:x2}{(int)b:x2}";

            return new List<Tree.Node>(){new Tree.Material(colName, Children(context))};
        }
    }

    public class Polyhedron : IModule
    {
        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var args = ParseArgs(context, "polyhedron", arguments, "points", "faces", "convexity", "triangles");

            var points = args["points"];
            var faces = args["faces"];

            if (faces is not Value.List) {
                faces = args["triangles"];
            }

            if (faces is not Value.List || points is not Value.List) {
                context.Log("Invalid arguments for polyhedron");
                return new();
            }

            var pts = new List<Vec3>();
            var fcs = new List<List<int>>();

            foreach (var v in ((Value.List)points).Val) {
                var vec = AsVec3(context, v);

                if (vec == null) {
                    context.Log($"Invalid arguments for polyhedron (invalid point: {v.ToString()})");
                    return new();
                }

                pts.Add(vec);
            }

            foreach (var l in ((Value.List)faces).Val) {
                if (l is Value.List lst) {
                    var f = new List<int>();
                    foreach (var idx in lst.Val) {
                        if (idx is Value.Number n) {
                            int i = (int)n.Val;
                            if (i >= pts.Count) {
                                context.Log("Invalid arguments for polyhedron (point index is out of range)");
                                return new();
                            }
                            f.Add(i);
                        } else {
                            context.Log("Invalid arguments for polyhedron (invalid index)");
                            return new();
                        }
                    }

                    if (f.Count < 3) {
                        context.Log("Invalid arguments for polyhedron (invalid face (less than 3 points))");
                        return new();
                    }

                    fcs.Add(f);
                } else {
                    context.Log("Invalid arguments for polyhedron");
                    return new();
                }
            }

            var res = new Tree.Polyhedron(pts, fcs);

            return new(){res};
        }
    }
}
