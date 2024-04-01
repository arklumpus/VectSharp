using System;
using System.IO.Compression;
using System.IO;

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// A PDF stream object.
    /// </summary>
    public class PDFStream : PDFDictionary, IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// The contents of the <see cref="PDFStream"/>.
        /// </summary>
        public MemoryStream Contents { get; protected set; }
        
        /// <summary>
        /// Indicates whether the stream is flate-compressed.
        /// </summary>
        public bool IsCompressed { get; }

        /// <summary>
        /// The lenght of the (compressed) stream.
        /// </summary>
        public PDFInt Length { get; protected set; }

        /// <summary>
        /// If the stream is compressed, the length of the uncompressed stream.
        /// </summary>
        public PDFInt Length1 { get; protected set; }

        /// <summary>
        /// Filter used to decode the stream.
        /// </summary>
        public PDFValueObject Filter { get; protected set; }

        /// <summary>
        /// Create a new <see cref="PDFStream"/> copying the contents of the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> whose contents should be copied to the new <see cref="PDFStream"/> object.</param>
        /// <param name="compressStream">Indicates whether the contents of the stream should be compressed.</param>
        public PDFStream(Stream stream, bool compressStream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            
            this.IsCompressed = compressStream;
            if (compressStream)
            {
                this.Length1 = new PDFInt((int)stream.Length);
                this.Filter = new PDFArray<PDFString>(new PDFString("FlateDecode", PDFString.StringDelimiter.StartingForwardSlash));

                this.Contents = ZLibCompress(stream);

                this.Length = new PDFInt((int)this.Contents.Length);
            }
            else
            {
                this.Contents = new MemoryStream();
                stream.CopyTo(this.Contents);

                this.Length = new PDFInt((int)this.Contents.Length);
                this.Length1 = new PDFInt((int)this.Contents.Length);
                this.Filter = null;
            }
        }

        /// <inheritdoc/>
        public override void FullWrite(Stream stream, StreamWriter writer)
        {
            base.FullWrite(stream, writer);

            writer.Write("\nstream\n");
            writer.Flush();
            this.Contents.Seek(0, SeekOrigin.Begin);
            this.Contents.CopyTo(stream);
            writer.Write("endstream");
            writer.Flush();
        }

        internal static MemoryStream ZLibCompress(Stream contentStream)
        {
            MemoryStream compressedStream = new MemoryStream();
            compressedStream.Write(new byte[] { 0x78, 0x01 }, 0, 2);

            using (DeflateStream deflate = new DeflateStream(compressedStream, CompressionLevel.Optimal, true))
            {
                contentStream.CopyTo(deflate);
            }
            contentStream.Seek(0, SeekOrigin.Begin);

            uint checksum = Adler32(contentStream);

            compressedStream.Write(new byte[] { (byte)((checksum >> 24) & 255), (byte)((checksum >> 16) & 255), (byte)((checksum >> 8) & 255), (byte)(checksum & 255) }, 0, 4);

            compressedStream.Seek(0, SeekOrigin.Begin);

            return compressedStream;
        }

        private static uint Adler32(Stream contentStream)
        {
            uint s1 = 1;
            uint s2 = 0;

            int readByte;

            while ((readByte = contentStream.ReadByte()) >= 0)
            {
                s1 = (s1 + (byte)readByte) % 65521U;
                s2 = (s2 + s1) % 65521U;
            }

            return (s2 << 16) + s1;
        }

        /// <summary>
        /// Release the contents of the <see cref="PDFStream"/>.
        /// </summary>
        /// <param name="disposing">Whether the <see cref="Contents"/> of the <see cref="PDFStream"/> should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Contents.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
