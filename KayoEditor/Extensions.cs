using System.IO;

namespace KayoEditor
{
    public static class Extensions
    {
        /// <summary>
        /// Lit <paramref name="length"/> octets depuis le flux d'entrée.
        /// </summary>
        /// <param name="stream">Instance du flux à lire.</param>
        /// <param name="length">Nombre <i>n</i> d'octets à recupérer.</param>
        /// <returns>Un tableau de <paramref name="length"/> octets provenant de ce flux.</returns>
        public static byte[] ReadBytes(this Stream stream, int length) // ReadBytes(stream, 3) <=> stream.ReadBytes(3)
        {
            byte[] array = new byte[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = (byte)stream.ReadByte();
            }


            return array;
        }

        /// <summary>
        /// Extrait une partie d'un tableau d'octets.
        /// </summary>
        /// <param name="array">Tableau source.</param>
        /// <param name="length">Nombre d'octets à extraire.</param>
        /// <param name="offset">Position à partir de laquelle extraire ces octets.</param>
        /// <returns>Un tableau contenant <paramref name="length"/> octets extraits.</returns>
        public static byte[] ExtractBytes(this byte[] array, int length, int offset = 0)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
                bytes[i] = array[offset + i];

            return bytes;
        }

        /// <summary>
        /// Modifie une partie d'un tableau d'octets à partir d'un autre tableau d'octets <paramref name="data"/>.
        /// </summary>
        /// <param name="array">Tableau d'octets à modifier.</param>
        /// <param name="data">Tableau d'octets à inserer.</param>
        /// <param name="offsetTo">Position de départ dans le tableau de destination.</param>
        /// <param name="offsetFrom">Position de départ dans le tableau source.</param>
        /// <param name="length">Nombre d'octets à copier entre les tableaux.</param>
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
