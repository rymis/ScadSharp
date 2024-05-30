namespace Scad {

    using Vec3 = Scad.Linalg.Vec3;
    using Vec2 = Scad.Linalg.Vec2;

    public class Face {
        public string MaterialId;
        public Vec3[] Vertices;
        public Vec3[] Normals;
        public Vec2[] TexCoordinates;

        public Face(Vec3 a, Vec3 b, Vec3 c)
        {
            MaterialId = "";

            Vertices = new Vec3[3]{a, b, c};
            var n = Normal();
            Normals = new Vec3[3]{n, n, n};

            TexCoordinates = new Vec2[3]{new Vec2(0.0f, 0.0f), new Vec2(0.0f, 1.0f), new Vec2(1.0f, 0.0f)};
        }

        public Face(Vec3 a, Vec3 na, Vec3 b, Vec3 nb, Vec3 c, Vec3 nc)
        {
            MaterialId = "";

            Vertices = new Vec3[3]{a, b, c};
            var n = Normal();
            Normals = new Vec3[3]{na, nb, nc};

            TexCoordinates = new Vec2[3]{new Vec2(0.0f, 0.0f), new Vec2(0.0f, 1.0f), new Vec2(1.0f, 0.0f)};
        }

        public Vec3 Normal()
        {
            return (Vertices[1] - Vertices[0]).Cross(Vertices[2] - Vertices[0]).Unit();
        }

        public Face Clone()
        {
            var res = new Face(
                new Vec3(Vertices[0]), new Vec3(Normals[0]),
                new Vec3(Vertices[1]), new Vec3(Normals[1]),
                new Vec3(Vertices[2]), new Vec3(Normals[2])
            );
            res.TexCoordinates[0] = new Vec2(TexCoordinates[0]);
            res.TexCoordinates[1] = new Vec2(TexCoordinates[1]);
            res.TexCoordinates[2] = new Vec2(TexCoordinates[2]);

            return res;
        }
    };

}
