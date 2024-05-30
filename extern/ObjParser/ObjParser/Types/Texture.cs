namespace ObjParser.Types
{

    public class Texture
    {
        public string Filename { get; set; }
        public bool BlendU { get; set; } = false;
        public bool BlendV { get; set; } = false;
        public float Boost { get; set; } = -1.0f;
        // TODO: other texture fields

        public Texture(string filename)
        {
            Filename = filename;
        }

        public override string ToString()
        {
            var b = new System.Text.StringBuilder();
            bool empty = true;
            var addVar = (string v) => {
                if (!empty) {
                    b.Append(" ");
                }
                b.Append(v);
                empty = false;
            };

            if (BlendU)
            {
                addVar("-blendu");
                addVar("on");
            }

            if (BlendV)
            {
                addVar("-blendv");
                addVar("on");
            }

            if (Boost > 0.0f)
            {
                addVar("-boost");
                addVar(Boost.ToString());
            }

            addVar(Filename);

            return b.ToString();
        }

        private static string? FindName(string []args)
        {
            for (int i = 0; i < args.Count(); ++i)
            {
                if (args[i].StartsWith("-")) {
                    // Skip key and argument
                    ++i;
                    continue;
                }
                if (args[i].EndsWith(".tga") || args[i].EndsWith(".jpg") || args[i].EndsWith(".png") || args[i].EndsWith(".bmp"))
                {
                    return args[i];
                }
            }

            return null;
        }

        public static Texture? FromArgs(string []args)
        {
            string? name = FindName(args);
            if (name == null)
            {
                return null;
            }

            var res = new Texture(name);
            // TODO: parse arguments

            return res;
        }
    }

}
