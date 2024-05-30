namespace Scad.Linalg {

    /// Vector of dimension 3
    public class Vec3 {
        public float X, Y, Z;

        public Vec3()
        {
            X = 0.0f;
            Y = 0.0f;
            Z = 0.0f;
        }

        public Vec3(float x, float y = 0.0f, float z = 0.0f)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vec3(Vec3 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public Vec3(Vec2 v)
        {
            X = v.X;
            Y = v.Y;
            Z = 0.0f;
        }

        public Vec3 Clone()
        {
            return new Vec3(this);
        }

        public static Vec3 operator-(Vec3 v)
        {
            return new Vec3(-v.X, -v.Y, -v.Z);
        }

        public static Vec3 operator+(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vec3 operator-(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vec3 operator*(Vec3 a, float b)
        {
            return new Vec3(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vec3 operator*(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vec3 operator/(Vec3 a, float b)
        {
            return new Vec3(a.X / b, a.Y / b, a.Z / b);
        }

        public float Dot(Vec3 a)
        {
            return X * a.X + Y * a.Y + Z * a.Z;
        }

        public Vec3 Lerp(Vec3 a, float t)
        {
            return this + (a - this) * t;
        }

        public float Length2()
        {
            return Dot(this);
        }

        public float Length()
        {
            return (float)Math.Sqrt(Length2());
        }

        public float Distance2(Vec3 v)
        {
            return (v - this).Length2();
        }

        public float Distance(Vec3 v)
        {
            return (v - this).Length();
        }

        public Vec3 Unit()
        {
            return this / this.Length();
        }

        public Vec3 Cross(Vec3 a)
        {
            return new Vec3(
                    Y * a.Z - Z * a.Y,
                    Z * a.X - X * a.Z,
                    X * a.Y - Y * a.X);
        }

        public Vec2 XY()
        {
            return new Vec2(X, Y);
        }

        public override string ToString()
        {
            return $"[{X},{Y},{Z}]";
        }
    };

} // namespace Scad
