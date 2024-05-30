namespace Scad.Csg {

    using Vec3 = Scad.Linalg.Vec3;

    public class Plane {
        public Vec3 Normal;
        public float W;
        const float Epsilon = 1e-5f;

        public Plane(Vec3 normal, float w)
        {
            Normal = normal;
            W = w;
        }

        static public Plane FromPoints(Vec3 a, Vec3 b, Vec3 c)
        {
            var n = (b - a).Cross(c - a).Unit();
            return new Plane(n, n.Dot(a));
        }

        public Plane Clone()
        {
            return new Plane(Normal.Clone(), W);
        }

        public void Flip()
        {
            Normal = -Normal;
            W = -W;
        }

        public Vec3 Mirror(Vec3 v)
        {
            float n = 2.0f * Normal.Dot(v);
            return v - Normal * n;
        }

        public void SplitPolygon(Polygon polygon,
                List<Polygon> coplanarFront,
                List<Polygon> coplanarBack,
                List<Polygon> front,
                List<Polygon> back)
        {
            const uint Coplanar = 0;
            const uint Back = 1;
            const uint Front = 2;
            const uint Spanning = 3;

            // Classify polygon points and polygon:
            uint polygonType = 0;
            var types = new List<uint>();

            foreach (var vertex in polygon.Vertices) {
                float t = Normal.Dot(vertex.Position) - W;
                uint pType = 0;
                if (t < -Epsilon) {
                    pType = Back;
                } else if (t > Epsilon) {
                    pType = Front;
                } else {
                    pType = Coplanar;
                }

                polygonType |= pType;
                types.Add(pType);
            }

            // Add polygon to correct list:
            if (polygonType == Coplanar) {
                if (Normal.Dot(polygon.Plane.Normal) > 0.0) {
                    coplanarFront.Add(polygon);
                } else {
                    coplanarBack.Add(polygon);
                }
            } else if (polygonType == Front) {
                front.Add(polygon);
            } else if (polygonType == Back) {
                back.Add(polygon);
            } else {
                // Spanning polygon
                var f = new List<Vertex>();
                var b = new List<Vertex>();

                for (int i = 0; i < polygon.Vertices.Count; ++i) {
                    int j = (i + 1) % polygon.Vertices.Count;
                    var ti = types[i];
                    var tj = types[j];
                    var vi = polygon.Vertices[i];
                    var vj = polygon.Vertices[j];

                    if (ti != Back) {
                        f.Add(vi);
                    }
                    if (ti != Front) {
                        b.Add(ti != Back? vi.Clone() : vi);
                    }
                    if ((ti | tj) == Spanning) {
                        var t = (W - Normal.Dot(vi.Position)) / Normal.Dot(vj.Position - vi.Position);
                        var v = vi.Interpolate(vj, t);
                        f.Add(v);
                        b.Add(v.Clone());
                    }
                }

                if (f.Count >= 3) {
                    front.Add(new Polygon(f));
                }

                if (b.Count >= 3) {
                    back.Add(new Polygon(b));
                }
            }
        }
    };

} // namespace Scad.Csg
