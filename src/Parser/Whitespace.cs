namespace Parser {

    /// <summary>
    /// Parser for whitespace characters and comments. It is possible to add your own comment functions to skip
    /// different kind of not-standard comments.
    /// </summary>
    public class Whitespace : IPegParsable {
        /**
         * <summary>
         * Skip different kinds of data.
         * </summary>
         */
        [Flags]
        public enum Skip {
            None = 0,
            WhiteChars = 1,
            CStyleComment = 2,
            CppStyleComment = 4,
            ShellComment = 8,
            // TODO: maybe HtmlComment, PascalComment, BatComment, VimComment, ...
        };

        public Skip SkipClasses = Skip.WhiteChars;
        public Action<Context>? SkipFunc = null;

        public Whitespace()
        {
        }

        public Whitespace(Skip skip)
        {
            SkipClasses = skip;
        }

        public Whitespace(Skip skip, Action<Context> skipFunc)
        {
            SkipClasses = skip;
            SkipFunc = skipFunc;
        }

        public Whitespace(Action<Context> skipFunc)
        {
            SkipFunc = skipFunc;
        }

        public void PegParse(Context ctx)
        {
            while (ctx.Position < ctx.Source.Length && SkipStep(ctx)) {};
        }

        private bool SkipStep(Context ctx)
        {
            int initialPosition = ctx.Position;
            if (SkipFunc != null) {
                SkipFunc(ctx);
            }

            if ((SkipClasses & Skip.WhiteChars) != Skip.None) {
                while (ctx.Position < ctx.Source.Length && Char.IsWhiteSpace(ctx.Source[ctx.Position])) {
                    ++ctx.Position;
                }
            }

            if ((SkipClasses & Skip.CStyleComment) != Skip.None) {
                SkipCStyleComment(ctx);
            }

            if ((SkipClasses & Skip.CppStyleComment) != Skip.None) {
                SkipCppStyleComment(ctx);
            }

            if ((SkipClasses & Skip.ShellComment) != Skip.None) {
                SkipShellComment(ctx);
            }

            return ctx.Position != initialPosition;
        }

        public void SkipCStyleComment(Context ctx)
        {
            if (ctx.Position + 3 >= ctx.Source.Length) {
                return;
            }

            if (ctx.Source[ctx.Position] != '/' || ctx.Source[ctx.Position + 1] != '*') {
                return;
            }

            ctx.Position += 2;

            int state = 0;
            for (; ctx.Position < ctx.Source.Length; ++ctx.Position) {
                if (state == 0) { // inside the comment
                    if (ctx.Source[ctx.Position] == '*') {
                        state = 1;
                    }
                } else {
                    if (ctx.Source[ctx.Position] == '/') {
                        ++ctx.Position;
                        return;
                    }
                    if (ctx.Source[ctx.Position] != '*') {
                        state = 0;
                    }
                }
            }

            ctx.Error("Comment did not end");
        }

        public void SkipCppStyleComment(Context ctx)
        {
            if (ctx.Position + 1 >= ctx.Source.Length) {
                return;
            }

            if (ctx.Source[ctx.Position] != '/' || ctx.Source[ctx.Position + 1] != '/') {
                return;
            }

            ctx.Position += 2;

            for (; ctx.Position < ctx.Source.Length; ++ctx.Position) {
                if (ctx.Source[ctx.Position] == '\n') {
                    ++ctx.Position;
                    break;
                }
            }
        }

        public void SkipShellComment(Context ctx)
        {
            if (ctx.Position >= ctx.Source.Length) {
                return;
            }

            if (ctx.Source[ctx.Position] != '#') {
                return;
            }

            ctx.Position += 1;

            for (; ctx.Position < ctx.Source.Length; ++ctx.Position) {
                if (ctx.Source[ctx.Position] == '\n') {
                    ++ctx.Position;
                    break;
                }
            }
        }

        public override string ToString()
        {
            return $"Whitespace[{SkipClasses}]";
        }
    }

}
