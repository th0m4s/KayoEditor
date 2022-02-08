﻿using System.IO;

namespace KayoEditor
{
    public static class Extensions
    {
        public static byte[] ReadBytes(this FileStream stream, int length) // ReadBytes(stream, 3) <=> stream.ReadBytes(3)
        {
            byte[] array = new byte[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = (byte)stream.ReadByte();
            }


            return array;
        }

        public static byte[] ExtractBytes(this byte[] array, int length, int offset = 0)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
                bytes[i] = array[offset + i];

            return bytes;
        }

        public static void InsertBytes(this byte[] array, byte[] data, int offsetTo = 0, int offsetFrom = 0, int length = -1)
        {
            if (length < 0)
                length = data.Length;

            for (int i = 0; i < length; i++)
            {
                array[offsetTo + i] = data[offsetFrom + i];
            }
        }
    }
}
