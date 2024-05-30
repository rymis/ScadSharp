namespace Scad.Linalg {

    public class Mat3 {
        public float A11, A12, A13;
        public float A21, A22, A23;
        public float A31, A32, A33;

        public Mat3(float a11 = 1.0f, float a12 = 0.0f, float a13 = 0.0f,
                float a21 = 0.0f, float a22 = 1.0f, float a23 = 0.0f,
                float a31 = 0.0f, float a32 = 0.0f, float a33 = 1.0f)
        {
            A11 = a11;
            A12 = a12;
            A13 = a13;
            A21 = a21;
            A22 = a22;
            A23 = a23;
            A31 = a31;
            A32 = a32;
            A33 = a33;
        }

        public Mat3(Mat3 m)
        {
            A11 = m.A11;
            A12 = m.A12;
            A13 = m.A13;
            A21 = m.A21;
            A22 = m.A22;
            A23 = m.A23;
            A31 = m.A31;
            A32 = m.A32;
            A33 = m.A33;
        }

        public Mat3 Clone()
        {
            return new Mat3(this);
        }

        public static Mat3 operator+(Mat3 a, Mat3 b)
        {
            return new Mat3(a.A11 + b.A11, a.A12 + b.A12, a.A13 + b.A13,
                    a.A21 + b.A21, a.A22 + b.A22, a.A23 + b.A23,
                    a.A31 + b.A31, a.A32 + b.A32, a.A33 + b.A33);
        }

        public static Mat3 operator-(Mat3 a, Mat3 b)
        {
            return new Mat3(a.A11 - b.A11, a.A12 - b.A12, a.A13 - b.A13,
                    a.A21 - b.A21, a.A22 - b.A22, a.A23 - b.A23,
                    a.A31 - b.A31, a.A32 - b.A32, a.A33 - b.A33);
        }

        public static Mat3 operator*(Mat3 a, Mat3 b)
        {
            return new Mat3(
                    a.A11 * b.A11 + a.A12 * b.A21 + a.A13 * b.A31,
                    a.A11 * b.A12 + a.A12 * b.A22 + a.A13 * b.A32,
                    a.A11 * b.A13 + a.A12 * b.A23 + a.A13 * b.A33,

                    a.A21 * b.A11 + a.A22 * b.A21 + a.A23 * b.A31,
                    a.A21 * b.A12 + a.A22 * b.A22 + a.A23 * b.A32,
                    a.A21 * b.A13 + a.A22 * b.A23 + a.A23 * b.A33,

                    a.A31 * b.A11 + a.A32 * b.A21 + a.A33 * b.A31,
                    a.A31 * b.A12 + a.A32 * b.A22 + a.A33 * b.A32,
                    a.A31 * b.A13 + a.A32 * b.A23 + a.A33 * b.A33);
        }

        public static Vec3 operator*(Mat3 a, Vec3 x)
        {
            return new Vec3(
                    a.A11 * x.X + a.A12 * x.Y + a.A13 * x.Z,
                    a.A21 * x.X + a.A22 * x.Y + a.A23 * x.Z,
                    a.A31 * x.X + a.A32 * x.Y + a.A33 * x.Z);
        }

        public override string ToString()
        {
            var b = new System.Text.StringBuilder();
            b.Append(A11.ToString());
            b.Append(",");
            b.Append(A12.ToString());
            b.Append(",");
            b.Append(A13.ToString());
            b.Append(",");
            b.Append(A21.ToString());
            b.Append(",");
            b.Append(A22.ToString());
            b.Append(",");
            b.Append(A23.ToString());
            b.Append(",");
            b.Append(A31.ToString());
            b.Append(",");
            b.Append(A32.ToString());
            b.Append(",");
            b.Append(A33.ToString());

            return b.ToString();
        }

        public static Mat3 Parse(string s)
        {
            var ss = s.Split(",");
            if (ss.Count() != 9) {
                throw new FormatException("Invalid Mat3 format");
            }

            Mat3 res = new();
            res.A11 = float.Parse(ss[0]);
            res.A12 = float.Parse(ss[1]);
            res.A13 = float.Parse(ss[2]);
            res.A21 = float.Parse(ss[3]);
            res.A22 = float.Parse(ss[4]);
            res.A23 = float.Parse(ss[5]);
            res.A31 = float.Parse(ss[6]);
            res.A32 = float.Parse(ss[7]);
            res.A33 = float.Parse(ss[8]);

            return res;
        }

    };

} // namespace Scad
