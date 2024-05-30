namespace Parser;

/// <summary>
/// Parse context used to parse data.
/// </summary>
public class Context
{
    // Cache for skipped whitespaces
    private Dictionary<int, int> _wsCache = new();

    // Pacrat cache. By default it is NullCache
    private IPacrat _pacrat = new NullPacrat();

    /// <summary>
    /// Source string that is parsed now.
    /// </summary>
    public string Source;
    /// <summary>
    /// Position in the source string.
    /// </summary>
    public int Position = 0;
    /// <summary>
    /// Whitespace parser. It can be set in constructor or you can use any IPegParsable to parse it.
    /// </summary>
    public IPegParsable? WS = null;

    /// <summary>
    /// Create context to parse source string.
    /// </summary>
    /// <param name="source">Source string to parse</param>
    public Context(string source)
    {
        Source = source;
        WS = new Whitespace();
    }

    /// <summary>
    /// Create context to parse source string with whitespace specification.
    /// </summary>
    /// <param name="source">Source string to parse</param>
    /// <param name="skip">Whitespace classes</param>
    public Context(string source, Whitespace.Skip skip)
    {
        Source = source;
        if (skip != Whitespace.Skip.None) {
            WS = new Whitespace(skip);
        }
    }

    /// <summary>
    /// Parse source string using parser. This method can be used in grammar to apply subparsers.
    /// </summary>
    /// <param name="parser">Parser that should be applied</param>
    /// <returns>boolean value indicating that full source string was parsed</returns>
    public bool Parse(IPegParsable parser)
    {
        ParseOne(parser);
        SkipWS();
        return Position == Source.Length;
    }

    private void ParseOne(IPegParsable parser)
    {
        int cachedPos = _pacrat.Check(Position, parser);
        if (cachedPos >= 0) {
            Position = cachedPos;
            return;
        }

        int savePos = Position;

        try {
            SkipWS();

            /*
            var src = Source.Substring(Position);
            if (src.Length > 20) {
                src = src.Substring(0, 20);
            }
            Console.WriteLine($"PARSE {parser.GetType().Name}:{Position}:{src}");
            */

            parser.PegParse(this);
            _pacrat.SetValue(savePos, parser, Position);
            // TODO: Maybe pakrat?
        } catch (ParseException exc) {
            exc.ErrorMessage = $"When parsing {parser.GetType().Name}: {exc.ErrorMessage}";
            Position = savePos;
            _pacrat.SetException(Position, parser, exc);
            throw;
        }
    }

    /// <summary>
    /// Skip whitespace characters in source string
    /// </summary>
    public void SkipWS()
    {
        if (WS != null) {
            if (_wsCache.ContainsKey(Position)) {
                Position = _wsCache[Position];
                return;
            }

            IPegParsable saveWS = WS;
            try {
                WS = null;
                int savePos = Position;

                try {
                    saveWS.PegParse(this);
                } catch (ParseException) {
                    Position = savePos;
                }

                _wsCache[savePos] = Position;
            } finally {
                WS = saveWS;
            }
        }
    }

    /// <summary>
    /// Function parses content using parser, but returns string from beginning position to end position.
    /// This function is a faster alternative for Regex.
    /// </summary>
    /// <param name="parser">Parser to apply</param>
    /// <param name="overrideSkip">Override skip whitespace. It is typical to use Skip.None here</param>
    /// <returns>String representing all the parsed input</returns>
    public string Combine(IPegParsable parser, Whitespace.Skip? overrideSkip = null)
    {
        var ctx = new Context(Source);
        int savePos = Position;

        if (overrideSkip != null) {
            ctx.WS = new Whitespace((Whitespace.Skip)overrideSkip);
        }

        try {
            SkipWS();

            int startPosition = Position;
            ctx.Position = Position;
            ctx.ParseOne(parser);
            Position = ctx.Position;

            return Source.Substring(startPosition, Position - startPosition);
        } catch (ParseException) {
            Position = savePos;
            throw;
        }
    }

    /// <summary>
    /// Function parses content using parser, but returns string from beginning position to end position.
    /// This function is a faster alternative for Regex.
    /// </summary>
    /// <param name="overrideSkip">Override skip whitespace. It is typical to use Skip.None here</param>
    /// <returns>String representing all the parsed input</returns>
    public string Combine<T>(Whitespace.Skip? overrideSkip = null) where T : IPegParsable, new()
    {
        T parser = new();
        return Combine(parser, overrideSkip);
    }

    /// <summary>
    /// Throw exception at current position.
    /// </summary>
    /// <param name="msg">Error message to include into error. Typical message is "Expected something"</param>
    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public void Error(string msg)
    {
        throw new ParseException(Source, Position, msg);
    }

    /// <summary>
    /// Check that it is not end of the string.
    /// </summary>
    public void NotEOF()
    {
        if (Position >= Source.Length) {
            Error("Unexpected end of file");
        }
    }

    /// <summary>
    /// Check that it is the end of the string.
    /// </summary>
    public void EOF()
    {
        if (Position < Source.Length) {
            Error("Not all context was parsed");
        }
    }

    /// <summary>
    /// Parse literal. That means parser sees this literal at current position and eats it.
    /// </summary>
    /// <param name="literal">Literal to parse</param>
    public void Literal(string literal)
    {
        SkipWS();
        NotEOF();

        if (Source[Position] != literal[0] || Position + literal.Length > Source.Length) {
            Error($"Expected `{literal}'");
        }

        if (Source.Substring(Position, literal.Length) != literal) {
            Error($"Expected `{literal}'");
        }

        Position += literal.Length;
    }

    /// <summary>
    /// Parse some of literals. This is the same as writing FirstOf with list of literals, but shorter.
    /// </summary>
    /// <param name="val">Variable to save the parsed literal</param>
    /// <param name="literals">List of literals to be parsed</param>
    public void Literals(out string val, params string[] literals)
    {
        SkipWS();
        NotEOF();

        foreach (var literal in literals) {
            if (Source[Position] != literal[0] || Position + literal.Length > Source.Length) {
                continue;
            }

            if (Source.Substring(Position, literal.Length) != literal) {
                continue;
            }

            val = literal;
            Position += literal.Length;

            return;
        }

        Error($"Waiting for one of {string.Join("/", literals)}");

        // Not reached code
        val = "";
    }

    /// <summary>
    /// Parse keyword. Keyword is like literal but after the word we should see non-character symbol.
    /// </summary>
    /// <param name="keyword">Keyword to parse</param>
    public void Keyword(string keyword)
    {
        SkipWS();
        NotEOF();

        if (Source[Position] != keyword[0] || Position + keyword.Length > Source.Length) {
            Error($"Expected keyword `{keyword}'");
        }

        if (Source.Substring(Position, keyword.Length) != keyword) {
            Error($"Expected keyword `{keyword}'");
        }

        int npos = Position + keyword.Length;
        if (npos < Source.Length) {
            if (Char.IsLetterOrDigit(Source[npos]) || Source[npos] == '_') {
                Error($"Expected keyword `{keyword}'");
            }
        }

        Position = npos;
    }

    /// <summary>
    /// Parse word defined by word object
    /// </summary>
    /// <param name="word">Word to be parsed</param>
    /// <param name="dst">Output string to put parsed substring into</param>
    public void Word(Word word, out string dst)
    {
        SkipWS();
        int l = word.ParseAt(Source, Position);

        if (l < 0) {
            Error($"Expected {word.ToString()}");
        }

        dst = Source.Substring(Position, l);
        Position += l;
    }

    /// <summary>
    /// Parse string based on regular expression.
    /// </summary>
    /// <param name="regex">Regular expression to be parsed</param>
    /// <param name="dst">Output string to put parsed substring into</param>
    public void Regex(System.Text.RegularExpressions.Regex regex, out string dst)
    {
        SkipWS();

        var match = regex.Match(Source, Position);
        if (!match.Success || match.Index > Position) {
            Error($"Waiting for {regex}");
        }
  
        dst = Source.Substring(match.Index, match.Length);
  
        Position = match.Index + match.Length;
    }

    /// <summary>
    /// Parse string based on regular expression.
    /// </summary>
    /// <param name="regex">Regular expression to be parsed</param>
    public void Regex(System.Text.RegularExpressions.Regex regex)
    {
        Regex(regex, out string dummy);
    }

    /// <summary>
    /// Parse string based on regular expression. It is not recomended to use this function, it is better to compile regular expression before usage.
    /// </summary>
    /// <param name="regex">Regular expression to be parsed</param>
    /// <param name="dst">Output string to put parsed substring into</param>
    public void Regex(string regex, out string dst)
    {
        var rx = new System.Text.RegularExpressions.Regex(regex);
        Regex(rx, out dst);
    }

    /// <summary>
    /// Parse string based on regular expression. It is not recomended to use this function, it is better to compile regular expression before usage.
    /// </summary>
    /// <param name="regex">Regular expression to be parsed</param>
    public void Regex(string regex)
    {
        var rx = new System.Text.RegularExpressions.Regex(regex);
        Regex(rx, out string dummy);
    }

    /*
     * Here I use C++ grammar:
     * integer ::= [+-]? [0-9]+
     * number  ::= integer ([Ee] integer)? | [+-]? [0-9]* "." [0-9]+ ([Ee] integer)?
     * This grammar was converted into following PEG:
     * number <- [+-]? ([0-9]* ".")? [0-9]+ ([eE] [+-]? [0-9]+)?
     */
    class FloatingPoint : IPegParsable
    {
        private static Word _digits = new Word(Parser.Word.Nums);
        private static Word _sign = new Word("+-", 1);
        private static Word _exp = new Word("eE", 1);

        private void ParseSign(Context context)
        {
            context.OptionalFunc((c) => c.Word(_sign, out string s));
        }

        public void PegParse(Context context)
        {
            // [+-]?
            ParseSign(context);

            // ([0-9]* ".")?
            context.OptionalFunc((c) => {
                    c.OptionalFunc((c) => c.Word(_digits, out string s));
                    c.Literal(".");
            });

            // [0-9]+
            context.Word(_digits, out string s);

            // ([eE] [+-]? [0-9]+)?
            context.OptionalFunc((c) => {
                    context.Word(_exp, out string s2);
                    ParseSign(context);
                    context.Word(_digits, out string s3);
            });
        }
    }
    // static private System.Text.RegularExpressions.Regex _doubleRegexp = new System.Text.RegularExpressions.Regex(@"[-+]?[0-9]+([eE][-+]?[0-9]+)?|[-+]?[0-9]+([eE][-+]?[0-9]+)?");
    static private System.Text.RegularExpressions.Regex _doubleRegexp = new System.Text.RegularExpressions.Regex(@"[-+]?([0-9]*[.][0-9]+|[0-9]+)([eE][-+]?[0-9]+)?");

    /// <summary>
    /// Parse floating point number and return double precision value.
    /// </summary>
    /// <param name="dst">Variable to write value into</param>
    public void Double(out double dst)
    {
        string val = "";
        int savePos = Position;
        try {
            val = Combine<FloatingPoint>(Whitespace.Skip.None);
        } catch (ParseException) {
            Error("Expected floating point number");
        }

        if (!System.Double.TryParse(val, out dst)) {
            Position = savePos;
            Error("Expected floating point number (parse error)");
        }
    }

    /// <summary>
    /// Parse floating point number and return double precision value.
    /// </summary>
    /// <returns>Parsed value</param>
    public double Double()
    {
        this.Double(out double res);
        return res;
    }

    /// <summary>
    /// Try all parsers and return the first successful result.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    public void FirstOf(out int index, params IPegParsable[] parsables)
    {
        ParseException err = new ParseException(Source, Position, "One of following options required");

        for (int i = 0; i < parsables.Count(); ++i) {
            try {
                ParseOne(parsables[i]);
                index = i;
                return;
            } catch (ParseException exc) {
                err.AddChild(exc);
            }
        }

        index = -1;
        throw err;
    }

    /// <summary>
    /// Try all parsers and return the first successful result.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    /// <param name="parsables">list of functions returning each of possible options</param>
    public IPegParsable FirstOf(out int index, params Func<IPegParsable>[] parsables)
    {
        ParseException err = new ParseException(Source, Position, "One of following options required");

        for (int i = 0; i < parsables.Count(); ++i) {
            try {
                IPegParsable parser = parsables[i]();
                ParseOne(parser);
                index = i;
                return parser;
            } catch (ParseException exc) {
                err.AddChild(exc);
            }
        }

        index = -1;
        throw err;
    }

    /// <summary>
    /// Try all parsers and return the first successful result. This function allows to not create all the possible options to parse them.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    public IPegParsable Union<T1, T2>(out int index)
        where T1: IPegParsable, new()
        where T2: IPegParsable, new()
    {
        return FirstOf(out index,
                () => new T1(),
                () => new T2());
    }

    /// <summary>
    /// Try all parsers and return the first successful result. This function allows to not create all the possible options to parse them.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    public IPegParsable Union<T1, T2, T3>(out int index)
        where T1: IPegParsable, new()
        where T2: IPegParsable, new()
        where T3: IPegParsable, new()
    {
        return FirstOf(out index,
                () => new T1(),
                () => new T2(),
                () => new T3());
    }

    /// <summary>
    /// Try all parsers and return the first successful result. This function allows to not create all the possible options to parse them.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    public IPegParsable Union<T1, T2, T3, T4>(out int index)
        where T1: IPegParsable, new()
        where T2: IPegParsable, new()
        where T3: IPegParsable, new()
        where T4: IPegParsable, new()
    {
        return FirstOf(out index,
                () => new T1(),
                () => new T2(),
                () => new T3(),
                () => new T4());
    }

    /// <summary>
    /// Try all parsers and return the first successful result. This function allows to not create all the possible options to parse them.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    public IPegParsable Union<T1, T2, T3, T4, T5>(out int index)
        where T1: IPegParsable, new()
        where T2: IPegParsable, new()
        where T3: IPegParsable, new()
        where T4: IPegParsable, new()
        where T5: IPegParsable, new()
    {
        return FirstOf(out index,
                () => new T1(),
                () => new T2(),
                () => new T3(),
                () => new T4(),
                () => new T5());
    }

    /// <summary>
    /// Try all parsers and return the first successful result. This function allows to not create all the possible options to parse them.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    public IPegParsable Union<T1, T2, T3, T4, T5, T6>(out int index)
        where T1: IPegParsable, new()
        where T2: IPegParsable, new()
        where T3: IPegParsable, new()
        where T4: IPegParsable, new()
        where T5: IPegParsable, new()
        where T6: IPegParsable, new()
    {
        return FirstOf(out index,
                () => new T1(),
                () => new T2(),
                () => new T3(),
                () => new T4(),
                () => new T5(),
                () => new T6());
    }

    /// <summary>
    /// Try all parsers and return the first successful result. This function allows to not create all the possible options to parse them.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    public IPegParsable Union<T1, T2, T3, T4, T5, T6, T7>(out int index)
        where T1: IPegParsable, new()
        where T2: IPegParsable, new()
        where T3: IPegParsable, new()
        where T4: IPegParsable, new()
        where T5: IPegParsable, new()
        where T6: IPegParsable, new()
        where T7: IPegParsable, new()
    {
        return FirstOf(out index,
                () => new T1(),
                () => new T2(),
                () => new T3(),
                () => new T4(),
                () => new T5(),
                () => new T6(),
                () => new T7());
    }

    /// <summary>
    /// Try all parsers and return the first successful result. This function allows to not create all the possible options to parse them.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    public IPegParsable Union<T1, T2, T3, T4, T5, T6, T7, T8>(out int index)
        where T1: IPegParsable, new()
        where T2: IPegParsable, new()
        where T3: IPegParsable, new()
        where T4: IPegParsable, new()
        where T5: IPegParsable, new()
        where T6: IPegParsable, new()
        where T7: IPegParsable, new()
        where T8: IPegParsable, new()
    {
        return FirstOf(out index,
                () => new T1(),
                () => new T2(),
                () => new T3(),
                () => new T4(),
                () => new T5(),
                () => new T6(),
                () => new T7(),
                () => new T8());
    }

    /// <summary>
    /// Try all parsers and return the first successful result. This function allows to not create all the possible options to parse them.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    public IPegParsable Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>(out int index)
        where T1: IPegParsable, new()
        where T2: IPegParsable, new()
        where T3: IPegParsable, new()
        where T4: IPegParsable, new()
        where T5: IPegParsable, new()
        where T6: IPegParsable, new()
        where T7: IPegParsable, new()
        where T8: IPegParsable, new()
        where T9: IPegParsable, new()
    {
        return FirstOf(out index,
                () => new T1(),
                () => new T2(),
                () => new T3(),
                () => new T4(),
                () => new T5(),
                () => new T6(),
                () => new T7(),
                () => new T8(),
                () => new T9());
    }

    /// <summary>
    /// Try all parsers and return the first successful result. This function allows to not create all the possible options to parse them.
    /// </summary>
    /// <param name="index">index of parsed element</param>
    public IPegParsable Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(out int index)
        where T1: IPegParsable, new()
        where T2: IPegParsable, new()
        where T3: IPegParsable, new()
        where T4: IPegParsable, new()
        where T5: IPegParsable, new()
        where T6: IPegParsable, new()
        where T7: IPegParsable, new()
        where T8: IPegParsable, new()
        where T9: IPegParsable, new()
        where T10: IPegParsable, new()
    {
        return FirstOf(out index,
                () => new T1(),
                () => new T2(),
                () => new T3(),
                () => new T4(),
                () => new T5(),
                () => new T6(),
                () => new T7(),
                () => new T8(),
                () => new T9(),
                () => new T10());
    }

    /// <summary>
    /// Parse optional value.
    /// </summary>
    /// <param name="parser">Parser that should be applied</param>
    public void Optional(IPegParsable parser)
    {
        try {
            ParseOne(parser);
        } catch (ParseException) {
        }
    }

    /// <summary>
    /// Parse optional value. If parser fails returns null.
    /// </summary>
    /// <returns>Parsed value if successful and null if failed</returns>
    public T? Optional<T>() where T: class, IPegParsable, new()
    {
        try {
            T res = new T();
            ParseOne(res);

            return res;
        } catch (ParseException) {
        }

        return null;
    }

    /// <summary>
    /// Parse optional literal at position.
    /// </summary>
    /// <param name="literal">Expected input at position</param>
    /// <returns>boolean indicating that literal was found at position</returns>
    public bool OptionalLiteral(string literal)
    {
        try {
            Literal(literal);
            return true;
        } catch (ParseException) {
            return false;
        }
    }

    /// <summary>
    /// Parse optional using function at position.
    /// This is a shortcut of using context.Optional(context.Func(func)), but faster.
    /// </summary>
    /// <param name="func">Function to use</param>
    /// <returns>boolean indicating that parse succeed</returns>
    public bool OptionalFunc(Action<Context> func)
    {
        int savePos = Position;
        try {
            func(this);
            return true;
        } catch (ParseException) {
            Position = savePos;
            return false;
        }
    }

    /// <summary>
    /// Parse value defined by parser. This function is a recomended way to parse something inside parsers.
    /// </summary>
    /// <param name="parser">Parser that should be applied</param>
    public void One(IPegParsable parser)
    {
        ParseOne(parser);
    }

    /// <summary>
    /// Parse value defined by parser. This function is a recomended way to parse something inside parsers.
    /// </summary>
    /// <returns>Parsed value</returns>
    public T One<T>() where T: IPegParsable, new()
    {
        T parser = new T();
        ParseOne(parser);
        return parser;
    }

    /// <summary>
    /// Check that type T is parsable on the position
    /// </summary>
    /// <param name="parser">Parser that should be applied</param>
    public void FollowedBy(IPegParsable parser)
    {
        int savePos = Position;
        try {
            ParseOne(parser);
        } finally {
            Position = savePos;
        }
    }

    /// <summary>
    /// Check that type T is parsable on the position
    /// </summary>
    public void FollowedBy<T>() where T: IPegParsable, new()
    {
        FollowedBy(new T());
    }

    /// <summary>
    /// Check that type T is not parsable on the position
    /// </summary>
    /// <param name="parser">Parser that should be applied</param>
    public void NotAny(IPegParsable parser)
    {
        int savePos = Position;
        try {
            ParseOne(parser);
        } catch (ParseException) {
            return;
        } finally {
            Position = savePos;
        }

        Error("Unexpected input");
    }

    /// <summary>
    /// Check that type T is not parsable on the position
    /// </summary>
    public void NotAny<T>() where T: IPegParsable, new()
    {
        NotAny(new T());
    }

    /// <summary>
    /// Apply parser several times.
    /// </summary>
    /// <param name="dst">Destination to write values into</param>
    /// <param name="minCount">Minimal number of appears</param>
    /// <param name="maxCount">Maximum number of appears</param>
    public void Repeated<T>(List<T> dst, int minCount = 0, int maxCount = -1) where T: IPegParsable, new()
    {
        dst.Clear();

        for (;;) {
            if (maxCount >= 0 && dst.Count >= maxCount) {
                break;
            }

            try {
                dst.Add(One<T>());
            } catch (ParseException) {
                if (dst.Count < minCount) {
                    throw;
                }

                break;
            }
        }
    }

    /// <summary>
    /// Apply parser zero or more times.
    /// </summary>
    /// <param name="dst">Destination to write values into</param>
    public void ZeroOrMore<T>(List<T> dst) where T: IPegParsable, new()
    {
        Repeated(dst);
    }

    /// <summary>
    /// Apply parser one or more times.
    /// </summary>
    /// <param name="dst">Destination to write values into</param>
    public void OneOrMore<T>(List<T> dst) where T: IPegParsable, new()
    {
        Repeated(dst, 1);
    }

    /// <summary>
    /// Parse list of elements divided by separator.
    /// </summary>
    /// <param name="dst">Destination to write values into</param>
    /// <param name="separator">Value separator</param>
    public void DelimitedList<T>(List<T> dst, IPegParsable separator) where T: IPegParsable, new()
    {
        dst.Clear();

        // Try to parse first element:
        try {
            T firstItem = new T();
            ParseOne(firstItem);
            dst.Add(firstItem);
        } catch (ParseException) {
            return;
        }

        // The rest is delimited by separator
        for (;;) {
            T item = new T();
            int pos = Position;
            try {
                ParseOne(separator);
                ParseOne(item);
                dst.Add(item);
            } catch (ParseException) {
                Position = pos;
                break;
            }
        }
    }

    /// <summary>
    /// Parse list of elements divided by separator.
    /// </summary>
    /// <param name="dst">Destination to write values into</param>
    /// <param name="separator">Value separator. This separator will be used as Literal.</param>
    public void DelimitedList<T>(List<T> dst, string separator) where T: IPegParsable, new()
    {
        DelimitedList(dst, Func((c) => c.Literal(separator)));
    }

    class FuncParser : IPegParsable {
        private Action<Context> _action;

        public FuncParser(Action<Context> action)
        {
            _action = action;
        }

        public void PegParse(Context ctx)
        {
            _action(ctx);
        }
    };

    /// <summary>
    /// Simple way to create new parser based on lambda function
    /// </summary>
    /// <param name="action">Actions to be done to parse the value</param>
    /// <returns>Parser that parses something using the action</returns>
    public IPegParsable Func(Action<Context> action)
    {
        return new FuncParser(action);
    }

    /// <summary>
    /// Expression representation to parse infix expressions
    /// </summary>
    public class ExpressionNode<T> where T: class, IPegParsable, new()
    {
        public string Operation = "";
        public ExpressionNode<T>? Left = null;
        public T? Right = null;

        public T Value {
            get {
                if (Right == null) {
                    throw new NullReferenceException();
                }
                return Right;
            }
            set {
                Right = value;
                Left = null;
            }
        }

        public bool IsValue()
        {
            return Left == null;
        }
    };

    public ExpressionNode<T> Infix<T>(params string[] operations) where T: class, IPegParsable, new()
    {
        var vals = new List<T>();
        var ops = new List<string>();
        vals.Add(One<T>());

        for (;;) {
            T item = new T();
            int pos = Position;
            try {
                Literals(out string op, operations);
                vals.Add(One<T>());
                ops.Add(op);
            } catch (ParseException) {
                Position = pos;
                break;
            }
        }

        // Now we can fold them:
        var res = new ExpressionNode<T>();
        res.Value = vals[0];

        if (ops.Count == 0) {
            return res;
        }

        for (int i = 0; i < ops.Count; ++i) {
            var e = new ExpressionNode<T>();
            e.Left = res;
            e.Operation = ops[i];
            e.Right = vals[i + 1];
            res = e;
        }

        return res;
    }
}

