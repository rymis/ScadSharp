namespace Parser.Test;

public class Basic
{
    class BasicParser : IPegParsable
    {
        public string Name = "";

        public void PegParse(Context ctx)
        {
            ctx.Literal("Hello");
            ctx.Literal(",");
            ctx.Regex("[A-Z][a-z]*", out Name);
            ctx.Literal("!");
        }
    };

    class Number : IPegParsable
    {
        public string Val = "";

        public void PegParse(Context ctx)
        {
            ctx.Regex("[0-9]+", out Val);
        }
    };

    [SetUp]
    public void Setup()
    {
    }

    public void TryParse(string src, string name, IPegParsable? ws)
    {
        var ctx = new Context(src);
        var err = "";
        ctx.WS = ws;
        var p = new BasicParser();
        try {
            Assert.That(ctx.Parse(p), Is.EqualTo(true));
        } catch (ParseException exc) {
            err = exc.ParseError();
        }

        Assert.That(err, Is.EqualTo(""));
        Assert.That(p.Name, Is.EqualTo(name));
    }

    [Test]
    public void Test1()
    {
        TryParse("Hello,John!", "John", null);
        Assert.Pass();
    }

    [Test]
    public void Test2()
    {
        TryParse("   Hello    ,    John        !              ", "John", new Whitespace());
        Assert.Pass();
    }

    [Test]
    public void Test3()
    {
        TryParse("Hello, /* it is a comment */ John!", "John", new Whitespace(Whitespace.Skip.WhiteChars | Whitespace.Skip.CStyleComment));
        Assert.Pass();
    }

    public void TestDelimitedList(string v, string t)
    {
        var ctx = new Context(v);
        var dst = new List<Number>();
        ctx.DelimitedList(dst, ",");
        var lst = new List<String>();
        foreach (var n in dst) {
            lst.Add(n.Val);
        }
        Assert.That(String.Join(",", lst), Is.EqualTo(t));
    }

    [Test]
    public void Test4()
    {
        TestDelimitedList(" 1, 2, 3, 4", "1,2,3,4");
        TestDelimitedList("", "");
        TestDelimitedList(" 0", "0");

        Assert.Pass();
    }

}
