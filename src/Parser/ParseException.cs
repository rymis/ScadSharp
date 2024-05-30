namespace Parser {

    public class ParseException : Exception {
        public string SourceString;
        public int Position;
        public string ErrorMessage;
        public List<ParseException>? Children = null;

        public ParseException(string source, int position, string message)
        {
            SourceString = source;
            Position = position;
            ErrorMessage = message;
        }

        (int, int) LineColumn()
        {
            return SourceLocation.LineColumn(SourceString, Position);
        }

        public int LineNo()
        {
            var res = LineColumn();
            return res.Item1;
        }

        public string ParseError(int offset = 0)
        {
            var lc = LineColumn();
            var lp = SourceLocation.MarkPosition(SourceString, Position);

            string off = "";
            while (off.Length < offset) {
                off += ' ';
            }
            string msg = $"{off}Error: {ErrorMessage} at line {lc.Item1} column {lc.Item2}\n{lp.Line}\n{lp.Pointer}";

            if (Children != null) {
                foreach (var exc in Children) {
                    msg += $"\n{off}Possible option:\n{exc.ParseError(offset + 1)}";
                }
            }

            return msg;
        }

        public void AddChild(ParseException exc, bool forceAdd = false)
        {
            if (Children == null) {
                Children = new List<ParseException>();
                Children.Add(exc);
                return;
            }

            if (!forceAdd) {
                if (exc.Position < Children[0].Position) {
                    return;
                }

                if (exc.Position > Children[0].Position) {
                    Children.Clear();
                }
            }

            Children.Add(exc);
        }
    };

} // namespace Scad.Peg
