namespace Scad;

public class Color
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }

    public Color(float r = 0.0f, float g = 0.0f, float b = 0.0f)
    {
        R = r;
        G = g;
        B = b;
    }
}
