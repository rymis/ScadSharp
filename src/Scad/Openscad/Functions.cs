namespace Scad.Openscad;

/// <summary>
/// Standard functions implementation for OpenScad
/// </summary>
public class Functions {
    const float RadianDegrees = 57.2957795130823f;

    public class Concat : Value.Function
    {
        public override Value Call(ExecutionContext context, params Argument[] arguments)
        {
            var res = new Value.List();

            foreach (var arg in arguments) {
                if (arg.Val is Value.List) {
                    res.Val.AddRange(((Value.List)arg.Val).Val);
                } else {
                    res.Val.Add(arg.Val);
                }
            }

            return res;
        }
    }

    // lookup
    // str
    // chr
    // ord
    // search
    public class Version : Value.Function
    {
        public override Value Call(ExecutionContext context, params Argument[] arguments)
        {
            var res = new List<Value>();
            res.Add(new Value.Number(2019));
            res.Add(new Value.Number(05));
            res.Add(new Value.Number(01));

            return new Value.List(res);
        }
    }

    // version_num
    public class VersionNum : Value.Function
    {
        public override Value Call(ExecutionContext context, params Argument[] arguments)
        {
            return new Value.Number(20190501);
        }
    }

    // parent_module(idx)
    //
    public class SingleArgumentFunction : Value.Function
    {
        public virtual float Impl(float val)
        {
            return 0.0f;
        }

        public override Value Call(ExecutionContext context, params Argument[] arguments)
        {
            if (arguments.Count() != 1) {
                context.Log($"Invalid number of arguments for {this.GetType().Name}");
                return Value.Undef;
            }

            if (arguments[0].Val is Value.Number) {
                float x = (float)((Value.Number)arguments[0].Val).Val;
                try {
                    return new Value.Number(Impl(x));
                } catch (Exception e) {
                    context.Log($"Error in {this.GetType().Name}: {e.ToString()}");
                    return Value.Undef;
                }
            }

            context.Log($"Invalid argument type for {this.GetType().Name}");
            return Value.Undef;
        }
    }

    public class Abs : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Abs(val);
        }
    }

    public class Sign : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Sign(val);
        }
    }

    public class Sin : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Sin(val / RadianDegrees);
        }
    }

    public class Cos : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Cos(val / RadianDegrees);
        }
    }

    public class Tan : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Tan(val / RadianDegrees);
        }
    }

    public class Acos : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return RadianDegrees * MathF.Acos(val);
        }
    }

    public class Asin : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return RadianDegrees * MathF.Asin(val);
        }
    }

    public class Atan : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return RadianDegrees * MathF.Atan(val);
        }
    }

    // atan2

    public class Floor : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Floor(val);
        }
    }

    public class Round : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Round(val);
        }
    }

    public class Ceil : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Ceiling(val);
        }
    }

    public class Ln : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Log(val);
        }
    }

    public class Len : Value.Function
    {
        public override Value Call(ExecutionContext context, params Argument[] arguments)
        {
            if (arguments.Count() != 1) {
                context.Log($"ERROR: invalid number of arguments for Len: {ArgumentsToString(arguments)}");
                return Value.Undef;
            }

            int l = 0;
            if (arguments[0].Val is Value.List) {
                l = ((Value.List)arguments[0].Val).Val.Count;
            } else if (arguments[0].Val is Value.String) {
                l = ((Value.String)arguments[0].Val).Val.Length;
            } else {
                context.Log($"ERROR: invalid argument type for Len: {ArgumentsToString(arguments)}");
                return Value.Undef;
            }

            return new Value.Number((double)l);
        }
    }

    public class Log : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Log10(val);
        }
    }

    // pow
    public class Pow : Value.Function
    {
        public override Value Call(ExecutionContext context, params Argument[] arguments)
        {
            if (arguments.Count() != 2) {
                context.Log($"Invalid arguments for Pow: ${ArgumentsToString(arguments)}");
                return Value.Undef;
            }

            if (arguments[0].Val is not Value.Number || arguments[1].Val is not Value.Number) {
                context.Log($"Invalid arguments for Pow: ${ArgumentsToString(arguments)}");
                return Value.Undef;
            }

            double a = ((Value.Number)arguments[0].Val).Val;
            double p = ((Value.Number)arguments[1].Val).Val;

            return new Value.Number(Math.Pow(a, p));
        }

    }

    public class Sqrt : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Sqrt(val);
        }
    }

    public class Exp : SingleArgumentFunction
    {
        public override float Impl(float val)
        {
            return MathF.Exp(val);
        }
    }

    public class Rands : Value.Function
    {
        public override Value Call(ExecutionContext context, params Argument[] arguments)
        {
            var args = Modules.ParseArgs(context, "rands", arguments, "min_value", "max_value", "value_count", "seed");
            float minValue = 0.0f;
            float maxValue = 1.0f;
            int valueCount = 1;
            if (args["min_value"] is Value.Number) {
                minValue = (float)((Value.Number)args["min_value"]).Val;
            }
            if (args["max_value"] is Value.Number) {
                maxValue = (float)((Value.Number)args["max_value"]).Val;
            }
            if (args["valueCount"] is Value.Number) {
                maxValue = (int)((Value.Number)args["value_count"]).Val;
            }
            // TODO: seed
            // TODO: my signature is not the same as OpenScad one: I have all arguments optional
            var res = new Value.List();
            var rnd = new Random();

            for (int i = 0; i < valueCount; ++i) {
                res.Val.Add(new Value.Number(rnd.NextDouble() * (maxValue - minValue) + minValue));
            }

            return res;
        }
    }

    public static string ArgumentsToString(Value.Argument[] arguments)
    {
        var b = new System.Text.StringBuilder();
        for (int i = 0; i < arguments.Count(); ++i) {
            if (i != 0) {
                b.Append(", ");
            }

            if (arguments[i].Name != null && arguments[i].Name != "") {
                b.Append($"{arguments[i].Name}=");
            }

            b.Append(arguments[i].Val.ToString());
        }

        return b.ToString();
    }

    public class Min : Value.Function
    {
        public override Value Call(ExecutionContext context, params Argument[] arguments)
        {
            if (arguments.Count() == 0) {
                context.Log("ERROR: No arguments for Min");
                return Value.Undef;
            }

            if (arguments.Count() == 1) {
                if (arguments[0].Val is not Value.List) {
                    context.Log($"ERROR: Invalid arguments for Min {ArgumentsToString(arguments)}");
                    return Value.Undef;
                }

                var lst = (Value.List)arguments[0].Val;
                if (lst.Val.Count == 0) {
                    return Value.Undef;
                }

                if (lst.Val[0] is not Value.Number) {
                    context.Log($"ERROR: Invalid arguments for Min {ArgumentsToString(arguments)}");
                    return Value.Undef;
                }

                double min = ((Value.Number)lst.Val[0]).Val;

                for (int i = 1; i < lst.Val.Count; ++i) {
                    if (lst.Val[i] is not Value.Number) {
                        context.Log($"ERROR: Invalid arguments for Min {ArgumentsToString(arguments)}");
                        return Value.Undef;
                    }

                    min = Math.Min(((Value.Number)lst.Val[i]).Val, min);
                }

                return new Value.Number(min);
            }

            if (arguments[0].Val is not Value.Number) {
                context.Log($"ERROR: Invalid arguments for Min {ArgumentsToString(arguments)}");
                return Value.Undef;
            }
            double res = ((Value.Number)arguments[0].Val).Val;
            for (int i = 1; i < arguments.Count(); ++i) {
                if (arguments[i].Val is not Value.Number) {
                    context.Log($"ERROR: Invalid arguments for Min {ArgumentsToString(arguments)}");
                    return Value.Undef;
                }
                res = Math.Min(((Value.Number)arguments[i].Val).Val, res);
            }

            return new Value.Number(res);
        }
    }

    public class Max : Value.Function
    {
        public override Value Call(ExecutionContext context, params Argument[] arguments)
        {
            if (arguments.Count() == 0) {
                context.Log("ERROR: No arguments for Max");
                return Value.Undef;
            }

            if (arguments.Count() == 1) {
                if (arguments[0].Val is not Value.List) {
                    context.Log($"ERROR: Invalid arguments for Max {ArgumentsToString(arguments)}");
                    return Value.Undef;
                }

                var lst = (Value.List)arguments[0].Val;
                if (lst.Val.Count == 0) {
                    return Value.Undef;
                }

                if (lst.Val[0] is not Value.Number) {
                    context.Log($"ERROR: Invalid arguments for Max {ArgumentsToString(arguments)}");
                    return Value.Undef;
                }

                double max = ((Value.Number)lst.Val[0]).Val;

                for (int i = 1; i < lst.Val.Count; ++i) {
                    if (lst.Val[i] is not Value.Number) {
                        context.Log($"ERROR: Invalid arguments for Max {ArgumentsToString(arguments)}");
                        return Value.Undef;
                    }

                    max = Math.Max(((Value.Number)lst.Val[i]).Val, max);
                }

                return new Value.Number(max);
            }

            if (arguments[0].Val is not Value.Number) {
                context.Log($"ERROR: Invalid arguments for Max {ArgumentsToString(arguments)}");
                return Value.Undef;
            }
            double res = ((Value.Number)arguments[0].Val).Val;
            for (int i = 1; i < arguments.Count(); ++i) {
                if (arguments[i].Val is not Value.Number) {
                    context.Log($"ERROR: Invalid arguments for Max {ArgumentsToString(arguments)}");
                    return Value.Undef;
                }
                res = Math.Max(((Value.Number)arguments[i].Val).Val, res);
            }

            return new Value.Number(res);
        }
    }
    // norm
    // cross

    public class TypeCheck<T> : Value.Function where T : Value
    {
        public override Value Call(ExecutionContext context, params Argument[] arguments)
        {
            if (arguments.Count() != 1) {
                context.Log($"Invalid arguments for {this.GetType().Name}");
                return Value.Undef;
            }

            return new Value.Bool(arguments[0].Val is T);
        }
    }
};
