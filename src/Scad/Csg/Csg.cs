namespace Scad.Csg {

    /*
     * Holds a binary space partition tree representing a 3D solid. Two solids can
  be combined using the `Union()`, `Subtract()`, and `Intersect()` methods.
  */
    public class Csg {
        private List<Polygon> _polygons;

        public Csg()
        {
            _polygons = new List<Polygon>();
        }

        public Csg(List<Polygon> polygons)
        {
            _polygons = polygons;
        }

        public Csg Clone()
        {
            var res = new List<Polygon>();
            foreach (var pol in _polygons) {
                res.Add(pol.Clone());
            }

            return new Csg(res);
        }

        public List<Polygon> Polygons {
            get { return _polygons; }
        }

        /**
  Return a new CSG solid representing space in either this solid or in the
  solid `csg`. Neither this solid nor the solid `csg` are modified.
A>
  A.union(B)

      +-------+            +-------+
      |       |            |       |
      |   A   |            |       |
      |    +--+----+   =   |       +----+
      +----+--+    |       +----+       |
           |   B   |            |       |
           |       |            |       |
           +-------+            +-------+
*/
        public Csg Union(Csg csg)
        {
            var a = new Node(Clone()._polygons);
            var b = new Node(csg.Clone()._polygons);
            a.ClipTo(b);
            b.ClipTo(a);
            b.Invert();
            b.ClipTo(a);
            b.Invert();
            a.Build(b.AllPolygons(), 0);

            return new Csg(a.AllPolygons());
        }

        /**
  Return a new CSG solid representing space in this solid but not in the
  solid `csg`. Neither this solid nor the solid `csg` are modified.
  
      A.subtract(B)
  
      +-------+            +-------+
      |       |            |       |
      |   A   |            |       |
      |    +--+----+   =   |    +--+
      +----+--+    |       +----+
           |   B   |
           |       |
           +-------+
*/
        public Csg Subtract(Csg csg)
        {
            var a = new Node(Clone()._polygons);
            var b = new Node(csg.Clone()._polygons);
            a.Invert();
            a.ClipTo(b);
            b.ClipTo(a);
            b.Invert();
            b.ClipTo(a);
            b.Invert();
            a.Build(b.AllPolygons(), 0);
            a.Invert();

            return new Csg(a.AllPolygons());
        }

        /**
  Return a new CSG solid representing space both this solid and in the
  solid `csg`. Neither this solid nor the solid `csg` are modified.
  
      A.intersect(B)
  
      +-------+
      |       |
      |   A   |
      |    +--+----+   =   +--+
      +----+--+    |       +--+
           |   B   |
           |       |
           +-------+
*/
        public Csg Intersect(Csg csg)
        {
            var a = new Node(Clone()._polygons);
            var b = new Node(csg.Clone()._polygons);
            a.Invert();
            b.ClipTo(a);
            b.Invert();
            a.ClipTo(b);
            b.ClipTo(a);
            a.Build(b.AllPolygons(), 0);
            a.Invert();

            return new Csg(a.AllPolygons());
        }
    };

} // namespace Scad.Csg
