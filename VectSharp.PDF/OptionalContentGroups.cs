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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectSharp.PDF.OptionalContentGroups
{
    /// <summary>
    /// Boolean operators for <see cref="OptionalContentGroup"/>s.
    /// </summary>
    public enum OptionalContentGroupBooleanOperation
    {
        /// <summary>
        /// Logical conjunction.
        /// </summary>
        And,

        /// <summary>
        /// Logical disjunction.
        /// </summary>
        Or,

        /// <summary>
        /// Logical negation.
        /// </summary>
        Not,

        /// <summary>
        /// Logical identity.
        /// </summary>
        Identity
    }

    /// <summary>
    /// Represents the result of a boolean operation on <see cref="OptionalContentGroup"/>s.
    /// </summary>
    public abstract class OptionalContentGroupExpression
    {
        /// <summary>
        /// The operation performed by the expression.
        /// </summary>
        public abstract OptionalContentGroupBooleanOperation Operation { get; }

        /// <summary>
        /// The arguments of the expression.
        /// </summary>
        public virtual IReadOnlyList<OptionalContentGroupExpression> Arguments { get; }

        /// <summary>
        /// Create a new <see cref="OptionalContentGroupExpression"/> with the specified <paramref name="arguments"/>.
        /// </summary>
        /// <param name="arguments">The arguments of the expression.</param>
        protected OptionalContentGroupExpression(params OptionalContentGroupExpression[] arguments)
        {
            this.Arguments = arguments;
        }

        /// <summary>
        /// Negates an <see cref="OptionalContentGroupExpression"/>.
        /// </summary>
        /// <param name="ocge">The <see cref="OptionalContentGroupExpression"/> to be negated.</param>
        public static OptionalContentGroupNotExpression operator !(OptionalContentGroupExpression ocge)
        {
            return new OptionalContentGroupNotExpression(ocge);
        }

        /// <summary>
        /// Used to enable short circuit operators.
        /// </summary>
        /// <param name="_">The expression.</param>
        /// <returns>Always <see langword="false"/>.</returns>
        public static bool operator false(OptionalContentGroupExpression _)
        {
            return false;
        }

        /// <summary>
        /// Used to enable short circuit operators.
        /// </summary>
        /// <param name="_">The expression.</param>
        /// <returns>Always <see langword="false"/>.</returns>
        public static bool operator true(OptionalContentGroupExpression _)
        {
            return false;
        }

        /// <summary>
        /// Performs a conjunction of two <see cref="OptionalContentGroupExpression"/>s.
        /// </summary>
        /// <param name="ocge1">The first expression.</param>
        /// <param name="ocge2">The second expression.</param>
        /// <returns>An <see cref="OptionalContentGroupExpression"/> representing the conjunction of the two expressions.</returns>
        public static OptionalContentGroupExpression operator &(OptionalContentGroupExpression ocge1, OptionalContentGroupExpression ocge2)
        {
            return new OptionalContentGroupAndExpression(ocge1, ocge2);
        }

        /// <summary>
        /// Performs a disjunction of two <see cref="OptionalContentGroupExpression"/>s.
        /// </summary>
        /// <param name="ocge1">The first expression.</param>
        /// <param name="ocge2">The second expression.</param>
        /// <returns>An <see cref="OptionalContentGroupExpression"/> representing the disjunction of the two expressions.</returns>
        public static OptionalContentGroupExpression operator |(OptionalContentGroupExpression ocge1, OptionalContentGroupExpression ocge2)
        {
            return new OptionalContentGroupOrExpression(ocge1, ocge2);
        }

        private string _cachedToString;

        /// <inheritdoc/>
        public override string ToString()
        {
            if (_cachedToString == null)
            {

                StringBuilder sb = new StringBuilder();
                sb.Append(Operation.ToString());
                sb.Append("[");
                for (int i = 0; i < Arguments.Count; i++)
                {
                    sb.Append(Arguments[i].ToString());
                    if (i < Arguments.Count - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.Append("]");
                _cachedToString = sb.ToString();
            }

            return _cachedToString;
        }
    }

    /// <summary>
    /// Represents an optional content group.
    /// </summary>
    public class OptionalContentGroup : OptionalContentGroupExpression
    {
        /// <inheritdoc/>
        public override OptionalContentGroupBooleanOperation Operation => OptionalContentGroupBooleanOperation.Identity;

        /// <summary>
        /// The name of the <see cref="OptionalContentGroup"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Possible intended uses of graphics in an <see cref="OptionalContentGroup"/>.
        /// </summary>
        public enum Intents
        {
            /// <summary>
            /// Intended for interactive use by document consumers. This is probably what you want.
            /// </summary>
            View = 1,

            /// <summary>
            /// Intended for structural organisation of artwork.
            /// </summary>
            Design = 2,
        }

        /// <summary>
        /// The intended use of graphics in the <see cref="OptionalContentGroup"/>.
        /// </summary>
        public Intents Intent { get; set; }

        /// <summary>
        /// The default state of the <see cref="OptionalContentGroup"/> when the document is opened.
        /// </summary>
        public bool DefaultState { get; set; }

        /// <summary>
        /// The default state of the <see cref="OptionalContentGroup"/> when the document is opened for visualisation. Set this to <see langword="null"/> to use the <see cref="DefaultState"/>.
        /// </summary>
        public bool? DefaultViewState { get; set; }

        /// <summary>
        /// The default state of the <see cref="OptionalContentGroup"/> when the document is opened for printing. Set this to <see langword="null"/> to use the <see cref="DefaultState"/>.
        /// </summary>
        public bool? DefaultPrintState { get; set; }

        /// <summary>
        /// The default state of the <see cref="OptionalContentGroup"/> when the document is opened for exporting. Set this to <see langword="null"/> to use the <see cref="DefaultState"/>.
        /// </summary>
        public bool? DefaultExportState { get; set; }

        /// <summary>
        /// A unique identifier for the <see cref="OptionalContentGroup"/>.
        /// </summary>
        private string Guid { get; } = System.Guid.NewGuid().ToString();

        /// <summary>
        /// Create a new <see cref="OptionalContentGroup"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="OptionalContentGroup"/>.</param>
        /// <param name="defaultState">The default state of the <see cref="OptionalContentGroup"/> when the document is opened.</param>
        /// <param name="defaultViewState">The default state of the <see cref="OptionalContentGroup"/> when the document is opened for visualisation. Set this to <see langword="null"/> to use the <see cref="DefaultState"/>.</param>
        /// <param name="defaultPrintState">The default state of the <see cref="OptionalContentGroup"/> when the document is opened for printing. Set this to <see langword="null"/> to use the <see cref="DefaultState"/>.</param>
        /// <param name="defaultExportState">The default state of the <see cref="OptionalContentGroup"/> when the document is opened for exporting. Set this to <see langword="null"/> to use the <see cref="DefaultState"/>.</param>
        /// <param name="intent">The intended use of graphics in the <see cref="OptionalContentGroup"/>.</param>
        public OptionalContentGroup(string name, bool defaultState = true, bool? defaultViewState = null, bool? defaultPrintState = null, bool? defaultExportState = null, Intents intent = Intents.View)
        {
            this.Name = name;
            this.Intent = intent;
            this.DefaultState = defaultState;
            this.DefaultViewState = defaultViewState;
            this.DefaultPrintState = defaultPrintState;
            this.DefaultExportState = defaultExportState;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Guid;
        }

        /// <summary>
        /// Creates a link destination that sets the state of one or more <see cref="OptionalContentGroup"/>s.
        /// </summary>
        /// <param name="on"><see cref="OptionalContentGroup"/>s to turn on.</param>
        /// <param name="off"><see cref="OptionalContentGroup"/>s to turn off.</param>
        /// <param name="toggle"><see cref="OptionalContentGroup"/>s to toggle.</param>
        /// <returns>A string representation of an action that sets the specified optional content group states. Use this within the <c>linkDestionations</c> parameter of the <see cref="PDFContextInterpreter.CreatePDFDocument(Document, PDFContextInterpreter.TextOptions, bool, Dictionary{string, string}, OutlineTree, PDFMetadata, PDFContextInterpreter.FilterOption, OptionalContentGroupSettings)"/> method.</returns>
        /// <exception cref="ArgumentException">Thrown if no <see cref="OptionalContentGroup"/>s are provided in any of the arguments.</exception>
        public static string CreateSetOCGLink(IReadOnlyList<OptionalContentGroup> on = null, IReadOnlyList<OptionalContentGroup> off = null, IReadOnlyList<OptionalContentGroup> toggle = null)
        {
            StringBuilder tbr = new StringBuilder("#@OCG:");

            bool any = false;

            if (on != null)
            {
                for (int i = 0; i < on.Count; i++)
                {
                    any = true;
                    tbr.Append(on[i].ToString());
                    tbr.Append(",on");

                    if (i < on.Count - 1 || off?.Count > 0 || toggle?.Count > 0)
                    {
                        tbr.Append(";");
                    }
                }
            }

            if (off != null)
            {
                for (int i = 0; i < off.Count; i++)
                {
                    any = true;
                    tbr.Append(off[i].ToString());
                    tbr.Append(",off");

                    if (i < off.Count - 1 || toggle?.Count > 0)
                    {
                        tbr.Append(";");
                    }
                }
            }

            if (toggle != null)
            {
                for (int i = 0; i < toggle.Count; i++)
                {
                    any = true;
                    tbr.Append(toggle[i].ToString());
                    tbr.Append(",toggle");

                    if (i < toggle.Count - 1)
                    {
                        tbr.Append(";");
                    }
                }
            }

            if (!any)
            {
                throw new ArgumentException("No optional content groups provided!");
            }

            return tbr.ToString();
        }
    }

    /// <summary>
    /// An expression that is true when its argument is false.
    /// </summary>
    public class OptionalContentGroupNotExpression : OptionalContentGroupExpression
    {
        /// <inheritdoc/>
        public override OptionalContentGroupBooleanOperation Operation => OptionalContentGroupBooleanOperation.Not;

        /// <summary>
        /// Create a new <see cref="OptionalContentGroupNotExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="OptionalContentGroupNotExpression"/> that should be negated.</param>
        public OptionalContentGroupNotExpression(OptionalContentGroupExpression expression) : base(expression) { }
    }

    /// <summary>
    /// An expression that is true when all its arguments are true.
    /// </summary>
    public class OptionalContentGroupAndExpression : OptionalContentGroupExpression
    {
        /// <inheritdoc/>
        public override OptionalContentGroupBooleanOperation Operation => OptionalContentGroupBooleanOperation.And;

        /// <summary>
        /// Create a new <see cref="OptionalContentGroupAndExpression"/>.
        /// </summary>
        /// <param name="expression1">The first argument.</param>
        /// <param name="expression2">The second argument.</param>
        /// <param name="expressions">Additional arguments.</param>
        public OptionalContentGroupAndExpression(OptionalContentGroupExpression expression1, OptionalContentGroupExpression expression2, params OptionalContentGroupExpression[] expressions) : base(new OptionalContentGroupExpression[] { expression1, expression2 }.Concat(expressions).ToArray()) { }
    }

    /// <summary>
    /// An expression that is true when any of its arguments are true.
    /// </summary>
    public class OptionalContentGroupOrExpression : OptionalContentGroupExpression
    {
        /// <inheritdoc/>
        public override OptionalContentGroupBooleanOperation Operation => OptionalContentGroupBooleanOperation.Or;

        /// <summary>
        /// Create a new <see cref="OptionalContentGroupOrExpression"/>.
        /// </summary>
        /// <param name="expression1">The first argument.</param>
        /// <param name="expression2">The second argument.</param>
        /// <param name="expressions">Additional arguments.</param>
        public OptionalContentGroupOrExpression(OptionalContentGroupExpression expression1, OptionalContentGroupExpression expression2, params OptionalContentGroupExpression[] expressions) : base(new OptionalContentGroupExpression[] { expression1, expression2 }.Concat(expressions).ToArray()) { }
    }

    /// <summary>
    /// A label for an <see cref="OptionalContentGroupTreeNode"/>.
    /// </summary>
    public class OptionalContentGroupTreeLabel
    {
        /// <summary>
        /// Types of label.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// A text string.
            /// </summary>
            String,

            /// <summary>
            /// An <see cref="OptionalContentGroup"/>.
            /// </summary>
            OptionalContentGroup
        }

        /// <summary>
        /// The type of label.
        /// </summary>
        public Type LabelType { get; }

        /// <summary>
        /// If this label's <see cref="Type"/> is <see cref="Type.String"/>, the text of the label.
        /// </summary>
        public string LabelString { get; }

        /// <summary>
        /// If this label's <see cref="Type"/> is <see cref="Type.OptionalContentGroup"/>, the <see cref="OptionalContentGroup"/> in the label.
        /// </summary>
        public OptionalContentGroup LabelOptionalContentGroup { get; }

        /// <summary>
        /// Create a new <see cref="OptionalContentGroupTreeLabel"/> representing a text string.
        /// </summary>
        /// <param name="label">The text for the label.</param>
        public OptionalContentGroupTreeLabel(string label)
        {
            this.LabelType = Type.String;
            this.LabelString = label;
        }

        /// <summary>
        /// Create a new <see cref="OptionalContentGroupTreeLabel"/> representing an <see cref="OptionalContentGroup"/>.
        /// </summary>
        /// <param name="label">The <see cref="OptionalContentGroup"/> in the label.</param>
        public OptionalContentGroupTreeLabel(OptionalContentGroup label)
        {
            this.LabelType = Type.OptionalContentGroup;
            this.LabelOptionalContentGroup = label;
        }

        /// <summary>
        /// Convert a <see langword="string"/> into an <see cref="OptionalContentGroupTreeLabel"/>.
        /// </summary>
        /// <param name="label">The <see langword="string"/> to convert.</param>
        public static implicit operator OptionalContentGroupTreeLabel(string label) => new OptionalContentGroupTreeLabel(label);


        /// <summary>
        /// Convert a <see cref="OptionalContentGroup"/> into an <see cref="OptionalContentGroupTreeLabel"/>.
        /// </summary>
        /// <param name="label">The <see cref="OptionalContentGroup"/> to convert.</param>
        public static implicit operator OptionalContentGroupTreeLabel(OptionalContentGroup label) => new OptionalContentGroupTreeLabel(label);
    }

    /// <summary>
    /// Represents a node in an optional content group presentation tree.
    /// </summary>
    public class OptionalContentGroupTreeNode : IList<OptionalContentGroupTreeNode>
    {
        /// <summary>
        /// Label for the node. This can be a text string or an <see cref="OptionalContentGroup"/>.
        /// </summary>
        public OptionalContentGroupTreeLabel Label { get; set; }

        /// <summary>
        /// Children for this node.
        /// </summary>
        public List<OptionalContentGroupTreeNode> Children { get; set; }

        /// <inheritdoc/>
        public int Count => ((ICollection<OptionalContentGroupTreeNode>)Children).Count;

        /// <inheritdoc/>
        public bool IsReadOnly => ((ICollection<OptionalContentGroupTreeNode>)Children).IsReadOnly;

        /// <inheritdoc/>
        public OptionalContentGroupTreeNode this[int index] { get => ((IList<OptionalContentGroupTreeNode>)Children)[index]; set => ((IList<OptionalContentGroupTreeNode>)Children)[index] = value; }

        /// <summary>
        /// Create a new <see cref="OptionalContentGroupTreeNode"/>.
        /// </summary>
        /// <param name="label">The label for the node. This can be a text string or an <see cref="OptionalContentGroup"/>.</param>
        public OptionalContentGroupTreeNode(OptionalContentGroupTreeLabel label)
        {
            this.Label = label;
            this.Children = new List<OptionalContentGroupTreeNode>();
        }
        /// <summary>
        /// Convert an <see cref="OptionalContentGroupTreeLabel"/> into a <see cref="OptionalContentGroupTreeNode"/>.
        /// </summary>
        /// <param name="label">The node label. This can be a text string or an <see cref="OptionalContentGroup"/>.</param>
        public static implicit operator OptionalContentGroupTreeNode(OptionalContentGroupTreeLabel label) => new OptionalContentGroupTreeNode(label);

        /// <summary>
        /// Convert a <see langword="string"/> into a <see cref="OptionalContentGroupTreeNode"/>.
        /// </summary>
        /// <param name="label">The node label.</param>
        public static implicit operator OptionalContentGroupTreeNode(string label) => new OptionalContentGroupTreeLabel(label);

        /// <summary>
        /// Convert an <see cref="OptionalContentGroup"/> into a <see cref="OptionalContentGroupTreeNode"/>.
        /// </summary>
        /// <param name="label">The node label.</param>
        public static implicit operator OptionalContentGroupTreeNode(OptionalContentGroup label) => new OptionalContentGroupTreeLabel(label);

        /// <inheritdoc/>
        public int IndexOf(OptionalContentGroupTreeNode item)
        {
            return ((IList<OptionalContentGroupTreeNode>)Children).IndexOf(item);
        }

        /// <inheritdoc/>
        public void Insert(int index, OptionalContentGroupTreeNode item)
        {
            ((IList<OptionalContentGroupTreeNode>)Children).Insert(index, item);
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            ((IList<OptionalContentGroupTreeNode>)Children).RemoveAt(index);
        }

        /// <inheritdoc/>
        public void Add(OptionalContentGroupTreeNode item)
        {
            ((ICollection<OptionalContentGroupTreeNode>)Children).Add(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            ((ICollection<OptionalContentGroupTreeNode>)Children).Clear();
        }

        /// <inheritdoc/>
        public bool Contains(OptionalContentGroupTreeNode item)
        {
            return ((ICollection<OptionalContentGroupTreeNode>)Children).Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(OptionalContentGroupTreeNode[] array, int arrayIndex)
        {
            ((ICollection<OptionalContentGroupTreeNode>)Children).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(OptionalContentGroupTreeNode item)
        {
            return ((ICollection<OptionalContentGroupTreeNode>)Children).Remove(item);
        }

        /// <inheritdoc/>
        public IEnumerator<OptionalContentGroupTreeNode> GetEnumerator()
        {
            return ((IEnumerable<OptionalContentGroupTreeNode>)Children).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Children).GetEnumerator();
        }
    }

    /// <summary>
    /// Represents the settings for optional content groups (sometimes referred to as layers) in a PDF document.
    /// </summary>
    public class OptionalContentGroupSettings
    {
        /// <summary>
        /// A dictionary associating graphics tags to the corresponding visibility criteria.
        /// </summary>
        public Dictionary<string, OptionalContentGroupExpression> Groups { get; set; }

        /// <summary>
        /// A tree structure used by PDF viewers to present the optional content groups to the user. If this is set to <see langword="null"/> (the default), a flat tree (i.e. a list of all content groups) will be generated automatically. If this is empty but not <see langword="null"/>, no content groups are presented.
        /// </summary>
        public List<OptionalContentGroupTreeNode> OptionalContentGroupTree { get; set; }

        /// <summary>
        /// Radio button groups. For each group, at most one <see cref="OptionalContentGroup"/> can be enabled at the same time.
        /// </summary>
        public List<List<OptionalContentGroup>> RadioButtonGroups { get; set; }

        /// <summary>
        /// Create a new <see cref="OptionalContentGroupSettings"/> object.
        /// </summary>
        public OptionalContentGroupSettings()
        {
            this.Groups = new Dictionary<string, OptionalContentGroupExpression>();
            this.OptionalContentGroupTree = null;
            this.RadioButtonGroups = new List<List<OptionalContentGroup>>();
        }
    }
}
