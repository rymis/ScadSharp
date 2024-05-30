namespace Parser;

public class SourceLocation
{
    /// <summary>
    /// Get pair of line and column of position in src
    /// </summary>
    public static (int Line, int Column) LineColumn(string src, int pos)
    {
        int line = 1;
        int column = 1;
        for (int i = 0; i < pos && i < src.Length; ++i) {
            ++column;
            if (src[i] == '\n') {
                ++line;
                column = 1;
            }
        }

        return (line, column);
    }

    /// <summary>
    /// Get source line and position marker for specific position.
    /// </summary>
    public static (string Line, string Pointer) MarkPosition(string src, int position)
    {
        int pos = position;
        string line = "";
        string pointer = "";
        if (pos == src.Length) {
            --pos;
        }
        for (; pos >= 0; --pos) {
            if (src[pos] == '\n') {
                break;
            }
        }
        if (pos < 0) pos = 0;
        if (pos < src.Length && src[pos] == '\n') {
            ++pos;
        }

        for (; pos < src.Length; ++pos) {
            if (src[pos] == '\n') {
                break;
            }

            if (pos == position) {
                pointer += "^--- here";
            } else if (pos < position) {
                pointer += ' ';
            }
            line += src[pos];
        }

        return (line, pointer);
    }
}
