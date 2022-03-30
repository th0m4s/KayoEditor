using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReedSolomon;

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

        private static Pixel IntPixel(int val)
        {
            if (val == 1)
            {
                return Black;
            }
            else
            {
                return White;
            }
        }

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

        private static byte[,] alignmentPattern =
        {
            { 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 1 },
            { 1, 0, 1, 0, 1 },
            { 1, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1 }
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
                int originX = markerOrigins[i, 0];
                int originY = markerOrigins[i, 1];

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

            // draw aligment pattern
            if(version == 2)
            {
                int alignStart = 16;
                for (int y = 0; y < alignmentPattern.GetLength(0); y++)
                {
                    for (int x = 0; x < alignmentPattern.GetLength(1); x++)
                    {
                        qrcode[alignStart + x, alignStart + y] = IntPixel(alignmentPattern[y, x]);
                    }
                }
            }

            // draw timing patterns
            for (int x = 8; x < size - 8; x++)
            {
                qrcode[x, 6] = IntPixel((x + 1) % 2);
                qrcode[6, x] = IntPixel((x + 1) % 2);
            }

            // draw error correction level and mask
            int[,,] eccAndMaskPos = {
                { { 0, 8 }, { 1, 8 }, { 2, 8 }, { 3, 8 }, { 4, 8 }, { 5, 8 }, { 7, 8 }, { 8, 8 }, { 8, 7 }, { 8, 5 }, { 8, 4 }, { 8, 3 }, { 8, 2 }, { 8, 1 }, { 8, 0} },
                { { 8, size - 1 }, { 8, size - 2 }, { 8, size - 3 }, { 8, size - 4 }, { 8, size - 5 }, { 8, size - 6 }, { 8, size - 7}, { size - 8, 8 }, { size - 7, 8 }, { size - 6, 8 }, { size - 5, 8 }, { size - 4, 8 }, { size - 3, 8 }, { size - 2, 8 }, { size- 1, 8 } } 
            };

            string eccAndMask = "111011111000100";

            for (int i = 0; i < eccAndMaskPos.GetLength(0); i++)
            {
                for (int j = 0; j < eccAndMaskPos.GetLength(1); j++)
                {
                    int x = eccAndMaskPos[i, j, 0];
                    int y = eccAndMaskPos[i, j, 1];
                    qrcode[x, y] = IntPixel(eccAndMask[j] - '0');
                }
            }

            string data = "0010" + Convert.ToString(text.Length, 2).PadLeft(9, '0');

            for(int i = 0; i < text.Length - 2; i+=2)
            {
                string pair = text.Substring(i, 2);
                data += Convert.ToString(encoding[pair[0]] * 45 + encoding[pair[1]], 2).PadLeft(11, '0');                
            }
            
            if (text.Length % 2 != 0)
            {
                data += Convert.ToString(encoding[text[text.Length-1]], 2).PadLeft(6, '0');
            }

            int maxbits = (version == 1 ? 19 : 34) * 8;

            if(data.Length < maxbits) {
                int zerosCount = Math.Min(maxbits - data.Length, 4);
                for(int i = 0; i < zerosCount; i++)
                {
                    data += "0";
                }
            }

            if(data.Length < maxbits && data.Length % 8 != 0)
            {
                int zerosCount = 8 - data.Length % 8;
                for(int i = 0; i < zerosCount; i++)
                {
                    data += "0";
                }
            }

            int counter = 0;
            string[] end = { "11101100", "00010001" };
            while(data.Length < maxbits)
            {
                data += end[(counter++) % 2];
            }

            byte[] dataBytes = new byte[data.Length / 8];
            for(int i = 0; i < dataBytes.Length; i++)
            {
                dataBytes[i] = Convert.ToByte(data.Substring(i * 8, 8), 2);
            }

            byte[] ecc = ReedSolomonAlgorithm.Encode(dataBytes, version == 1 ? 7 : 10, ErrorCorrectionCodeType.QRCode);

            for(int i = 0; i < ecc.Length; i++)
            {
                data += Convert.ToString(ecc[i], 2).PadLeft(8, '0');
            }

            // throw new Exception(string.Join(' ', dataBytes.Select(x => x.ToString())) + " ecc " + String.Join(' ', ecc.Select(x => x.ToString())));

            counter = 0;
            for (int x = size - 1; x >= 0; x -= 2)
            {
                if (x == 6)
                {
                    x--;
                }

                bool up = (x / 2) % 2 == 0;
                if (x < 6) up = !up;

                for (int y = size - 1; y >= 0; y--)
                {
                    int ny = up ? y : size - 1 - y;

                    if (IsModuleFree(version, x, ny))
                    {
                        qrcode[x, ny] = /*new Pixel((byte)((counter++) % 255), 0, 0);*/ GetPixelFromData(data, counter++, x, ny, size);
                    }

                    if (IsModuleFree(version, x - 1, ny))
                    {
                        qrcode[x - 1, ny] = /*new Pixel((byte)((counter++) % 255), 0, 0);*/ GetPixelFromData(data, counter++, x - 1, ny, size);
                    }
                }
            }

            // throw new Exception(data);
            return qrcode;
        }

        private static Pixel GetPixelFromData(string data, int counter, int x, int y, int size)
        {
            return IntPixel(Math.Abs((counter < data.Length ? data[counter] : '0') - 48 - (x + size - y) % 2));
        }

        private static bool IsModuleFree(int version, int x, int y) {
            int size = version == 1 ? 21 : 25;
            if(version == 2 && y >= 16 && y < 21 && x >= 16 && x < 21) return false;
            if(y > 8 && y < size - 8) return x != 6;
            if(x > 8 && x < size - 8) return y != 6;
            return x > 8 && y > 8;
        }
    }
}
