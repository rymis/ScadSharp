namespace Scad {

    using Vec2 = Scad.Linalg.Vec2;
    using Vec3 = Scad.Linalg.Vec3;

    /// <summary>
    /// Simple implementation of Ear-Cut algorithm.
    /// </summary>
    public class Earcut {
        public class Error: Exception {
            public Error(string msg) : base(msg)
            {}
        };

        public struct Triangle {
            public int A, B, C;
        };

        const float Epsilon = 1e-7f;

        public static bool IsInTriangle(Vec2 x, Vec2 a, Vec2 b, Vec2 c)
        {
            Func<float, uint> side = (f) => {
                if (f < -Epsilon) {
                    return 1;
                } else if (f > Epsilon) {
                    return 2;
                } else {
                    return 3;
                }
            };

            uint aSide = side((b - a).Cross(x - a).Z);
            uint bSide = side((c - b).Cross(x - b).Z);
            uint cSide = side((a - c).Cross(x - c).Z);

            return (aSide & bSide & cSide) != 0;
        }

        public static List<Triangle> TriangulatePolygon(List<Vec2> polygon)
        {
            var res = new List<Triangle>();

            if (polygon.Count < 3) {
                throw new Error("Need at least 3 points to cut ears");
            }

            var idx = new List<(Vec2 V, int I)>();
            for (int i = 0; i < polygon.Count; ++i) {
                idx.Add((polygon[i], i));
            }

            while (idx.Count > 3) {
                var oldSize = idx.Count;

                for (int i = 0; idx.Count > 3 && i < idx.Count; ++i) {
                    var a = idx[i].V;
                    var b = idx[(i + 1) % idx.Count].V;
                    var c = idx[(i + 2) % idx.Count].V;

                    var isEar = true;
                    for (int j = 3; j < idx.Count; ++j) {
                        if (IsInTriangle(idx[(i + j) % idx.Count].V, a, b, c)) {
                            isEar = false;
                            break;
                        }
                    }

                    if (isEar) {
                        Triangle t;

                        t.A = idx[i].I;
                        t.B = idx[(i + 1) % idx.Count].I;
                        t.C = idx[(i + 2) % idx.Count].I;

                        res.Add(t);

                        idx.RemoveAt((i + 1) % idx.Count);

                        break;
                    }
                }

                if (oldSize == idx.Count) {
                    if (res.Count > 0) {
                        // We have enough :)
                        return res;
                    }
                    throw new Error("Theorem about ears does not work for your polygone");
                }
            }

            Triangle lastT;
            lastT.A = idx[0].I;
            lastT.B = idx[1].I;
            lastT.C = idx[2].I;

            res.Add(lastT);

            return res;
        }

        public static List<Triangle> TriangulatePolygon(List<Vec3> polygon)
        {
            if (polygon.Count < 3) {
                throw new Error("Need at least 3 points to cut ears");
            }

            var n = (polygon[2] - polygon[0]).Cross(polygon[1] - polygon[0]).Unit();
            var i = (polygon[1] - polygon[0]).Unit();
            var j = i.Cross(n);

            var points = new List<Vec2>();
            foreach (var p in polygon) {
                points.Add(new Vec2(p.Dot(i), p.Dot(j)));
            }

            return TriangulatePolygon(points);
        }
    };

}
