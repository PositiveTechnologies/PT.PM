using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PT.PM
{
    public class StringCompressorEscaper
    {
        public static string CompressEscape(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            byte[] compressedData = null;
            using (var memoryStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, true))
                {
                    gZipStream.Write(buffer, 0, buffer.Length);
                }
                compressedData = new byte[memoryStream.Length + 4];
                memoryStream.Position = 0;
                memoryStream.Read(compressedData, 4, (int)memoryStream.Length);
            }
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, compressedData, 0, 4);
            return Convert.ToBase64String(compressedData).Replace('/', '@');
        }

        public static string UnescapeDecompress(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText.Replace('@', '/'));
            byte[] buffer = null;
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                buffer = new byte[dataLength];
                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }
            }

            return Encoding.UTF8.GetString(buffer);
        }
    }
}
