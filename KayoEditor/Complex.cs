using System;

namespace KayoEditor
{   
    /// <summary>
    /// Représente un nombre complexe, composé d'une partie réelle et d'une partie imaginaire avec les opérateurs mathématiques associés.
    /// </summary>
    public class Complex
    {
        public double Re { get; set; }
        public double Im { get; set; }

        /// <summary>
        /// Créé un complexe à partir de leurs parties réelle et imaginaire.
        /// </summary>
        /// <param name="re">Partie réelle du complexe.</param>
        /// <param name="im">Partie imaginaire du complexe.</param>
        public Complex(double re, double im)
        {
            Re = re;
            Im = im;
        }

        /// <summary>
        /// Opérateur somme de complexe (réalise le calcul <i>a</i> + <i>b</i>).
        /// </summary>
        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.Re + b.Re, a.Im + b.Im);
        }

        /// <summary>
        /// Opérateur différence entre un complexe <i>a</i> et un complexe <i>b</i>.
        /// </summary>
        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.Re - b.Re, a.Im - b.Im);
        }

        /// <summary>
        /// Opérateur de multiplication d'un complexe <i>a</i> par un complexe <i>b</i>.
        /// </summary>
        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.Re * b.Re - a.Im * b.Im, a.Re * b.Im + a.Im * b.Re);
        }

        /// <summary>
        /// Opérateur de division entre 2 complexes.
        /// </summary>
        public static Complex operator /(Complex a, Complex b)
        {
            return new Complex((a.Re * b.Re + a.Im * b.Im) / (b.Re * b.Re + b.Im * b.Im), (a.Im * b.Re - a.Re * b.Im) / (b.Re * b.Re + b.Im * b.Im));
        }

        /// <summary>
        /// Opérateur d'addition d'un réel à un complexe.
        /// </summary>
        public static Complex operator +(Complex a, double b)
        {
            return new Complex(a.Re + b, a.Im);
        }

        /// <summary>
        /// Opérateur de soustraction d'un réel à un complexe.
        /// </summary>
        public static Complex operator -(Complex a, double b)
        {
            return new Complex(a.Re - b, a.Im);
        }

        /// <summary>
        /// Opérateur de multiplication d'un complexe par un réel.
        /// </summary>
        public static Complex operator *(Complex a, double b)
        {
            return new Complex(a.Re * b, a.Im * b);
        }

        /// <summary>
        /// Opérateur de division d'un complexe par un réel.
        /// </summary>
        public static Complex operator /(Complex a, double b)
        {
            return new Complex(a.Re / b, a.Im / b);
        }

        /// <summary>
        /// Opérateur d'addition d'un réel à un complexe.
        /// </summary>
        public static Complex operator +(double a, Complex b)
        {
            return new Complex(a + b.Re, b.Im);
        }

        /// <summary>
        /// Opérateur de soustraction d'un réel à un complexe.
        /// </summary>
        public static Complex operator -(double a, Complex b)
        {
            return new Complex(a - b.Re, a - b.Im);
        }

        /// <summary>
        /// Obtient la valeur absolue (ie. norme) de ce complexe.
        /// </summary>
        public double Abs()
        {
            return (double)Math.Sqrt(Re * Re + Im * Im);
        }

        /// <summary>
        /// Représentation textuelle de la valeur de ce complexe.
        /// </summary>
        public override string ToString()
        {
            return Re + " + " + Im + "i";
        }
    }
}
