namespace Scad.Openscad;

using System.Xml;

public class Value
{
    public static Value Undef = new Undefined();

    public class Argument
    {
        public string? Name;
        public Value Val;

        public Argument(Value val)
        {
            Val = val;
            Name = null;
        }

        public Argument(string name, Value val)
        {
            Name = name;
            Val = val;
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("argument");
            if (Name != null) {
                res.SetAttribute("name", Name);
            }

            res.AppendChild(Val.ToXml(doc));

            return res;
        }

        public override string ToString()
        {
            if (Name != null && Name.Length > 0) {
                return $"{Name}={Val.ToString()}";
            }

            return Val.ToString();
        }
    }

    public class Parameter
    {
        public string Name;
        public Value? Val;

        public Parameter(string name)
        {
            Name = name;
            Val = null;
        }

        public Parameter(string name, Value val)
        {
            Name = name;
            Val = val;
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("parameter");
            res.SetAttribute("name", Name);

            if (Val != null) {
                res.AppendChild(Val.ToXml(doc));
            }

            return res;
        }
    }

    public class Undefined : Value
    {
        public override XmlNode ToXml(XmlDocument doc)
        {
            return doc.CreateElement("undefined");
        }

        public override string ToString()
        {
            return "undef";
        }
    }

    public class Bool : Value
    {
        public bool Val;

        public Bool(bool v)
        {
            Val = v;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("bool");
            res.SetAttribute("value", Val? "true": "false");
            return res;
        }

        public override string ToString()
        {
            return Val? "true": "false";
        }
    }

    public class Number : Value
    {
        public double Val;

        public Number(double v)
        {
            Val = v;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("number");
            res.SetAttribute("value", Val.ToString());
            return res;
        }

        public override string ToString()
        {
            return Val.ToString();
        }
    }

    public class String : Value
    {
        public string Val;

        public String(string v)
        {
            Val = v;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("string");
            res.SetAttribute("value", Val);
            return res;
        }

        public override string ToString()
        {
            return $"'{Val}'";
        }
    }

    public class List : Value
    {
        public List<Value> Val;
        public List(params Value[] values)
        {
            Val = new List<Value>(values);
        }

        public List(List<Value> data)
        {
            Val = data;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("list");
            foreach (var el in Val) {
                res.AppendChild(el.ToXml(doc));
            }

            return res;
        }

        public override string ToString()
        {
            var b = new System.Text.StringBuilder();

            b.Append('[');
            for (int i = 0; i < Val.Count; ++i) {
                if (i != 0) {
                    b.Append(',');
                }
                b.Append(Val[i].ToString());
            }
            b.Append(']');

            return b.ToString();
        }
    }

    public class Function : Value
    {
        public virtual Value Call(ExecutionContext context, params Argument[] arguments)
        {
            return new Undefined();
        }

        public override string ToString()
        {
            return "function";
        }
    }

    public virtual XmlNode ToXml(XmlDocument doc)
    {
        throw new NotImplementedException();
    }

    public static bool OperatorLess(Value a, Value b)
    {
        if (a is Number && b is Number) {
            return (((Number)a).Val < ((Number)b).Val);
        }

        if (a is Bool && b is Bool) {
            return (!((Bool)a).Val && ((Bool)b).Val);
        }

        if (a is String && b is String) {
            return (((String)a).Val.CompareTo(((String)b).Val) < 0);
        }

        return false;
    }

    public static bool OperatorGreater(Value a, Value b)
    {
        if (a is Number && b is Number) {
            return (((Number)a).Val < ((Number)b).Val);
        }

        if (a is Bool && b is Bool) {
            return (!((Bool)a).Val && ((Bool)b).Val);
        }

        if (a is String && b is String) {
            return (((String)a).Val.CompareTo(((String)b).Val) < 0);
        }

        return false;
    }

    public static bool OperatorEqual(Value a, Value b)
    {
        if (a is Number && b is Number) {
            return (((Number)a).Val == ((Number)b).Val);
        }

        if (a is Bool && b is Bool) {
            return (((Bool)a).Val == ((Bool)b).Val);
        }

        if (a is String && b is String) {
            return (((String)a).Val.CompareTo(((String)b).Val) == 0);
        }

        if (a is Undefined && b is Undefined) {
            return true;
        }

        return false;
    }

    public static bool OperatorNotEqual(Value a, Value b)
    {
        return !OperatorEqual(a, b);
    }

    public static bool OperatorLessOrEqual(Value a, Value b)
    {
        return OperatorLess(a, b) || OperatorEqual(a, b);
    }

    public static bool OperatorGreaterOrEqual(Value a, Value b)
    {
        return OperatorGreater(a, b) || OperatorEqual(a, b);
    }

    public static bool LogicalOr(Value a, Value b)
    {
        if (a is Bool && b is Bool) {
            return (((Bool)a).Val || ((Bool)b).Val);
        }

        return false;
    }

    public static bool LogicalAnd(Value a, Value b)
    {
        if (a is Bool && b is Bool) {
            return (((Bool)a).Val && ((Bool)b).Val);
        }

        return false;
    }

    public static Value operator+(Value a, Value b)
    {
        if (a is Number && b is Number) {
            return new Number(((Number)a).Val + ((Number)b).Val);
        }

        if (a is List && b is List) {
            var l1 = ((List)a).Val;
            var l2 = ((List)b).Val;

            if (l1.Count != l2.Count) {
                return Undef;
            }

            var res = new List<Value>();
            for (int i = 0; i < l1.Count; ++i) {
                res.Add(l1[i] + l2[i]);
            }

            return new List(res);
        }

        return Undef;
    }

    public static Value operator-(Value a)
    {
        if (a is Number) {
            return new Number(-((Number)a).Val);
        }

        if (a is List) {
            var lst = ((List)a).Val;
            var res = new List();

            foreach (var x in lst) {
                res.Val.Add(-x);
            }

            return res;
        }

        return Undef;
    }

    public static Value operator-(Value a, Value b)
    {
        if (a is Number && b is Number) {
            return new Number(((Number)a).Val - ((Number)b).Val);
        }

        if (a is List && b is List) {
            var l1 = ((List)a).Val;
            var l2 = ((List)b).Val;

            if (l1.Count != l2.Count) {
                return Undef;
            }

            var res = new List<Value>();
            for (int i = 0; i < l1.Count; ++i) {
                res.Add(l1[i] - l2[i]);
            }

            return new List(res);
        }

        return Undef;
    }

    public static Value operator*(Value a, Value b)
    {
        if (a is Number && b is Number) {
            return new Number(((Number)a).Val * ((Number)b).Val);
        }

        if (a is List && b is Number) {
            var l = ((List)a).Val;

            var res = new List<Value>();
            foreach (var x in l) {
                res.Add(x * b);
            }

            return new List(res);
        }

        return Undef;
    }

    public static Value operator/(Value a, Value b)
    {
        if (a is Number && b is Number) {
            if (((Number)b).Val == 0.0) {
                return Undef;
            }

            return new Number(((Number)a).Val / ((Number)b).Val);
        }

        if (a is List && b is Number) {
            var l = ((List)a).Val;

            var res = new List<Value>();
            foreach (var x in l) {
                res.Add(x / b);
            }

            return new List(res);
        }

        return Undef;
    }

    public static Value operator%(Value a, Value b)
    {
        if (a is Number && b is Number) {
            if (((Number)b).Val == 0.0) {
                return Undef;
            }

            return new Number(((Number)a).Val % ((Number)b).Val);
        }

        return Undef;
    }

    public static bool AsBool(Value a)
    {
        if (a is Bool) {
            return ((Bool)a).Val;
        }

        if (a is Number) {
            return ((Number)a).Val != 0.0;
        }

        if (a is List) {
            return ((List)a).Val.Count > 0;
        }

        if (a is String) {
            return ((String)a).Val.Length > 0;
        }

        return false;
    }

    public override string ToString()
    {
        return "";
    }
}
