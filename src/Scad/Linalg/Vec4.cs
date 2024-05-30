namespace Scad.Linalg {

    /// Vector of dimension 3 with scale
    public class Vec4 {
        public float X, Y, Z, W;

        public Vec4()
        {
            X = 0.0f;
            Y = 0.0f;
            Z = 0.0f;
            W = 1.0f;
        }

        public Vec4(float x, float y = 0.0f, float z = 0.0f, float w = 1.0f)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vec4(Vec4 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = v.W;
        }

        public Vec4(Vec3 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = 1.0f;
        }

        public Vec4 Clone()
        {
            return new Vec4(this);
        }

        public Vec3 AsVec3()
        {
            return new Vec3(X * W, Y * W, Z * W);
        }

        public float At(int i)
        {
            switch (i) {
                case 0: return X;
                case 1: return Y;
                case 2: return Z;
                case 3: return W;
            }

            // TODO: OutOfRangeException???
            throw new NullReferenceException();
        }

        public static Vec4 operator-(Vec4 v)
        {
            return new Vec4(-v.X, -v.Y, -v.Z, v.W);
        }

        public static Vec4 operator+(Vec4 a, Vec4 b)
        {
            return new Vec4(a.AsVec3() + b.AsVec3());
        }

        public static Vec4 operator-(Vec4 a, Vec4 b)
        {
            return new Vec4(a.AsVec3() - b.AsVec3());
        }

        public static Vec4 operator*(Vec4 a, float b)
        {
            return new Vec4(a.X, a.Y, a.Z, a.W * b);
        }

        public static Vec4 operator/(Vec4 a, float b)
        {
            return new Vec4(a.X, a.Y, a.Z, a.W / b);
        }

        public float Dot(Vec4 a)
        {
            return AsVec3().Dot(a.AsVec3());
        }

        public Vec4 Lerp(Vec4 a, float t)
        {
            return new Vec4(AsVec3().Lerp(a.AsVec3(), t));
        }

        public float Length()
        {
            return AsVec3().Length();
        }

        public Vec4 Unit()
        {
            return new Vec4(AsVec3().Unit());
        }

        public Vec4 Cross(Vec4 a)
        {
            return new Vec4(AsVec3().Cross(a.AsVec3()));
        }
    };

} // namespace Scad
