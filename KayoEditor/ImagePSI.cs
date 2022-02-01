using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KayoEditor
{
    public class ImagePSI
    {
        public const int OFFSET_TYPE = 0x00;
        public const int OFFSET_FILESIZE = 0x02;
        public const int OFFSET_STARTOFFSET = 0x0a;

        public const int OFFSET_INFOHEADERSIZE = 0x0e;
        public const int OFFSET_WIDTH = 0x12;
        public const int OFFSET_HEIGHT = 0x16;
        public const int OFFSET_COLORPLANES = 0x1a;
        public const int OFFSET_COLORDEPTH = 0x1c;

        byte[] rawHeader;
        byte[] rawPixels;

        public string Type => Encoding.ASCII.GetString(rawHeader.ExtractBytes(2, OFFSET_TYPE));
        public uint FileSize => Utils.LittleEndianToUInt(rawHeader, OFFSET_FILESIZE);
        public uint StartOffset => Utils.LittleEndianToUInt(rawHeader, OFFSET_STARTOFFSET);

        public uint InfoHeaderSize => Utils.LittleEndianToUInt(rawHeader, OFFSET_INFOHEADERSIZE);
        public int Width => Utils.LittleEndianToInt(rawHeader, OFFSET_WIDTH);
        public int Height => Utils.LittleEndianToInt(rawHeader, OFFSET_HEIGHT);
        public ushort ColorPlanes => Utils.LittleEndianToUShort(rawHeader, OFFSET_COLORPLANES);
        public ushort ColorDepth => Utils.LittleEndianToUShort(rawHeader, OFFSET_COLORDEPTH);
        public int Stride => (Width * ColorDepth / 8 + 3) / 4 * 4; // by dividing then multiplying, we floor to the nearest integer

        public ReadOnlyCollection<byte> RawHeader => Array.AsReadOnly(rawHeader);
        public ReadOnlyCollection<byte> RawPixels => Array.AsReadOnly(rawPixels);

        public ImagePSI(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                rawHeader = stream.ReadBytes(54);
                if (Type != "BM")
                    throw new FormatException("Invalid magic file type!");

                rawPixels = stream.ReadBytes((int)(FileSize - StartOffset));
            }
        }

        public ImagePSI(int width, int height)
        {
            rawHeader = new byte[54];

            rawHeader.InsertBytes(Encoding.ASCII.GetBytes("BM"), OFFSET_TYPE);
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
            Array.Copy(original.rawHeader, rawHeader, rawHeader.Length);

            rawPixels = new byte[original.rawPixels.Length];
            Array.Copy(original.rawPixels, rawPixels, rawPixels.Length);
        }

        public ImagePSI Copy()
        {
            return new ImagePSI(this);
        }

        private int _position(int x, int y) => x * 3 + (Height - y - 1) * Stride;
        public Pixel this[int x, int y] // la propriété c'est l'instance elle-même
        { // instance[x, y] <=> instance._pixels[y, x]
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

        public ImagePSI Scale(float scale)
        {
            if (scale == 0)
                throw new ArgumentOutOfRangeException("scale", "scale must not be 0");

            if (scale < 0)
                throw new ArgumentOutOfRangeException("scale", "scale must be a positive number");

            int newWidth = (int)(Width * scale);
            int newHeight = (int)(Height * scale);

            ImagePSI result = new ImagePSI(newWidth, newHeight);

            for(int x = 0; x < newWidth; x++)
            {
                for(int y = 0; y < newHeight; y++)
                {
                    result[x, y] = new Pixel(this[(int)(x / scale), (int)(y / scale)]);
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
        FlipX, FlipY, FlipBoth
    }
}
