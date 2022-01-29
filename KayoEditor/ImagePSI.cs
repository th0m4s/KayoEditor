using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KayoEditor
{
    class ImagePSI
    {
        public const int OFFSET_TYPE = 0x00;
        public const int OFFSET_FILESIZE = 0x02;
        public const int OFFSET_STARTOFFSET = 0x0a;

        public const int OFFSET_INFOHEADERSIZE = 0x0e;
        public const int OFFSET_WIDTH = 0x12;
        public const int OFFSET_HEIGHT = 0x16;
        public const int OFFSET_COLORPLANES = 0x1a;
        public const int OFFSET_COLORDEPTH = 0x1c;

        byte[] header;
        Pixel[,] _pixels;

        public string Type => Encoding.ASCII.GetString(header.ExtractBytes(2, OFFSET_TYPE));
        public uint FileSize => Utils.LittleEndianToUInt(header, OFFSET_FILESIZE);
        public uint StartOffset => Utils.LittleEndianToUInt(header, OFFSET_STARTOFFSET);

        public uint InfoHeaderSize => Utils.LittleEndianToUInt(header, OFFSET_INFOHEADERSIZE);
        public int Width => Utils.LittleEndianToInt(header, OFFSET_WIDTH);
        public int Height => Utils.LittleEndianToInt(header, OFFSET_HEIGHT);
        public ushort ColorPlanes => Utils.LittleEndianToUShort(header, OFFSET_COLORPLANES);
        public ushort ColorDepth => Utils.LittleEndianToUShort(header, OFFSET_COLORDEPTH);
        int PaddingLength => (4 - (Width * (ColorDepth / 8) % 4)) % 4;

        public ImagePSI(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                header = stream.ReadBytes(54);

                _pixels = new Pixel[Height, Width];
                int padding = PaddingLength;

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        byte[] color = stream.ReadBytes(3);
                        _pixels[Height - y - 1, x] = new Pixel(color[2], color[1], color[0]);
                    }

                    stream.ReadBytes(padding);
                }
            }
        }

        public ImagePSI(int width, int height, Pixel defaultColor = null)
        {
            if (defaultColor == null)
                defaultColor = new Pixel();

            header = new byte[54];

            header.InsertBytes(new byte[] { 0x42, 0x4d }, OFFSET_TYPE);
            header.InsertBytes(Utils.UIntToLittleEndian(0x36), OFFSET_STARTOFFSET);


            header.InsertBytes(Utils.UShortToLittleEndian(0x28), OFFSET_INFOHEADERSIZE);
            header.InsertBytes(Utils.IntToLittleEndian(width), OFFSET_WIDTH);
            header.InsertBytes(Utils.IntToLittleEndian(height), OFFSET_HEIGHT);
            header.InsertBytes(Utils.UShortToLittleEndian(1), OFFSET_COLORPLANES);
            header.InsertBytes(Utils.UShortToLittleEndian(24), OFFSET_COLORDEPTH);

            // OFFSET_STARTHEADER c'est le début des pixels, donc égal à la taille de l'entête
            header.InsertBytes(Utils.UIntToLittleEndian((uint)(OFFSET_STARTOFFSET + height * (width * ColorDepth / 8 + PaddingLength))), OFFSET_FILESIZE);

            _pixels = new Pixel[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _pixels[y, x] = defaultColor;
                }
            }
        }

        public ImagePSI(ImagePSI original)
        {
            header = new byte[original.header.Length];
            Array.Copy(original.header, header, header.Length);

            _pixels = new Pixel[original._pixels.GetLength(0), original._pixels.GetLength(1)];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _pixels[y, x] = new Pixel(original._pixels[y, x]);
                }
            }
        }

        public ImagePSI Copy()
        {
            return new ImagePSI(this);
        }

        public Pixel this[int x, int y] // la propriété c'est l'instance elle-même
        { // instance[x, y] <=> instance._pixels[y, x]
            get
            {
                return _pixels[y, x];
            }

            set
            {
                _pixels[y, x] = value;
            }
        }

        public void Save(string filename)
        {
            using (FileStream stream = File.OpenWrite(filename))
            {
                stream.Write(header, 0, header.Length);
                byte[] padding = new byte[PaddingLength];

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Pixel pixel = this[x, Height - y - 1];
                        byte[] color = { pixel.B, pixel.G, pixel.R };
                        stream.Write(color, 0, color.Length);
                    }

                    stream.Write(padding, 0, padding.Length);
                }
            }
        }

        public ImagePSI Greyscale()
        {
            ImagePSI result = this.Copy();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Pixel pixel = this[x, y];
                    byte avg = (byte)((pixel.R + pixel.G + pixel.B) / 3);
                    result[x, y] = new Pixel(avg, avg, avg);
                }
            }

            return result;
        }

        public ImagePSI Scale(float scale)
        {
            if (scale == 0)
                throw new ArgumentOutOfRangeException("scale must not be 0");

            if (scale < 0)
                throw new ArgumentOutOfRangeException("scale must be a positive number");

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
