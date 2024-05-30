namespace Scad.Linalg {

    public class Mat4 {
        public float[] Data;

        public Mat4(float a11 = 1.0f, float a12 = 0.0f, float a13 = 0.0f, float a14 = 0.0f,
                float a21 = 0.0f, float a22 = 1.0f, float a23 = 0.0f, float a24 = 0.0f,
                float a31 = 0.0f, float a32 = 0.0f, float a33 = 1.0f, float a34 = 0.0f,
                float a41 = 0.0f, float a42 = 0.0f, float a43 = 0.0f, float a44 = 1.0f)
        {
            Data = new float[16];
            Data[0] = a11;
            Data[1] = a12;
            Data[2] = a13;
            Data[3] = a14;
            Data[4] = a21;
            Data[5] = a22;
            Data[6] = a23;
            Data[7] = a24;
            Data[8] = a31;
            Data[9] = a32;
            Data[10] = a33;
            Data[11] = a34;
            Data[12] = a41;
            Data[13] = a42;
            Data[14] = a43;
            Data[15] = a44;
        }

        public Mat4(Mat4 m)
        {
            Data = new float[16];
            for (int i = 0; i < 16; ++i) {
                Data[i] = m.Data[i];
            }
        }

        public Mat4 Clone()
        {
            return new Mat4(this);
        }

        public Mat3 AsMat3()
        {
            Mat3 res = new();
            res.A11 = this.At(0, 0);
            res.A12 = this.At(0, 1);
            res.A13 = this.At(0, 2);
            res.A21 = this.At(1, 0);
            res.A22 = this.At(1, 1);
            res.A23 = this.At(1, 2);
            res.A31 = this.At(2, 0);
            res.A32 = this.At(2, 1);
            res.A33 = this.At(2, 2);

            return res;
        }

        public static Mat4 operator+(Mat4 a, Mat4 b)
        {
            Mat4 res = new Mat4();
            for (int i = 0; i < 16; ++i) {
                res.Data[i] = a.Data[i] + b.Data[i];
            }
            return res;
        }

        public static Mat4 operator-(Mat4 a, Mat4 b)
        {
            Mat4 res = new Mat4();
            for (int i = 0; i < 16; ++i) {
                res.Data[i] = a.Data[i] - b.Data[i];
            }
            return res;
        }

        public float At(int row, int col)
        {
            return Data[row * 4 + col];
        }

        public void SetAt(int row, int col, float x)
        {
            Data[row * 4 + col] = x;
        }

        public static Mat4 operator*(Mat4 a, Mat4 b)
        {
            Mat4 res = new();
            for (int i = 0; i < 4; ++i) {
                for (int j = 0; j < 4; ++j) {
                    float x = 0.0f;
                    for (int k = 0; k < 4; ++k) {
                        x += a.At(i, k) * b.At(k, j);
                    }
                    res.Data[i * 4 + j] = x;
                }
            }

            return res;
        }

        public static Vec4 operator*(Mat4 a, Vec4 x)
        {
            Vec4 res = new();
            for (int i = 0; i < 4; ++i) {
                res.X += a.At(0, i) * x.At(i);
            }

            return res;
        }

        public static Vec3 operator*(Mat4 a, Vec3 x)
        {
            return (a * new Vec4(x)).AsVec3();
        }

        public override string ToString()
        {
            var b = new System.Text.StringBuilder();
            for (int i = 0; i < 16; ++i) {
                if (i != 0) {
                    b.Append(",");
                }
                b.Append(Data[i].ToString());
            }

            return b.ToString();
        }

        public static Mat4 Parse(string s)
        {
            var ss = s.Split(",");
            if (ss.Count() > 16) {
                throw new FormatException("Invalid Mat4 format");
            }

            Mat4 res = new();
            for (int i = 0; i < 16 && i < ss.Count(); ++i) {
                res.Data[i] = float.Parse(ss[i]);
            }

            return res;
        }
    };

} // namespace Scad
