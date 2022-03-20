using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KayoEditor
{
    public class FractalGenerator
    {      
        public static ImagePSI GenerateFractal(int width, int height, Complex c)
        {
            Pixel[] colors = Enumerable.Range(0, 256).Select(c => new Pixel((byte)((c >> 5) * 36), (byte)((c >> 3 & 7) * 36), (byte)((c & 3) * 85))).ToArray();

            ImagePSI fractal = new ImagePSI(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // -2 pour x = 0 et 2 pour x = width
                    // -2 pour y = 0 et 2 pour y = height
                    // sauf qu'on zoome donc on va de -1.25 à 1.25
                    Complex z = new Complex(x / (double)width * 2.5 - 1.25, y / (double)height * 2.5 - 1.25);

                    int iterations = 0;
                    while (z.Abs() < 2 && iterations < 255)
                    {
                        z = z * z + c;
                        iterations++;
                    }

                    fractal[x, y] = colors[iterations];
                }
            }

            return fractal;
        }
    }
}
