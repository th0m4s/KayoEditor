using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace KayoEditor
{
    public class ImagePSI                               // en hexadécimal
    {
        public const int OFFSET_TYPE = 0x00;            // type de l'image
        public const int OFFSET_FILESIZE = 0x02;        // taille totale du fichier (en octets)
        public const int OFFSET_STARTOFFSET = 0x0a;     // position du premier pixel dans le fichier (en octets)

        public const int OFFSET_INFOHEADERSIZE = 0x0e;  // taille de la deuxième partie de l'entête
        public const int OFFSET_WIDTH = 0x12;           // largeur de l'image en pixels
        public const int OFFSET_HEIGHT = 0x16;          // hauteur de l'image en pixels
        public const int OFFSET_COLORPLANES = 0x1a;     // vaudra toujours 1 pour un bitmap
        public const int OFFSET_COLORDEPTH = 0x1c;      // nombre de bits par pixel (souvent 24 car 3 octets * 8 bits = 24)

        byte[] rawHeader;                               // création du tableau de bytes/octets du header
        byte[] rawPixels;                               // création du tableau de bytes/octets de l'image

        public string Type => Encoding.ASCII.GetString(rawHeader.ExtractBytes(2, OFFSET_TYPE));     // Encoding.ASCII.GetString est une méthode permettant de transformer un tableau d'octets en string
        public uint FileSize => Utils.LittleEndianToUInt(rawHeader, OFFSET_FILESIZE);
        public uint StartOffset => Utils.LittleEndianToUInt(rawHeader, OFFSET_STARTOFFSET);

        public uint InfoHeaderSize => Utils.LittleEndianToUInt(rawHeader, OFFSET_INFOHEADERSIZE);
        public int Width => Utils.LittleEndianToInt(rawHeader, OFFSET_WIDTH);
        public int Height => Utils.LittleEndianToInt(rawHeader, OFFSET_HEIGHT);
        public ushort ColorPlanes => Utils.LittleEndianToUShort(rawHeader, OFFSET_COLORPLANES);
        public ushort ColorDepth => Utils.LittleEndianToUShort(rawHeader, OFFSET_COLORDEPTH);
        public int Stride => (Width * ColorDepth / 8 + 3) / 4 * 4; // by dividing then multiplying, we floor to the nearest smallest integer

        /*public ReadOnlyCollection<byte> RawHeader => Array.AsReadOnly(rawHeader);
        public ReadOnlyCollection<byte> RawPixels => Array.AsReadOnly(rawPixels);*/

        public byte[] RawHeader => rawHeader;
        public byte[] RawPixels => rawPixels;

        public ImagePSI(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))     // appelle stream.Close() automatiquement
            {
                rawHeader = stream.ReadBytes(54);

                if (Type != "BM")
                    throw new FormatException("invalid magic file type!");

                if (ColorDepth != 24)   
                    throw new FormatException("KayoEditor can only load images with a depth of 24 bits");

                rawPixels = stream.ReadBytes((int)(FileSize - StartOffset));    // principe de stream : ne peut lire qu'une seule fois chaque octet (donc à la position 54 à ce moment là)
            }
        }

        public ImagePSI(int width, int height)
        {
            rawHeader = new byte[54];

            rawHeader.InsertBytes(Encoding.ASCII.GetBytes("BM"), OFFSET_TYPE);      // Encoding.ASCII.GetBytes est une méthode permettant de transformer un string en tableau octets
            rawHeader.InsertBytes(Utils.UIntToLittleEndian((uint)rawHeader.Length), OFFSET_STARTOFFSET);

            rawHeader.InsertBytes(Utils.UShortToLittleEndian(0x28), OFFSET_INFOHEADERSIZE);
            rawHeader.InsertBytes(Utils.IntToLittleEndian(width), OFFSET_WIDTH);
            rawHeader.InsertBytes(Utils.IntToLittleEndian(height), OFFSET_HEIGHT);
            rawHeader.InsertBytes(Utils.UShortToLittleEndian(1), OFFSET_COLORPLANES);
            rawHeader.InsertBytes(Utils.UShortToLittleEndian(24), OFFSET_COLORDEPTH);

            rawPixels = new byte[height * Stride];

            rawHeader.InsertBytes(Utils.UIntToLittleEndian((uint)(rawHeader.Length + rawPixels.Length)), OFFSET_FILESIZE);
        }

        public ImagePSI(ImagePSI original)
        {
            rawHeader = new byte[original.rawHeader.Length];
            Array.Copy(original.rawHeader, rawHeader, rawHeader.Length);        // Array.Copy agit comme une boucle for afin de copier le tableau

            rawPixels = new byte[original.rawPixels.Length];
            Array.Copy(original.rawPixels, rawPixels, rawPixels.Length);
        }

        public ImagePSI Copy()
        {
            return new ImagePSI(this);
        }

        private int _position(int x, int y) => x * 3 + (Height - y - 1) * Stride;           // position du premier octet décrivant ce pixel
        public Pixel this[int x, int y] // la propriété c'est l'instance elle-même      similaire à static bool operator ==(...)
        { // imageOriginale[x, y] <=> imageOriginale._pixels[y, x]
            get
            {
                int position = _position(x, y);
                return new Pixel(rawPixels[position + 2], rawPixels[position + 1], rawPixels[position + 0]);
            }

            set
            {
                int position = _position(x, y);
                rawPixels[position + 2] = value.R;
                rawPixels[position + 1] = value.G;
                rawPixels[position + 0] = value.B;
            }
        }

        public void Save(string filename)
        {
            using (FileStream stream = File.OpenWrite(filename))
            {
                stream.Write(rawHeader, 0, rawHeader.Length);
                stream.Write(rawPixels, 0, rawPixels.Length);
            }
        }

        public ImagePSI Greyscale()
        {
            ImagePSI result = this.Copy();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    result[x, y] = this[x, y].Greyscale;
                }
            }

            return result;
        }

        public ImagePSI BlackAndWhite()
        {
            ImagePSI result = this.Copy();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Pixel pixel = this[x, y];
                    result[x, y] = pixel.Greyscale.R > 127 ? new Pixel(255) : new Pixel(0);
                }
            }

            return result;
        }

        public ImagePSI Negative()
        {
            ImagePSI result = this.Copy();

            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    Pixel pixel = this[x, y];
                    result[x, y] = new Pixel((byte)(255 - pixel.R), (byte)(255 - pixel.G), (byte)(255 - pixel.B));
                }
            }

            return result;
        }
        
        public ImagePSI Invert()
        {
            ImagePSI result = this.Copy();
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Pixel pixel = this[x, y];
                    result[x, y] = new Pixel(pixel.B, pixel.G, pixel.R);
                }
            }

            return result;
        }

        public ImagePSI GetHidden()
        {
            ImagePSI result = this.Copy();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Pixel pixel = this[x, y];
                    result[x, y] = new Pixel((byte)(pixel.R << 4), (byte)(pixel.G << 4), (byte)(pixel.B << 4));
                }
            }

            return result;
        }

        public ImagePSI Scale(float scale)
        {
            if (scale == 0)
                throw new ArgumentOutOfRangeException("scale", "scale must not be 0");

            if (scale < 0)
                throw new ArgumentOutOfRangeException("scale", "scale must be a positive number");

            int newWidth = (int)(Width * scale);
            int newHeight = (int)(Height * scale);

            if (newWidth == 0)
                newWidth = 1;

            if (newHeight == 0)
                newHeight = 1;

            ImagePSI source = this.Copy();

            if(scale < 1)
            {
                // si on réduit, on met dans les pixels en haut à gauche de chaque groupe la moyenne des pixels du groupe

                int convolSize = (int)Math.Ceiling(1 / scale);
                float[,] kernel = new float[convolSize, convolSize];
                float coef = 1f / kernel.Length;

                for(int y = 0; y < convolSize; y++)
                {
                    for(int x = 0; x < convolSize; x++)
                    {
                        kernel[y, x] = coef;
                    }
                }

                source = source.ApplyKernel(kernel, Convolution.KernelOrigin.TopLeft, Convolution.EdgeProcessing.Extend);
            }

            ImagePSI result = new ImagePSI(newWidth, newHeight);

            for (int x = 0; x < newWidth; x++)
            {
                for(int y = 0; y < newHeight; y++)
                {
                    result[x, y] = new Pixel(source[(int)(x / scale), (int)(y / scale)]);     // 1/2 = 0 ; 3/2 = 1
                }
            }

            return result;
        }

        public ImagePSI Flip(FlipMode mode)
        {
            ImagePSI result = this.Copy();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int nx = x;
                    int ny = y;

                    if(/* vrai quand on change que x ou les 2 coords */mode == FlipMode.FlipX || mode == FlipMode.FlipBoth)
                        nx = Width - x - 1;

                    if (/* vrai quand on change que y ou les 2 coords */mode == FlipMode.FlipY || mode == FlipMode.FlipBoth)
                        ny = Height - y - 1;

                    result[nx, ny] = new Pixel(this[x, y]);
                }
            }

            return result;
        }
    }

    public enum FlipMode
    {
        [Description("Inverser les X")]
        FlipX,

        [Description("Inverser les Y")]
        FlipY,

        [Description("Inverser les 2 axes")]
        FlipBoth
    }
}
