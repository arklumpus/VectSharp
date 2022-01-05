/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2022 Giorgio Bianchini

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
using System.Text.Json;
using System.IO;
using VectSharp.Markdown;
using System.Collections.Generic;
using VectSharp;
using VectSharp.SVG;

namespace MarkdownExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            //Deserialize examples
            MarkdownExample[] examples;

            using (StreamReader sr = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("MarkdownExamples.spec.json")))
            {
                examples = JsonSerializer.Deserialize<MarkdownExample[]>(sr.ReadToEnd());
            }

            //GitHub Markdown css, from https://github.com/sindresorhus/github-markdown-css
            string css;
            using (StreamReader sr = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("MarkdownExamples.github-markdown.css")))
            {
                css = sr.ReadToEnd();
            }

            string exampleHtmlHeader = "<html><head><style>" + css + "</style></head><body class=\"markdown-body\">";
            string exampleHtmlTrailer = "</body></html>";

            int exampleFileCount = (int)Math.Ceiling(examples.Length / 20.0);

            MarkdownRenderer renderer = new MarkdownRenderer() { Margins = new Margins(10, 10, 10, 10), BaseFontSize = 16 };

            for (int i = 0; i < exampleFileCount; i++)
            {
                using (StreamWriter writer = new StreamWriter("Examples" + (i + 1).ToString() + ".html"))
                {
                    writer.WriteLine("<html>");
                    writer.WriteLine("\t<head>");
                    writer.WriteLine("\t\t<title>VectSharp.Markdown rendering examples</title>");
                    writer.WriteLine("\t\t<style>");
                    writer.WriteLine("\t\t\tpre {");
                    writer.WriteLine("\t\t\t\tbackground: #F0F0F0;");
                    writer.WriteLine("\t\t\t}");
                    writer.WriteLine("\t\t\ttable {");
                    writer.WriteLine("\t\t\t\twidth: 100%;");
                    writer.WriteLine("\t\t\t}");
                    writer.WriteLine("\t\t\tiframe {");
                    writer.WriteLine("\t\t\t\twidth: 100%;");
                    writer.WriteLine("\t\t\t}");
                    writer.WriteLine("\t\t</style>");
                    writer.WriteLine("\t</head>");
                    writer.WriteLine("\t<body>");

                    writer.WriteLine("\t\t<h1>VectSharp.Markdown rendering examples " + (i * 20 + 1).ToString() + " - " + Math.Min(i * 20 + 20, examples.Length).ToString() + " / " + examples.Length.ToString() + "</h1>");

                    for (int j = i * 20; j < Math.Min(i * 20 + 20, examples.Length); j++)
                    {
                        MarkdownExample example = examples[j];

                        Page pag = renderer.RenderSinglePage(Markdig.Markdown.Parse(example.markdown), 500, out Dictionary<string, string> linkDestinations);

                        writer.WriteLine("\t\t<h2>Example #" + example.example.ToString() + "</h2>");
                        writer.WriteLine("\t\t<h3>" + example.section + ", lines " + example.start_line.ToString() + " - " + example.end_line + "</h3>");

                        writer.WriteLine("\t\t<table>");
                        writer.WriteLine("\t\t\t<thead>");
                        writer.WriteLine("\t\t\t\t<tr>");
                        writer.WriteLine("\t\t\t\t\t<th scope=\"col\">Markdown source</th>");
                        writer.WriteLine("\t\t\t\t\t<th scope=\"col\">Reference HTML</th>");
                        writer.WriteLine("\t\t\t\t\t<th scope=\"col\">VectSharp.Markdown SVG</th>");
                        writer.WriteLine("\t\t\t\t</tr>");
                        writer.WriteLine("\t\t\t</thead>");
                        writer.WriteLine("\t\t\t<tbody>");
                        writer.WriteLine("\t\t\t\t<tr>");
                        writer.WriteLine("\t\t\t\t\t<td>");
                        writer.WriteLine("\t\t\t\t\t\t<pre><code>");
                        writer.Write(System.Web.HttpUtility.HtmlEncode(example.markdown));
                        writer.WriteLine("\t\t\t\t\t\t</code></pre>");
                        writer.WriteLine("\t\t\t\t\t</td>");
                        writer.WriteLine("\t\t\t\t\t<td>");
                        writer.Write("\t\t\t\t\t\t<iframe src=\"data:text/html;charset=UTF-8;base64,");
                        writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(exampleHtmlHeader + example.html + exampleHtmlTrailer)));
                        writer.WriteLine("\"></iframe>");

                        writer.WriteLine("\t\t\t\t\t</td>");
                        writer.WriteLine("\t\t\t\t\t<td>");

                        string svgSource;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            pag.SaveAsSVG(ms, SVGContextInterpreter.TextOptions.DoNotEmbed, linkDestinations: linkDestinations);

                            ms.Seek(0, SeekOrigin.Begin);

                            using (StreamReader sr = new StreamReader(ms))
                            {
                                svgSource = sr.ReadToEnd();
                            }
                        }

                        writer.Write("\t\t\t\t\t\t<iframe style=\"height: " + pag.Height.ToString(System.Globalization.CultureInfo.InvariantCulture) + "px\" src=\"data:image/svg+xml;charset=UTF-8;base64,");
                        writer.Write(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svgSource)));
                        writer.WriteLine("\"></iframe>");
                        writer.WriteLine("\t\t\t\t\t</td>");
                        writer.WriteLine("\t\t\t\t</tr>");
                        writer.WriteLine("\t\t\t</tbody>");
                        writer.WriteLine("\t\t</table>");
                    }

                    writer.WriteLine("\t\t<h2 style=\"text-align: center\">");

                    if (i > 0)
                    {
                        writer.WriteLine("\t\t\t<a href=\"Examples" + (i).ToString() + ".html\">&lt; Prev</a>");
                    }

                    if (i < exampleFileCount - 1)
                    {
                        writer.WriteLine("\t\t\t<a href=\"Examples" + (i + 2).ToString() + ".html\">Next &gt;</a>");
                    }

                    writer.WriteLine("\t\t</h2>");

                    writer.WriteLine("\t</body>");
                    writer.WriteLine("</html>");
                }
            }

        }
    }

    internal class MarkdownExample
    {
        public string markdown { get; set; }
        public string html { get; set; }
        public int example { get; set; }
        public int start_line { get; set; }
        public int end_line { get; set; }
        public string section { get; set; }
    }
}
