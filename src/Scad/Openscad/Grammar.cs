namespace Scad.Openscad;

using Parser;

public class Grammar {
    static private Parser.Word _identWord = new(Parser.Word.IdentStart + "$", Parser.Word.IdentChars);

    static bool IsFileIncluded(ExecutionContext ctx, string filename)
    {
        var incList = ctx.GetLocalVariable("@includes");
        if (incList is not Value.List) {
            incList = new Value.List();
        }

        var lst = ((Value.List)incList).Val;

        bool res = false;
        foreach (var inc in lst) {
            if (inc is Value.String && ((Value.String)inc).Val == filename) {
                res = true;
                break;
            }
        }

        if (!res) {
            lst.Add(new Value.String(filename));
        }

        return res;
    }

    /// Base class for all Ast classes
    public class Ast : IPegParsable
    {
        public int PegLocation = -1;
        public string? PegString = null;

        public void Log(ExecutionContext context, string message)
        {
            if (PegLocation >= 0 && PegString != null) {
                var p = SourceLocation.LineColumn(PegString, PegLocation);
                context.Log($"AT LINE: {p.Line}\n{message}");
            }

            context.Log(message);
        }

        public void PegParse(Context ctx)
        {
            PegLocation = ctx.Position;
            PegString = ctx.Source;
            Peg(ctx);
        }

        public virtual void Peg(Context ctx)
        {
            throw new NotImplementedException();
        }
    };

    public class ExpressionBase : Ast
    {
        public virtual Value Eval(ExecutionContext context)
        {
            Console.WriteLine($"NOT IMPLEMENTED EVAL FOR {this.GetType().Name}");
            throw new NotImplementedException();
        }
    };

    public class InternalConstant : ExpressionBase {
        private Value? _value = null;

        public override void Peg(Context ctx)
        {
            ctx.FirstOf(out int choice,
                    ctx.Func((c) => c.Keyword("true")),
                    ctx.Func((c) => c.Keyword("false")),
                    ctx.Func((c) => c.Keyword("undef")));
            if (choice == 0) {
                _value = new Value.Bool(true);
            } else if (choice == 1) {
                _value = new Value.Bool(false);
            } else {
                _value = new Value.Undefined();
            }
        }

        public override Value Eval(ExecutionContext context)
        {
            if (_value == null) {
                throw new NullReferenceException();
            }
            return _value;
        }
    }

    public class Number : ExpressionBase {
        private double _value = 0.0;

        public Number()
        {}

        public Number(double n)
        {
            _value = n;
        }

        public override void Peg(Context ctx)
        {
            ctx.Double(out _value);
        }

        public override Value Eval(ExecutionContext context)
        {
            return new Value.Number(_value);
        }
    };

    public class Identifier : ExpressionBase {
        private string _ident = "";

        public Identifier()
        {
        }

        public Identifier(string id)
        {
            _ident = id;
        }

        public override void Peg(Context ctx)
        {
            ctx.Word(_identWord, out _ident);
        }

        public override Value Eval(ExecutionContext context)
        {
            return context.GetVariable(_ident);
        }
    };

    public class String : ExpressionBase {
        private string _value = "";

        public override void Peg(Context ctx)
        {
            // TODO: better string parsing with escape sequences, etc
            ctx.SkipWS();
            ctx.NotEOF();

            if (ctx.Source[ctx.Position] != '"') {
                ctx.Error("Expected string");
            }

            ++ctx.Position;
            var val = new System.Text.StringBuilder();
            for (; ctx.Position < ctx.Source.Length; ++ctx.Position) {
                if (ctx.Source[ctx.Position] == '"') {
                    _value = val.ToString();
                    ++ctx.Position;
                    return;
                }

                if (ctx.Source[ctx.Position] == '\\') {
                    ++ctx.Position;
                    ctx.NotEOF();
                }

                val.Append(ctx.Source[ctx.Position]);
            }

            ctx.Error("End of file inside a string");
        }

        public override Value Eval(ExecutionContext context)
        {
            return new Value.String(_value);
        }
    }

    public class ListComprehensionElements: IPegParsable
    {
        public class Let : IPegParsable
        {
            public Arguments Args = new();
            ListComprehensionElements? Child = null;

            public void PegParse(Context ctx)
            {
                ctx.Keyword("let");
                ctx.Literal("(");
                ctx.One(Args);
                ctx.Literal(")");
                Child = ctx.One<ListComprehensionElements>();
            }
        }

        public class Each: IPegParsable
        {
            ListComprehensionElements? Child = null;

            public void PegParse(Context ctx)
            {
                ctx.Keyword("each");
                Child = ctx.One<ListComprehensionElements>();
            }
        }

        public class For : IPegParsable
        {
            public class ExprPart : IPegParsable
            {
                Expression Expr = new();
                Arguments Args = new();

                public void PegParse(Context ctx)
                {
                    ctx.One(Expr);
                    ctx.One(Args);
                }
            }

            public Arguments Args = new();
            ExprPart? Expr = null;
            ListComprehensionElements? Child = null;

            public void PegParse(Context ctx)
            {
                ctx.Keyword("for");
                ctx.Literal("(");
                ctx.One(Args);
                Expr = ctx.Optional<ExprPart>();
                ctx.Literal(")");
                Child = ctx.One<ListComprehensionElements>();
            }
        }

        public class If : IPegParsable
        {
            public Expression Cond = new();
            public ListComprehensionElements TrueExpr = new();
            public ListComprehensionElements? ElseExpr = null;

            public class ElseParser: IPegParsable
            {
                public ListComprehensionElements Expr = new();
                public void PegParse(Context ctx)
                {
                    ctx.Keyword("else");
                    ctx.One(Expr);
                }
            }

            public void PegParse(Context ctx)
            {
                ctx.Keyword("if");
                ctx.Literal("(");
                ctx.One(Cond);
                ctx.Literal(")");
                ctx.One(TrueExpr);
                var elseStmt = ctx.Optional<ElseParser>();
                if (elseStmt != null) {
                    ElseExpr = elseStmt.Expr;
                }
            }
        }

        public IPegParsable? Element = null;

        public void PegParse(Context ctx)
        {
            Element = ctx.Union<Let, Each, For, If, Expression>(out int idx);
        }

        public List<Value> Eval(ExecutionContext context)
        {
            if (Element is Expression) {
                return new List<Value>{((Expression)Element).Eval(context)};
            }
            // TODO: implement
            return new List<Value>{Value.Undef};
        }
    }

    public class ListExpr : ExpressionBase
    {
        public List<ListComprehensionElements> Data = new();

        class Comma : IPegParsable {
            public void PegParse(Context ctx)
            {
                ctx.Literal(",");
            }
        };

        public override void Peg(Context ctx)
        {
            // TODO: ListItem which is ListComprehensionElement/Expression
            ctx.Literal("[");
            ctx.DelimitedList(Data, ",");
            ctx.Optional<Comma>();
            ctx.Literal("]");
        }

        public override Value Eval(ExecutionContext context)
        {
            var data = new List<Value>();
            foreach (var el in Data) {
                data.AddRange(el.Eval(context));
            }
            return new Value.List(data);
        }
    }

    public class ListGen : ExpressionBase
    {
        Expression First = new();
        Expression Second = new();
        Expression? Third = null;

        class PExpr : IPegParsable
        {
            public Expression Expr = new();

            public void PegParse(Context ctx)
            {
                ctx.Literal(":");
                ctx.One(Expr);
            }
        }

        public override void Peg(Context ctx)
        {
            ctx.Literal("[");
            ctx.One(First);
            ctx.Literal(":");
            ctx.One(Second);
            var t = ctx.Optional<PExpr>();
            if (t != null) {
                Third = t.Expr;
            }
            ctx.Literal("]");
        }

        public override Value Eval(ExecutionContext context)
        {
            double begin = 0.0;
            double end = 0.0;
            double step = 1.0;

            var f = First.Eval(context);
            if (f is not Value.Number) {
                Log(context, "ERROR: Invalid type of list generator expression");
                return Value.Undef;
            }
            begin = ((Value.Number)f).Val;

            var s = Second.Eval(context);
            if (s is not Value.Number) {
                Log(context, "ERROR: Invalid type of list generator expression");
                return Value.Undef;
            }

            if (Third != null) {
                var t = Third.Eval(context);
                if (t is not Value.Number) {
                    Log(context, "ERROR: Invalid type of list generator expression");
                    return Value.Undef;
                }

                end = ((Value.Number)t).Val;
                step = ((Value.Number)s).Val;
            } else {
                end = ((Value.Number)s).Val;
            }

            var res = new Value.List();
            for (double i = begin; i <= end; i += step) {
                res.Val.Add(new Value.Number(i));
                if (res.Val.Count > 65536) {
                    Log(context, "ERROR: list is too long");
                    return Value.Undef;
                }
            }

            return res;
        }
    }

    public class Primary : ExpressionBase
    {
        ExpressionBase? Expr = null;

        public override void Peg(Context ctx)
        {
            IPegParsable res = ctx.Union<InternalConstant, Number, String, Identifier, BracedExpression, ListExpr, ListGen>(out int idx);
            Expr = res as ExpressionBase;
        }

        public override Value Eval(ExecutionContext context)
        {
            if (Expr == null) {
                throw new NullReferenceException();
            }

            return Expr.Eval(context);
        }
    };

    public class Parameter : IPegParsable
    {
        public string Name = "";
        public Expression? DefaultValue = null;

        class DefVal : IPegParsable
        {
            public Expression Expr = new Expression();

            public void PegParse(Context ctx)
            {
                ctx.Literal("=");
                ctx.NotAny(ctx.Func((c) => c.Literal("=")));
                ctx.Parse(Expr);
            }
        };

        public void PegParse(Context ctx)
        {
            ctx.Word(_identWord, out Name);
            var v = ctx.Optional<DefVal>();
            if (v != null) {
                DefaultValue = v.Expr;
            }
        }
    };

    public class Parameters : IPegParsable
    {
        public List<Parameter> Params = new List<Parameter>();

        public void PegParse(Context ctx)
        {
            ctx.DelimitedList(Params, ",");
        }

        public List<Value.Parameter> GetParameters(ExecutionContext context)
        {
            var res = new List<Value.Parameter>();
            foreach (var arg in Params) {
                if (arg.DefaultValue == null) {
                    res.Add(new Value.Parameter(arg.Name));
                } else {
                    res.Add(new Value.Parameter(arg.Name, arg.DefaultValue.Eval(context)));
                }
            }

            return res;
        }
    };

    public class Argument : IPegParsable
    {
        public string Name = "";
        public ExpressionBase Value = new ExpressionBase();

        public Argument()
        {
        }

        public Argument(ExpressionBase val)
        {
            Value = val;
        }

        class ValName : IPegParsable
        {
            public string Name = "";

            public void PegParse(Context ctx)
            {
                ctx.Word(_identWord, out Name);
                ctx.Literal("=");
                ctx.NotAny(ctx.Func((c) => c.Literal("=")));
            }
        };

        public void PegParse(Context ctx)
        {
            var n = ctx.Optional<ValName>();
            var value = new Expression();
            ctx.Parse(value);
            Value = value;
            if (n != null) {
                Name = n.Name;
            }
        }
    };

    public class Arguments : IPegParsable
    {
        public List<Argument> Params = new List<Argument>();

        public void PegParse(Context ctx)
        {
            ctx.DelimitedList(Params, ",");
        }

        public Value.Argument[] GetArguments(ExecutionContext context)
        {
            var res = new List<Value.Argument>();
            foreach (var arg in Params) {
                if (arg.Name == null) {
                    res.Add(new Value.Argument(arg.Value.Eval(context)));
                } else {
                    res.Add(new Value.Argument(arg.Name, arg.Value.Eval(context)));
                }
            }

            return res.ToArray();
        }
    };

    public class BracedExpression : ExpressionBase
    {
        Expression Expr = new Expression();

        public override void Peg(Context ctx)
        {
            ctx.Literal("(");
            ctx.One(Expr);
            ctx.Literal(")");
        }

        public override Value Eval(ExecutionContext context)
        {
            return Expr.Eval(context);
        }
    };

    public class Modifier : ExpressionBase
    {
        public class ModifierBase
        {
            public virtual Value Modify(ExecutionContext context, Value expr)
            {
                return expr;
            }
        }

        public ModifierBase? Mod = null;

        class CallFunc : ModifierBase, IPegParsable
        {
            public Arguments Arguments = new Arguments();

            public void PegParse(Context ctx)
            {
                ctx.Literal("(");
                ctx.Parse(Arguments);
                ctx.Literal(")");
            }

            public override Value Modify(ExecutionContext context, Value expr)
            {
                if (expr is Value.Function) {
                    var f = (Value.Function)expr;
                    return f.Call(context, Arguments.GetArguments(context));
                }

                context.Log("Trying to call non-function object");
                return new Value.Undefined();
            }
        };

        class GetItem : ModifierBase, IPegParsable
        {
            public Expression Index = new Expression();

            public void PegParse(Context ctx)
            {
                ctx.Literal("[");
                ctx.Parse(Index);
                ctx.Literal("]");
            }

            public override Value Modify(ExecutionContext context, Value expr)
            {
                if (expr is Value.List) {
                    var l = (Value.List)expr;
                    var i = Index.Eval(context);
                    if (!(i is Value.Number)) {
                        context.Log("Invalid index for list");
                        return new Value.Undefined();
                    }

                    int idx = (int)((Value.Number)i).Val;
                    if (idx < 0 || idx >= l.Val.Count) {
                        context.Log("Index is out of range");
                        return new Value.Undefined();
                    }

                    return l.Val[idx];
                }

                context.Log("Trying to apply [] to non-list value");
                return new Value.Undefined();
            }
        };

        class GetAttr : ModifierBase, IPegParsable
        {
            public string Attribute = "";

            public void PegParse(Context ctx)
            {
                ctx.Literal(".");
                ctx.Word(_identWord, out Attribute);
            }

            public override Value Modify(ExecutionContext context, Value expr)
            {
                if (!(expr is Value.List)) {
                    context.Log("Trying to access field of non-list");
                    return new Value.Undefined();
                }

                var l = (Value.List)expr;

                if (Attribute == "x") {
                    if (l.Val.Count < 1) {
                        context.Log(".x is out of range");
                        return new Value.Undefined();
                    }
                    return l.Val[0];
                } else if (Attribute == "y") {
                    if (l.Val.Count < 2) {
                        context.Log(".y is out of range");
                        return new Value.Undefined();
                    }
                    return l.Val[1];
                } else if (Attribute == "z") {
                    if (l.Val.Count < 3) {
                        context.Log(".z is out of range");
                        return new Value.Undefined();
                    }
                    return l.Val[2];
                } else {
                    context.Log($"Unknown attribute {Attribute}");
                }
                return new Value.Undefined();
            }
        };

        public override void Peg(Context ctx)
        {
            IPegParsable res = ctx.Union<CallFunc, GetItem, GetAttr>(out int idx);

            Mod = res as ModifierBase;
        }
    };

    public class Call : ExpressionBase
    {
        public Primary Primary = new();
        public List<Modifier> Mods = new List<Modifier>();

        public override void Peg(Context ctx)
        {
            ctx.Parse(Primary);
            ctx.ZeroOrMore(Mods);
        }

        public override Value Eval(ExecutionContext context)
        {
            if (Primary == null) {
                throw new NullReferenceException();
            }

            var val = Primary.Eval(context);

            foreach (var mod in Mods) {
                if (mod.Mod != null) {
                    val = mod.Mod.Modify(context, val);
                }
            }

            return val;
        }
    };

    public class Exponent : ExpressionBase
    {
        public Call Call = new Call();
        public ExpressionBase? Power = null;

        class PowerParser : IPegParsable
        {
            public Call Expr = new Call();

            public void PegParse(Context ctx)
            {
                ctx.Literal("^");
                ctx.Parse(Expr);
            }
        };

        public override void Peg(Context ctx)
        {
            ctx.Parse(Call);
            var p = ctx.Optional<PowerParser>();
            if (p != null) {
                Power = p.Expr;
            }
        }

        public override Value Eval(ExecutionContext context)
        {
            if (Power == null) {
                return Call.Eval(context);
            }
            var a = Call.Eval(context);
            var p = Power.Eval(context);

            if (!(a is Value.Number) || !(p is Value.Number)) {
                context.Log("Invalid argument types for power");
                return new Value.Undefined();
            }

            return new Value.Number(Math.Pow(((Value.Number)a).Val, ((Value.Number)p).Val));
        }
    };

    public class Unary : ExpressionBase
    {
        public class UnaryOp : IPegParsable {
            public string Op = "";

            public void PegParse(Context ctx)
            {
                ctx.Literals(out Op, "-", "+", "!");
            }
        };

        public List<UnaryOp> Ops = new List<UnaryOp>();
        public Exponent Expr = new Exponent();

        public override void Peg(Context ctx)
        {
            ctx.ZeroOrMore(Ops);
            ctx.Parse(Expr);
        }

        public override Value Eval(ExecutionContext context)
        {
            var a = Expr.Eval(context);

            if (Ops.Count == 0) {
                return a;
            }

            for (int i = Ops.Count - 1; i >= 0; --i) {
                if (Ops[i].Op == "-") {
                    if (a is Value.Number || a is Value.List) {
                        a = -a;
                    } else {
                        Log(context, $"Invalid argument types for unary - ({a.GetType().Name})");
                        return new Value.Undefined();
                    }
                }

                if (Ops[i].Op == "!") {
                    bool v = Value.AsBool(a);

                    a = new Value.Bool(!v);
                }
            }

            return a;
        }
    };

    static Value EvalInfixExpr<T>(ExecutionContext context, Parser.Context.ExpressionNode<T> expr) where T: ExpressionBase, IPegParsable, new()
    {
        if (expr.IsValue()) {
            return expr.Value.Eval(context);
        }

        if (expr.Left == null || expr.Right == null) {
            throw new NullReferenceException();
        }

        var l = EvalInfixExpr<T>(context, expr.Left);
        var r = expr.Right.Eval(context);

        if (expr.Operation == "<") {
            return new Value.Bool(Value.OperatorLess(l, r));
        } else if (expr.Operation == ">") {
            return new Value.Bool(Value.OperatorGreater(l, r));
        } else if (expr.Operation == "<=") {
            return new Value.Bool(Value.OperatorLessOrEqual(l, r));
        } else if (expr.Operation == ">=") {
            return new Value.Bool(Value.OperatorGreaterOrEqual(l, r));
        } else if (expr.Operation == "==") {
            return new Value.Bool(Value.OperatorEqual(l, r));
        } else if (expr.Operation == "!=") {
            return new Value.Bool(Value.OperatorNotEqual(l, r));
        } else if (expr.Operation == "||") {
            return new Value.Bool(Value.LogicalOr(l, r));
        } else if (expr.Operation == "&&") {
            return new Value.Bool(Value.LogicalAnd(l, r));
        } else if (expr.Operation == "+") {
            return l + r;
        } else if (expr.Operation == "-") {
            return l - r;
        } else if (expr.Operation == "*") {
            return l * r;
        } else if (expr.Operation == "/") {
            return l / r;
        } else if (expr.Operation == "%") {
            return l % r;
        }

        return Value.Undef;
    }

    public class InfixOp<T> : ExpressionBase where T: ExpressionBase, new() {
        public Parser.Context.ExpressionNode<T>? Expr = null;

        public override Value Eval(ExecutionContext context)
        {
            if (Expr == null) {
                throw new NullReferenceException();
            }

            return EvalInfixExpr(context, Expr);
        }
    };

    public class Multiplication : InfixOp<Unary>
    {
        public override void Peg(Context ctx)
        {
            this.Expr = ctx.Infix<Unary>("*", "/", "%");
        }
    };

    public class Addition : InfixOp<Multiplication>
    {
        public override void Peg(Context ctx)
        {
            this.Expr = ctx.Infix<Multiplication>("+", "-");
        }
    };

    public class Comparison : InfixOp<Addition>
    {
        public override void Peg(Context ctx)
        {
            this.Expr = ctx.Infix<Addition>("<=", ">=", "<", ">");
        }
    };

    public class Equality : InfixOp<Comparison>
    {
        public override void Peg(Context ctx)
        {
            this.Expr = ctx.Infix<Comparison>("==", "!=");
        }
    };

    public class LogicAnd : InfixOp<Equality>
    {
        public override void Peg(Context ctx)
        {
            this.Expr = ctx.Infix<Equality>("&&");
        }
    };

    public class LogicOr : InfixOp<LogicAnd>
    {
        public override void Peg(Context ctx)
        {
            this.Expr = ctx.Infix<LogicAnd>("||");
        }
    };

    public class TernarOperator : ExpressionBase
    {
        public LogicOr Cond = new LogicOr();
        TrueFalse? Tf = null;

        class TrueFalse: IPegParsable {
            public Expression ExprTrue = new Expression();
            public Expression ExprFalse = new Expression();

            public void PegParse(Context ctx)
            {
                ctx.Literal("?");
                ctx.Parse(ExprTrue);
                ctx.Literal(":");
                ctx.Parse(ExprFalse);
            }
        };

        public override void Peg(Context ctx)
        {
            Cond = ctx.One<LogicOr>();
            Tf = ctx.Optional<TrueFalse>();
        }

        public override Value Eval(ExecutionContext context)
        {
            var cond = Cond.Eval(context);

            if (Tf == null) {
                return cond;
            }

            if (Value.AsBool(cond)) {
                return Tf.ExprTrue.Eval(context);
            }

            return Tf.ExprFalse.Eval(context);
        }
    };

    public static void SetLocalVariables(ExecutionContext ctx, Parameters parameters, Value.Argument[] arguments)
    {
        // First set default:
        foreach (var param in parameters.Params) {
            if (param.DefaultValue != null) {
                ctx.SetVariable(param.Name, param.DefaultValue.Eval(ctx));
            }
        }

        int idx = 0;
        foreach (var arg in arguments) {
            if (arg.Name == null || arg.Name == "") {
                if (idx < parameters.Params.Count) {
                    ctx.SetVariable(parameters.Params[idx].Name, arg.Val);
                    ++idx;
                } else {
                    ctx.Log("Too many arguments for module");
                }
            } else {
                bool found = false;
                foreach (var param in parameters.Params) {
                    if (param.Name == arg.Name) {
                        found = true;
                        ctx.SetVariable(param.Name, arg.Val);
                        break;
                    }
                }

                if (!found) {
                    ctx.Log($"Unknown argument with name {arg.Name}");
                }
            }
        }
    }

    public class FunctionImpl : Value.Function
    {
        public Parameters Params;
        public ExpressionBase Body;
        private ExecutionContext _context;

        public FunctionImpl(ExecutionContext context, Parameters parameters, ExpressionBase body)
        {
            Body = body;
            _context = context;
            Params = parameters;
        }

        public override Value Call(ExecutionContext context, params Value.Argument[] arguments)
        {
            var ctx = context.Enter();
            SetLocalVariables(ctx, Params, arguments);
            return Body.Eval(ctx);
        }
    }

    public class ModuleImpl : IModule
    {
        public Parameters Params;
        public StatementBase Body;
        private ExecutionContext _context;

        public ModuleImpl(ExecutionContext context, Parameters parameters, StatementBase body)
        {
            Body = body;
            _context = context;
            Params = parameters;
        }

        public List<Tree.Node> Execute(ExecutionContext context, params Value.Argument[] arguments)
        {
            var ctx = context.Enter();
            SetLocalVariables(ctx, Params, arguments);
            Body.Prepare(ctx);
            Body.Assign(ctx);
            return Body.Execute(ctx);
        }
    }

    public class Lambda : ExpressionBase
    {
        public Parameters Params = new Parameters();
        public Expression Body = new Expression();

        public override void Peg(Context ctx)
        {
            ctx.Keyword("function");
            ctx.Literal("(");
            ctx.Parse(Params);
            ctx.Literal(")");
            ctx.Parse(Body);
        }

        public override Value Eval(ExecutionContext context)
        {
            return new FunctionImpl(context, Params, Body);
        }
    };

    public class LetExpr : ExpressionBase
    {
        public Arguments Arguments = new Arguments();
        public Expression Expr = new Expression();

        public override void Peg(Context ctx)
        {
            ctx.Keyword("let");
            ctx.Literal("(");
            try {
            ctx.Parse(Arguments);
            ctx.Literal(")");
            ctx.Parse(Expr);
            } catch (ParseException exc) {
                Console.WriteLine($"EXC: {exc.ParseError()}");
                throw;
            }
        }

        public override Value Eval(ExecutionContext context)
        {
            var ctx = context.Enter();
            foreach (var arg in Arguments.Params) {
                if (arg.Name != null && arg.Name != "") {
                    ctx.SetVariable(arg.Name, arg.Value.Eval(ctx));
                }
            }

            return Expr.Eval(ctx);
        }
    };

    public class AssertExpr : ExpressionBase
    {
        public Arguments Arguments = new Arguments();
        public Expression? Expr = null;

        public override void Peg(Context ctx)
        {
            ctx.Keyword("assert");
            ctx.Literal("(");
            ctx.Parse(Arguments);
            ctx.Literal(")");
            Expr = ctx.Optional<Expression>();
        }

        public override Value Eval(ExecutionContext context)
        {
            Log(context, "TODO: assert is not implemented");

            if (Expr == null) {
                return new Value.Undefined();
            }

            return Expr.Eval(context);
        }
    }

    public class EchoExpr : ExpressionBase
    {
        public Arguments Arguments = new Arguments();
        public Expression? Expr = null;

        public override void Peg(Context ctx)
        {
            ctx.Keyword("echo");
            ctx.Literal("(");
            ctx.Parse(Arguments);
            ctx.Literal(")");
            Expr = ctx.Optional<Expression>();
        }

        public override Value Eval(ExecutionContext context)
        {
            var b = new System.Text.StringBuilder();
            b.Append("ECHO:");
            foreach (var arg in Arguments.Params) {
                b.Append(" ");

                var v = arg.Value.Eval(context);
                if (arg.Name.Length > 0) {
                    b.Append($"{arg.Name}={v.ToString()}");
                } else {
                    b.Append(v.ToString());
                }
            }

            Log(context, b.ToString());

            if (Expr == null) {
                return new Value.Undefined();
            }

            return Expr.Eval(context);
        }
    };

    public class Expression : ExpressionBase
    {
        ExpressionBase? Expr = null;

        public override void Peg(Context ctx)
        {
            var obj = ctx.Union<Lambda, LetExpr, AssertExpr, EchoExpr, TernarOperator>(out int idx);
            Expr = obj as ExpressionBase;
        }

        public override Value Eval(ExecutionContext context)
        {
            if (Expr == null) {
                throw new NullReferenceException();
            }
            return Expr.Eval(context);
        }
    };

    /* Top-level constructions */
    public class StatementBase : Ast
    {
        public virtual void Prepare(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public virtual void Assign(ExecutionContext context)
        {
        }

        public virtual List<Tree.Node> Execute(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    };

    static private System.Text.RegularExpressions.Regex _includeRegex = new System.Text.RegularExpressions.Regex(@"[^>]*");
    public class Use : StatementBase
    {
        public string Filename = "";
        public Prog? Prog = null;

        public override void Peg(Context ctx)
        {
            ctx.Keyword("use");
            ctx.Literal("<");
            ctx.Regex(_includeRegex, out Filename);
            ctx.Literal(">");
        }

        public override void Prepare(ExecutionContext context)
        {
            if (Prog == null) {
                throw new NotImplementedException();
            }

            if (IsFileIncluded(context, Filename)) {
                return;
            }

            var ctx = new UseContext(context);

            foreach (var child in Prog.Statements) {
                child.Prepare(ctx);
            }

            foreach (var child in Prog.Statements) {
                child.Assign(ctx);
            }
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            if (Prog == null) {
                throw new NotImplementedException();
            }

            return new();
        }
    };

    public class Include : StatementBase
    {
        public string Filename = "";
        public Prog? Prog = null;

        public override void Peg(Context ctx)
        {
            ctx.Keyword("include");
            ctx.Literal("<");
            ctx.Regex(_includeRegex, out Filename);
            ctx.Literal(">");
        }

        public override void Prepare(ExecutionContext context)
        {
            if (Prog == null) {
                throw new NotImplementedException();
            }

            foreach (var child in Prog.Statements) {
                child.Prepare(context);
            }
        }

        public override void Assign(ExecutionContext context)
        {
            if (Prog == null) {
                throw new NotImplementedException();
            }

            foreach (var child in Prog.Statements) {
                child.Assign(context);
            }
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            if (Prog == null) {
                throw new NotImplementedException();
            }

            var res = new List<Tree.Node>();
            bool included = IsFileIncluded(context, Filename);
            foreach (var child in Prog.Statements) {
                if (included && child.IsDefinition()) {
                    continue;
                }

                var r = child.Execute(context);
                res.AddRange(r);
            }

            return res;
        }
    };

    public class Assignment : StatementBase
    {
        public string Name = "";
        public Expression Expr = new Expression();

        public override void Peg(Context ctx)
        {
            ctx.Word(_identWord, out Name);
            ctx.Literal("=");
            ctx.Parse(Expr);
            ctx.Literal(";");
        }

        public override void Prepare(ExecutionContext context)
        {
        }

        public override void Assign(ExecutionContext context)
        {
            var val = Expr.Eval(context);
            context.SetVariable(Name, val);
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            return new();
        }
    };

    public class ModuleDef : StatementBase
    {
        public string Name = "";
        public Parameters Params = new Parameters();
        public Statement Body = new Statement();

        public override void Peg(Context ctx)
        {
            ctx.Keyword("module");
            ctx.Word(_identWord, out Name);
            ctx.Literal("(");
            ctx.Parse(Params);
            ctx.Literal(")");
            ctx.One(Body);
        }

        public override void Prepare(ExecutionContext context)
        {
            var mod = new ModuleImpl(context, Params, Body);
            context.SetModule(Name, mod);
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            return new();
        }
    };

    public class EmptyModule : StatementBase
    {
        public override void Peg(Context ctx)
        {
            ctx.Literal(";");
        }

        public override void Prepare(ExecutionContext context)
        {
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            return new();
        }
    };

    public class FunctionDef : StatementBase
    {
        public string Name = "";
        public Parameters Params = new Parameters();
        public Expression Expr = new Expression();

        public override void Peg(Context ctx)
        {
            ctx.Keyword("function");
            ctx.Word(_identWord, out Name);
            ctx.Literal("(");
            ctx.Parse(Params);
            ctx.Literal(")");
            ctx.Literal("=");
            ctx.Parse(Expr);
            ctx.Literal(";");
        }

        public override void Prepare(ExecutionContext context)
        {
            var fcn = new FunctionImpl(context, Params, Expr);
            context.SetFunction(Name, fcn);
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            return new();
        }
    };

    public class SingleModuleInstantiation : StatementBase
    {
        public string Name = "";
        public Arguments Args = new Arguments();
        public ChildModuleInstantiation Children = new ChildModuleInstantiation();

        public override void Peg(Context ctx)
        {
            ctx.Word(_identWord, out Name);
            ctx.Literal("(");
            ctx.One(Args);
            ctx.Literal(")");
            ctx.One(Children);
        }

        public override void Prepare(ExecutionContext context)
        {
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            if (Name == "for") {
                return ExecuteFor(context);
            }

            if (Name == "intersection_for") {
                return ExecuteFor(context, true);
            }

            if (Name == "let") {
                return ExecuteLet(context);
            }

            List<Tree.Node> children = new List<Tree.Node>();
            if (Children.Children != null && !(Children.Children is EmptyModule)) {
                var ctx = context.Enter();
                Children.Prepare(ctx);
                Children.Assign(ctx);
                children = Children.Children.Execute(ctx);
            }

            // TODO: some better solution for this
            if (Name == "children") {
                var m = new ChildrenModule(children);
                return m.Execute(context, Args.GetArguments(context));
            }

            // Now we can modify children module:
            var modctx = context.Enter();
            modctx.SetModule("children", new ChildrenModule(children));
            modctx.SetVariable("$children", new Value.Number(children.Count));

            var mod = context.GetModule(Name);
            if (mod == null) {
                Log(context, $"ERROR: unknown module {Name}");
                return new();
            }

            // TODO: set children in context
            return mod.Execute(modctx, Args.GetArguments(context));
        }

        private List<Tree.Node> ExecuteForBody(ExecutionContext context, Value.Argument[] args, int idx)
        {
            if (idx >= args.Count()) {
                // Actual body execution:

                if (Children.Children != null && !(Children.Children is EmptyModule)) {
                    var ctx = context.Enter();
                    Children.Children.Prepare(ctx);
                    Children.Children.Assign(ctx);
                    var children = Children.Children.Execute(ctx);

                    return children;
                }

                return new();
            }

            var arg = args[idx];
            if (arg.Name == null || arg.Name == "") {
                Log(context, "ERROR: unnamed argument in for");
                return ExecuteForBody(context, args, idx + 1);
            }

            var val = arg.Val;
            List<Tree.Node> res = new();

            if (val is Value.List) {
                var lst = ((Value.List)val).Val;

                foreach (var v in lst) {
                    context.SetVariable(arg.Name, v);
                    res.AddRange(ExecuteForBody(context, args, idx + 1));
                }
            } else if (val is Value.String) {
                var s = ((Value.String)val).Val;

                for (int i = 0; i < s.Length; ++i) {
                    context.SetVariable(arg.Name, new Value.String(s.Substring(i, 1)));
                    res.AddRange(ExecuteForBody(context, args, idx + 1));
                }
            } else {
                context.SetVariable(arg.Name, val);
                return ExecuteForBody(context,  args, idx + 1);
            }

            return res;
        }

        public List<Tree.Node> ExecuteFor(ExecutionContext context, bool intersection = false)
        {
            var args = Args.GetArguments(context);

            var nodes = ExecuteForBody(context, args, 0);
            var res = new List<Tree.Node>();

            if (intersection) {
                res.Add(new Tree.Intersection(nodes));
            } else {
                res.Add(new Tree.Union(nodes));
            }

            return res;
        }

        public List<Tree.Node> ExecuteLet(ExecutionContext context)
        {
            context.Log("TODO: let is not defined");
            // TODO: actual execution
            return new();
        }
    };

    public class BlockStatement : StatementBase
    {
        public List<BlockChildStatement> Instances = new List<BlockChildStatement>();

        public override void Peg(Context ctx)
        {
            ctx.Literal("{");
            ctx.ZeroOrMore(Instances);
            ctx.Literal("}");
        }

        public override void Prepare(ExecutionContext context)
        {
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            var ctx = context.Enter();

            List<Tree.Node> res = new();

            foreach (var i in Instances) {
                if (i.Instance != null) {
                    i.Instance.Prepare(ctx);
                }
            }

            foreach (var i in Instances) {
                if (i.Instance != null) {
                    i.Instance.Assign(ctx);
                }
            }

            foreach (var i in Instances) {
                if (i.Instance == null) {
                    continue;
                }

                res.AddRange(i.Instance.Execute(ctx));
            }

            return res;
        }
    };

    public class BlockChildStatement : IPegParsable
    {
        public StatementBase? Instance = null;

        public void PegParse(Context ctx)
        {
            Instance = ctx.Union<Assignment, ModuleDef, FunctionDef, IfElseStatement, SingleModuleInstantiation, BlockStatement, EmptyModule>(out int idx) as StatementBase;
        }
    };

    public class ChildModuleInstantiation : StatementBase
    {
        public StatementBase? Children = null;

        public override void Peg(Context ctx)
        {
            Children = ctx.Union<IfElseStatement, EmptyModule, SingleModuleInstantiation, BlockStatement>(out int idx) as StatementBase;
        }

        public override void Prepare(ExecutionContext context)
        {
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            if (Children == null) {
                throw new NullReferenceException();
            }

            Children.Prepare(context);
            Children.Assign(context);

            return Children.Execute(context);
        }
    };

    public class IfElseStatement : StatementBase
    {
        public Expression Cond = new Expression();
        public ChildModuleInstantiation Stmt = new ChildModuleInstantiation();
        public ChildModuleInstantiation? ElseStmt = null;

        class ElseParser : IPegParsable
        {
            public ChildModuleInstantiation Stmt = new ChildModuleInstantiation();

            public void PegParse(Context ctx)
            {
                ctx.Keyword("else");
                ctx.Parse(Stmt);
            }
        };

        public override void Peg(Context ctx)
        {
            ctx.Keyword("if");
            ctx.Literal("(");
            ctx.Parse(Cond);
            ctx.Literal(")");
            ctx.Parse(Stmt);
            var elseStmt = ctx.Optional<ElseParser>();
            if (elseStmt != null) {
                ElseStmt = elseStmt.Stmt;
            }
        }

        public override void Prepare(ExecutionContext context)
        {
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            var cond = Cond.Eval(context);
            if (Value.AsBool(cond)) {
                Stmt.Prepare(context);
                Stmt.Assign(context);
                return Stmt.Execute(context);
            } else {
                if (ElseStmt != null) {
                    ElseStmt.Prepare(context);
                    ElseStmt.Assign(context);
                    return ElseStmt.Execute(context);
                }
            }
            return new();
        }
    };

    public class ModuleModifier : IPegParsable
    {
        public string Op = "";

        public void PegParse(Context ctx)
        {
            ctx.Literals(out Op, "!", "#", "%", "*");
        }
    };

    public class ModuleInstantiation : StatementBase
    {
        public List<ModuleModifier> Modifiers = new List<ModuleModifier>();
        public StatementBase? Stmt = null;

        public override void Peg(Context ctx)
        {
            ctx.ZeroOrMore(Modifiers);
            Stmt = ctx.Union<IfElseStatement, SingleModuleInstantiation, BlockStatement>(out int idx) as StatementBase;
        }

        public override void Prepare(ExecutionContext context)
        {
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            if (Stmt == null) {
                throw new NullReferenceException();
            }

            Stmt.Prepare(context);
            Stmt.Assign(context);

            // TODO: Put context into some special state
            return Stmt.Execute(context);

            /*
            for (int i = Modifiers.Count - 1; i >= 0; --i) {
                if (Modifiers[i].Op == "!") {
                    res = new ByteCode.ShowOnly(res);
                } else if (Modifiers[i].Op == "*") {
                    res = new ByteCode.Disable(res);
                } else if (Modifiers[i].Op == "#") {
                    res = new ByteCode.Debug(res);
                } else if (Modifiers[i].Op == "%") {
                    res = new ByteCode.Transparent(res);
                } else {
                    // TODO: throw some exception
                }
            }
            */
        }
    };

    public class Statement : StatementBase
    {
        StatementBase? Stmt = null;

        public override void Peg(Context ctx)
        {
            var res = ctx.Union<EmptyModule, Include, Use, Assignment, FunctionDef, ModuleDef, ModuleInstantiation>(out int idx);
            Stmt = res as StatementBase;
        }

        public override void Prepare(ExecutionContext context)
        {
            if (Stmt == null) {
                throw new NullReferenceException();
            }

            Stmt.Prepare(context);
        }

        public override void Assign(ExecutionContext context)
        {
            if (Stmt == null) {
                throw new NullReferenceException();
            }

            Stmt.Assign(context);
        }

        public override List<Tree.Node> Execute(ExecutionContext context)
        {
            if (Stmt == null) {
                throw new NullReferenceException();
            }

            return Stmt.Execute(context);
        }

        public Include? GetInclude()
        {
            if (Stmt != null && Stmt is Include) {
                return (Include)Stmt;
            }
            return null;
        }

        public Use? GetUse()
        {
            if (Stmt != null && Stmt is Use) {
                return (Use)Stmt;
            }
            return null;
        }

        public bool IsDefinition()
        {
            if (Stmt == null) {
                return false;
            }

            return (Stmt is Assignment) || (Stmt is FunctionDef) || (Stmt is ModuleDef);
        }
    };

    public class Prog : IPegParsable
    {
        public List<Statement> Statements = new List<Statement>();

        class EOF : IPegParsable {
            public void PegParse(Context ctx)
            {
                ctx.EOF();
            }
        };

        public void PegParse(Context ctx)
        {
            ctx.ZeroOrMore(Statements);
            // Parsing union will make error message more informative
            ctx.Union<Statement, EOF>(out int idx);
        }

        public virtual List<Tree.Node> Execute(ExecutionContext context)
        {
            foreach (var stmt in Statements) {
                stmt.Prepare(context);
            }

            foreach (var stmt in Statements) {
                stmt.Assign(context);
            }

            List<Tree.Node> res = new();
            foreach (var stmt in Statements) {
                res.AddRange(stmt.Execute(context));
            }

            return res;
        }
    };

};

