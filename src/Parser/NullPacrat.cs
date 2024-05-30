namespace Parser;

public class NullPacrat : IPacrat
{
    /// <summary>
    /// Check value of parser type at position. If found funcion fills parser with correct fields and properties values.
    /// If there was an parsing error function throws the exception.
    /// Function detects recursion and throws LeftRecursionException if recursion is detected.
    /// </summary>
    /// <returns>Final position if found and -1 otherwise. If function returns false user has to set the final cache value.</returns>
    public int Check(int pos, object parser)
    {
        return -1;
    }

    /// <summary>
    /// Set parsed value for position.
    /// </summary>
    public void SetValue(int pos, object parser, int newPosition)
    {
    }

    /// <summary>
    /// Set parsing error for position and parser type.
    /// </summary>
    public void SetException(int pos, object parser, Exception exc)
    {
    }
};
