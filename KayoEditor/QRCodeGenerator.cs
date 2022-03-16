using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KayoEditor
{
    public static class QRCodeGenerator
    {
        private static readonly Regex extentedRegex = new Regex(@"^[a-zA-Z0-9$%*+\-./: ]{0,47}$");
        private static readonly Regex strictRegex = new Regex(@"^[A-Z0-9$%*+\-./: ]{0,47}$");

        public static Regex ExtendedRegex => extentedRegex;
        public static Regex StrictRegex => strictRegex;

        private static Pixel White => new Pixel(255);
        private static Pixel Black => new Pixel(0);

        private static Pixel IntPixel(int val) => val == 1 ? Black : White;

        private static byte[,] marker =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 0, 1, 1, 1, 0, 1, 0 },
            { 0, 1, 0, 1, 1, 1, 0, 1, 0 },
            { 0, 1, 0, 1, 1, 1, 0, 1, 0 },
            { 0, 1, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        private static Dictionary<char, byte> encoding = new Dictionary<char, byte>
        {
            { '0', 0 }, { '1', 1 }, { '2', 2 }, { '3', 3 }, { '4', 4 }, 
            { '5', 5 }, { '6', 6 }, { '7', 7 }, { '8', 8 }, { '9', 9 }, 
            
            { 'A', 10 }, { 'B', 11 }, { 'C', 12 }, { 'D', 13 }, { 'E', 14 }, { 'F', 15 }, { 'G', 16 },
            { 'H', 17 }, { 'I', 18 }, { 'J', 19 }, { 'K', 20 }, { 'L', 21 }, { 'M', 22 }, { 'N', 23 }, 
            { 'O', 24 }, { 'P', 25 }, { 'Q', 26 }, { 'R', 27 }, { 'S', 28 }, { 'T', 29 }, { 'U', 30 }, 
            { 'V', 31 }, { 'W', 32 }, { 'X', 33 }, { 'Y', 34 }, { 'Z', 35 },

            { ' ', 36 }, { '$', 37 }, { '%', 38 }, { '*', 39 }, { '+', 40 }, 
            { '-', 41 }, { '.', 42 }, { '/', 43 }, { ':', 44 }
        };

        public static ImagePSI GenerateQRCode(string text)
        {
            if (!strictRegex.IsMatch(text))
                throw new FormatException("invalid text inside the QR code!");

            int version = text.Length > 25 ? 2 : 1;
            int size = version == 1 ? 21 : 25;

            ImagePSI qrcode = new ImagePSI(size, size);

            // adding markers and separation lines
            int startFarMarker = 13 + (version - 1) * 4;
            int[,] markerOrigins = { { -1, -1 }, { -1, startFarMarker }, { startFarMarker, -1 } };
            for (int i = 0; i < markerOrigins.GetLength(0); i++)
            {
                int originX = markerOrigins[i, 0], originY = markerOrigins[i, 1];

                for (int y = 0; y < marker.GetLength(0); y++)
                {
                    for (int x = 0; x < marker.GetLength(1); x++)
                    {
                        int nx = originX + x;
                        int ny = originY + y;
                        if(nx >= 0 && ny >= 0 && nx < size && ny < size)
                        {
                            qrcode[nx, ny] = IntPixel(marker[y, x]);
                        }
                    }
                }
            }

            bool[] header = { false, false, true, false };
            bool[] data = new bool[(version == 1 ? 26 : 44) * 8];
            // W.I.P.


            return qrcode.Scale(10);
            // throw new NotImplementedException("Cannot generate a QR code yet!");
        }
    }
}
