using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KayoEditor
{
    public class Complex
    {
        public double Re { get; set; }
        public double Im { get; set; }

        public Complex(double re, double im)
        {
            Re = re;
            Im = im;
        }

        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.Re + b.Re, a.Im + b.Im);
        }

        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.Re - b.Re, a.Im - b.Im);
        }

        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.Re * b.Re - a.Im * b.Im, a.Re * b.Im + a.Im * b.Re);
        }

        public static Complex operator /(Complex a, Complex b)
        {
            return new Complex((a.Re * b.Re + a.Im * b.Im) / (b.Re * b.Re + b.Im * b.Im), (a.Im * b.Re - a.Re * b.Im) / (b.Re * b.Re + b.Im * b.Im));
        }

        public static Complex operator +(Complex a, double b)
        {
            return new Complex(a.Re + b, a.Im);
        }

        public static Complex operator -(Complex a, double b)
        {
            return new Complex(a.Re - b, a.Im);
        }

        public static Complex operator *(Complex a, double b)
        {
            return new Complex(a.Re * b, a.Im * b);
        }

        public static Complex operator /(Complex a, double b)
        {
            return new Complex(a.Re / b, a.Im / b);
        }

        public static Complex operator +(double a, Complex b)
        {
            return new Complex(a + b.Re, b.Im);
        }

        public static Complex operator -(double a, Complex b)
        {
            return new Complex(a - b.Re, -b.Im);
        }

        public double Abs()
        {
            return (double)Math.Sqrt(Re * Re + Im * Im);
        }

        public override string ToString()
        {
            return Re + " + " + Im + "i";
        }
    }
}
