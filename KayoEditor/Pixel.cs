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

        /// <summary>
        /// Calcule la valeur en niveau de gris de ce pixel.
        /// </summary>
        /// <returns>Un nouveau pixel � 3 composantes �gales (moyenne des 3 composantes originales).</returns>
        public Pixel Greyscale() => new Pixel((byte)((r + g + b) / 3)); // m�thode en 1 ligne

        /// <summary>
        /// Cr�� un *Pixel* selon ses 3 composantes RGB.
        /// </summary>
        /// <param name="r">Composante *R* (red/rouge).</param>
        /// <param name="g">Composante *G* (green/verte).</param>
        /// <param name="b">Composante *B* (blue/bleue).</param>
        public Pixel(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        /// <summary>
        /// Cr�� un *Pixel* noir (3 composantes RGB � 0).
        /// </summary>
        public Pixel() : this(0, 0, 0) { }

        /// <summary>
        /// Cr�� un *Pixel* selon son niveau de gris entre 0 et 255.
        /// </summary>
        /// <param name="val"></param>
        public Pixel(byte val) : this(val, val, val) { }

        /// <summary>
        /// Cr�� une copie d'un *Pixel*.
        /// </summary>
        /// <param name="original">Instance � copier.</param>
        public Pixel(Pixel original)
        {
            r = original.r;
            g = original.g;
            b = original.b;
        }

        /// <summary>
        /// Cr�� une copie du *Pixel*.
        /// </summary>
        /// <returns>Un nouveau *Pixel* compos� des m�mes composantes que cette instance.</returns>
        public Pixel Copy()
        {
            return new Pixel(this);
        }

        /// <summary>
        /// Repr�sentation textuelle de ce *Pixel* (composantes RGB).
        /// </summary>
        /// <returns>Composantes RGB.</returns>
        public override string ToString()
        {
            return "(" + R + ", " + G + ", " + B + ")";
        }
    }
}
