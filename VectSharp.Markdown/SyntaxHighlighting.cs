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

using Highlight;
using Highlight.Engines;
using Highlight.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VectSharp.Markdown
{
    /// <summary>
    /// Represents a string with associated formatting information.
    /// </summary>
    public struct FormattedString
    {
        /// <summary>
        /// The text represented by this object.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The colour of the text.
        /// </summary>
        public Colour Colour { get; }

        /// <summary>
        /// Whether the text should be rendered as bold or not.
        /// </summary>
        public bool IsBold { get; }

        /// <summary>
        /// Whether the text should be rendered as italic or not.
        /// </summary>
        public bool IsItalic { get; }

        /// <summary>
        /// Creates a new <see cref="FormattedString"/> instance.
        /// </summary>
        /// <param name="text">The text of the object.</param>
        /// <param name="colour">The colour of the text.</param>
        /// <param name="isBold">Whether the text should be rendered as bold or not.</param>
        /// <param name="isItalic">Whether the text should be rendered as italic or not.</param>
        public FormattedString(string text, Colour colour, bool isBold, bool isItalic)
        {
            this.Text = text;
            this.Colour = colour;
            this.IsBold = isBold;
            this.IsItalic = isItalic;
        }
    }

    /// <summary>
    /// Contains methods to perform syntax highlighting.
    /// </summary>
    public static class SyntaxHighlighter
    {
        private static Dictionary<string, string[]> LanguageAliases = new Dictionary<string, string[]>()
        {
            { "ASPX", new string[] { "ASP.NET", "aspx", "aspx-vb" } },
            { "C", new string[] { } },
            { "C++", new string[] { "cpp" } },
            { "C#", new string[] { "csharp" } },
            { "COBOL", new string[] { } },
            { "Eiffel", new string[] { } },
            { "Fortran", new string[] { } },
            { "Haskell", new string[] { } },
            { "HTML", new string[] { "xhtml" } },
            { "Java", new string[] { } },
            { "JavaScript", new string[] { "js", "node" } },
            { "Mercury", new string[] { } },
            { "MSIL", new string[] { } },
            { "Pascal", new string[] { "delphi", "objectpascal" } },
            { "Perl", new string[] { "cperl" } },
            { "PHP", new string[] { "inc" } },
            { "Python", new string[] { "python3", "rusthon" } },
            { "Ruby", new string[] { "jruby", "macruby", "rake", "rb", "rbx" } },
            { "SQL", new string[] { } },
            { "Visual Basic", new string[] { "vba", "vb6", "visual basic 6", "visual basic for applications" } },
            { "VBScript", new string[] { } },
            { "VB.NET", new string[] { "Visual Basic .NET", "vbnet", "vb .net" } },
            { "XML", new string[] { "rss", "xsd", "wsdl" } },
        };

        private static string GetLanguage(string language)
        {
            foreach (KeyValuePair<string, string[]> element in LanguageAliases)
            {
                if (element.Key.Equals(language, StringComparison.OrdinalIgnoreCase))
                {
                    return element.Key;
                }

                foreach (string alias in element.Value)
                {
                    if (alias.Equals(language, StringComparison.OrdinalIgnoreCase))
                    {
                        return element.Key;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Performs syntax highlighting for a specified language on some source code.
        /// </summary>
        /// <param name="sourceCode">The source code to be highlighted.</param>
        /// <param name="language">The name of the language to use for the highlighting.</param>
        /// <returns>A list of lists of <see cref="FormattedString"/>s. Each list of <see cref="FormattedString"/>s represents a line.</returns>
        public static List<List<FormattedString>> GetSyntaxHighlightedLines(string sourceCode, string language)
        {
            language = GetLanguage(language);

            if (string.IsNullOrEmpty(language))
            {
                return null;
            }

            HighlighterEngine engine = new HighlighterEngine();

            Highlighter highlighter = new Highlighter(engine);

            highlighter.Highlight(language, sourceCode);

            List<List<FormattedString>> tbr = new List<List<FormattedString>>();

            List<FormattedString> currentLine = new List<FormattedString>();

            for (int i = 0; i < engine.HighlightedSpans.Count; i++)
            {
                string[] split = engine.HighlightedSpans[i].Text.Replace("\r", "").Split('\n');

                for (int j = 0; j < split.Length; j++)
                {
                    currentLine.Add(new FormattedString(split[j], engine.HighlightedSpans[i].Colour, engine.HighlightedSpans[i].IsBold, engine.HighlightedSpans[i].IsItalic));

                    if (j < split.Length - 1)
                    {
                        tbr.Add(currentLine);
                        currentLine = new List<FormattedString>();
                    }
                }
            }

            tbr.Add(currentLine);

            return tbr;
        }
    }

    internal class HighlighterEngine : Engine, IEngine
    {
        private const RegexOptions DefaultRegexOptions = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;

        public List<FormattedString> HighlightedSpans { get; } = new List<FormattedString>();

        protected override string ProcessBlockPatternMatch(Definition definition, BlockPattern pattern, Match match)
        {
            if (!string.IsNullOrEmpty(pattern.RawRegex))
            {
                Match reMatch = Regex.Match(match.Value, pattern.RawRegex);

                if (reMatch.Groups.Count > 1)
                {
                    HighlightedSpans.Add(new FormattedString(match.Value.Substring(0, reMatch.Groups[1].Index), Colours.Black, false, false));
                    HighlightedSpans.Add(new FormattedString(reMatch.Groups[1].Value, pattern.Style.Colors.ForeColor, pattern.Style.Font.IsBold, pattern.Style.Font.IsItalic));
                    HighlightedSpans.Add(new FormattedString(match.Value.Substring(reMatch.Groups[1].Index + reMatch.Groups[1].Length), Colours.Black, false, false));
                }
                else
                {
                    HighlightedSpans.Add(new FormattedString(match.Value, pattern.Style.Colors.ForeColor, pattern.Style.Font.IsBold, pattern.Style.Font.IsItalic));
                }
            }
            else
            {
                HighlightedSpans.Add(new FormattedString(match.Value, pattern.Style.Colors.ForeColor, pattern.Style.Font.IsBold, pattern.Style.Font.IsItalic));
            }
            return match.Value;
        }

        protected override string ProcessMarkupPatternMatch(Definition definition, MarkupPattern pattern, Match match)
        {
            HighlightedSpans.Add(new FormattedString(match.Value, pattern.Style.Colors.ForeColor, pattern.Style.Font.IsBold, pattern.Style.Font.IsItalic));
            return match.Value;
        }

        protected override string ProcessWordPatternMatch(Definition definition, WordPattern pattern, Match match)
        {
            HighlightedSpans.Add(new FormattedString(match.Value, pattern.Style.Colors.ForeColor, pattern.Style.Font.IsBold, pattern.Style.Font.IsItalic));
            return match.Value;
        }

        string IEngine.Highlight(Definition definition, string input)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            var output = PreHighlight(definition, input);
            output = HighlightUsingRegex(definition, output);
            output = PostHighlight(definition, output);

            return output;
        }

        private string HighlightUsingRegex(Definition definition, string input)
        {
            var regexOptions = GetRegexOptions(definition);
            var evaluator = GetMatchEvaluator(definition);
            var regexPattern = definition.GetRegexPattern();

            int currentIndex = 0;

            foreach (Match match in Regex.Matches(input, regexPattern))
            {
                if (match.Index > currentIndex)
                {
                    this.HighlightedSpans.Add(new FormattedString(input.Substring(currentIndex, match.Index - currentIndex), Colours.Black, false, false));
                }

                currentIndex = match.Index + match.Length;

                evaluator(match);
            }

            if (currentIndex < input.Length)
            {
                this.HighlightedSpans.Add(new FormattedString(input.Substring(currentIndex, input.Length - currentIndex), Colours.Black, false, false));
            }

            return null;
        }

        private RegexOptions GetRegexOptions(Definition definition)
        {
            if (definition.CaseSensitive)
            {
                return DefaultRegexOptions | RegexOptions.IgnoreCase;
            }

            return DefaultRegexOptions;
        }

        private string ElementMatchHandler(Definition definition, Match match)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            var pattern = definition.Patterns.First(x => match.Groups[x.Key].Success).Value;
            if (pattern != null)
            {
                if (pattern is BlockPattern)
                {
                    return ProcessBlockPatternMatch(definition, (BlockPattern)pattern, match);
                }
                if (pattern is MarkupPattern)
                {
                    return ProcessMarkupPatternMatch(definition, (MarkupPattern)pattern, match);
                }
                if (pattern is WordPattern)
                {
                    return ProcessWordPatternMatch(definition, (WordPattern)pattern, match);
                }
            }

            return match.Value;
        }

        private MatchEvaluator GetMatchEvaluator(Definition definition)
        {
            return match => ElementMatchHandler(definition, match);
        }
    }
}
