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
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 1, 1, 1, 0, 1 },
            { 1, 0, 1, 1, 1, 0, 1 },
            { 1, 0, 1, 1, 1, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1 }
        };

        public static ImagePSI GenerateQRCode(string text)
        {
            if (!strictRegex.IsMatch(text))
                throw new FormatException("invalid text inside the QR code!");

            int version = text.Length > 25 ? 2 : 1;
            int size = version == 1 ? 21 : 25;

            ImagePSI qrcode = new ImagePSI(size, size);

            // adding markers
            int startFarMarker = 14 + (version - 1) * 4;
            int[,] markerOrigins = { { 0, 0 }, { 0, startFarMarker }, { startFarMarker, 0 } };
            for (int i = 0; i < markerOrigins.GetLength(0); i++)
            {
                int originX = markerOrigins[i, 0], originY = markerOrigins[i, 1];

                for (int y = 0; y < marker.GetLength(0); y++)
                {
                    for (int x = 0; x < marker.GetLength(1); x++)
                    {
                        qrcode[originX + x, originY + y] = IntPixel(marker[y, x]);
                    }
                }
            }

            // adding separation lines
            for (int y = 0; y < size; y++)
            {
                if (y < 8 || y > startFarMarker - 1)
                    qrcode[7, y] = White;

                if (y == 7)
                {
                    for (int x = 0; x < size; x++)
                    {
                        if (x < 7 || x > startFarMarker - 1)
                            qrcode[x, y] = White;
                    }
                }
                else if (y == startFarMarker - 1)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        qrcode[x, y] = White;
                    }
                }
            }
            for (int y = 0; y < 8; y++)
            {
                qrcode[startFarMarker - 1, y] = White;
            }

            return qrcode.Scale(10);
            // throw new NotImplementedException("Cannot generate a QR code yet!");
        }
    }
}
