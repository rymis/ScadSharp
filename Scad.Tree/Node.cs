namespace Scad.Tree;

using System.Xml;

// Universal tree that can be rendered to the Model using cache for faster updates.

public enum NodeType
{
    Cube,
    Sphere,
    Cylinder,
    Import,
    // ...
    // And operations:
    Union,
    Difference,
    Intersection,
    Hull,
    Minkowski,
};

public class Tree {
    static string Quantize(float v)
    {
        long val = (long)((double)v * 65536);
        return val.ToString();
    }

    public class Node
    {
        public virtual XmlElement Serialize(XmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public virtual void Deserialize(XmlElement val)
        {
            throw new NotImplementedException();
        }

        public virtual string Key()
        {
            throw new NotImplementedException();
        }

        public virtual bool NeedToSave()
        {
            return false;
        }
    };

    public class Cube : Node
    {
        float A, B, C;

        public Cube(float a = 1.0f, float b = 1.0f, float c = 1.0f)
        {
            A = a;
            B = b;
            C = c;
        }

        public override XmlElement Serialize(XmlDocument doc)
        {
            var res = doc.CreateElement("cube");
            res.SetAttribute("a", A.ToString());
            res.SetAttribute("b", B.ToString());
            res.SetAttribute("c", C.ToString());

            return res;
        }

        public override string Key()
        {
            return $"C[{Quantize(A)}:{Quantize(B)}:{Quantize(C)}]";
        }
    };

}
