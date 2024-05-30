namespace Scad.Csg {

    public class Node {
        public List<Polygon> Polygons;
        public Plane? Plane;
        private Node? _front;
        private Node? _back;
        const int MaxBuildLevel = 65536;

        public Node()
        {
            Polygons = new List<Polygon>();
        }

        public Node(List<Polygon> polygons)
        {
            Polygons = new List<Polygon>();
            Build(polygons, 0);
        }

        public void Build(List<Polygon> polygons, int level)
        {
            if (level > MaxBuildLevel) {
                Console.WriteLine("WARNING: Build level is too high");
                return;
            }

            if (polygons.Count() == 0) {
                return;
            }

            if (Plane == null) {
                var rnd = new System.Random();
                Plane = polygons[rnd.Next() % polygons.Count].Plane.Clone();
            }

            var front = new List<Polygon>();
            var back = new List<Polygon>();
            foreach (var polygon in polygons) {
                Plane.SplitPolygon(polygon, Polygons, Polygons, front, back);
            }

            if (front.Count != 0) {
                if (_front == null) {
                    _front = new Node();
                }
                _front.Build(front, level + 1);
            }

            if (back.Count != 0) {
                if (_back == null) {
                    _back = new Node();
                }
                _back.Build(back, level + 1);
            }
        }

        public void AllPolygons(List<Polygon> polygons)
        {
            polygons.AddRange(Polygons);

            if (_back != null) {
                _back.AllPolygons(polygons);
            }

            if (_front != null) {
                _front.AllPolygons(polygons);
            }
        }

        public List<Polygon> AllPolygons()
        {
            var res = new List<Polygon>();
            AllPolygons(res);
            return res;
        }

        public Node Clone()
        {
            var res = new Node();

            if (Plane != null) {
                res.Plane = Plane.Clone();
            }

            if (_front != null) {
                res._front = _front.Clone();
            }

            if (_back != null) {
                res._back = _back.Clone();
            }

            foreach (var polygon in Polygons) {
                res.Polygons.Add(polygon.Clone());
            }

            return res;
        }

        /// Convert solid space to empty space and empty space to solid space.
        public void Invert()
        {
            foreach (var p in Polygons) {
                p.Flip();
            }

            if (Plane != null) {
                Plane.Flip();
            }

            if (_front != null) {
                _front.Invert();
            }

            if (_back != null) {
                _back.Invert();
            }

            var tmp = _back;
            _back = _front;
            _front = tmp;
        }

        /// Recursively remove all polygons in `polygons` that are inside this BSP tree
        public List<Polygon> ClipPolygons(List<Polygon> polygons)
        {
            if (Plane == null) {
                var res = new List<Polygon>();
                res.AddRange(polygons);
                return res;
            }

            var front = new List<Polygon>();
            var back = new List<Polygon>();

            foreach (var p in polygons) {
                Plane.SplitPolygon(p, front, back, front, back);
            }

            if (_front != null) {
                front = _front.ClipPolygons(front);
            }

            if (_back != null) {
                back = _back.ClipPolygons(back);
                front.AddRange(back);
            } else {
                back.Clear();
            }

            return front;
        }

        /// Remove all polygons in this BSP tree that are inside the other BSP tree `bsp`.
        public void ClipTo(Node bsp) {
            Polygons = bsp.ClipPolygons(Polygons);

            if (_front != null) {
                _front.ClipTo(bsp);
            }

            if (_back != null) {
                _back.ClipTo(bsp);
            }
        }
    };

} // namespace Scad.Csg
