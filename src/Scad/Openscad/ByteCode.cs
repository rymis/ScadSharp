namespace Scad.Openscad;

using System.Xml;

// TODO: P-code should contain modules (with definitions) and expressions and Scad.Tree builders.
public class ByteCode {
    public virtual XmlNode ToXml(XmlDocument doc)
    {
        throw new NotImplementedException();
    }

    public virtual void FromXml(XmlNode xml)
    {
        throw new NotImplementedException();
    }

    public class Argument : ByteCode
    {
        public string? Name;
        public Expression Val;

        public Argument(Expression val)
        {
            Val = val;
            Name = null;
        }

        public Argument(string name, Expression val)
        {
            Name = name;
            Val = val;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("argument");
            if (Name != null) {
                res.SetAttribute("name", Name);
            }

            res.AppendChild(Val.ToXml(doc));

            return res;
        }
    };

    public class Parameter : ByteCode
    {
        public string Name;
        public Expression? Val;

        public Parameter(string name)
        {
            Name = name;
            Val = null;
        }

        public Parameter(string name, Expression val)
        {
            Name = name;
            Val = val;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("parameter");
            res.SetAttribute("name", Name);

            if (Val != null) {
                res.AppendChild(Val.ToXml(doc));
            }

            return res;
        }
    };


    public interface Context
    {
        public Value GetVariable(string name);
        public Func<List<Argument>, Value>? GetFunction(string name);

        /// Enter new level of context
        public Context Enter();

        /// Set variable value
        public void Let(string name, Value val);

        /// Define function
        public void Function(string name, FuncExpr fcn);

        /// Define module
        public void Module(string name, ModuleExpr mod);
    };

    public class Expression : ByteCode
    {
        public virtual Value Eval(Context ctx)
        {
            return new Undefined();
        }
    };

    public class Statement : ByteCode
    {
    };

    public class Value : Expression
    {
    };

    public class Undefined : Value
    {
        public override XmlNode ToXml(XmlDocument doc)
        {
            return doc.CreateElement("undefined");
        }

        public override Value Eval(Context ctx)
        {
            return this;
        }
    };

    public class Bool : Value
    {
        public bool Val { get; set; }

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
    }

    public class Number : Value
    {
        public double Val { get; set; }

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
    };

    public class String : Value
    {
        public string Val { get; set; }

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
    };

    public class List : Value
    {
        public List<Value> Val;
        public List(params Value[] values)
        {
            Val = new List<Value>(values);
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("list");
            foreach (var el in Val) {
                res.AppendChild(el.ToXml(doc));
            }

            return res;
        }
    };

    public class Variable : Expression
    {
        public string Name;

        public Variable(string name)
        {
            Name = name;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("variable");
            res.SetAttribute("name", Name);
            return res;
        }

        public override Value Eval(Context ctx)
        {
            return ctx.GetVariable(Name);
        }
    };

    public class FuncCall : Expression
    {
        private string Name = "";
        public List<Argument> Arguments = new List<Argument>();

        public FuncCall(string name, params Argument[] args)
        {
            Name = name;
            Arguments = new List<Argument>(args);
        }

        public FuncCall(string name, params Expression[] args)
        {
            Name = name;
            Arguments = new List<Argument>();
            foreach (var arg in args) {
                Arguments.Add(new Argument(arg));
            }
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("call");
            res.SetAttribute("name", Name);
            var args = doc.CreateElement("arguments");
            res.AppendChild(args);

            foreach (var arg in Arguments) {
                args.AppendChild(arg.ToXml(doc));
            }

            return res;
        }

        public override Value Eval(Context ctx)
        {
            var fexpr = ctx.GetVariable(Name);
            if (fexpr is FuncExpr) {
                return ((FuncExpr)fexpr).Call(ctx, Arguments.ToArray());
            }

            var fcn = ctx.GetFunction(Name);
            if (fcn == null) {
                // TODO: show warning?
                return new Undefined();
            }

            return fcn(Arguments);
        }
    };

    public class FuncExpr : Value
    {
        public List<Parameter> Parameters;
        public Expression Expr;

        public FuncExpr(List<Parameter> args, Expression stmt)
        {
            Parameters = args;
            Expr = stmt;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("function");
            var args = doc.CreateElement("arguments");
            res.AppendChild(args);
            foreach (var arg in Parameters) {
                args.AppendChild(arg.ToXml(doc));
            }
            var expr = doc.CreateElement("body");
            res.AppendChild(expr);
            expr.AppendChild(Expr.ToXml(doc));

            return res;
        }

        public Value Call(Context ctx, params Argument[] arguments)
        {
            // TODO: provide call
            return new Undefined();
        }
    };

    public class Assignment : Statement
    {
        public string Destination;
        public Expression Val;

        public Assignment(string name, Expression val)
        {
            Destination = name;
            Val = val;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("assignment");
            res.SetAttribute("destination", Destination);
            res.AppendChild(Val.ToXml(doc));

            return res;
        }
    }

    public class ModCall : Statement
    {
        private string Name = "";
        public List<Argument> Arguments = new List<Argument>();
        public List<Statement> Children = new List<Statement>();

        public ModCall(string name, params Argument[] args)
        {
            Name = name;
            Arguments = new List<Argument>(args);
        }

        public ModCall(string name, params Expression[] args)
        {
            Name = name;
            Arguments = new List<Argument>();
            foreach (var arg in args) {
                Arguments.Add(new Argument(arg));
            }
        }

        public void AddChild(Statement stmt)
        {
            Children.Add(stmt);
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("modcall");
            res.SetAttribute("name", Name);
            var args = doc.CreateElement("arguments");
            res.AppendChild(args);
            var children = doc.CreateElement("children");
            res.AppendChild(children);

            foreach (var arg in Arguments) {
                args.AppendChild(arg.ToXml(doc));
            }

            foreach (var mod in Children) {
                children.AppendChild(mod.ToXml(doc));
            }

            return res;
        }
    };

    public class Disable : Statement
    {
        public Statement Stmt;

        public Disable(Statement statement)
        {
            Stmt = statement;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("disable");
            res.AppendChild(Stmt.ToXml(doc));
            return res;
        }
    };

    public class ShowOnly : Statement
    {
        public Statement Stmt;

        public ShowOnly(Statement statement)
        {
            Stmt = statement;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("show-only");
            res.AppendChild(Stmt.ToXml(doc));
            return res;
        }
    };

    public class Debug : Statement
    {
        public Statement Stmt;

        public Debug(Statement statement)
        {
            Stmt = statement;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("debug");
            res.AppendChild(Stmt.ToXml(doc));
            return res;
        }
    };

    public class Transparent : Statement
    {
        public Statement Stmt;

        public Transparent(Statement statement)
        {
            Stmt = statement;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("transparent");
            res.AppendChild(Stmt.ToXml(doc));
            return res;
        }
    };

    public class Cond : Statement
    {
        public Expression Condition;
        public Statement True;
        public Statement? False;

        public Cond(Expression cond, Statement t, Statement? f = null)
        {
            Condition = cond;
            True = t;
            False = f;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("if");
            var cond = doc.CreateElement("condition");
            res.AppendChild(cond);
            cond.AppendChild(Condition.ToXml(doc));
            var t = doc.CreateElement("true");
            res.AppendChild(t);
            t.AppendChild(True.ToXml(doc));
            if (False != null) {
                var f = doc.CreateElement("false");
                res.AppendChild(f);
                t.AppendChild(False.ToXml(doc));
            }

            return res;
        }

    };

    public class Block : Statement
    {
        public List<Statement> Statements;

        public Block(List<Statement> statements)
        {
            Statements = statements;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("block");

            foreach (var stmt in Statements) {
                res.AppendChild(stmt.ToXml(doc));
            }

            return res;
        }
    };

    public class ModuleExpr : ByteCode
    {
        public List<Parameter> Parameters;
        public Statement Expr;

        public ModuleExpr(List<Parameter> args, Statement stmt)
        {
            Parameters = args;
            Expr = stmt;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("module");
            var args = doc.CreateElement("arguments");
            res.AppendChild(args);
            foreach (var arg in Parameters) {
                args.AppendChild(arg.ToXml(doc));
            }
            var expr = doc.CreateElement("body");
            res.AppendChild(expr);
            expr.AppendChild(Expr.ToXml(doc));

            return res;
        }

        public Value Call(Context ctx, params Argument[] arguments)
        {
            // TODO: provide call
            return new Undefined();
        }
    };

    public class ModuleDefinition : Expression {
        public string Name;
        public ModuleExpr Expr;

        public ModuleDefinition(string name, ModuleExpr expr)
        {
            Name = name;
            Expr = expr;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("defmodule");
            res.SetAttribute("name", Name);
            res.AppendChild(Expr.ToXml(doc));

            return res;
        }
    };

    public class FunctionDefinition : Expression {
        public string Name;
        public FuncExpr Expr;

        public FunctionDefinition(string name, FuncExpr expr)
        {
            Name = name;
            Expr = expr;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var res = doc.CreateElement("deffunction");
            res.SetAttribute("name", Name);
            res.AppendChild(Expr.ToXml(doc));

            return res;
        }
    };

};
