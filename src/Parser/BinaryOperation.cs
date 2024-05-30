namespace Parser {

    /**
    <summary>
    BinaryOperation represents expression in the form:
      Expr ::= Value | Expr Operation Value
    This form can not be parsed correctly using simple PEG grammar but this form is very
    useful for arithmetic expressions parsing, so I decided to include this wrapper here.

    Actually this class works simple. It parses grammar in form:
      Expr ::= Value | Value Operation Expr
    and then reverts the order of operations.
    </summary>
    */
    public class BinaryOperation<Val, Op> : IPegParsable where Val: IPegParsable, new() where Op: IPegParsable, new()
    {
        public BinaryOperation<Val, Op>? Expression;
        public Op Operation = new Op();
        public Val Value = new Val();

        class OpVal : IPegParsable
        {
            public Op Operation = new Op();
            public Val Value = new Val();

            public void PegParse(Context ctx)
            {
                ctx.Parse(Operation);
                ctx.Parse(Value);
            }
        };

        public void PegParse(Context ctx)
        {
            var tail = new List<OpVal>();

            ctx.Parse(Value);
            ctx.ZeroOrMore(tail);

            if (tail.Count == 0) {
                return;
            }

            Expression = new BinaryOperation<Val, Op>();
            Expression.Value = Value;
            Operation = tail[0].Operation;
            Value = tail[0].Value;

            // Now extend expression until it is possible
            for (int i = 1; i < tail.Count; ++i) {
                BinaryOperation<Val, Op> expr = new BinaryOperation<Val, Op>();
                expr.Expression = Expression;
                expr.Operation = Operation;
                expr.Value = Value;
                Expression = expr;
                Operation = tail[i].Operation;
                Value = tail[i].Value;
            }
        }

    };

}
