namespace Parser;

/// <summary>
/// Fast word parsing for PEG
/// </summary>
public class Word
{
    private HashSet<char> _first;
    private HashSet<char> _rest;
    private int _maxCount = -1;
    private string _str = "";

    public const string LowerAlphas = "abcdefghijklmnopqrstuvwxyz";
    public const string UpperAlphas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Nums = "0123456789";
    public const string Alphas = LowerAlphas + UpperAlphas;
    public const string AlphaNums = Alphas + Nums;
    public const string IdentStart = Alphas + "_";
    public const string IdentChars = AlphaNums + "_";

    /// <summary>
    /// Create word parser with first character from first and other characters from rest.
    /// </summary>
    /// <param name="first">Set of characters for the first char</param>
    /// <param name="rest">Set of characters for rest chars</param>
    /// <param name="maxCount">Maximum number of characters in the word. If negative unlimited.</param>
    public Word(string first, string rest, int maxCount = -1)
    {
        _first = StrToSet(first);
        _rest = StrToSet(rest);
        _maxCount = maxCount;
        PrepareToString();
    }

    /// <summary>
    /// Create word parser with first character from first and other characters from rest.
    /// </summary>
    /// <param name="chars">Set of characters for all chars</param>
    /// <param name="maxCount">Maximum number of characters in the word. If negative unlimited.</param>
    public Word(string chars, int maxCount = -1)
    {
        _first = StrToSet(chars);
        _rest = StrToSet(chars);
        _maxCount = maxCount;
        PrepareToString();
    }

    /// <summary>
    /// Try to parse Word at position pos.
    /// </summary>
    /// <returns>Length of parsed string or -1 if failed.</returns>
    public int ParseAt(string source, int pos)
    {
        if (pos >= source.Length) {
            return -1;
        }

        if (!_first.Contains(source[pos])) {
            return -1;
        }

        int l = 1;
        for (; pos + l < source.Length && (_maxCount < 0 || l < _maxCount); ++l) {
            if (!_rest.Contains(source[pos + l])) {
                return l;
            }
        }

        return l;
    }

    private HashSet<char> StrToSet(string s)
    {
        HashSet<char> res = new();

        foreach (char c in s) {
            res.Add(c);
        }

        return res;
    }

    private string SetToStr(HashSet<char> chars)
    {
        List<char> set = new();
        set.AddRange(chars);

        if (set.Count == 0) {
            return "[]";
        }

        set.Sort();

        var res = new System.Text.StringBuilder();
        res.Append('[');
        char first = set[0];
        char last = first;
        for (int i = 1; i < set.Count; ++i) {
            if (set[i] != last + 1) {
                res.Append(first);
                if ((int)last - (int)first >= 2) {
                    res.Append('-');
                }
                if (last != first) {
                    res.Append(last);
                }
                first = set[i];
                last = set[i];
            } else if (i + 1 == set.Count) {
                res.Append(first);
                if ((int)set[i] - (int)first >= 2) {
                    res.Append('-');
                }
                res.Append(set[i]);
            } else {
                last = set[i];
            }
        }
        res.Append(']');

        return res.ToString();
    }

    private void PrepareToString()
    {
        if (_maxCount > 0) {
            _str = SetToStr(_first) + SetToStr(_rest) + "{0," + _maxCount.ToString() + "}";
        } else {
            _str = SetToStr(_first) + SetToStr(_rest) + "*";
        }
    }

    public override string ToString()
    {
        return _str;
    }
}

