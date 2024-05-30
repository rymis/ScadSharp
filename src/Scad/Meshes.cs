namespace Scad {

    using Vec3 = Scad.Linalg.Vec3;
    using Vec2 = Scad.Linalg.Vec2;
    using Scad.Linalg;

    public class Meshes {
        public static Model Cube(float sx = 1.0f, float sy = 1.0f, float sz = 1.0f)
        {
            var mesh = new Model();
            var scale = new Vec3(sx, sy, sz);

            var points = new Vec3[]{
                new Vec3(-0.5f, 0.5f, 0.5f),
                new Vec3(0.5f, -0.5f, 0.5f),
                new Vec3(0.5f, 0.5f, 0.5f),
                new Vec3(-0.5f, -0.5f, 0.5f),
                new Vec3(-0.5f, -0.5f, -0.5f),
                new Vec3(0.5f, 0.5f, -0.5f),
                new Vec3(0.5f, -0.5f, -0.5f),
                new Vec3(-0.5f, 0.5f, -0.5f),
            };

            Action<int, int, int> polygon = (i1, i2, i3) => {
                mesh.AddFace(new Face(scale * points[i1 - 1], scale * points[i2 - 1], scale * points[i3 - 1]));
            };

            polygon(1, 2, 3);
            polygon(2, 1, 4);
            polygon(5, 6, 7);
            polygon(6, 5, 8);
            polygon(5, 2, 4);
            polygon(2, 5, 7);
            polygon(2, 6, 3);
            polygon(6, 2, 7);
            polygon(6, 1, 3);
            polygon(1, 6, 8);
            polygon(5, 1, 8);
            polygon(1, 5, 4);

            return mesh;
        }

        public static Model Sphere(float r, int slices = 16, int stacks = 8)
        {
            var res = new Model();

            Action<List<(Vec3, Vec3, Vec2)>, float, float> vertex = (dst, theta_in, phi_in) => {
                float theta = theta_in * MathF.PI * 2.0f;
                float phi = phi_in * MathF.PI;
                Vec3 dir = new Vec3(MathF.Cos(theta) * MathF.Sin(phi), MathF.Cos(phi), MathF.Sin(theta) * MathF.Sin(phi));
                Vec2 uv = new Vec2(theta, phi);
                dst.Add((dir * r, dir, uv));
            };

            for (int i = 0; i < slices; ++i) {
                for (int j = 0; j < stacks; ++j) {
                    var vertices = new List<(Vec3, Vec3, Vec2)>();

                    vertex(vertices, (float)i / slices, (float)j / stacks);
                    if (j > 0) {
                        vertex(vertices, (float)(i + 1) / slices, (float)j / stacks);
                    }
                    if (j < stacks - 1) {
                        vertex(vertices, (float)(i + 1) / slices, (float)(j + 1) / stacks);
                    }
                    vertex(vertices, (float)i / slices, (float)(j + 1) / stacks);

                    for (int k = 2; k < vertices.Count; ++k) {
                        Face f = new Face(
                            vertices[0].Item1, vertices[0].Item2,
                            vertices[k - 1].Item1, vertices[k - 1].Item2,
                            vertices[k].Item1, vertices[k].Item2
                        );
                        f.TexCoordinates[0] = vertices[0].Item3;
                        f.TexCoordinates[1] = vertices[k - 1].Item3;
                        f.TexCoordinates[2] = vertices[k].Item3;

                        res.AddFace(f);
                    }
                }
            }

            return res;
        }

        public static Model Cylinder(float r1, float r2, float h = 1.0f, int slices = 16)
        {
            var mesh = new Model();

            if (h <= 0.0f) {
                h = 1.0f;
            }

            Func<int, float, Vec3> circle = (int i, float z) => {
                float a = (2.0f * MathF.PI * (float)i) / (float)slices;
                float r = r1 + (r2 - r1) * z / h;
                return new Vec3(r1 * MathF.Cos(a), r1 * MathF.Sin(a), 0.0f);
            };

            // Add top and bottom:
            for (int i = 0; i < slices; ++i) {
                var f = new Face(
                    circle((i + 1) % slices, 0.0f), new Vec3(0.0f, 0.0f, -1.0f),
                    circle(i, 0.0f), new Vec3(0.0f, 0.0f, -1.0f),
                    new Vec3(0.0f, 0.0f, 0.0f), new Vec3(0.0f, 0.0f, -1.0f));
                f.TexCoordinates[0] = new Vec2(0.5f, 0.5f) + f.Vertices[0].XY() / 2.0f / r2;
                f.TexCoordinates[1] = new Vec2(0.5f, 0.5f) + f.Vertices[1].XY() / 2.0f / r2;
                f.TexCoordinates[2] = new Vec2(0.5f, 0.5f);

                mesh.AddFace(f);
            }

            var dh = new Vec3(0.0f, 0.0f, h);
            for (int i = 0; i < slices; ++i) {
                var f = new Face(
                    new Vec3(0.0f, 0.0f, h), new Vec3(0.0f, 0.0f, 1.0f),
                    circle(i, h) + dh, new Vec3(0.0f, 0.0f, 1.0f),
                    circle((i + 1) % slices, h) + dh, new Vec3(0.0f, 0.0f, 1.0f)
                );
                f.TexCoordinates[2] = new Vec2(0.5f, 0.5f) + f.Vertices[2].XY() / 2.0f / r1;
                f.TexCoordinates[1] = new Vec2(0.5f, 0.5f) + f.Vertices[1].XY() / 2.0f / r1;
                f.TexCoordinates[0] = new Vec2(0.5f, 0.5f);

                mesh.AddFace(f);
            }

            for (int l = 0; l < slices; ++l) {
                Vec3 h1 = new Vec3(0.0f, 0.0f, ((float)l * h) / (float)slices);
                Vec3 h2 = new Vec3(0.0f, 0.0f, (((float)l + 1.0f) * h) / (float)slices);

                float th1 = (float)l / (float)slices;
                float th2 = (float)(l + 1) / (float)slices;

                for (int i = 0; i < slices; ++i) {
                    int j = (i + 1) % slices;

                    var f1 = new Face(circle(i, th1) + h1, circle(i, th1), circle(j, th2) + h2, circle(j, th2), circle(i, th2) + h2, circle(i, th2));

                    f1.TexCoordinates[0] = new Vec2(th1, (float)i / (float)slices);
                    f1.TexCoordinates[1] = new Vec2(th2, (float)(i + 1) / (float)slices);
                    f1.TexCoordinates[2] = new Vec2(th2, (float)i / (float)slices);

                    var f2 = new Face(circle(i, th1) + h1, circle(i, th1), circle(j, th1) + h1, circle(j, th1), circle(j, th2) + h2, circle(j, th2));

                    f2.TexCoordinates[0] = new Vec2(th1, (float)(i) / (float)slices);
                    f2.TexCoordinates[1] = new Vec2(th1, (float)(i + 1) / (float)slices);
                    f2.TexCoordinates[2] = new Vec2(th2, (float)(i + 1) / (float)slices);

                    mesh.AddFace(f1);
                    mesh.AddFace(f2);
                }
            }

            return mesh;
        }

    };

}
