namespace Scad {

    using Vec2 = Scad.Linalg.Vec2;
    using Vec3 = Scad.Linalg.Vec3;

    // TODO: we do not need to have Face class here.

    public class Hull {
        /*
         * Idea of this implementation came from:
         * https://cw.fel.cvut.cz/wiki/_media/misc/projects/oppa_oi_english/courses/ae4m39vg/lectures/05-convexhull-3d.pdf
         */

        public static (int, int, int) Key(int a, int b, int c)
        {
            // Put the lowest index first
            if (a < b && a < c) {
                return (a, b, c);
            }

            if (b < a && b < c) {
                return (b, c, a);
            }

            return (c, a, b);
        }

        class Impl {
            List<Vec3> Points;
            Dictionary<(int, int, int), Face> Faces;

            enum FaceClass {
                Front, Back, Coplanar
            };

            public class Error : Exception {
                public Error(string msg) : base(msg)
                {}
            };

            class Face {
                Impl Hull; 
                public int A, B, C;

                public Vec3 GetA()
                {
                    return Hull.Points[A];
                }

                public Vec3 GetB()
                {
                    return Hull.Points[B];
                }

                public Vec3 GetC()
                {
                    return Hull.Points[C];
                }

                public Face(Impl h, int a, int b, int c)
                {
                    Hull = h;

                    // Put the lowest index first
                    if (a < b && a < c) {
                        A = a;
                        B = b;
                        C = c;
                    } else if (b < a && b < c) {
                        A = b;
                        B = c;
                        C = a;
                    } else {
                        A = c;
                        B = a;
                        C = b;
                    }
                }

                public Vec3 Normal()
                {
                    return (GetC() - GetA()).Cross(GetB() - GetA()).Unit();
                }

                public float Dist(Vec3 x)
                {
                    return Normal().Dot(x - GetA());
                }

                const float Epsilon = 1e-5f;
                public FaceClass PointClass(Vec3 x)
                {
                    float v = Dist(x);
                    if (v > Epsilon) {
                        return FaceClass.Front;
                    }

                    if (v < -Epsilon) {
                        return FaceClass.Back;
                    }

                    return FaceClass.Coplanar;
                }

                public (int, int, int) Key()
                {
                    return (A, B, C);
                }
            };

            public Impl(List<Vec3> points)
            {
                Points = points;
                Faces = new Dictionary<(int, int, int), Face>();
            }

            bool IsInside(Vec3 p)
            {
                foreach (var f in Faces) {
                    if (f.Value.PointClass(p) == FaceClass.Back) {
                        return false;
                    }
                }

                return true;
            }

            float LineDistance(int a, int b, int c)
            {
                float p = (Points[b].X - Points[a].X) * (Points[a].Y - Points[c].Y) - (Points[a].X - Points[c].X) * (Points[b].Y - Points[a].Y);
                float q = (Points[b] - Points[a]).Length2();

                return p * p / q;
            }

            void AddFace(int a, int b, int c)
            {
                var f = new Face(this, a, b, c);
                Faces[f.Key()] = f;
            }

            void InitialHull()
            {
                int splitA = 0;
                int splitB = 1;

                // Find the most distant points:
                {
                    var maxDist = Points[splitA].Distance2(Points[splitB]);
                    for (int i = splitA; i < Points.Count; ++i) {
                        for (int j = i + 1; j < Points.Count; ++j) {
                            float d = Points[i].Distance2(Points[j]);
                            if (d > maxDist) {
                                maxDist = d;
                                splitA = i;
                                splitB = j;
                            }
                        }
                    }
                }

                int splitC = 0;
                while (splitC == splitA || splitC == splitB) {
                    ++splitC;
                }

                {
                    float maxDist = LineDistance(splitA, splitB, splitC);
                    for (int i = 0; i < Points.Count; ++i) {
                        if (i == splitA || i == splitB) {
                            continue;
                        }

                        float d = LineDistance(splitA, splitB, i);
                        if (d > maxDist) {
                            maxDist = d;
                            splitC = i;
                        }
                    }
                }

                // OK: we have a plane here
                Vec3 a = Points[splitA];
                Vec3 n = (Points[splitC] - a).Cross(Points[splitB] - a).Unit();
                Func<int, float> planeDist = (i) => n.Dot(Points[i] - a);

                // Add front and back points to the plane:
                {
                    int front = 0;
                    int back = 0;
                    float minDist = planeDist(0);
                    float maxDist = minDist;

                    for (int i = 1; i < Points.Count; ++i) {
                        float d = planeDist(i);
                        if (d > maxDist) {
                            maxDist = d;
                            front = i;
                        }

                        if (d < minDist) {
                            minDist = d;
                            back = i;
                        }
                    }

                    if (maxDist > 0.0f) {
                        AddFace(splitA, splitB, front);
                        AddFace(splitB, splitC, front);
                        AddFace(splitC, splitA, front);
                    } else {
                        AddFace(splitA, splitB, splitC);
                    }

                    if (minDist < 0.0f) {
                        AddFace(splitB, splitA, back);
                        AddFace(splitC, splitB, back);
                        AddFace(splitA, splitC, back);
                    } else {
                        AddFace(splitC, splitB, splitA);
                    }
                }

                if (Faces.Count < 3) {
                    throw new Error("Trying to make degraded Hull");
                }
            }

            void AddEyePoint(int p)
            {
                var sides = new Dictionary<(int, int), int>();
                var toRemove = new HashSet<(int, int, int)>();

                foreach (var f in Faces) {
                    var cls = f.Value.PointClass(Points[p]);
                    if (cls == FaceClass.Front || cls == FaceClass.Coplanar) {
                        sides[(f.Value.A, f.Value.B)] = sides.GetValueOrDefault((f.Value.A, f.Value.B), 0) + 1;
                        sides[(f.Value.B, f.Value.C)] = sides.GetValueOrDefault((f.Value.B, f.Value.C), 0) + 1;
                        sides[(f.Value.C, f.Value.A)] = sides.GetValueOrDefault((f.Value.C, f.Value.A), 0) + 1;

                        toRemove.Add(f.Value.Key());
                    }
                }

                // Erase invisible faces:
                foreach (var k in toRemove) {
                    Faces.Remove(k);
                }

                // Add new faces:
                foreach (var s in sides) {
                    if (s.Value == 1) {
                        if (!sides.ContainsKey((s.Key.Item2, s.Key.Item1))) {
                            AddFace(s.Key.Item1, s.Key.Item2, p);
                        }
                    }
                }
            }

            public List<(int, int, int)> Create()
            {
                if (Points.Count < 4) {
                    throw new Error("Trying to make degraded hull (less than 4 points)");
                }

                InitialHull();

                var used = new HashSet<int>();
                foreach (var f in Faces) {
                    used.Add(f.Value.A);
                    used.Add(f.Value.B);
                    used.Add(f.Value.C);
                }

                for (int i = 0; i < Points.Count; ++i) {
                    if (used.Contains(i)) {
                        continue;
                    }

                    if (IsInside(Points[i])) {
                        continue;
                    }

                    AddEyePoint(i);
                }

                var res = new List<(int, int, int)>();
                foreach (var f in Faces) {
                    res.Add((f.Value.A, f.Value.B, f.Value.C));
                }

                return res;
            }
        };

        public static List<(int, int, int)> MakeHullFromPoints(List<Vec3> points)
        {
            var impl = new Impl(points);

            return impl.Create();
        }

    };

}
