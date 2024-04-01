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
using System.Linq.Expressions;

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// Base class for PDF dictionary objects.
    /// </summary>
    public abstract class PDFDictionary : PDFReferenceableObject
    {
        internal static readonly object GettersLock = new object();
        internal static readonly Dictionary<string, Dictionary<string, Func<PDFDictionary, IPDFObject>>> Getters = new Dictionary<string, Dictionary<string, Func<PDFDictionary, IPDFObject>>>();

        internal static void Initialize(Type t)
        {
            Dictionary<string, Func<PDFDictionary, IPDFObject>> getters = new Dictionary<string, Func<PDFDictionary, IPDFObject>>();

            foreach (System.Reflection.PropertyInfo pinfo in t.GetProperties().Where(x => typeof(IPDFObject).IsAssignableFrom(x.PropertyType)))
            {
                ParameterExpression obj = Expression.Parameter(typeof(PDFDictionary));

                Expression<Func<PDFDictionary, IPDFObject>> getterExpression = Expression.Lambda<Func<PDFDictionary, IPDFObject>>(Expression.Convert(Expression.Call(Expression.Convert(obj, t), pinfo.GetGetMethod()), typeof(IPDFObject)), obj);

                getters[pinfo.Name] = getterExpression.Compile();
            }

            Getters[t.FullName] = getters;
        }

        /// <inheritdoc/>
        public override void FullWrite(Stream stream, StreamWriter writer)
        {
            string typeName = this.GetType().FullName;

            Dictionary<string, Func<PDFDictionary, IPDFObject>> getters;

            lock (GettersLock)
            {
                if (!Getters.TryGetValue(typeName, out getters))
                {
                    Initialize(this.GetType());
                    getters = Getters[typeName];
                }
            }

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
