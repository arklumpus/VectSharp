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
using System.Linq;
using VectSharp.PDF.OptionalContentGroups;

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// An optional content usage dictionary.
    /// </summary>
    public class PDFOptionalContentUsage : PDFDictionary
    {
        /// <summary>
        /// The default print state of the optional content group.
        /// </summary>
        public PDFRawDictionary Print { get; }

        /// <summary>
        /// The default visibility of the optional content group.
        /// </summary>
        public PDFRawDictionary View { get; }

        /// <summary>
        /// The default export state of the optional content group.
        /// </summary>
        public PDFRawDictionary Export { get; }

        /// <summary>
        /// Create a new <see cref="PDFOptionalContentUsage"/>.
        /// </summary>
        /// <param name="view">The default print state of the optional content group, or <see langword="null"/> to leave this unspecified.</param>
        /// <param name="print">The default visibility of the optional content group, or <see langword="null"/> to leave this unspecified.</param>
        /// <param name="export">The default export state of the optional content group, or <see langword="null"/> to leave this unspecified.</param>
        public PDFOptionalContentUsage(bool? view, bool? print, bool? export)
        {
            if (print is bool p)
            {
                this.Print = new PDFRawDictionary();
                this.Print.Keys["PrintState"] = new PDFString(p ? "ON" : "OFF", PDFString.StringDelimiter.StartingForwardSlash);
            }

            if (view is bool v)
            {
                this.View = new PDFRawDictionary();
                this.View.Keys["ViewState"] = new PDFString(v ? "ON" : "OFF", PDFString.StringDelimiter.StartingForwardSlash);
            }

            if (export is bool e)
            {
                this.Export = new PDFRawDictionary();
                this.Export.Keys["ExportState"] = new PDFString(e ? "ON" : "OFF", PDFString.StringDelimiter.StartingForwardSlash);
            }
        }
    }

    /// <summary>
    /// A usage application dictionary.
    /// </summary>
    public class PDFUsageApplication : PDFDictionary
    {
        /// <summary>
        /// Usage application events.
        /// </summary>
        public enum UsageApplicationEvent
        {
            /// <summary>
            /// The document is opened for visualisation.
            /// </summary>
            View,

            /// <summary>
            /// The document is being printed.
            /// </summary>
            Print,

            /// <summary>
            /// The document is being exported.
            /// </summary>
            Export
        }

        /// <summary>
        /// The event this <see cref="PDFUsageApplication"/> refers to.
        /// </summary>
        public PDFString Event { get; }

        /// <summary>
        /// The usage category affected by this <see cref="PDFUsageApplication"/>.
        /// </summary>
        public PDFArray<PDFString> Category { get; }

        /// <summary>
        /// Optional content groups affected by this <see cref="PDFUsageApplication"/>.
        /// </summary>
        public PDFArray<PDFOptionalContentGroup> OCGs { get; }

        /// <summary>
        /// Create a new <see cref="PDFUsageApplication"/>.
        /// </summary>
        /// <param name="applicationEvent">The <see cref="UsageApplicationEvent"/> to which the new <see cref="PDFUsageApplication"/> should refer.</param>
        /// <param name="ocgs">Optional content groups affected by the new <see cref="PDFUsageApplication"/>.</param>
        public PDFUsageApplication(UsageApplicationEvent applicationEvent, IEnumerable<PDFOptionalContentGroup> ocgs)
        {
            List<PDFOptionalContentGroup> applicableOCGs = new List<PDFOptionalContentGroup>();

            foreach (PDFOptionalContentGroup ocg in ocgs)
            {
                if (applicationEvent == UsageApplicationEvent.View && ocg.Usage?.View != null ||
                    applicationEvent == UsageApplicationEvent.Print && ocg.Usage?.Print != null ||
                    applicationEvent == UsageApplicationEvent.Export && ocg.Usage?.Export != null)
                {
                    applicableOCGs.Add(ocg);
                }
            }

            this.OCGs = new PDFArray<PDFOptionalContentGroup>(applicableOCGs);

            switch (applicationEvent)
            {
                case UsageApplicationEvent.View:
                    this.Event = new PDFString("View", PDFString.StringDelimiter.StartingForwardSlash);
                    this.Category = new PDFArray<PDFString>(new PDFString("View", PDFString.StringDelimiter.StartingForwardSlash));
                    break;

                case UsageApplicationEvent.Print:
                    this.Event = new PDFString("Print", PDFString.StringDelimiter.StartingForwardSlash);
                    this.Category = new PDFArray<PDFString>(new PDFString("Print", PDFString.StringDelimiter.StartingForwardSlash));
                    break;

                case UsageApplicationEvent.Export:
                    this.Event = new PDFString("Export", PDFString.StringDelimiter.StartingForwardSlash);
                    this.Category = new PDFArray<PDFString>(new PDFString("Export", PDFString.StringDelimiter.StartingForwardSlash));
                    break;
            }
        }
    }

    /// <summary>
    /// An optional content group.
    /// </summary>
    public class PDFOptionalContentGroup : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("OCG", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Name of the optional content group (displayed to the user in the PDF viewer interface).
        /// </summary>
        public PDFString Name { get; }

        /// <summary>
        /// Intent for the optional content group.
        /// </summary>
        public PDFValueObject Intent { get; }

        /// <summary>
        /// Usage dictionary for the <see cref="PDFOptionalContentGroup"/>.
        /// </summary>
        public PDFOptionalContentUsage Usage { get; }

        /// <summary>
        /// Create a new <see cref="PDFOptionalContentGroup"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="intent"></param>
        /// <param name="defaultViewState"></param>
        /// <param name="defaultPrintState"></param>
        /// <param name="defaultExportState"></param>
        public PDFOptionalContentGroup(string name, OptionalContentGroup.Intents intent, bool? defaultViewState, bool? defaultPrintState, bool? defaultExportState)
        {
            this.Name = new PDFString(name, PDFString.StringDelimiter.Brackets);

            switch (intent)
            {
                case OptionalContentGroup.Intents.Design:
                    this.Intent = new PDFString("Design", PDFString.StringDelimiter.StartingForwardSlash);
                    break;
                case OptionalContentGroup.Intents.View | OptionalContentGroup.Intents.Design:
                    this.Intent = new PDFArray<PDFString>(new PDFString("View", PDFString.StringDelimiter.StartingForwardSlash), new PDFString("Design", PDFString.StringDelimiter.StartingForwardSlash));
                    break;
                case OptionalContentGroup.Intents.View:
                default:
                    this.Intent = new PDFString("View", PDFString.StringDelimiter.StartingForwardSlash);
                    break;
            }

            if (defaultViewState != null || defaultPrintState != null || defaultExportState != null)
            {
                this.Usage = new PDFOptionalContentUsage(defaultViewState, defaultPrintState, defaultExportState);
            }
        }
    }

    /// <summary>
    /// An optional content group membership dictionary.
    /// </summary>
    public class PDFOptionalContentGroupMembership : PDFDictionary
    {
        /// <summary>
        /// Name used to refer to this <see cref="PDFOptionalContentGroupMembership"/> within content streams.
        /// </summary>
        public string ReferenceName { get; }

        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("OCMD", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Optional content groups within this membership dictionary.
        /// </summary>
        public PDFArray<PDFOptionalContentGroup> OCGs { get; }

        /// <summary>
        /// Operation for the optional content groups.
        /// </summary>
        public PDFString P { get; }

        /// <summary>
        /// Visibility expression.
        /// </summary>
        public PDFArray<IPDFObject> VE { get; }

        private static IPDFObject ConvertExpresssion(OptionalContentGroupExpression expression, Dictionary<string, PDFOptionalContentGroup> ocgs)
        {
            switch (expression.Operation)
            {
                case OptionalContentGroupBooleanOperation.Identity:
                    return ocgs[((OptionalContentGroup)expression).ToString()];

                case OptionalContentGroupBooleanOperation.Not:
                    return new PDFArray<IPDFObject>(new PDFString("Not", PDFString.StringDelimiter.StartingForwardSlash), ConvertExpresssion(expression.Arguments[0], ocgs));

                case OptionalContentGroupBooleanOperation.And:
                    return new PDFArray<IPDFObject>(new IPDFObject[] { new PDFString("And", PDFString.StringDelimiter.StartingForwardSlash) }.Concat(expression.Arguments.Select(x => ConvertExpresssion(x, ocgs))));

                case OptionalContentGroupBooleanOperation.Or:
                    return new PDFArray<IPDFObject>(new IPDFObject[] { new PDFString("Or", PDFString.StringDelimiter.StartingForwardSlash) }.Concat(expression.Arguments.Select(x => ConvertExpresssion(x, ocgs))));

                default:
                    throw new ArgumentException("Unknown operation: " + expression.Operation.ToString());
            }
        }

        /// <summary>
        /// Create a new <see cref="PDFOptionalContentGroupMembership"/>.
        /// </summary>
        /// <param name="expression">The membership expression.</param>
        /// <param name="ocgs">A dictionary of all defined <see cref="PDFOptionalContentGroup"/>s.</param>
        /// <param name="referenceName">The name that will be used to refer to the new <see cref="PDFOptionalContentGroupMembership"/> within content streams.</param>
        public PDFOptionalContentGroupMembership(OptionalContentGroupExpression expression, Dictionary<string, PDFOptionalContentGroup> ocgs, string referenceName)
        {
            if (expression is OptionalContentGroup group)
            {
                this.OCGs = new PDFArray<PDFOptionalContentGroup>(ocgs[group.ToString()]);
                this.P = new PDFString("AllOn", PDFString.StringDelimiter.StartingForwardSlash);
            }
            else
            {
                this.VE = (PDFArray<IPDFObject>)ConvertExpresssion(expression, ocgs);
            }
            this.ReferenceName = referenceName;
        }
    }

    /// <summary>
    /// Optional content configuration dictionary.
    /// </summary>
    public class PDFOptionalContentConfiguration : PDFDictionary
    {
        /// <summary>
        /// Name of the configuration.
        /// </summary>
        public PDFString Name { get; } = new PDFString("Default", PDFString.StringDelimiter.Brackets);
        
        /// <summary>
        /// Program that created this configuration.
        /// </summary>
        public PDFString Creator { get; } = new PDFString("VectSharp.PDF v" + typeof(PDFOptionalContentConfiguration).Assembly.GetName().Version.ToString(3), PDFString.StringDelimiter.Brackets);

        /// <summary>
        /// Default state for optional content groups.
        /// </summary>
        public PDFString BaseState { get; } = new PDFString("ON", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Optional content groups that should be off by default.
        /// </summary>
        public PDFArray<PDFOptionalContentGroup> OFF { get; }

        /// <summary>
        /// Intent for the optional content configuration.
        /// </summary>
        public PDFString Intent { get; } = new PDFString("View", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Usage application dictionaries.
        /// </summary>
        public PDFArray<PDFUsageApplication> AS { get; }

        /// <summary>
        /// Optional content group display tree.
        /// </summary>
        public PDFArray<IPDFObject> Order { get; }

        /// <summary>
        /// Radio button groups.
        /// </summary>
        public PDFArray<PDFArray<PDFOptionalContentGroup>> RBGroups { get; }

        /// <summary>
        /// Create a new <see cref="PDFOptionalContentConfiguration"/>.
        /// </summary>
        /// <param name="off"><see cref="PDFOptionalContentGroup"/>s that should be off by default.</param>
        /// <param name="usageApplications">Usage application dictionaries.</param>
        /// <param name="order">Optional content group display tree.</param>
        /// <param name="radioButtonGroups">Radio button groups.</param>
        public PDFOptionalContentConfiguration(IEnumerable<PDFOptionalContentGroup> off, IEnumerable<PDFUsageApplication> usageApplications, IEnumerable<IPDFObject> order, IEnumerable<IEnumerable<PDFOptionalContentGroup>> radioButtonGroups)
        {
            PDFArray<PDFOptionalContentGroup> offs = new PDFArray<PDFOptionalContentGroup>(off);

            if (offs.Values.Count > 0)
            {
                this.OFF = offs;
            }

            PDFArray<PDFUsageApplication> uas = new PDFArray<PDFUsageApplication>(usageApplications);

            if (uas.Values.Count > 0)
            {
                this.AS = uas;
            }

            this.Order = new PDFArray<IPDFObject>(order);

            if (radioButtonGroups != null)
            {
                this.RBGroups = new PDFArray<PDFArray<PDFOptionalContentGroup>>(radioButtonGroups.Select(x => new PDFArray<PDFOptionalContentGroup>(x)));

                if (this.RBGroups.Values.Count == 0)
                {
                    this.RBGroups = null;
                }
            }
        }
    }

    /// <summary>
    /// Optional content properties.
    /// </summary>
    public class PDFOptionalContentProperties : PDFDictionary
    {
        /// <summary>
        /// Optional content groups defined in the document.
        /// </summary>
        public PDFArray<PDFOptionalContentGroup> OCGs { get; }

        /// <summary>
        /// Default configuration.
        /// </summary>
        public PDFOptionalContentConfiguration D { get; }

        /// <summary>
        /// Create a new <see cref="PDFOptionalContentProperties"/>.
        /// </summary>
        /// <param name="ocgs">All optional content groups defined in the document.</param>
        /// <param name="defaultConfiguration">Default configuration.</param>
        public PDFOptionalContentProperties(IEnumerable<PDFOptionalContentGroup> ocgs, PDFOptionalContentConfiguration defaultConfiguration)
        {
            this.OCGs = new PDFArray<PDFOptionalContentGroup>(ocgs);
            this.D = defaultConfiguration;
        }
    }


    /// <summary>
    /// Represents an action that changes the state of one or more <see cref="PDFOptionalContentGroup"/>s.
    /// </summary>
    public class PDFSetOCGStateAction : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Action", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Type of action.
        /// </summary>
        public PDFString S { get; } = new PDFString("SetOCGState", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Optional content group states.
        /// </summary>
        public PDFArray<IPDFObject> State { get; }

        /// <summary>
        /// Creates a new <see cref="PDFSetOCGStateAction"/>.
        /// </summary>
        /// <param name="on"><see cref="PDFOptionalContentGroup"/>s to turn on.</param>
        /// <param name="off"><see cref="PDFOptionalContentGroup"/>s to turn off.</param>
        /// <param name="toggle"><see cref="PDFOptionalContentGroup"/>s to toggle.</param>
        public PDFSetOCGStateAction(IEnumerable<PDFOptionalContentGroup> on, IEnumerable<PDFOptionalContentGroup> off, IEnumerable<PDFOptionalContentGroup> toggle)
        {
            List<IPDFObject> state = new List<IPDFObject>();

            if (on != null && on.Any())
            {
                state.Add(new PDFString("ON", PDFString.StringDelimiter.StartingForwardSlash));
                state.AddRange(on);
            }

            if (off != null && off.Any())
            {
                state.Add(new PDFString("OFF", PDFString.StringDelimiter.StartingForwardSlash));
                state.AddRange(off);
            }

            if (toggle != null && toggle.Any())
            {
                state.Add(new PDFString("Toggle", PDFString.StringDelimiter.StartingForwardSlash));
                state.AddRange(toggle);
            }

            if (state.Count > 0)
            {
                this.State = new PDFArray<IPDFObject>(state);
            }
        }
    }

    /// <summary>
    /// A PDF link annotation that triggers a <see cref="PDFSetOCGStateAction"/>.
    /// </summary>
    public class PDFSetOCGStateActionAnnotation : PDFLinkAnnotation
    {
        /// <summary>
        /// The link destination.
        /// </summary>
        public PDFSetOCGStateAction A { get; }

        /// <summary>
        /// Create a new <see cref="PDFSetOCGStateActionAnnotation"/> that changes the state of optional content groups when it is clicked.
        /// </summary>
        /// <param name="rect">The annotation rectangle.</param>
        /// <param name="action">The action triggered by the annotation.</param>
        public PDFSetOCGStateActionAnnotation(Rectangle rect, PDFSetOCGStateAction action) : base(rect)
        {
            this.A = action;
        }
    }
}
