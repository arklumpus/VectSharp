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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Contains types describing PDF document primitives.
/// </summary>
namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// Represents a PDF object.
    /// </summary>
    public interface IPDFObject
    {
        /// <summary>
        /// Writes the <see cref="IPDFObject"/> to a stream.
        /// </summary>
        /// <param name="stream">The stream on which the <see cref="IPDFObject"/> should be written.</param>
        /// <param name="writer">A <see cref="StreamWriter"/> used to write text to the stream. Always call <see cref="StreamWriter.Flush"/> before returning from this method!</param>
        void Write(Stream stream, StreamWriter writer);
    }

    /// <summary>
    /// An <see cref="IPDFObject"/> representing a value that cannot be referred to on its own.
    /// </summary>
    public abstract class PDFValueObject : IPDFObject
    {
        /// <inheritdoc/>
        public abstract void Write(Stream stream, StreamWriter writer);
    }

    /// <summary>
    /// An <see cref="IPDFObject"/> that can be inlined or referred to by an object number.
    /// </summary>
    public abstract class PDFReferenceableObject : IPDFObject
    {
        internal virtual int ObjectNumber { get; set; } = -1;
        
        /// <summary>
        /// PDF object generation.
        /// </summary>
        public virtual int Generation { get; set; } = 0;

        /// <summary>
        /// Write a full representation of the object to the specified <paramref name="stream"/>, using the <paramref name="writer"/> if necessary.
        /// </summary>
        /// <param name="stream">The stream on which the <see cref="PDFReferenceableObject"/> should be written.</param>
        /// <param name="writer">A <see cref="StreamWriter"/> used to write text to the stream. Always call <see cref="StreamWriter.Flush"/> before returning from this method!</param>
        public abstract void FullWrite(Stream stream, StreamWriter writer);

        /// <summary>
        /// If the <see cref="PDFReferenceableObject"/> is referenced in the object catalog, write an indirect reference to it. Otherwise, calls <see cref="FullWrite(Stream, StreamWriter)"/> to write a full representation.
        /// </summary>
        /// <param name="stream">The stream on which the <see cref="PDFReferenceableObject"/> should be written.</param>
        /// <param name="writer">A <see cref="StreamWriter"/> used to write text to the stream. Always call <see cref="StreamWriter.Flush"/> before returning from this method!</param>
        public virtual void Write(Stream stream, StreamWriter writer)
        {
            if (this.ObjectNumber >= 0)
            {
                writer.Write(this.ObjectNumber);
                writer.Write(" ");
                writer.Write(this.Generation);
                writer.Write(" R");
                writer.Flush();
            }
            else
            {
                FullWrite(stream, writer);
            }
        }
    }

    /// <summary>
    /// A PDF string.
    /// </summary>
    public class PDFString : PDFValueObject
    {
        /// <summary>
        /// Delimiter used for the string.
        /// </summary>
        public enum StringDelimiter
        {
            /// <summary>
            /// No delimiter.
            /// </summary>
            None,

            /// <summary>
            /// A single forward slash (<c>/</c>).
            /// </summary>
            StartingForwardSlash,

            /// <summary>
            /// Brackets at the start and end of the string (<c>(</c> and <c>)</c>).
            /// </summary>
            Brackets
        }

        /// <summary>
        /// The string delimiter.
        /// </summary>
        public StringDelimiter Delimiter { get; }

        /// <summary>
        /// The value of the string.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Create a new <see cref="PDFString"/> with the specified <paramref name="value"/> and <paramref name="delimiter"/>.
        /// </summary>
        /// <param name="value">The value of the string.</param>
        /// <param name="delimiter">The string delimiter.</param>
        public PDFString(string value, StringDelimiter delimiter)
        {
            this.Value = value;
            this.Delimiter = delimiter;
        }

        /// <inheritdoc/>
        public override void Write(Stream stream, StreamWriter writer)
        {
            switch (Delimiter)
            {
                case StringDelimiter.StartingForwardSlash:
                    writer.Write("/");
                    writer.Write(EscapeStringForPDF(Value));
                    writer.Flush();
                    break;
                case StringDelimiter.Brackets:
                    writer.Write("(");
                    writer.Write(EscapeStringForPDF(Value));
                    writer.Write(")");
                    writer.Flush();
                    break;
                case StringDelimiter.None:
                default:
                    writer.Write(EscapeStringForPDF(Value));
                    writer.Flush();
                    break;
            }
        }

        internal static string EscapeStringForPDF(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];

                if (PDFContextInterpreter.CP1252Chars.Contains(ch))
                {
                    if ((int)ch < 128)
                    {
                        if (!"\n\r\t\b\f()\\".Contains(ch))
                        {
                            sb.Append(ch);
                        }
                        else
                        {
                            switch (ch)
                            {
                                case '\n':
                                    sb.Append("\\n");
                                    break;
                                case '\r':
                                    sb.Append("\\r");
                                    break;
                                case '\t':
                                    sb.Append("\\t");
                                    break;
                                case '\b':
                                    sb.Append("\\b");
                                    break;
                                case '\f':
                                    sb.Append("\\f");
                                    break;
                                case '\\':
                                    sb.Append("\\\\");
                                    break;
                                case '(':
                                    sb.Append("\\(");
                                    break;
                                case ')':
                                    sb.Append("\\)");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        string octal = Convert.ToString((int)ch, 8);
                        while (octal.Length < 3)
                        {
                            octal = "0" + octal;
                        }
                        sb.Append("\\" + octal);
                    }
                }
                else
                {
                    sb.Append('?');
                }
            }
            return sb.ToString();
        }
        
        /// <inheritdoc/>
        public override string ToString()
        {
            switch (Delimiter)
            {
                case StringDelimiter.StartingForwardSlash:
                    return "/" + EscapeStringForPDF(this.Value);
                case StringDelimiter.Brackets:
                    return "(" + EscapeStringForPDF(this.Value) + ")";
                case StringDelimiter.None:
                default:
                    return EscapeStringForPDF(this.Value);
            }
        }
    }

    /// <summary>
    /// A PDF floating point number.
    /// </summary>
    public class PDFDouble : PDFString
    {
        /// <summary>
        /// The value represented by this object.
        /// </summary>
        public new double Value { get; }

        /// <summary>
        /// Create a new <see cref="PDFDouble"/> holding the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value represented by the new <see cref="PDFDouble"/> object.</param>
        public PDFDouble(double value) : base(value.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture), StringDelimiter.None)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// A PDF integer number.
    /// </summary>
    public class PDFInt : PDFString
    {
        /// <summary>
        /// The value represented by this object.
        /// </summary>
        public new int Value { get; }

        /// <summary>
        /// Create a new <see cref="PDFInt"/> holding the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value represented by the new <see cref="PDFInt"/> object.</param>
        public PDFInt(int value) : base(value.ToString(System.Globalization.CultureInfo.InvariantCulture), StringDelimiter.None)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// A PDF unsigned integer number.
    /// </summary>
    public class PDFUInt : PDFString
    {
        /// <summary>
        /// The value represented by this object.
        /// </summary>
        public new uint Value { get; }

        /// <summary>
        /// Create a new <see cref="PDFUInt"/> holding the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value represented by the new <see cref="PDFUInt"/> object.</param>
        public PDFUInt(uint value) : base(value.ToString(System.Globalization.CultureInfo.InvariantCulture), StringDelimiter.None)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// A PDF boolean value.
    /// </summary>
    public class PDFBool : PDFString
    {
        /// <summary>
        /// The value represented by this object.
        /// </summary>
        public new bool Value { get; }

        /// <summary>
        /// Create a new <see cref="PDFBool"/> holding the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value represented by the new <see cref="PDFBool"/> object.</param>
        public PDFBool(bool value) : base(value ? "true" : "false", StringDelimiter.None)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// An array of PDF objects.
    /// </summary>
    /// <typeparam name="T">The type of PDF objects contained in the array. Arrays of mixed types can be created by using an abstract type.</typeparam>
    public class PDFArray<T> : PDFValueObject where T : IPDFObject
    {
        /// <summary>
        /// Values contained in the <see cref="PDFArray{T}"/>.
        /// </summary>
        public List<T> Values { get; }

        /// <summary>
        /// Create a new <see cref="PDFArray{T}"/> containing the specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">The items contained in the array.</param>
        public PDFArray(IEnumerable<T> items)
        {
            this.Values = items.ToList();
        }

        /// <summary>
        /// Create a new <see cref="PDFArray{T}"/> containing the specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">The items contained in the array.</param>
        public PDFArray(params T[] items) : this((IEnumerable<T>)items) { }

        /// <inheritdoc/>
        public override void Write(Stream stream, StreamWriter writer)
        {
            writer.Write("[ ");
            foreach (var item in Values)
            {
                writer.Flush();
                item.Write(stream, writer);
                writer.Write(" ");
            }
            writer.Write("]");
            writer.Flush();
        }
    }
}
