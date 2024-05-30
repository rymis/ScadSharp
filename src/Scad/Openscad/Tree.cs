namespace Scad.Openscad;

using System.Xml;
using Scad.Linalg;

public class Tree {
    static string Quantize(float v)
    {
        long val = (long)((double)v * 65536);
        return val.ToString();
    }

    static string Quantize(Vec3 v)
    {
        return $"[{Quantize(v.X)},{Quantize(v.Y)},{Quantize(v.Z)}]";
    }

    static string Quantize(Vec2 v)
    {
        return $"[{Quantize(v.X)},{Quantize(v.Y)}]";
    }

    static string Quantize(Mat4 m)
    {
        var res = new System.Text.StringBuilder();
        res.Append("[[");
        for (int i = 0; i < 16; ++i) {
            if (i != 0) {
                res.Append(",");
            }
            res.Append(Quantize(m.Data[i]));
        }
        res.Append("]]");

        return res.ToString();
    }

    static string Quantize(Mat3 m)
    {
        var res = new System.Text.StringBuilder();
        res.Append("[[");
        res.Append(Quantize(m.A11));
        res.Append(",");
        res.Append(Quantize(m.A12));
        res.Append(",");
        res.Append(Quantize(m.A13));
        res.Append(",");
        res.Append(Quantize(m.A21));
        res.Append(",");
        res.Append(Quantize(m.A22));
        res.Append(",");
        res.Append(Quantize(m.A23));
        res.Append(",");
        res.Append(Quantize(m.A31));
        res.Append(",");
        res.Append(Quantize(m.A32));
        res.Append(",");
        res.Append(Quantize(m.A33));
        res.Append("]]");

        return res.ToString();
    }

    public static Affine Translate(Vec3 d, List<Node> children)
    {
        return new Affine(d, children);
    }

    public static Affine Translate(Vec3 d, params Node[] children)
    {
        return Translate(d, new List<Node>(children));
    }

    public static void Offset(System.IO.TextWriter writer, int offset)
    {
        for (int i = 0; i < offset; ++i) {
            writer.Write("  ");
        }
    }

    public class Node
    {
        public virtual XmlElement ToXml(XmlDocument doc)
        {
            return doc.CreateElement("empty");
        }

        public virtual void FromXml(XmlElement el)
        {
        }

        public virtual string Key()
        {
            return "EMPTY";
        }

        public virtual Scad.Model DoRender(IRenderCache cache)
        {
            throw new NotImplementedException();
        }

        public virtual void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            throw new NotImplementedException($"in {this.GetType().Name}");
        }

        public Scad.Model Render(IRenderCache cache)
        {
            var c = cache.Lookup(Key());
            if (c != null) {
                return c;
            }

            var res = DoRender(cache);
            cache.Set(Key(), res);
            return res;
        }
    };

    public class Cube : Node
    {
        public Vec3 Size;

        public Cube(float x = 0.0f, float y = 0.0f, float z = 0.0f)
        {
            Size = new Vec3(x, y, z);
        }

        public Cube(Vec3 p)
        {
            Size = new Vec3(p);
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("cube");
            res.SetAttribute("key", this.Key());

            res.SetAttribute("x", Size.X.ToString());
            res.SetAttribute("y", Size.Y.ToString());
            res.SetAttribute("z", Size.Z.ToString());

            return res;
        }

        public override string Key()
        {
            return $"CUBE{Quantize(Size)}";
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            return Scad.Meshes.Cube(Size.X, Size.Y, Size.Z);
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.WriteLine($"cube([{Size.X}, {Size.Y}, {Size.Z}], center = true);");
        }
    }

    public class Sphere : Node
    {
        public float Radius = 1.0f;
        public int N = 12;

        public Sphere(float r = 1.0f, int n = 12)
        {
            Radius = r;
            N = n;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("sphere");
            res.SetAttribute("key", this.Key());

            res.SetAttribute("r", Radius.ToString());
            res.SetAttribute("n", N.ToString());

            return res;
        }

        public override string Key()
        {
            return $"SPHERE({Quantize(Radius)}${N})";
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            return Scad.Meshes.Sphere(Radius, N * 2 - 1, N);
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.WriteLine($"sphere([{Radius}, $fn = {N});");
        }
    }

    public class Cylinder : Node
    {
        public float RadiusTop = 1.0f;
        public float RadiusBottom = 1.0f;
        public float Height = 1.0f;
        public int N = 12;

        public Cylinder(float height = 1.0f, float r1 = 1.0f, float r2 = 1.0f, int n = 12)
        {
            Height = height;
            RadiusTop = r1;
            RadiusBottom = r2;
            N = n;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("sphere");
            res.SetAttribute("key", this.Key());

            res.SetAttribute("r1", RadiusTop.ToString());
            res.SetAttribute("r2", RadiusBottom.ToString());
            res.SetAttribute("h", Height.ToString());
            res.SetAttribute("n", N.ToString());

            return res;
        }

        public override string Key()
        {
            return $"CYLINDER({Quantize(Height)}:{Quantize(RadiusTop)}:{Quantize(RadiusBottom)}${N})";
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            return Scad.Meshes.Cylinder(RadiusBottom, RadiusTop, Height, N);
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.WriteLine($"cylinder([r1 = {RadiusTop}, r2 = {RadiusBottom}, h = {Height}, $fn = {N});");
        }
    }

    public class Polygon : Node
    {
        public List<Vec2> Points;

        public Polygon()
        {
            Points = new();
        }

        public Polygon(List<Vec2> points)
        {
            Points = points;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("polygon");
            res.SetAttribute("key", this.Key());

            var b = new System.Text.StringBuilder();
            for (int i = 0; i < Points.Count; ++i) {
                if (i != 0) {
                    b.Append("/");
                }
                b.Append(Points[i].ToString());
            }
            res.SetAttribute("points", b.ToString());

            return res;
        }

        public override string Key()
        {
            var b = new System.Text.StringBuilder();
            b.Append("POLYGON(");
            for (int i = 0; i < Points.Count; ++i) {
                if (i != 0) {
                    b.Append("/");
                }
                b.Append(Quantize(Points[i]));
            }
            b.Append(")");

            return b.ToString();
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            var shape = Shape.Polygon(Points);
            return shape.Extrude(1.0f);
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.Write("polygon([");
            for (int i = 0; i < Points.Count; ++i) {
                if (i != 0) {
                    writer.Write(", ");
                }
                writer.Write($"[{Points[i].X}, {Points[i].Y}]");
            }
            writer.WriteLine("]);");
        }
    }

    public class Union : Node
    {
        public List<Node> Children = new();

        public Union() {}

        public Union(List<Node> children)
        {
            Children = children;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("union");
            foreach (var child in Children) {
                res.AppendChild(child.ToXml(doc));
            }

            return res;
        }

        public override string Key()
        {
            var builder = new System.Text.StringBuilder();
            builder.Append("UNION{");
            for (int i = 0; i < Children.Count; ++i) {
                if (i != 0) {
                    builder.Append("_");
                }
                builder.Append(Children[i].Key());
            }
            builder.Append("}");

            return builder.ToString();
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            List<Scad.Model> children = new();
            foreach (var child in Children) {
                children.Add(child.Render(cache));
            }

            if (children.Count == 0) {
                return new();
            }

            Scad.Model res = children[0];
            for (int i = 1; i < children.Count; ++i) {
                res = res.Join(children[i]);
            }

            return res;
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.WriteLine("union() {");
            foreach (var c in Children) {
                c.ToOpenscad(writer, offset + 1);
            }
            Offset(writer, offset);
            writer.WriteLine("}");
        }

    }

    public class Difference : Node
    {
        public List<Node> Children = new();

        public Difference() {}

        public Difference(List<Node> children)
        {
            Children = children;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("difference");
            foreach (var child in Children) {
                res.AppendChild(child.ToXml(doc));
            }

            return res;
        }

        public override string Key()
        {
            var builder = new System.Text.StringBuilder();
            builder.Append("DIFFERENCE{");
            for (int i = 0; i < Children.Count; ++i) {
                if (i != 0) {
                    builder.Append("_");
                }
                builder.Append(Children[i].Key());
            }
            builder.Append("}");

            return builder.ToString();
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            List<Scad.Model> children = new();
            foreach (var child in Children) {
                children.Add(child.Render(cache));
            }

            if (children.Count == 0) {
                return new();
            }

            Scad.Model res = children[0];
            for (int i = 1; i < children.Count; ++i) {
                res = res.Subtract(children[i]);
            }

            return res;
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.WriteLine("difference() {");
            foreach (var c in Children) {
                c.ToOpenscad(writer, offset + 1);
            }
            Offset(writer, offset);
            writer.WriteLine("}");
        }
    }

    public class Intersection : Node
    {
        public List<Node> Children = new();

        public Intersection() {}

        public Intersection(List<Node> children)
        {
            Children = children;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("intersection");
            foreach (var child in Children) {
                res.AppendChild(child.ToXml(doc));
            }

            return res;
        }

        public override string Key()
        {
            var builder = new System.Text.StringBuilder();
            builder.Append("INTERSECTION{");
            for (int i = 0; i < Children.Count; ++i) {
                if (i != 0) {
                    builder.Append("_");
                }
                builder.Append(Children[i].Key());
            }
            builder.Append("}");

            return builder.ToString();
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            List<Scad.Model> children = new();
            foreach (var child in Children) {
                children.Add(child.Render(cache));
            }

            if (children.Count == 0) {
                return new();
            }

            Scad.Model res = children[0];
            for (int i = 1; i < children.Count; ++i) {
                res = res.Intersect(children[i]);
            }

            return res;
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.WriteLine("intersection() {");
            foreach (var c in Children) {
                c.ToOpenscad(writer, offset + 1);
            }
            Offset(writer, offset);
            writer.WriteLine("}");
        }
    }

    public class Hull : Node
    {
        public List<Node> Children = new();

        public Hull() {}

        public Hull(List<Node> children)
        {
            Children = children;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("hull");
            foreach (var child in Children) {
                res.AppendChild(child.ToXml(doc));
            }

            return res;
        }

        public override string Key()
        {
            var builder = new System.Text.StringBuilder();
            builder.Append("HULL{");
            for (int i = 0; i < Children.Count; ++i) {
                if (i != 0) {
                    builder.Append("_");
                }
                builder.Append(Children[i].Key());
            }
            builder.Append("}");

            return builder.ToString();
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            List<Scad.Model> children = new();
            foreach (var child in Children) {
                children.Add(child.Render(cache));
            }

            if (children.Count == 0) {
                return new();
            }

            Scad.Model res = children[0];
            for (int i = 1; i < children.Count; ++i) {
                res = res.Hull(children[i]);
            }

            return res;
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.WriteLine("hull() {");
            foreach (var c in Children) {
                c.ToOpenscad(writer, offset + 1);
            }
            Offset(writer, offset);
            writer.WriteLine("}");
        }
    }

    public class Minkowski : Node
    {
        public List<Node> Children = new();

        public Minkowski() {}

        public Minkowski(List<Node> children)
        {
            Children = children;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("minkowski");
            foreach (var child in Children) {
                res.AppendChild(child.ToXml(doc));
            }

            return res;
        }

        public override string Key()
        {
            var builder = new System.Text.StringBuilder();
            builder.Append("MINKOWSKI{");
            for (int i = 0; i < Children.Count; ++i) {
                if (i != 0) {
                    builder.Append("_");
                }
                builder.Append(Children[i].Key());
            }
            builder.Append("}");

            return builder.ToString();
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            if (Children.Count != 2) {
                return new();
            }

            List<Scad.Model> children = new();
            foreach (var child in Children) {
                children.Add(child.Render(cache));
            }

            return children[0].Minkowski(children[1]);
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.WriteLine("minkowski() {");
            foreach (var c in Children) {
                c.ToOpenscad(writer, offset + 1);
            }
            Offset(writer, offset);
            writer.WriteLine("}");
        }

    }

    public class Affine : Node
    {
        public Mat3? A = null;
        public Vec3? B = null;
        public List<Node> Children;

        public Affine()
        {
            Children = new();
        }

        public Affine(Mat3 a, List<Node> children)
        {
            A = a;
            Children = children;
        }

        public Affine(Mat3 a, Vec3 b, List<Node> children)
        {
            A = a;
            B = b;
            Children = children;
        }

        public Affine(Vec3 b, List<Node> children)
        {
            B = b;
            Children = children;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("affine");
            res.SetAttribute("key", this.Key());
            if (A != null) {
                res.SetAttribute("a", A.ToString());
            }
            if (B != null) {
                res.SetAttribute("b", B.ToString());
            }

            foreach (var child in Children) {
                res.AppendChild(child.ToXml(doc));
            }

            return res;
        }

        public override string Key()
        {
            var s = new System.Text.StringBuilder();
            s.Append($"AFFINE{(A == null? "": Quantize(A))}/{(B == null? "": Quantize(B))}(");
            for (int i = 0; i < Children.Count; ++i) {
                if (i != 0) {
                    s.Append("_");
                }
                s.Append(Children[i].Key());
            }
            s.Append(")");
            return s.ToString();
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            if (Children.Count == 0) {
                return new();
            }

            Scad.Model res = Children[0].Render(cache);

            for (int i = 1; i < Children.Count; ++i) {
                var child = Children[i].Render(cache);
                res = res.Join(child);
            }

            if (A != null) {
                res.Transform(A);
            }

            if (B != null) {
                res.Move(B);
            }

            return res;
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            var b = B == null? new Vec3() : B;
            if (A == null) {
                writer.Write($"translate([ {b.X}, {b.Y}, {b.Z} ])");
            } else {
                writer.Write($"multmatrix([[ {A.A11}, {A.A12}, {A.A13}, {b.X} ], [ {A.A21}, {A.A22}, {A.A23}, {b.Y} ], [ {A.A31}, {A.A32}, {A.A33}, {b.Z} ]])");
            }
            writer.WriteLine(" {");

            foreach (var c in Children) {
                c.ToOpenscad(writer, offset + 1);
            }
            Offset(writer, offset);
            writer.WriteLine("}");
        }
    }

    public class LinearExtrude : Node
    {
        public List<Node> Children = new();
        public float H = 1.0f;
        public float Twist = 0.0f;
        public int Slices = 20;
        public float Scale = 1.0f;

        public LinearExtrude() {}

        public LinearExtrude(List<Node> children)
        {
            Children = children;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("linear_extrude");
            res.SetAttribute("key", Key());
            res.SetAttribute("h", H.ToString());
            res.SetAttribute("twist", Twist.ToString());
            res.SetAttribute("Slices", Slices.ToString());

            foreach (var child in Children) {
                res.AppendChild(child.ToXml(doc));
            }

            return res;
        }

        public override string Key()
        {
            var builder = new System.Text.StringBuilder();
            builder.Append($"EXTRUDE[{Quantize(H)},{Quantize(Twist)},{Quantize(Scale)},{Slices}");
            builder.Append("{");
            for (int i = 0; i < Children.Count; ++i) {
                if (i != 0) {
                    builder.Append("_");
                }
                builder.Append(Children[i].Key());
            }
            builder.Append("}");

            return builder.ToString();
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            List<Scad.Model> children = new();
            foreach (var child in Children) {
                children.Add(child.Render(cache));
            }

            if (children.Count == 0) {
                return new();
            }

            Scad.Model m = children[0];
            for (int i = 1; i < children.Count; ++i) {
                m = m.Join(children[i]);
            }

            var shape = Scad.Shape.Projection(m);
            return shape.Extrude(H, Twist, Scale, Slices);
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.Write($"linear_extrude({H}, twist = {Twist}, scale = {Scale}, slices = {Slices})");
            writer.WriteLine(" {");

            foreach (var c in Children) {
                c.ToOpenscad(writer, offset + 1);
            }
            Offset(writer, offset);
            writer.WriteLine("}");
        }
    }

    public class Material : Node
    {
        public string MaterialId { get; set; } = "";
        public List<Node> Children;

        public Material()
        {
            Children = new();
        }

        public Material(string mat, List<Node> children)
        {
            MaterialId = mat;
            Children = children;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("material");
            res.SetAttribute("key", this.Key());
            res.SetAttribute("name", this.MaterialId);

            foreach (var child in Children) {
                res.AppendChild(child.ToXml(doc));
            }

            return res;
        }

        public override string Key()
        {
            var s = new System.Text.StringBuilder();
            s.Append($"MATERIAL{MaterialId}(");
            for (int i = 0; i < Children.Count; ++i) {
                if (i != 0) {
                    s.Append("_");
                }
                s.Append(Children[i].Key());
            }
            s.Append(")");
            return s.ToString();
        }

        public override Scad.Model DoRender(IRenderCache cache)
        {
            if (Children.Count == 0) {
                return new();
            }

            Scad.Model res = Children[0].Render(cache);

            for (int i = 1; i < Children.Count; ++i) {
                var child = Children[i].Render(cache);
                res = res.Join(child);
            }

            res.SetMaterial(MaterialId);

            return res;
        }

        public override void ToOpenscad(System.IO.TextWriter writer, int offset)
        {
            Offset(writer, offset);
            writer.Write($"color(\"{MaterialId}\")");
            writer.WriteLine(" {");

            foreach (var c in Children) {
                c.ToOpenscad(writer, offset + 1);
            }

            Offset(writer, offset);
            writer.WriteLine("}");
        }
    }

}
