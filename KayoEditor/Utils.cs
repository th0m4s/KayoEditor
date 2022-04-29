using System;

namespace KayoEditor
{
    class Utils
    {
        /// <summary>
        /// Transforme un tableau d'octets en <see cref="uint"/>.
        /// </summary>
        /// <param name="array">Tableau d'octets à convertir.</param>
        /// <param name="offset">Position de départ.</param>
        public static uint LittleEndianToUInt(byte[] array, int offset = 0)
        {
            uint res = 0;
            for (int i = 0; i < 4; i++)
                res += array[offset + i] * (uint)Math.Pow(256, i);

            return res;
        }

        /// <summary>
        /// Transforme un tableau d'octets en <see cref="int"/>.
        /// </summary>
        /// <param name="array">Tableau d'octets à convertir.</param>
        /// <param name="offset">Position de départ.</param>
        public static int LittleEndianToInt(byte[] array, int offset = 0)
        {
            int res = 0;
            for (int i = 0; i < 4; i++)
                res += array[offset + i] * (int)Math.Pow(256, i);

            return res;
        }

        /// <summary>
        /// Transforme un tableau d'octets en <see cref="ushort"/>.
        /// </summary>
        /// <param name="array">Tableau d'octets à convertir.</param>
        /// <param name="offset">Position de départ.</param>
        public static ushort LittleEndianToUShort(byte[] array, int offset = 0)
        {
            ushort res = 0;
            for (int i = 0; i < 2; i++)
                res += (ushort)(array[offset + i] * Math.Pow(256, i));

            return res;
        }

        /// <summary>
        /// Transforme un tableau d'octets en <see cref="short"/>.
        /// </summary>
        /// <param name="array">Tableau d'octets à convertir.</param>
        /// <param name="offset">Position de départ.</param>
        public static short LittleEndianToShort(byte[] array, int offset = 0)
        {
            short res = 0;
            for (int i = 0; i < 2; i++)
                res += (short)(array[offset + i] * Math.Pow(256, i));

            return res;
        }
        
        /// <summary>
        /// Transforme un <see cref="int"/> en tableau d'octets.
        /// </summary>
        /// <param name="value">Valeur à convertir.</param>
        public static byte[] IntToLittleEndian(int value)
        {
            byte[] result = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = (byte)(value % 256);
                value /= 256;
            }

            return result;
        }

        /// <summary>
        /// Transforme un <see cref="uint"/> en tableau d'octets.
        /// </summary>
        /// <param name="value">Valeur à convertir.</param>
        public static byte[] UIntToLittleEndian(uint value)
        {
            byte[] result = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = (byte)(value % 256);
                value >>= 8; // value /= 256;
            }

            return result;
        }

        /// <summary>
        /// Transforme un <see cref="short"/> en tableau d'octets.
        /// </summary>
        /// <param name="value">short à convertir.</param>
        public static byte[] ShortToLittleEndian(short value)
        {
            byte[] result = new byte[2];
            for (int i = 0; i < 2; i++)
            {
                result[i] = (byte)(value % 256);
                value /= 256;
            }

            return result;
        }

        /// <summary>
        /// Transforme un <see cref="ushort"/> en tableau d'octets.
        /// </summary>
        /// <param name="value">ushort à convertir.</param>
        public static byte[] UShortToLittleEndian(ushort value)
        {
            byte[] result = new byte[2];
            for (int i = 0; i < 2; i++)
            {
                result[i] = (byte)(value % 256);
                value /= 256;
            }

            return result;
        }
    }
}
