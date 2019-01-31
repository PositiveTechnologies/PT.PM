using System;
using MessagePack;
using MessagePack.LZ4;

namespace PT.PM.Common.MessagePack
{
    public static class MessagePackUtils
    {
        private const sbyte ExtensionTypeCode = 99;

        [ThreadStatic]
        private static byte[] lz4buffer;

        public static byte[] UnpackDataIfRequired(byte[] bytes)
        {
            if (MessagePackBinary.GetMessagePackType(bytes, 0) == MessagePackType.Extension)
            {
                var header = MessagePackBinary.ReadExtensionFormatHeader(bytes, 0, out var readSize);
                if (header.TypeCode == ExtensionTypeCode)
                {
                    // decode lz4
                    int offset = readSize;
                    int length = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                    offset += readSize;

                    byte[] buffer = GetLz4Buffer();
                    if (buffer.Length < length)
                    {
                        buffer = new byte[length];
                    }

                    // LZ4 Decode
                    int len = bytes.Length - offset;
                    LZ4Codec.Decode(bytes, offset, len, buffer, 0, length);

                    return buffer;
                }
            }

            return bytes;
        }

        private static byte[] GetLz4Buffer()
        {
            return lz4buffer ?? (lz4buffer = new byte[GetLz4MaximumOutputLength(65536)]);
        }

        private static int GetLz4MaximumOutputLength(int inputLength)
        {
            return inputLength + inputLength / 255 + 16;
        }
    }
}