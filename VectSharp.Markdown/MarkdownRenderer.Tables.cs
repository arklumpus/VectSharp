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

using Markdig.Extensions.Tables;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VectSharp.Markdown
{
    public partial class MarkdownRenderer
    {

        private void RenderTable(Table table, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            if (table.Count > 0)
            {
                if (!table.IsValid())
                {
                    table.NormalizeUsingMaxWidth();
                    table.NormalizeUsingHeaderRow();
                }

                if (table.IsValid() && table.ColumnDefinitions?.Count > 0)
                {
                    bool isGridTable = false;

                    foreach (TableColumnDefinition def in table.ColumnDefinitions)
                    {
                        if (def.Width > 0)
                        {
                            isGridTable = true;
                            break;
                        }
                    }

                    if (isGridTable)
                    {
                        int maxColumns = 0;

                        foreach (TableRow row in table)
                        {
                            maxColumns = Math.Max(maxColumns, ((TableCell)row.Last()).ColumnIndex + 1);
                        }

                        if (table.ColumnDefinitions.Count > maxColumns)
                        {
                            table.ColumnDefinitions.RemoveRange(maxColumns, table.ColumnDefinitions.Count - maxColumns);
                        }
                    }
                    else
                    {
                        int maxColumns = 0;

                        foreach (TableRow row in table)
                        {
                            maxColumns = Math.Max(maxColumns, row.Count);
                        }

                        if (table.ColumnDefinitions.Count > maxColumns)
                        {
                            table.ColumnDefinitions.RemoveRange(maxColumns, table.ColumnDefinitions.Count - maxColumns);
                        }
                    }

                    double[] columnWidths = new double[table.ColumnDefinitions.Count];

                    if (table.ColumnDefinitions.Count == 0)
                    {
                        int columnCount = 0;
                        foreach (TableRow row in table)
                        {
                            columnCount = Math.Max(row.Count, columnCount);
                        }

                        columnWidths = new double[columnCount];
                    }

                    int missingColumns = columnWidths.Length;

                    for (int i = 0; i < columnWidths.Length; i++)
                    {
                        columnWidths[i] = double.NaN;
                    }

                    double remainingPerc = 1;

                    for (int i = 0; i < table.ColumnDefinitions.Count; i++)
                    {
                        if (table.ColumnDefinitions[i].Width > 0)
                        {
                            missingColumns--;
                            remainingPerc -= table.ColumnDefinitions[i].Width / 100.0;
                            columnWidths[i] = table.ColumnDefinitions[i].Width / 100.0;
                        }
                    }

                    if (missingColumns > 0)
                    {
                        remainingPerc /= missingColumns;
                        for (int i = 0; i < columnWidths.Length; i++)
                        {
                            if (double.IsNaN(columnWidths[i]))
                            {
                                columnWidths[i] = remainingPerc;
                            }
                        }
                    }

                    double maxX = context.GetMaxX(context.Cursor.Y, context.Cursor.Y, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

                    for (int i = 0; i < columnWidths.Length; i++)
                    {
                        columnWidths[i] *= maxX;
                    }

                    int index = 0;

                    foreach (TableRow row in table)
                    {
                        RenderTableRow(row, columnWidths, index == table.Count - 1, ref context, ref graphics, newPageAction);
                        index++;
                    }

                    context.Cursor = new Point(context.Cursor.X, context.Cursor.Y + SpaceAfterParagraph * context.Font.FontSize);
                }
            }
        }

        private void RenderTableRow(TableRow row, double[] columnWidths, bool isLastRow, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            context.CurrentLine ??= new Line(0);

            int index = 0;

            foreach (TableCell cell in row)
            {
                if (cell.ColumnIndex < 0)
                {
                    cell.ColumnIndex = index;
                    index += cell.ColumnSpan;
                }
                else
                {
                    index = cell.ColumnIndex + cell.ColumnSpan;
                }
            }

            double maxHeight = 0;
            double startX = context.Cursor.X;

            MarkdownContext prevContext = context.Clone();

            if (row.IsHeader)
            {
                context.Font = new Font(this.BoldFontFamily, context.Font.FontSize);
            }


            foreach (TableCell cell in row)
            {
                double cellWidth = 0;

                for (int i = 0; i < cell.ColumnSpan && cell.ColumnIndex + i < columnWidths.Length; i++)
                {
                    cellWidth += columnWidths[cell.ColumnIndex + i];
                }

                Page cellPage = RenderTableCell(cell, cellWidth, ref context, ref graphics, newPageAction);

                double prevMaxHeight = maxHeight;
                maxHeight = Math.Max(maxHeight, cellPage.Height);

                context.CurrentLine.Fragments.Add(new GraphicsFragment(new Point(context.Cursor.X, context.Cursor.Y - cellPage.Height), cellPage.Graphics, cellPage.Height));

                context.Cursor = new Point(context.Cursor.X + cellWidth, context.Cursor.Y);
            }

            if (this.TableVAlign == VerticalAlignment.Top)
            {
                for (int i = 0; i < context.CurrentLine.Fragments.Count; i++)
                {
                    context.CurrentLine.Fragments[i].Translate(0, -maxHeight + ((GraphicsFragment)context.CurrentLine.Fragments[i]).Ascent);
                }
            }
            else if (this.TableVAlign == VerticalAlignment.Middle)
            {
                for (int i = 0; i < context.CurrentLine.Fragments.Count; i++)
                {
                    context.CurrentLine.Fragments[i].Translate(0, (-maxHeight + ((GraphicsFragment)context.CurrentLine.Fragments[i]).Ascent) * 0.5);
                }
            }

            context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(startX, context.Cursor.Y), new Point(columnWidths.Sum() + startX, context.Cursor.Y), row.IsHeader ? this.TableHeaderRowSeparatorColour : this.TableRowSeparatorColour, row.IsHeader ? this.TableHeaderRowSeparatorThickness : this.TableHeaderSeparatorThickness, context.Tag));

            context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
            context.CurrentLine = null;

            context.Cursor = new Point(startX, context.Cursor.Y);

            context.Cursor = new Point(startX, context.Cursor.Y + SpaceAfterLine * context.Font.FontSize + (row.IsHeader ? this.TableHeaderRowSeparatorThickness : this.TableHeaderSeparatorThickness));

            prevContext.Cursor = context.Cursor;
            prevContext.BottomRight = context.BottomRight;
            prevContext.CurrentPage = context.CurrentPage;
            prevContext.CurrentLine = context.CurrentLine;

            context = prevContext;
        }

        private Page RenderTableCell(TableCell cell, double cellWidth, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            MarkdownRenderer clonedRenderer = this.Clone();
            clonedRenderer.PageSize = new Size(cellWidth, double.PositiveInfinity);
            clonedRenderer.Margins = this.TableCellMargins;

            MarkdownContext clonedContext = context.Clone();
            clonedContext.Translation = new Point(0, 0);
            clonedContext.Cursor = new Point(0, 0);
            clonedContext.BottomRight = new Point(0, 0);
            clonedContext.CurrentLine = null;
            clonedContext.CurrentPage = null;
            clonedContext.ForbiddenAreasLeft = new List<(double MinX, double MinY, double MaxY)>();
            clonedContext.ForbiddenAreasRight = new List<(double MinX, double MinY, double MaxY)>();

            Page cellPage = clonedRenderer.RenderSubDocument(cell, ref clonedContext).Pages[0];

            cellPage.Crop(new Point(0, 0), new Size(cellWidth, clonedContext.BottomRight.Y + this.TableCellMargins.Bottom));

            return cellPage;
        }

        private Document RenderSubDocument(ContainerBlock document, ref MarkdownContext context)
        {
            Document doc = new Document();
            Page pag = new Page(PageSize.Width, PageSize.Height) { Background = BackgroundColour };
            doc.Pages.Add(pag);

            Graphics graphics = pag.Graphics;

            graphics.Save();

            graphics.Translate(Margins.Left, Margins.Top);

            void newPageAction(ref MarkdownContext mdContext, ref Graphics pageGraphics)
            {
                pageGraphics.Restore();
                Page newPag = new Page(PageSize.Width, PageSize.Height) { Background = BackgroundColour };
                doc.Pages.Add(newPag);

                newPag.Graphics.Save();
                newPag.Graphics.Translate(mdContext.Translation);
                mdContext.Cursor = new Point(0, 0);
                mdContext.ForbiddenAreasLeft.Clear();
                mdContext.ForbiddenAreasRight.Clear();

                pageGraphics = newPag.Graphics;
                mdContext.CurrentPage = newPag;
            }

            context.Translation = new Point(context.Translation.X + Margins.Left, context.Translation.Y + Margins.Top);
            context.CurrentPage = pag;
            context.ListDepth = 0;

            int index = 0;
            foreach (Block block in document)
            {
                RenderBlock(block, ref context, ref graphics, newPageAction, index > 0, index < document.Count - 1);
                index++;
            }

            context.CurrentLine?.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);

            graphics.Restore();

            return doc;
        }
    }
}
