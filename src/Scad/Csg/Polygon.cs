namespace Scad.Csg {

    public class Polygon {
        public List<Vertex> Vertices;
        public Plane Plane;
        public string MaterialId;

        public Polygon(List<Vertex> vertices, Plane plane, string materialId = "")
        {
            Vertices = vertices;
            Plane = plane;
            MaterialId = materialId;
        }

        public Polygon(List<Vertex> vertices, string materialId = "")
        {
            Vertices = vertices;
            Plane = Plane.FromPoints(vertices[0].Position, vertices[1].Position, vertices[2].Position);
            MaterialId = materialId;
        }

        public Polygon Clone()
        {
            var vertices = new List<Vertex>();
            foreach (var v in Vertices) {
                vertices.Add(v.Clone());
            }

            return new Polygon(vertices, MaterialId);
        }

        public void Flip()
        {
            foreach (var v in Vertices) {
                v.Flip(Plane);
            }
            Vertices.Reverse();
            Plane.Flip();
        }
    };

} // namespace Scad.Csg
