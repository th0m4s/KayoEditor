using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReedSolomon;

namespace KayoEditor
{
    public static class QRCode
    {
        private static readonly Regex extentedRegex = new Regex(@"^[a-zA-Z0-9$%*+\-./: ]{0,47}$");
        private static readonly Regex strictRegex = new Regex(@"^[A-Z0-9$%*+\-./: ]{0,47}$");

        public static Regex ExtendedRegex => extentedRegex;
        public static Regex StrictRegex => strictRegex;

        private static Pixel White => new Pixel(255);
        private static Pixel Black => new Pixel(0);

        /// <summary>
        /// Transforme un nombre en pixel, blanc pour 0 et noir pour 1.
        /// </summary>
        /// <param name="val">Valeur du bit, 0 ou 1.</param>
        /// <returns>Un <see cref="Pixel"/> noir ou blanc.</returns>
        private static Pixel IntPixel(int val)
        {
            return val == 1 ? Black : White;
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

        /// <summary>
        /// Génère un QRCode à partir de la chaîne de caractères donnée.
        /// </summary>
        /// <param name="text">Texte à encoder.</param>
        /// <returns>Une <i>ImagePSI</i> représentant le QRCode.</returns>
        public static ImagePSI GenerateQRCode(string text)
        {
            if (!strictRegex.IsMatch(text))
                throw new FormatException("invalid text inside the QR code!");

            int version = text.Length > 25 ? 2 : 1;
            int size = version == 1 ? 21 : 25;

            ImagePSI qrcode = new ImagePSI(size, size);

            // adding markers and separation lines
            int startFarMarker = 13 + (version - 1) * 4; // selon la version
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
                    qrcode[x, y] = IntPixel(eccAndMask[j] - '0'); // ou - 48 pour transformer un INT en pixel
                }
            }

            string data = "0010" + Convert.ToString(text.Length, 2).PadLeft(9, '0');

            for(int i = 0; i < text.Length - 1; i+=2)
            {
                string pair = text.Substring(i, 2);
                int val = encoding[pair[0]] * 45 + encoding[pair[1]]; // base est *45^1 et 45^0h
                data += Convert.ToString(val, 2).PadLeft(11, '0');     // (val,2) car on veut en base de 2            
            }
            
            if (text.Length % 2 != 0)
            {
                data += Convert.ToString(encoding[text[text.Length-1]], 2).PadLeft(6, '0');
            }

            int maxbits = (version == 1 ? 19 : 34) * 8; // car on veut en bit : 1 octet = 8 bits

            if(data.Length < maxbits) {
                int zerosCount = Math.Min(maxbits - data.Length, 4); // on veut un padding maximum de quatre 0 
                for(int i = 0; i < zerosCount; i++)
                {
                    data += "0";
                }
            }

            
            if (data.Length < maxbits && data.Length % 8 != 0) // on veut un multiple de 8 
            {
                int zerosCount = 8 - data.Length % 8;
                for(int i = 0; i < zerosCount; i++)
                {
                    data += "0";
                }
            }

            int counter = 0;
            string[] end = { "11101100", "00010001" }; // on remplit avec ça jusqu'à atteindre la taille max (152 ou 272)
            while(data.Length < maxbits)
            {
                data += end[(counter++) % 2];
            }

            byte[] dataBytes = new byte[data.Length / 8];
            for(int i = 0; i < dataBytes.Length; i++)
            {
                dataBytes[i] = Convert.ToByte(data.Substring(i * 8, 8), 2); // i*8 pour récupérer à partir de la bonne position, 8 car 8 bits, 2 car binaire
            }

            byte[] ecc = ReedSolomonAlgorithm.Encode(dataBytes, version == 1 ? 7 : 10, ErrorCorrectionCodeType.QRCode); // errorcorrectioncode

            for(int i = 0; i < ecc.Length; i++)
            {
                data += Convert.ToString(ecc[i], 2).PadLeft(8, '0'); // padding sur la gauche
            }

            counter = 0;
            for (int x = size - 1; x >= 0; x -= 2) // x colonne
            {
                if (x == 6)
                {
                    x--;
                }

                bool up = ((x + 1) / 2) % 2 == 0; // variable indiquant si on monte ou descend 
                
                for (int y = size - 1; y >= 0; y--)
                {
                    int ny = up ? y : size - 1 - y;

                    if (IsModuleFree(version, x, ny))
                    {
                        qrcode[x, ny] = GetPixelFromData(data, counter++, x, ny, size);
                    }

                    if (IsModuleFree(version, x - 1, ny))
                    {
                        qrcode[x - 1, ny] = GetPixelFromData(data, counter++, x - 1, ny, size);
                    }
                }
            }

            return qrcode;
        }

        /// <summary>
        /// Transforme un bit en pixel à partir de sa position dans les données et de sa position sur le QRCode en appliquant un masque.
        /// </summary>
        /// <param name="data">Suite de bits représentant les données du QRCode.</param>
        /// <param name="counter">Position du bit à extraire.</param>
        /// <param name="x">Position en <i>x</i> du bit sur le QRCode.</param>
        /// <param name="y">Position en <i>y</i> du bit sur le QRCode.</param>
        /// <param name="size">Taille du QRCode en nombre de modules par côté.</param>
        /// <returns>Un <see cref="Pixel"/> noir ou blanc.</returns>
        private static Pixel GetPixelFromData(string data, int counter, int x, int y, int size)
        {
            return IntPixel(Math.Abs((counter < data.Length ? data[counter] : '0') - 48 - (x + size - y) % 2)); // ou - '0'
        }

        /// <summary>
        /// Lit le contenu du QRCode sous forme de chaîne de caractères.
        /// </summary>
        /// <param name="qrcode">Le QRCode à décoder sous forme d'<i>ImagePSI</i>.</param>
        /// <returns>Le texte obtenu à partir de l'image.</returns>
        public static string ReadQRCode(ImagePSI qrcode)
        {
            int version = 0;
            int height = qrcode.Height;
            int width = qrcode.Width;

            if (height != width)
                throw new FormatException("image width and height should be identical");

            if (height % 21 == 0) version = 1;
            else if (height % 25 == 0) version = 2;

            if (version == 0)
                throw new FormatException("unknown qrcode version!");

            int size = version == 1 ? 21 : 25;

            if (height != size)
                qrcode = qrcode.Scale(size / (float)height);

            qrcode = qrcode.BlackAndWhite();

            string bits = "";
            for (int x = size - 1; x >= 0; x -= 2)
            {
                if (x == 6)
                {
                    x--;
                }

                bool up = ((x + 1) / 2) % 2 == 0;

                for (int y = size - 1; y >= 0; y--)
                {
                    int ny = up ? y : size - 1 - y;

                    if (IsModuleFree(version, x, ny))
                    {
                        bits += GetBitFromPixel(qrcode[x, ny], x, ny, size);
                    }

                    if (IsModuleFree(version, x - 1, ny))
                    {
                        bits += GetBitFromPixel(qrcode[x - 1, ny], x - 1, ny, size);
                    }
                }
            }

            int bytesLength = bits.Length / 8;
            int eccBytesLength = version == 1 ? 7 : 10;

            byte[] message = new byte[bytesLength - eccBytesLength];
            byte[] ecc = new byte[eccBytesLength];

            for (int i = 0; i < bytesLength; i++)
            {
                byte val = Convert.ToByte(bits.Substring(i * 8, 8), 2);
                if (i < bytesLength - eccBytesLength)
                    message[i] = val;
                else ecc[i - bytesLength + eccBytesLength] = val;
            }

            byte[] correctedMessage = ReedSolomonAlgorithm.Decode(message, ecc, ErrorCorrectionCodeType.QRCode);

            if (correctedMessage == null)
                throw new FormatException("qrcode is damaged!");

            string data = string.Join("", correctedMessage.Select(x => Convert.ToString(x, 2).PadLeft(8, '0'))); // x => méthode en 1 ligne
                                                                                                                 // Select -> tableau string
            int textLength = Convert.ToInt32(data.Substring(4, 9), 2); // taille à partir du 5ème bit
            string text = "";

            for (int i = 0; i < textLength - 1; i += 2)
            {
                int pairNumber = Convert.ToInt32(data.Substring(4 + 9 + i / 2 * 11, 11), 2); // 4 + 9 car à partir 14ème

                int secondNum = pairNumber % 45;
                int firstNum = (pairNumber - secondNum) / 45;

                text += DecodeSingle(firstNum);
                text += DecodeSingle(secondNum);
            }

            if (textLength % 2 != 0)
            {
                text += DecodeSingle(Convert.ToInt32(data.Substring(4 + 9 + (textLength - 1) / 2 * 11, 6), 2));
            }

            return text;
        }

        /// <summary>
        /// Transforme un <see cref="Pixel"/> en bit en appliquant un masque.
        /// </summary>
        /// <param name="pixel"><see cref="Pixel"/> à analyser.</param>
        /// <param name="x">Position en <i>x</i> du bit sur le QRCode.</param>
        /// <param name="y">Position en <i>y</i> du bit sur le QRCode.</param>
        /// <param name="size">Taille du QRCode en nombre de modules par côté.</param>
        /// <returns>Le bit représenté par ce <see cref="Pixel"/>.</returns>
        private static char GetBitFromPixel(Pixel pixel, int x, int y, int size)
        {
            return Math.Abs(pixel.R / 255 - (x + size - y) % 2) == 0 ? '1' : '0';
        }

        /// <summary>
        /// Recherche par valeur dans le dictionnaire d'encodage des caractères.
        /// </summary>
        /// <param name="val">Valeur représentant le caractère recherché.</param>
        /// <returns>Le caractère représenté par la valeur donnée.</returns>
        private static char DecodeSingle(int val)
        {
            return encoding.First(x => x.Value == val).Key; // First car seule méthode de recherche déjà existante 
        }

        /// <summary>
        /// Vérifie si le module à la position indiquée peut contenir ou non un bit de données.
        /// </summary>
        /// <param name="version">Version du QRCode, 1 ou 2.</param>
        /// <param name="x">Position en <i>x</i> du bit sur le QRCode.</param>
        /// <param name="y">Position en <i>y</i> du bit sur le QRCode.</param>
        /// <returns><i>false</i> si la position se trouve dans un motif d'aligmement, un marqueur ou tout autre module réservé, <i>true</i> sinon.</returns>
        private static bool IsModuleFree(int version, int x, int y) {
            int size = version == 1 ? 21 : 25;
            if(version == 2 && y >= 16 && y < 21 && x >= 16 && x < 21) return false;
            if(y > 8 && y < size - 8) return x != 6;
            if(x > 8 && x < size - 8) return y != 6;
            return x > 8 && y > 8;
        }
    }
}
