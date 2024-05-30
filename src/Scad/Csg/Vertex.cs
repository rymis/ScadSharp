namespace Scad.Csg {

    using Vec3 = Scad.Linalg.Vec3;
    using Vec2 = Scad.Linalg.Vec2;

    public class Vertex {
        public Vec3 Position;
        public Vec3 Normal;
        public Vec2 UV;

        public Vertex(Vec3 pos, Vec3 normal)
        {
            Position = pos;
            Normal = normal;
            UV = new Vec2();
        }

        public Vertex(Vec3 pos, Vec3 normal, Vec2 uv)
        {
            Position = pos;
            Normal = normal;
            UV = uv;
        }

        public Vertex Clone()
        {
            return new Vertex(Position.Clone(), Normal.Clone());
        }

        public void Flip(Plane p)
        {
            Normal = p.Mirror(Normal);
        }

        public Vertex Interpolate(Vertex other, float t)
        {
            return new Vertex(Position.Lerp(other.Position, t), Normal.Lerp(other.Normal, t), UV.Lerp(other.UV, t));
        }
    };

} // namespace Scad.Csg
