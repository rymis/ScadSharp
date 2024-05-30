namespace Scad {

    using Vec3 = Scad.Linalg.Vec3;

    public class Vec3Index {
        private Dictionary<(int, int, int), int> _index = new();
        public List<Vec3> Data = new();

        public int Index(Vec3 v)
        {
            return _index.GetValueOrDefault(Quantize(v), -1);
        }

        public int Add(Vec3 v)
        {
            return Add(v, out _);
        }

        public int Add(Vec3 v, out bool added)
        {
            int idx = Index(v);
            if (idx >= 0) {
                added = false;
                return idx;
            }

            idx = Data.Count;
            Data.Add(v);
            _index[Quantize(v)] = idx;
            added = true;

            return idx;
        }

        private (int, int, int) Quantize(Vec3 v)
        {
            return (
                (int)(v.X * 10000.0f),
                (int)(v.Y * 10000.0f),
                (int)(v.Z * 10000.0f)
            );
        }
    };

}
