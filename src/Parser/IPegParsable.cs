namespace Parser {

    /// <summary>
    ///     Parser interface. PegParse method implements real parser.
    /// </summary>
    public interface IPegParsable {
        public void PegParse(Context ctx);
    };

}
