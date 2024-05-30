namespace Scad;

using Vec3 = Linalg.Vec3;
using Vec2 = Linalg.Vec2;

/// <summary>
/// 2D shape representation for 2D operations. Actually this library does not support 2D, so we are working only with projections.
/// </summary>
public class Shape {
    private const float Epsilon = 1e-5f;
    public List<Scad.Face> Faces;

    public Shape()
    {
        Faces = new();
    }

    /// <summary>
    /// Generate model projection shape.
    /// </summary>
    public static Shape Projection(Model m)
    {
        if (m.Count() == 0) {
            return new();
        }

        Func<Face, bool> isVert = (Face f) => {
            var n = f.Normal();
            return MathF.Abs(n.Z) < Epsilon;
        };

        Csg.Csg? res = null;
        for (int i = 0; i < m.Count(); ++i) {
            var f = m.GetFace(i);
            if (isVert(f)) {
                continue;
            }

            if (res == null) {
                res = ExtrudeFace(f);
            } else {
                res = res.Union(ExtrudeFace(f));
            }
        }

        if (res == null) {
            return new();
        }

        return TopFace(Model.FromCsg(res));
    }

    /// <summary>
    /// Generate polygon from list of points
    /// </summary>
    public static Shape Polygon(List<Vec3> points)
    {
        if (points.Count < 3) {
            return new();
        }

        List<Vec2> pts = new();
        foreach (var p in points) {
            pts.Add(new Vec2(p.X, p.Y));
        }

        return Polygon(pts);
    }

    /// <summary>
    /// Generate polygon from list of points
    /// </summary>
    public static Shape Polygon(List<Vec2> pts)
    {
        if (pts.Count < 3) {
            return new();
        }

        var triangles = Earcut.TriangulatePolygon(pts);

        Shape res = new();
        foreach (var t in triangles) {
            var a = new Vec3(pts[t.A]);
            var b = new Vec3(pts[t.B]);
            var c = new Vec3(pts[t.C]);

            res.Faces.Add(new Face(a, b, c));
        }

        return res;
    }

    public Model Extrude(float h, float twist = 0.0f, float scale = 1.0f, int slices = 10)
    {
        if (slices < 2) {
            slices = 2;
        }

        if (scale < Epsilon) {
            scale = 1.0f;
        }

        if (h < Epsilon) {
            h = 1.0f;
        }

        if (Faces.Count == 0) {
            return new();
        }

        List<Triangle> triangles = new();
        Vec3Index index = new();

        Func<Vec3, Vec3> z0 = (x) => new Vec3(x.X, x.Y, 0.0f);

        // Add all faces as triangles:
        foreach (var f in Faces) {
            triangles.Add(new Triangle(index.Add(z0(f.Vertices[0])), index.Add(z0(f.Vertices[1])), index.Add(z0(f.Vertices[2]))));
        }

        // Find all outer lines:
        List<(int A, int B)> sides = new();
        {
            Dictionary<(int A, int B), List<int>> tset = new();

            Action<int, int, int> addSide = (a, b, c) => {
                (int, int) key = a < b? (a, b) : (b, a);
                if (!tset.ContainsKey(key)) {
                    tset[key] = new();
                }

                tset[key].Add(c);
            };

            foreach (var t in triangles) {
                addSide(t.A, t.B, t.C);
                addSide(t.B, t.C, t.A);
                addSide(t.C, t.A, t.B);
            }

            foreach (var ts in tset) {
                if (ts.Value.Count == 1) {
                    Vec3 va = index.Data[ts.Key.A];
                    Vec3 vb = index.Data[ts.Key.B];
                    Vec3 vc = index.Data[ts.Value[0]];

                    float n = (vb - va).Cross(vc - va).Z;

                    if (n > 0.0f) {
                        sides.Add((ts.Key.A, ts.Key.B));
                    } else {
                        sides.Add((ts.Key.B, ts.Key.A));
                    }
                }
            }
        }

        Model res = new();
        Func<Vec3, float, float, Vec3> rot = (x, z, a) => {
            float ca = MathF.Cos(a);
            float sa = MathF.Sin(a);
            return new Vec3(ca * x.X - sa * x.Y, sa * x.X + ca * x.Y, z);
        };

        // Add sides:
        float ph = 0.0f;
        float pa = 0.0f;

        for (int i = 1; i <= slices; ++i) {
            float ch = (h * (float)i) / (float)slices;
            float ca = (twist * (float)i) / (float)slices;

            foreach (var s in sides) {
                res.AddFace(new Face(rot(index.Data[s.A], ch, ca), rot(index.Data[s.A], ph, pa), rot(index.Data[s.B], ch, ca)));
                res.AddFace(new Face(rot(index.Data[s.A], ph, pa), rot(index.Data[s.B], ph, pa), rot(index.Data[s.B], ch, ca)));
            }

            ph = ch;
            pa = ca;
        }

        // Add top and bottom:
        var zh = new Vec3(0.0f, 0.0f, h);
        foreach (var t in triangles) {
            Vec3 a = index.Data[t.A];
            Vec3 b = index.Data[t.B];
            Vec3 c = index.Data[t.C];
            var n = (b - a).Cross(c - a);

            if (n.Z > 0.0f) {
                res.AddFace(new Face(a + zh, b + zh, c + zh));
                res.AddFace(new Face(c, b, a));
            } else {
                res.AddFace(new Face(c + zh, b + zh, a + zh));
                res.AddFace(new Face(a, b, c));
            }
        }

        return res;
    }

    struct Triangle {
        public int A;
        public int B;
        public int C;

        public Triangle(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }

    private static Csg.Polygon CreatePol(params Vec3[] points)
    {
        Vec3 normal = (points[1] - points[0]).Cross(points[2] - points[0]).Unit();
        var vs = new List<Csg.Vertex>();
        for (int i = 0; i < points.Count(); ++i) {
            vs.Add(new Csg.Vertex(points[i], normal));
        }

        return new Csg.Polygon(vs);

    }

    private static Csg.Csg ExtrudeFace(Face f, float h = 1.0f)
    {
        var a = new Vec3(f.Vertices[0].X, f.Vertices[0].Y, 0.0f);
        var b = new Vec3(f.Vertices[1].X, f.Vertices[1].Y, 0.0f);
        var c = new Vec3(f.Vertices[2].X, f.Vertices[2].Y, 0.0f);
        var d = new Vec3(0.0f, 0.0f, MathF.Abs(h));

        float n = (b - a).Cross(c - a).Z;

        List<Csg.Polygon> polygons = new();

        if (n > 0.0f) {
            polygons.Add(CreatePol(a + d, b + d, c + d));
            polygons.Add(CreatePol(c, b, a));
            polygons.Add(CreatePol(a, b, b + d, a + d));
            polygons.Add(CreatePol(b, c, c + d, b + d));
            polygons.Add(CreatePol(c, a, a + d, c + d));
        } else {
            polygons.Add(CreatePol(a + d, b + d, b, a));
            polygons.Add(CreatePol(b + d, c + d, c, b));
            polygons.Add(CreatePol(c + d, a + d, a, c));
            polygons.Add(CreatePol(a, b, c));
            polygons.Add(CreatePol(c + d, b + d, a + d));
        }

        return new Csg.Csg(polygons);
    }

    public static Shape TopFace(Model m)
    {
        var s = new Shape();
        Func<Vec3, Vec3> v = (x) => new Vec3(x.X, x.Y, 0.0f);

        for (int i = 0; i < m.Count(); ++i) {
            var f = m.GetFace(i);

            if (f.Normal().Z > 0.5f) {
                s.Faces.Add(new Face(v(f.Vertices[0]), v(f.Vertices[1]), v(f.Vertices[2])));
            }
        }

        return s;
    }

}
