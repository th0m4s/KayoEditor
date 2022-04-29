namespace KayoEditor
{
    /// <summary>
    /// Représente une couleur selon 3 composantes RGB entre 0 et 255.
    /// </summary>
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
        public Pixel Greyscale() => new Pixel((byte)((r + g + b) / 3)); // méthode en 1 ligne

        /// <summary>
        /// Créé un <see cref="Pixel"/> selon ses 3 composantes RGB.
        /// </summary>
        public Pixel(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        /// <summary>
        /// Créé un <see cref="Pixel"/> noir (3 composantes RGB à 0).
        /// </summary>
        public Pixel() : this(0, 0, 0) { }

        /// <summary>
        /// Créé un <see cref="Pixel"/> selon son niveau de gris <paramref name="val"/> entre 0 et 255.
        /// </summary>
        /// <param name="val">Niveau de gris.</param>
        public Pixel(byte val) : this(val, val, val) { }

        /// <summary>
        /// Créé une copie du <see cref="Pixel"/> <paramref name="original"/>.
        /// </summary>
        /// <param name="original">Instance à copier.</param>
        /// <seealso cref="Copy"/>
        public Pixel(Pixel original)
        {
            r = original.r;
            g = original.g;
            b = original.b;
        }

        /// <summary>
        /// Créé une copie de ce <see cref="Pixel"/>.
        /// </summary>
        /// <seealso cref="Pixel(Pixel)"/>
        public Pixel Copy()
        {
            return new Pixel(this);
        }

        /// <summary>
        /// Représentation textuelle de ce <see cref="Pixel"/> (composantes RGB).
        /// </summary>
        public override string ToString()
        {
            return "(" + R + ", " + G + ", " + B + ")";
        }

        /// <summary>
        /// Vérifie l'égalité entre 2 <see cref="Pixel"/> (composantes R, G et B égales).
        /// </summary>
        /// <seealso cref="Equals(object)"/>
        public static bool operator ==(Pixel a, Pixel b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b;
        }

        /// <summary>
        /// Vérifie l'inégalité entre 2 <see cref="Pixel"/> (composantes R, G ou B différentes).
        /// </summary>
        /// <seealso cref="operator ==(Pixel, Pixel)"/>
        public static bool operator !=(Pixel a, Pixel b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Vérifie l'égalité entre ce <see cref="Pixel"/> et l'objet <paramref name="other"/>.
        /// </summary>
        /// <seealso cref="operator ==(Pixel, Pixel)"/>
        public override bool Equals(object other)
        {
            return other is Pixel && this == (Pixel)other;
        }

        public override int GetHashCode()
        {
            // méthode pour enlever le warning à cause des opérateurs == et !=.
            return base.GetHashCode();
        }
    }
}
