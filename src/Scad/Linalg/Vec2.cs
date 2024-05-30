namespace Scad.Linalg {

    /// Vector of dimension 2
    public class Vec2 {
        public float X, Y;

        public Vec2()
        {
            X = 0.0f;
            Y = 0.0f;
        }

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vec2(Vec2 v)
        {
            X = v.X;
            Y = v.Y;
        }

        public Vec2 Clone()
        {
            return new Vec2(this);
        }

        public static Vec2 operator-(Vec2 v)
        {
            return new Vec2(-v.X, -v.Y);
        }

        public static Vec2 operator+(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X + b.X, a.Y + b.Y);
        }

        public static Vec2 operator-(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X - b.X, a.Y - b.Y);
        }

        public static Vec2 operator*(Vec2 a, float b)
        {
            return new Vec2(a.X * b, a.Y * b);
        }

        public static Vec2 operator/(Vec2 a, float b)
        {
            return new Vec2(a.X / b, a.Y / b);
        }

        public float Dot(Vec2 a)
        {
            return X * a.X + Y * a.Y;
        }

        public Vec2 Lerp(Vec2 a, float t)
        {
            return this + (a - this) * t;
        }

        public float Length()
        {
            return (float)Math.Sqrt(Dot(this));
        }

        public Vec2 Unit()
        {
            return this / this.Length();
        }

        public Vec3 Cross(Vec2 a)
        {
            return new Vec3(0.0f, 0.0f, X * a.Y - Y * a.X);
        }

        public override string ToString()
        {
            return $"[{X},{Y}]";
        }
    };

} // namespace Scad
