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

using System.Collections;
using System.Collections.Generic;

namespace VectSharp.PDF
{
    /// <summary>
    /// Describes the appearance of a PDF annotation.
    /// </summary>
    public class AnnotationStyle
    {
        /// <summary>
        /// Thickness of the annotation border.
        /// </summary>
        public double BorderWidth { get; set; } = 1;

        /// <summary>
        /// Dash style for the annotation border.
        /// </summary>
        public LineDash? BorderDash { get; set; } = null;

        /// <summary>
        /// Colour for the annotation border.
        /// </summary>
        public Colour BorderColour { get; set; } = Colours.Black;


        /// <summary>
        /// Create a new <see cref="AnnotationStyle"/>.
        /// </summary>
        public AnnotationStyle(double borderWidth = 1, LineDash? borderDash = null, Colour? borderColour = default)
        {
            BorderWidth = borderWidth;
            BorderDash = borderDash;
            BorderColour = borderColour ?? Colours.Black;
        }
    }

    /// <summary>
    /// Represents annotation style settings.
    /// </summary>
    public class AnnotationStyleCollection : IDictionary<string, AnnotationStyle>
    {
        /// <summary>
        /// The default annotation style.
        /// </summary>
        public AnnotationStyle DefaultStyle { get; set; } = new AnnotationStyle();

        /// <summary>
        /// Annotation style overrides for individual annotations.
        /// </summary>
        public Dictionary<string, AnnotationStyle> Styles { get; set; }

        /// <inheritdoc/>
        public ICollection<string> Keys => ((IDictionary<string, AnnotationStyle>)Styles).Keys;

        /// <inheritdoc/>
        public ICollection<AnnotationStyle> Values => ((IDictionary<string, AnnotationStyle>)Styles).Values;

        /// <inheritdoc/>
        public int Count => ((ICollection<KeyValuePair<string, AnnotationStyle>>)Styles).Count;

        /// <inheritdoc/>
        public bool IsReadOnly => ((ICollection<KeyValuePair<string, AnnotationStyle>>)Styles).IsReadOnly;

        /// <inheritdoc/>
        public AnnotationStyle this[string key] { get => ((IDictionary<string, AnnotationStyle>)Styles)[key]; set => ((IDictionary<string, AnnotationStyle>)Styles)[key] = value; }

        /// <summary>
        /// Create a new set of <see cref="AnnotationStyleCollection"/>.
        /// </summary>
        /// <param name="defaultStyle">The default <see cref="AnnotationStyle"/>. If this is not provided or set to <see langword="null"/>, default values will be used.</param>
        public AnnotationStyleCollection(AnnotationStyle defaultStyle = null)
        {
            if (defaultStyle == null)
            {
                defaultStyle = new AnnotationStyle();
            }
            this.DefaultStyle = defaultStyle;

            this.Styles = new Dictionary<string, AnnotationStyle>();
        }

        /// <inheritdoc/>
        public void Add(string key, AnnotationStyle value)
        {
            ((IDictionary<string, AnnotationStyle>)Styles).Add(key, value);
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, AnnotationStyle>)Styles).ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            return ((IDictionary<string, AnnotationStyle>)Styles).Remove(key);
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, out AnnotationStyle value)
        {
            return ((IDictionary<string, AnnotationStyle>)Styles).TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<string, AnnotationStyle> item)
        {
            ((ICollection<KeyValuePair<string, AnnotationStyle>>)Styles).Add(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            ((ICollection<KeyValuePair<string, AnnotationStyle>>)Styles).Clear();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, AnnotationStyle> item)
        {
            return ((ICollection<KeyValuePair<string, AnnotationStyle>>)Styles).Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, AnnotationStyle>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, AnnotationStyle>>)Styles).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<string, AnnotationStyle> item)
        {
            return ((ICollection<KeyValuePair<string, AnnotationStyle>>)Styles).Remove(item);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, AnnotationStyle>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, AnnotationStyle>>)Styles).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Styles).GetEnumerator();
        }
    }
}
