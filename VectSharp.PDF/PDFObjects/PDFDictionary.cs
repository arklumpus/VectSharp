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

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// Base class for PDF dictionary objects.
    /// </summary>
    public abstract partial class PDFDictionary : PDFReferenceableObject
    {
        /// <inheritdoc/>
        public override void FullWrite(Stream stream, StreamWriter writer)
        {
            Dictionary<string, Func<PDFDictionary, IPDFObject>> getters = Getters[this.GetType().FullName];

            writer.Write("<<");
            foreach (KeyValuePair<string, Func<PDFDictionary, IPDFObject>> kvp in getters)
            {
                IPDFObject val = kvp.Value(this);

                if (val != null)
                {
                    writer.Write(" /");
                    writer.Write(kvp.Key);
                    writer.Write(" ");
                    writer.Flush();
                    val.Write(stream, writer);
                }
            }

            writer.Write(" >>");
            writer.Flush();
        }
    }

    /// <summary>
    /// Raw PDF dictionary object represented as a set of keys and values.
    /// </summary>
    public class PDFRawDictionary : PDFReferenceableObject
    {
        /// <summary>
        /// Key-value pairs in the dictionary. A forward slash will be automatically prepended to key names.
        /// </summary>
        public Dictionary<string, IPDFObject> Keys { get; } = new Dictionary<string, IPDFObject>();

        /// <inheritdoc/>
        public override void FullWrite(Stream stream, StreamWriter writer)
        {
            writer.Write("<<");
            foreach (KeyValuePair<string, IPDFObject> kvp in Keys)
            {
                writer.Write(" /");
                writer.Write(kvp.Key);
                writer.Write(" ");

                writer.Flush();
                kvp.Value.Write(stream, writer);
            }

            writer.Write(" >>");
            writer.Flush();
        }
    }
}
