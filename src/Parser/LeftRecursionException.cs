namespace Parser;

public class LeftRecursionException : Exception
{
    public LeftRecursionException(object obj) : base($"Left recursion detected when parsing {obj.GetType().Name}")
    {
    }
};
