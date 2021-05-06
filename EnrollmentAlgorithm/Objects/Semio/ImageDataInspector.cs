using Semio.ClientService.OpenXml.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Semio.ClientService.OpenXml
{
    /// <summary>
    ///     Provides access to intrinsic properties of image file formats.
    /// </summary>
    public static class ImageDataInspector
    {
        private const string ErrorMessage = "Could not recognize image format.";

        private static readonly Dictionary<byte[], Func<BinaryReader, Size>> ImageFormatDecoders = new Dictionary<byte[], Func<BinaryReader, Size>>
                                                                                                   {
                                                                                                       { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, DecodePngSize },
                                                                                                       { new byte[] { 0xff, 0xd8 }, DecodeJpegSize },
                                                                                                   };

        /// <summary>
        ///     Gets the dimensions of the image data.
        /// </summary>
        /// <param name="binaryReader">The binary reader.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static Size GetDimensions(BinaryReader binaryReader, out ImageFormat format)
        {
            int maxMagicBytesLength = ImageFormatDecoders.Keys.OrderByDescending(x => x.Length).First().Length;

            var magicBytes = new byte[maxMagicBytesLength];

            for (int i = 0; i < maxMagicBytesLength; i += 1)
            {
                magicBytes[i] = binaryReader.ReadByte();

                foreach (var decoder in ImageFormatDecoders)
                {
                    if (StartsWith(magicBytes, decoder.Key))
                    {
                        format = decoder.Value == DecodeJpegSize ? ImageFormat.Jpeg : ImageFormat.Png;
                        return decoder.Value(binaryReader);
                    }
                }
            }

            throw new ArgumentException(ErrorMessage, "binaryReader");
        }

        private static bool StartsWith(byte[] thisBytes, byte[] thatBytes)
        {
            for (int i = 0; i < thatBytes.Length; i += 1)
            {
                if (thisBytes[i] != thatBytes[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static short ReadLittleEndianInt16(BinaryReader binaryReader)
        {
            var bytes = new byte[sizeof(short)];
            for (int i = 0; i < sizeof(short); i += 1)
            {
                bytes[sizeof(short) - 1 - i] = binaryReader.ReadByte();
            }
            return BitConverter.ToInt16(bytes, 0);
        }

        private static int ReadLittleEndianInt32(BinaryReader binaryReader)
        {
            var bytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i += 1)
            {
                bytes[sizeof(int) - 1 - i] = binaryReader.ReadByte();
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        private static Size DecodePngSize(BinaryReader binaryReader)
        {
            binaryReader.ReadBytes(8);
            int width = ReadLittleEndianInt32(binaryReader);
            int height = ReadLittleEndianInt32(binaryReader);
            return new Size(width, height);
        }

        private static Size DecodeJpegSize(BinaryReader binaryReader)
        {
            while (binaryReader.ReadByte() == 0xff)
            {
                byte marker = binaryReader.ReadByte();
                short chunkLength = ReadLittleEndianInt16(binaryReader);

                if (marker == 0xc0)
                {
                    binaryReader.ReadByte();

                    int height = ReadLittleEndianInt16(binaryReader);
                    int width = ReadLittleEndianInt16(binaryReader);
                    return new Size(width, height);
                }

                binaryReader.ReadBytes(chunkLength - 2);
            }

            throw new ArgumentException(ErrorMessage);
        }

        public static string GetMimeType(this ImageFormat format)
        {
            string mimeType;
            switch (format)
            {
                //case ImageFormat.Jpeg:
                //    mimeType = Restful.ContentTypes.ContentTypeJpeg;
                //    break;

                //case ImageFormat.Png:
                //    mimeType = Restful.ContentTypes.ContentTypePng;
                //    break;

                //case ImageFormat.Gif:
                //    mimeType = Restful.ContentTypes.ContentTypeGif;
                //    break;

                default:
                 //   SLogger.Debug("Unsupported image format.");
                    mimeType = String.Empty;
                    break;
            }
            return mimeType;
        }
    }
}