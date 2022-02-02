namespace KayoEditor
{
    public class Pixel
    {
        byte r;
        byte g;
        byte b;

        public byte R => r;
        public byte G => g;
        public byte B => b;

        public Pixel Greyscale => new Pixel((byte)((r + g + b) / 3));

        public Pixel(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Pixel() : this(0, 0, 0) { }

        public Pixel(byte val) : this(val, val, val) { }

        public Pixel(Pixel original)
        {
            r = original.r;
            g = original.g;
            b = original.b;
        }

        public Pixel Copy()
        {
            return new Pixel(this);
        }

        public override string ToString()
        {
            return "(" + R + ", " + G + ", " + B + ")";
        }
    }
}
