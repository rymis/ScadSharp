namespace Parser.Test;

class FirstOfParser : IPegParsable {
    public string Val = "";
    public int Choice = -1;

    public void PegParse(Context ctx)
    {
        ctx.FirstOf(out Choice, ctx.Func((c) => c.Literal("one")), ctx.Func((c) => c.Literal("two")), ctx.Func((c) => c.Regex("[a-z]+", out Val)));
    }

};

public class FirstOfTest
{
    [SetUp]
    public void Setup()
    {
    }

    public void TryParse(string src, int choice)
    {
        var ctx = new Context(src);
        var err = "";
        var p = new FirstOfParser();
        try {
            Assert.That(ctx.Parse(p), Is.EqualTo(true));
        } catch (ParseException exc) {
            err = exc.ParseError();
        }

        Assert.That(err, Is.EqualTo(""));
        Assert.That(p.Choice, Is.EqualTo(choice));
        if (choice == 2) {
            Assert.That(p.Val, Is.EqualTo(src));
        }
    }

    [Test]
    public void Test()
    {
        TryParse("one", 0);
        TryParse("two", 1);
        TryParse("three", 2);

        Assert.Pass();
    }
}
