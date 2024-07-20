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

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceGenerator
{
    /// <summary>
    /// Source code generator that creates the static PDFDictionary.Getters dictionary.
    /// </summary>
    [Generator]
    public class PDFDictionarySourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            StringBuilder newSource = new StringBuilder();
            newSource.AppendLine("using System;");
            newSource.AppendLine("using System.Collections.Generic;");

            newSource.AppendLine("namespace VectSharp.PDF.PDFObjects");
            newSource.AppendLine("{");
            newSource.AppendLine("    partial class PDFDictionary");
            newSource.AppendLine("    {");
            newSource.AppendLine("        internal static readonly Dictionary<string, Dictionary<string, Func<PDFDictionary, IPDFObject>>> Getters = new Dictionary<string, Dictionary<string, Func<PDFDictionary, IPDFObject>>>()");
            newSource.AppendLine("        {");

            INamespaceSymbol pdfObjects = context.Compilation.GlobalNamespace.GetChildNamespace("VectSharp").GetChildNamespace("PDF").GetChildNamespace("PDFObjects");
            INamedTypeSymbol pdfDictionary = pdfObjects.GetTypeMembers("PDFDictionary").First();
            INamedTypeSymbol iPdfObject = pdfObjects.GetTypeMembers("IPDFObject").First();

            foreach (ITypeSymbol type in pdfObjects.GetTypeMembers())
            {
                if (type.DescendsFrom(pdfDictionary))
                {
                    newSource.AppendLine("            {");
                    newSource.AppendLine("                \"VectSharp.PDF.PDFObjects." + type.Name + "\",");
                    newSource.AppendLine("                new Dictionary<string, Func<PDFDictionary, IPDFObject>>()");
                    newSource.AppendLine("                {");

                    ITypeSymbol[] typeAncestry = type.AncestorsUpTo(pdfDictionary).Reverse().ToArray();

                    HashSet<string> handledProperties = new HashSet<string>();

                    for (int i = 0; i < typeAncestry.Length; i++)
                    {
                        foreach (ISymbol member in typeAncestry[i].GetMembers())
                        {
                            if (member is IPropertySymbol propertySymbol)
                            {
                                if (propertySymbol.Type.Implements(iPdfObject))
                                {
                                    if (handledProperties.Add(propertySymbol.Name))
                                    {
                                        newSource.AppendLine("                    {");
                                        newSource.AppendLine("                        \"" + propertySymbol.Name + "\",");
                                        newSource.AppendLine("                        o => ((VectSharp.PDF.PDFObjects." + type.Name + ")o)." + propertySymbol.Name);
                                        newSource.AppendLine("                    },");
                                    }
                                }
                            }
                        }
                    }

                    newSource.AppendLine("                }");
                    newSource.AppendLine("            },");
                }
            }

            newSource.AppendLine("        };");
            newSource.AppendLine("    }");
            newSource.AppendLine("}");

            context.AddSource("PDFDictionary.g.cs", newSource.ToString());

            context.ReportDiagnostic(Diagnostic.Create("IPDFDict", "Compiler", "Source generation for IPDFDictionary implementations completed.", DiagnosticSeverity.Info, DiagnosticSeverity.Info, true, 1));
        }

        public void Initialize(GeneratorInitializationContext context)
        {

        }
    }

    internal static class Extensions
    {
        public static INamespaceSymbol GetChildNamespace(this INamespaceSymbol parent, string name)
        {
            foreach (INamespaceSymbol child in parent.GetNamespaceMembers())
            {
                if (child.Name == name)
                {
                    return child;
                }
            }

            return null;
        }

        public static bool DescendsFrom(this ITypeSymbol symbol, INamedTypeSymbol baseType)
        {
            if (symbol.BaseType == null)
            {
                return false;
            }
            else if (SymbolEqualityComparer.Default.Equals(symbol.BaseType, baseType))
            {
                return true;
            }
            else
            {
                return symbol.BaseType.DescendsFrom(baseType);
            }
        }

        public static IEnumerable<ITypeSymbol> AncestorsUpTo(this ITypeSymbol type, INamedTypeSymbol baseType)
        {
            ITypeSymbol currType = type;

            while (!SymbolEqualityComparer.Default.Equals(currType, baseType) && currType != null)
            {
                yield return currType;
                currType = currType.BaseType;
            }
        }

        public static bool Implements(this ITypeSymbol symbol, INamedTypeSymbol target)
        {
            foreach (INamedTypeSymbol interf in symbol.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(interf, target))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
