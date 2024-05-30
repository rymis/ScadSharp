namespace Scad;

using Vec3 = Scad.Linalg.Vec3;
using Vec2 = Scad.Linalg.Vec2;

public class WaveFrontObj
{
    // Quantized face representation
    class QFace {
        private int[] _data;
        public string Material = "";

        public QFace()
        {
            _data = new int[9];
        }

        public void SetPoint(int idx, int val)
        {
            _data[idx] = val;
        }

        public void SetNormal(int idx, int val)
        {
            _data[idx + 6] = val;
        }

        public void SetUv(int idx, int val)
        {
            _data[idx + 3] = val;
        }

        public override string ToString()
        {
            return $"f {_data[0]+1}/{_data[3]+1}/{_data[6]+1} {_data[1]+1}/{_data[4]+1}/{_data[7]+1} {_data[2]+1}/{_data[5]+1}/{_data[8]+1}";
        }
    };

    public static void SaveGeometries(Model m, System.IO.TextWriter w)
    {
        var idxPoints = new Vec3Index();
        var idxNormals = new Vec3Index();
        var idxUvs = new Vec3Index();
        List<QFace> faces = new();

        // We need to add all vectors to index:
        for (int i = 0; i < m.Count(); ++i) {
            var f = m.GetFace(i);
            var qf = new QFace();

            for (int j = 0; j < 3; ++j) {
                qf.SetPoint(j, idxPoints.Add(f.Vertices[j]));
                qf.SetNormal(j, idxNormals.Add(f.Normals[j]));
                qf.SetUv(j, idxUvs.Add(new Vec3(f.TexCoordinates[j])));
            }
            qf.Material = f.MaterialId;

            faces.Add(qf);
        }

        // TODO: group, save materials, ...
        foreach (var v in idxPoints.Data) {
            w.WriteLine($"v {v.X} {v.Y} {v.Z} 1.0");
        }

        foreach (var v in idxNormals.Data) {
            w.WriteLine($"vn {v.X} {v.Y} {v.Z} 1.0");
        }

        foreach (var v in idxUvs.Data) {
            w.WriteLine($"vt {v.X} {v.Y}");
        }

        foreach (var f in faces) {
            w.WriteLine(f.ToString());
        }
    }

    public static void Save(Model m, System.IO.TextWriter w)
    {
        SaveGeometries(m, w);
    }

}
