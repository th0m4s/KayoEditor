using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KayoEditor
{
    class Utils
    {
        public static uint LittleEndianToUInt(byte[] array, int offset = 0)
        {
            uint res = 0;
            for (int i = 0; i < 4; i++)
                res += array[offset + i] * (uint)Math.Pow(256, i);

            return res;
        }

        public static int LittleEndianToInt(byte[] array, int offset = 0)
        {
            int res = 0;
            for (int i = 0; i < 4; i++)
                res += array[offset + i] * (int)Math.Pow(256, i);

            return res;
        }

        public static ushort LittleEndianToUShort(byte[] array, int offset = 0)
        {
            ushort res = 0;
            for (int i = 0; i < 2; i++)
                res += (ushort)(array[offset + i] * Math.Pow(256, i));

            return res;
        }

        public static short LittleEndianToShort(byte[] array, int offset = 0)
        {
            short res = 0;
            for (int i = 0; i < 2; i++)
                res += (short)(array[offset + i] * Math.Pow(256, i));

            return res;
        }

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
