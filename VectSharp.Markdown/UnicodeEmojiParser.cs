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

using Markdig;
using Markdig.Extensions.Emoji;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using System.Globalization;
using System.Linq;

namespace VectSharp.Markdown
{
    /// <summary>
    /// Contains extension methods for the <see cref="MarkdownPipelineBuilder"/> class.
    /// </summary>
    public static class MarkdownEmojiExtension
    {
        /// <summary>
        /// Uses the extension enabling unicode emoji resolution.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <returns>The modified pipeline.</returns>
        public static MarkdownPipelineBuilder UseUnicodeEmoji(this MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.Extensions.Contains<UnicodeEmojiExtension>())
            {
                pipeline.Extensions.Add(new UnicodeEmojiExtension());
            }

            return pipeline;
        }
    }

    internal class UnicodeEmojiExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.InlineParsers.Contains<UnicodeEmojiParser>())
            {
                pipeline.InlineParsers.InsertAfter<CodeInlineParser>(new UnicodeEmojiParser());
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            
        }
    }

    internal class UnicodeEmojiParser : InlineParser
    {
        public UnicodeEmojiParser()
        {
            OpeningCharacters = Enumerable.Range(0xD800, 0xDBFF - 0xD800 + 1).Select(x => (char)x).ToArray();
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            if (char.IsHighSurrogate(slice.CurrentChar))
            {
                StringInfo info = new StringInfo(slice.Text.Substring(slice.Start));
                string emoji = info.SubstringByTextElements(0, 1);

                processor.Inline = new EmojiInline("unicode://" + emoji)
                {
                    Span = {
                        Start = processor.GetSourcePosition(slice.Start, out int line, out int column)
                    },
                    Line = line,
                    Column = column,
                };
                processor.Inline.Span.End = processor.Inline.Span.Start + emoji.Length - 1;

                slice.Start += emoji.Length;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
