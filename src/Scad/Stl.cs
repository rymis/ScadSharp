namespace Scad {

    using Vec3 = Linalg.Vec3;

    public class Stl {
        /// <summary>
        /// Save text STL model to writer
        /// </summary>
        public static void Save(Model m, System.IO.TextWriter w)
        {
            w.WriteLine("solid ScadSharp");
            for (int i = 0; i < m.Count(); ++i) {
                var f = m.GetFace(i);
                var n = f.Normal();
                w.WriteLine($"  facet normal {n.X} {n.Y} {n.Z}");
                w.WriteLine("     outer loop");
                w.WriteLine($"       vertex {f.Vertices[0].X} {f.Vertices[0].Y} {f.Vertices[0].Z}");
                w.WriteLine($"       vertex {f.Vertices[1].X} {f.Vertices[1].Y} {f.Vertices[1].Z}");
                w.WriteLine($"       vertex {f.Vertices[2].X} {f.Vertices[2].Y} {f.Vertices[2].Z}");
                w.WriteLine("    endloop");
                w.WriteLine("  endfacet");
            }
            w.WriteLine("endsolid");
        }

        /// <summary>
        /// Save binary STL model to writer
        /// </summary>
        public static void SaveBin(Model m, System.IO.BinaryWriter w)
        {
            // First write header:
            var header = new byte[80];
            string text = "ScadView Model\n";
            for (int i = 0; i < text.Length; ++i) {
                header[i] = (byte)text[i];
            }

            w.Write(header);          // 80 bytes header
            w.Write((uint)m.Count()); // number of triangles
            var writeVec = (Linalg.Vec3 v) => {
                w.Write(v.X);
                w.Write(v.Y);
                w.Write(v.Z);
            };

            // Triangles:
            for (int i = 0; i < m.Count(); ++i) {
                var f = m.GetFace(i);
                var n = f.Normal();
                writeVec(n);
                writeVec(f.Vertices[0]);
                writeVec(f.Vertices[1]);
                writeVec(f.Vertices[2]);
                w.Write((short)0);
                /* TODO:
                 * The VisCAM and SolidView software packages use the two "attribute byte count"
                 * bytes at the end of every triangle to store a 15-bit RGB color:
                 *   bits 0–4 are the intensity level for blue (0–31),
                 *   bits 5–9 are the intensity level for green (0–31),
                 *   bits 10–14 are the intensity level for red (0–31),
                 *   bit 15 is 1 if the color is valid, or 0 if the color is not valid (as with normal STL files).
                 */
            }
        }

        /// <summary>
        /// Save model into the file.
        /// </summary>
        /// <param name="m">Model to save</param>
        /// <param name="filename">Name of the file to save model into</param>
        /// <param name="useBinary">Indicates whether to use binary format or text format. If not specified defaults to text for small models and binary for larger ones.</param>
        public static void Save(Model m, string filename, bool? useBinary = null)
        {
            bool binary = (useBinary == null)? m.Count() < 100: (bool)useBinary;
            if (binary) {
                using var stream = System.IO.File.Create(filename);
                using var writer = new System.IO.BinaryWriter(stream, System.Text.Encoding.UTF8, false);
                SaveBin(m, writer);
            } else {
                using var f = System.IO.File.CreateText(filename);
                Save(m, f);
            }
        }

        class StlFacet : Parser.IPegParsable
        {
            public Vec3 Normal = new();
            public Vec3 A = new();
            public Vec3 B = new();
            public Vec3 C = new();

            public void PegParse(Parser.Context context)
            {
                context.Keyword("facet");
                context.Keyword("normal");
                Normal.X = (float)context.Double();
                Normal.Y = (float)context.Double();
                Normal.Z = (float)context.Double();
                context.Keyword("outer");
                context.Keyword("loop");

                context.Keyword("vertex");
                A.X = (float)context.Double();
                A.Y = (float)context.Double();
                A.Z = (float)context.Double();

                context.Keyword("vertex");
                B.X = (float)context.Double();
                B.Y = (float)context.Double();
                B.Z = (float)context.Double();

                context.Keyword("vertex");
                C.X = (float)context.Double();
                C.Y = (float)context.Double();
                C.Z = (float)context.Double();

                context.Keyword("endloop");
                context.Keyword("endfacet");
            }
        }

        class StlModel : Parser.IPegParsable
        {
            static Parser.Word _name = new Parser.Word(Parser.Word.AlphaNums + "_");
            public List<StlFacet> Facets = new();

            public void PegParse(Parser.Context context)
            {
                context.Keyword("solid");
                context.OptionalFunc((c) => {
                        c.NotAny(c.Func((c2) => { c2.Keyword("facet"); }));
                        c.Word(_name, out _);
                });
                context.ZeroOrMore(Facets);
                context.Literal("endsolid");
            }
        }

        public static Model LoadText(TextReader reader)
        {
            Model res = new();
            string src = reader.ReadToEnd();
            var ctx = new Parser.Context(src);
            var model = new StlModel();

            ctx.Parse(model);

            foreach (var facet in model.Facets) {
                var f = new Face(facet.A, facet.Normal, facet.B, facet.Normal, facet.C, facet.Normal);
                res.AddFace(f);
            }

            return res;
        }

        public static Model LoadBin(BinaryReader reader)
        {
            Model res = new();
            reader.ReadBytes(80); // Ignore header
            uint count = reader.ReadUInt32();
            var readVec = () => {
                Vec3 r = new();
                r.X = reader.ReadSingle();
                r.Y = reader.ReadSingle();
                r.Z = reader.ReadSingle();
                return r;
            };

            for (uint i = 0; i < count; ++i) {
                Vec3 n = readVec();
                Vec3 a = readVec();
                Vec3 b = readVec();
                Vec3 c = readVec();
                reader.ReadUInt16(); // TODO: parse color

                var f = new Face(a, n, b, n, c, n);
                res.AddFace(f);
            }

            return res;
        }

        public static Model Load(string filename)
        {
            using var f = System.IO.File.OpenRead(filename);
            // solid_
            byte[] buf = new byte[6];
            f.ReadExactly(buf, 0, 6);
            f.Seek(0, SeekOrigin.Begin);

            if (buf[0] == 115 && buf[1] == 111 && buf[2] == 108 && buf[3] == 105 && buf[4] == 100 && buf[5] == 32) {
                // "solid "
                f.Close();
                using var stream = System.IO.File.OpenText(filename);
                return LoadText(stream);
            } else {
                using var reader = new System.IO.BinaryReader(f, System.Text.Encoding.UTF8, false);
                return LoadBin(reader);
            }
        }
    };

}
