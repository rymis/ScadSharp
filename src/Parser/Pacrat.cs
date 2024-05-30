namespace Parser;

public class Pacrat: IPacrat
{
    struct Key
    {
        public int Position;
        public System.Type Type;
    };

    struct Value
    {
        public bool Parsing;
        public int Position;
        public object? Parser;
        public Exception? Exception;
    };

    private Dictionary<Key, Value> _data = new Dictionary<Key, Value>();

    /// <summary>
    /// Check value of parser type at position. If found funcion fills parser with correct fields and properties values.
    /// If there was an parsing error function throws the exception.
    /// Function detects recursion and throws LeftRecursionException if recursion is detected.
    /// </summary>
    /// <returns>Final position if found and -1 otherwise. If function returns false user has to set the final cache value.</returns>
    public int Check(int pos, object parser)
    {
        Key key;
        key.Position = pos;
        key.Type = parser.GetType();
        if (_data.ContainsKey(key)) {
            Value cache = _data[key];
            if (cache.Parsing) {
                throw new LeftRecursionException(parser);
            }

            if (cache.Exception != null) {
                throw cache.Exception;
            }

            // Now we can copy all fields:
            foreach (var f in key.Type.GetFields()) {
                f.SetValue(parser, f.GetValue(cache.Parser));
            }

            // and properties:
            foreach (var p in key.Type.GetProperties()) {
                if (p.CanWrite && p.CanRead) {
                    p.SetValue(parser, p.GetValue(cache.Parser));
                }
            }

            return cache.Position;
        }

        Value val;
        val.Parsing = true;
        val.Parser = null;
        val.Exception = null;
        val.Position = -1;

        _data[key] = val;

        return -1;
    }

    /// <summary>
    /// Set parsed value for position.
    /// </summary>
    public void SetValue(int pos, object parser, int newPosition)
    {
        Key key;
        key.Position = pos;
        key.Type = parser.GetType();

        Value val;
        val.Parsing = false;
        val.Parser = parser;
        val.Exception = null;
        val.Position = newPosition;

        _data[key] = val;
    }

    /// <summary>
    /// Set parsing error for position and parser type.
    /// </summary>
    public void SetException(int pos, object parser, Exception exc)
    {
        Key key;
        key.Position = pos;
        key.Type = parser.GetType();

        Value val;
        val.Parsing = false;
        val.Parser = null;
        val.Exception = exc;
        val.Position = pos;

        _data[key] = val;
    }
};
