namespace Scad.Linalg {

    public class Mat2 {
        float A11, A12, A21, A22;

        public Mat2(float a11 = 1.0f, float a12 = 0.0f, float a21 = 0.0f, float a22 = 1.0f)
        {
            A11 = a11;
            A12 = a12;
            A21 = a21;
            A22 = a22;
        }

        public Mat2(Mat2 m)
        {
            A11 = m.A11;
            A12 = m.A12;
            A21 = m.A21;
            A22 = m.A22;
        }

        public Mat2 Clone()
        {
            return new Mat2(this);
        }

        public static Mat2 operator+(Mat2 a, Mat2 b)
        {
            return new Mat2(a.A11 + b.A11, a.A12 + b.A12, a.A21 + b.A21, a.A22 + b.A22);
        }

        public static Mat2 operator-(Mat2 a, Mat2 b)
        {
            return new Mat2(a.A11 - b.A11, a.A12 - b.A12, a.A21 - b.A21, a.A22 - b.A22);
        }

        public static Mat2 operator*(Mat2 a, Mat2 b)
        {
            return new Mat2(
                    a.A11 * b.A11 + a.A12 * b.A21,
                    a.A11 * b.A12 + a.A12 * b.A22,
                    a.A21 * b.A11 + a.A22 * b.A21,
                    a.A21 * b.A12 + a.A22 * b.A22);
        }

        public static Vec2 operator*(Mat2 a, Vec2 x)
        {
            return new Vec2(a.A11 * x.X + a.A12 * x.Y, a.A21 * x.X + a.A22 * x.Y);
        }
    };

} // namespace Scad
