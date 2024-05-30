using System.Collections;

namespace Scad {

    using Vec3 = Scad.Linalg.Vec3;
    using Mat3 = Scad.Linalg.Mat3;
    using Mat4 = Scad.Linalg.Mat4;

    public class Model {
        private List<Face> _faces;

        public Model()
        {
            _faces = new List<Face>();
        }

        public void AddFace(Face face)
        {
            _faces.Add(face);
        }

        public int Count()
        {
            return _faces.Count;
        }

        public void SetMaterial(string mat)
        {
            foreach (var f in _faces) {
                f.MaterialId = mat;
            }
        }

        public Model Clone()
        {
            var m = new Model();
            foreach (var f in _faces) {
                m.AddFace(f.Clone());
            }

            return m;
        }

        public Face GetFace(int i)
        {
            return _faces[i];
        }

        public IEnumerable<Face> GetFacesByMaterial(string materialId)
        {
            foreach (var f in _faces) {
                if (f.MaterialId.Equals(materialId)) {
                    yield return f;
                }
            }
        }

        public void Move(Vec3 d)
        {
            foreach (var f in _faces) {
                for(int i = 0; i < f.Vertices.Count(); ++i) {
                    f.Vertices[i] += d;
                }
            }
        }

        public void Transform(Mat3 m)
        {
            foreach (var f in _faces) {
                for(int i = 0; i < f.Vertices.Count(); ++i) {
                    f.Vertices[i] = m * f.Vertices[i];
                    f.Normals[i] = (m * f.Normals[i]).Unit();
                }
            }
        }

        public void Transform(Mat4 m)
        {
            var m3 = m.AsMat3();

            foreach (var f in _faces) {
                for(int i = 0; i < f.Vertices.Count(); ++i) {
                    f.Vertices[i] = m * f.Vertices[i];
                    f.Normals[i] = (m3 * f.Normals[i]).Unit();
                }
            }
        }

        public void Mirror(Vec3 plane)
        {
            Vec3 n = plane.Unit();

            Func<Vec3, Vec3> m = (v) => v - n * 2.0f * v.Dot(n);

            foreach (var f in _faces) {
                for(int i = 0; i < f.Vertices.Count(); ++i) {
                    f.Vertices[i] = m(f.Vertices[i]);
                    f.Normals[i] = m(f.Normals[i]);
                }
            }
        }

        public void Scale(Vec3 s)
        {
            Mat3 m = new Mat3(s.X, 0.0f, 0.0f, 0.0f, s.Y, 0.0f, 0.0f, 0.0f, s.Z);
            Transform(m);
        }

        public void SmoothNormals()
        {}

        public Model Join(Model m)
        {
            var csg1 = ToCsg();
            var csg2 = m.ToCsg();

            var csg = csg1.Union(csg2);

            return Model.FromCsg(csg);
        }

        public Model Subtract(Model m)
        {
            var csg1 = ToCsg();
            var csg2 = m.ToCsg();

            var csg = csg1.Subtract(csg2);

            return Model.FromCsg(csg);
        }

        public Model Intersect(Model m)
        {
            var csg1 = ToCsg();
            var csg2 = m.ToCsg();

            var csg = csg1.Intersect(csg2);

            return Model.FromCsg(csg);
        }

        public Model Hull(Model m)
        {
            var idx = new Vec3Index();
            var materials = new Dictionary<(int, int, int), string>();
            var ridx = new Dictionary<int, List<(int, int, int)>>();

            // Prepare all points:
            Action<Face> addFace = (f) => {
                int a = idx.Add(f.Vertices[0]);
                int b = idx.Add(f.Vertices[1]);
                int c = idx.Add(f.Vertices[2]);

                // Now we can add the material to database
                var k = Scad.Hull.Key(a, b, c);
                materials[k] = f.MaterialId;
//                ridx[a].Add(k);
//                ridx[b].Add(k);
//                ridx[c].Add(k);
            };

            foreach (var f in _faces) {
                addFace(f);
            }

            foreach (var f in m._faces) {
                addFace(f);
            }

            if (idx.Data.Count() < 4) {
                return new Model();
            }

            var hull = Scad.Hull.MakeHullFromPoints(idx.Data);
            var res = new Model();

            // TODO: old materials
            foreach (var f in hull) {
                res.AddFace(new Face(idx.Data[f.Item1], idx.Data[f.Item2], idx.Data[f.Item3]));
            }

            return res;
        }

        public Model Minkowski(Model m)
        {
            // This algorithm is very slow, but who cares?
            Func<Vec3, Model> translated = (v) => { var res = m.Clone(); res.Move(v); return res; };
            Func<Face, Model> faceToModel = (f) => {
                var m = translated(f.Vertices[0]).Hull(translated(f.Vertices[1]));
                return m.Hull(translated(f.Vertices[2]));
            };

            var res = Clone();
            foreach (var f in _faces) {
                res = res.Hull(faceToModel(f));
            }

            return res;
        }

        public Csg.Csg ToCsg()
        {
            var polygons = new List<Csg.Polygon>();

            foreach (var face in _faces) {
                var vcs = new List<Csg.Vertex>();

                for (int i = 0; i < face.Vertices.Count(); ++i) {
                    vcs.Add(new Csg.Vertex(face.Vertices[i].Clone(), face.Normals[i].Clone(), face.TexCoordinates[i].Clone()));
                }

                polygons.Add(new Csg.Polygon(vcs, face.MaterialId));
            }

            return new Csg.Csg(polygons);
        }

        public static Model FromCsg(Csg.Csg csg)
        {
            // Now create the model and add faces:
            var res = new Model();
            foreach (var polygon in csg.Polygons) {
                res.AddCsgPolygon(polygon, polygon.MaterialId);
            }

            return res;
        }

        void AddCsgPolygon(Csg.Polygon polygon, string materialId)
        {
            var points = new List<Vec3>();
            foreach (var p in polygon.Vertices) {
                points.Add(new Vec3(p.Position.X, p.Position.Y, p.Position.Z));
            }

            try {
                var triangles = Earcut.TriangulatePolygon(points);

                foreach (var t in triangles) {
                    var f = new Face(points[t.A], points[t.B], points[t.C]);
                    f.Normals[0] = polygon.Vertices[t.A].Normal.Clone();
                    f.Normals[1] = polygon.Vertices[t.B].Normal.Clone();
                    f.Normals[2] = polygon.Vertices[t.C].Normal.Clone();

                    f.TexCoordinates[0] = polygon.Vertices[t.A].UV.Clone();
                    f.TexCoordinates[1] = polygon.Vertices[t.B].UV.Clone();
                    f.TexCoordinates[2] = polygon.Vertices[t.C].UV.Clone();

                    f.MaterialId = materialId;

                    AddFace(f);
                }
            } catch {
                // Invalid figure but we can ignore it
                return;
            }
        }
    };

}
