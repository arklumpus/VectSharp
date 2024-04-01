/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2024 Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System.IO;
using System.Text;

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// PDF representation of the alpha channel of a raster image.
    /// </summary>
    public class PDFImageAlpha : PDFStream
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("XObject", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// XObject subtype.
        /// </summary>
        public PDFString Subtype { get; } = new PDFString("Image", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Image width.
        /// </summary>
        public PDFInt Width { get; }
        
        /// <summary>
        /// Image height.
        /// </summary>
        public PDFInt Height { get; }
        
        /// <summary>
        /// Colour space.
        /// </summary>
        public PDFString ColorSpace { get; } = new PDFString("DeviceGray", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Bits per component.
        /// </summary>
        public PDFInt BitsPerComponent { get; } = new PDFInt(8);
        
        /// <summary>
        /// Determines whether the image should be interpolated when it is rescaled.
        /// </summary>
        public PDFBool Interpolate { get; }

        /// <summary>
        /// Create a new <see cref="PDFImageAlpha"/> from the specified <paramref name="image"/>, assuming that it contains alpha information.
        /// </summary>
        /// <param name="image">The <see cref="RasterImage"/> whose alpha channel will be represented by the new <see cref="PDFImageAlpha"/> object.</param>
        /// <param name="compressStream">Indicates whether the image stream should be compressed.</param>
        public PDFImageAlpha(RasterImage image, bool compressStream) : base(new MemoryStream(), false)
        {
            this.Width = new PDFInt(image.Width);
            this.Height = new PDFInt(image.Height);
            this.Interpolate = new PDFBool(image.Interpolate);

            MemoryStream alphaStream = this.Contents;

            unsafe
            {
                byte* dataPointer = (byte*)image.ImageDataAddress;

                if (compressStream)
                {
                    this.Filter = new PDFString("FlateDecode", PDFString.StringDelimiter.StartingForwardSlash);

                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int x = 0; x < image.Width; x++)
                        {
                            dataPointer += 3;
                            alphaStream.WriteByte(*dataPointer);
                            dataPointer++;
                        }
                    }

                    alphaStream.Seek(0, SeekOrigin.Begin);
                    MemoryStream compressed = PDFStream.ZLibCompress(alphaStream);
                    alphaStream.Dispose();
                    this.Contents = compressed;
                }
                else
                {
                    this.Filter = new PDFString("ASCIIHexDecode", PDFString.StringDelimiter.StartingForwardSlash);

                    using (StreamWriter imageWriter = new StreamWriter(alphaStream, Encoding.ASCII, 1024, true))
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                dataPointer += 3;
                                imageWriter.Write((*dataPointer).ToString("X2"));
                                dataPointer++;
                            }
                        }
                    }
                }
            }

            this.Length = new PDFInt((int)this.Contents.Length);
            this.Length1 = null;
        }
    }

    /// <summary>
    /// PDF representation of an opaque RGB raster image.
    /// </summary>
    public class PDFImage : PDFStream
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("XObject", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// XObject subtype.
        /// </summary>
        public PDFString Subtype { get; } = new PDFString("Image", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Image width.
        /// </summary>
        public PDFInt Width { get; }

        /// <summary>
        /// Image height.
        /// </summary>
        public PDFInt Height { get; }

        /// <summary>
        /// Image colour space.
        /// </summary>
        public PDFString ColorSpace { get; } = new PDFString("DeviceRGB", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Bits per component.
        /// </summary>
        public PDFInt BitsPerComponent { get; } = new PDFInt(8);
        
        /// <summary>
        /// Indicates whether the image should be interpolated when it is resized.
        /// </summary>
        public PDFString Interpolate { get; }

        /// <summary>
        /// Alpha channel (or <see langword="null"/> if the image is opaque).
        /// </summary>
        public PDFImageAlpha SMask { get; }
        
        /// <summary>
        /// Name used to refer to the image within content streams.
        /// </summary>
        public string ReferenceName { get; }

        /// <summary>
        /// Create a new <see cref="PDFImage"/> from the specified <see cref="RasterImage"/>.
        /// </summary>
        /// <param name="image">The <see cref="RasterImage"/> that will be contained in the new <see cref="PDFImage"/> object.</param>
        /// <param name="imageAlpha">The alpha channel of the image (or <see langword="null"/> if the image is opaque).</param>
        /// <param name="compressStream">Indicates whether the image stream should be compressed.</param>
        /// <param name="referenceName">The name used to refer to the image within content streams.</param>
        public PDFImage(RasterImage image, PDFImageAlpha imageAlpha, bool compressStream, string referenceName) : base(new MemoryStream(), false)
        {
            this.Width = new PDFInt(image.Width);
            this.Height = new PDFInt(image.Height);
            this.Interpolate = new PDFString(image.Interpolate ? "true" : "false", PDFString.StringDelimiter.None);

            this.SMask = imageAlpha;

            MemoryStream imageStream = this.Contents;

            unsafe
            {
                byte* dataPointer = (byte*)image.ImageDataAddress;

                if (compressStream)
                {
                    this.Filter = new PDFString("FlateDecode", PDFString.StringDelimiter.StartingForwardSlash);

                    if (image.HasAlpha)
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                imageStream.WriteByte(*dataPointer);
                                dataPointer++;
                                imageStream.WriteByte(*dataPointer);
                                dataPointer++;
                                imageStream.WriteByte(*dataPointer);
                                dataPointer++;
                                dataPointer++;
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                imageStream.WriteByte(*dataPointer);
                                dataPointer++;
                                imageStream.WriteByte(*dataPointer);
                                dataPointer++;
                                imageStream.WriteByte(*dataPointer);
                                dataPointer++;
                            }
                        }
                    }

                    imageStream.Seek(0, SeekOrigin.Begin);
                    MemoryStream compressed = ZLibCompress(imageStream);
                    imageStream.Dispose();
                    this.Contents = compressed;
                }
                else
                {
                    this.Filter = new PDFString("ASCIIHexDecode", PDFString.StringDelimiter.StartingForwardSlash);

                    using (StreamWriter imageWriter = new StreamWriter(imageStream, Encoding.ASCII, 1024, true))
                    {
                        if (image.HasAlpha)
                        {
                            for (int y = 0; y < image.Height; y++)
                            {
                                for (int x = 0; x < image.Width; x++)
                                {
                                    imageWriter.Write((*dataPointer).ToString("X2"));
                                    dataPointer++;
                                    imageWriter.Write((*dataPointer).ToString("X2"));
                                    dataPointer++;
                                    imageWriter.Write((*dataPointer).ToString("X2"));
                                    dataPointer++;
                                    dataPointer++;
                                }
                            }
                        }
                        else
                        {
                            for (int y = 0; y < image.Height; y++)
                            {
                                for (int x = 0; x < image.Width; x++)
                                {
                                    imageWriter.Write((*dataPointer).ToString("X2"));
                                    dataPointer++;
                                    imageWriter.Write((*dataPointer).ToString("X2"));
                                    dataPointer++;
                                    imageWriter.Write((*dataPointer).ToString("X2"));
                                    dataPointer++;
                                }
                            }
                        }
                    }
                }
            }

            this.Length = new PDFInt((int)this.Contents.Length);
            this.Length1 = null;
            ReferenceName = referenceName;
        }
    }
}
