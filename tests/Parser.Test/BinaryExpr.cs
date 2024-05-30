namespace Parser.Test;

using BinaryExpr = BinaryOperation<ValueParser, OperationParser>;

class ValueParser : IPegParsable {
    public string Val = "";

    public void PegParse(Context ctx)
    {
        ctx.Regex("[-+]?[1-9][0-9]*", out Val);
    }
};

class OperationParser : IPegParsable {
    public string Op = "";

    public void PegParse(Context ctx)
    {
        ctx.Regex("[-+]", out Op);
    }
};

public class BinaryExprTest
{
    [SetUp]
    public void Setup()
    {
    }

    string Encode(BinaryExpr expr)
    {
        if (expr.Expression == null) {
            return expr.Value.Val;
        }

        return $"({expr.Operation.Op} {Encode(expr.Expression)} {expr.Value.Val})";
    }

    string Encode(Context.ExpressionNode<ValueParser>? expr)
    {
        if (expr == null) {
            return "";
        }

        if (expr.IsValue()) {
            return expr.Value.Val;
        }

        return $"({expr.Operation} {Encode(expr.Left)} {expr.Right?.Val})";
    }

    public void TryParse(string src, string res)
    {
        var ctx = new Context(src);
        var err = "";
        var p = new BinaryExpr();
        ctx.WS = new Whitespace();
        try {
            Assert.That(ctx.Parse(p), Is.EqualTo(true));
        } catch (ParseException exc) {
            err = exc.ParseError();
        }

        Assert.That(err, Is.EqualTo(""));
        Assert.That(Encode(p), Is.EqualTo(res));
    }

    public void TryParse2(string src, string res)
    {
        var ctx = new Context(src);
        var err = "";
        Context.ExpressionNode<ValueParser>? val = null;
        try {
            Assert.That(ctx.Parse(ctx.Func((c) => { val = ctx.Infix<ValueParser>("+", "-"); })), Is.EqualTo(true));
        } catch (ParseException exc) {
            err = exc.ParseError();
        }

        Assert.That(err, Is.EqualTo(""));
        Assert.That(Encode(val), Is.EqualTo(res));
    }

    [Test]
    public void Test()
    {
        TryParse("3", "3");
        TryParse("2 + 4", "(+ 2 4)");
        TryParse("2 + 3 - 4 + 5", "(+ (- (+ 2 3) 4) 5)");

        Assert.Pass();
    }

    [Test]
    public void TestSimple()
    {
        TryParse2("3", "3");
        TryParse2("2 + 4", "(+ 2 4)");
        TryParse2("2 + 3 - 4 + 5", "(+ (- (+ 2 3) 4) 5)");

        Assert.Pass();
    }

}
