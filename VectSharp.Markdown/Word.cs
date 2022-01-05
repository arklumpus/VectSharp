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
using System.Collections.Generic;
using System.Text;

namespace VectSharp.Markdown
{
    internal class Word
    {
        public char PrecedingWhitespace { get; }
        public int WhitespaceCount { get; }
        public string Text { get; }

        public Font.DetailedFontMetrics Metrics { get; }

        private Word(char precedingWhitespace, int whitespaceCount, string text, Font font)
        {
            this.PrecedingWhitespace = precedingWhitespace;
            this.WhitespaceCount = whitespaceCount;
            this.Text = text;

            if (!string.IsNullOrEmpty(text))
            {
                this.Metrics = font.MeasureTextAdvanced(text);
            }
            else
            {
                this.Metrics = font.MeasureTextAdvanced(" ");
            }
        }

        private static Dictionary<FontFamily, double> SpaceWidths { get; } = new Dictionary<FontFamily, double>();

        private static IEnumerable<Word> SplitWord(Word word, Font font, double maxWidth)
        {
            if (!SpaceWidths.TryGetValue(font.FontFamily, out double spaceWidth))
            {
                spaceWidth = font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(' ') / 1000.0;
                SpaceWidths[font.FontFamily] = spaceWidth;
            }

            spaceWidth *= font.FontSize;

            while (!string.IsNullOrEmpty(word.Text) && word.Metrics.Width + word.Metrics.LeftSideBearing + word.Metrics.RightSideBearing + spaceWidth * word.WhitespaceCount * (word.PrecedingWhitespace == '\t' ? 4 : 1) > maxWidth)
            {
                int minIndex = 1;
                int maxIndex = word.Text.Length;

                while (maxIndex - minIndex > 1)
                {
                    int index = (minIndex + maxIndex) / 2;

                    double width = font.MeasureText(word.Text.Substring(0, index)).Width;

                    if (width > maxWidth)
                    {
                        maxIndex = index;
                    }
                    else if (width < maxWidth)
                    {
                        minIndex = index;
                    }
                    else
                    {
                        minIndex = index;
                        maxIndex = index;
                    }
                }

                Word newWord = new Word(word.PrecedingWhitespace, word.WhitespaceCount, word.Text.Substring(0, minIndex), font);
                yield return newWord;

                word = new Word('\0', 0, word.Text.Substring(minIndex), font);
            }

            yield return word;
        }

        public static IEnumerable<Word> GetWords(string text, Font font, double maxWidth)
        {
            if (!SpaceWidths.TryGetValue(font.FontFamily, out double spaceWidth))
            {
                spaceWidth = font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(' ') / 1000.0;
                SpaceWidths[font.FontFamily] = spaceWidth;
            }

            spaceWidth *= font.FontSize;

            foreach (Word w in GetWordsInternal(text, font))
            {
                if (w.Metrics.Width + w.Metrics.LeftSideBearing + w.Metrics.RightSideBearing + spaceWidth * w.WhitespaceCount * (w.PrecedingWhitespace == '\t' ? 4 : 1) > maxWidth)
                {
                    foreach (Word w2 in SplitWord(w, font, maxWidth))
                    {
                        yield return w2;
                    }
                }
                else
                {
                    yield return w;
                }
            }
        }

        private static IEnumerable<Word> GetWordsInternal(string text, Font font)
        {
            StringBuilder currWord = new StringBuilder();

            char currWhitespace = '\0';
            int whitespaceCount = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    if (currWord.Length > 0)
                    {
                        yield return new Word(currWhitespace, whitespaceCount, currWord.ToString(), font);

                        currWord.Clear();
                        currWhitespace = text[i];
                        whitespaceCount = 1;
                    }
                    else if (currWhitespace == text[i])
                    {
                        whitespaceCount++;
                    }
                    else
                    {
                        yield return new Word(currWhitespace, whitespaceCount, null, font);

                        currWhitespace = text[i];
                        whitespaceCount = 1;
                    }
                }
                else
                {
                    currWord.Append(text[i]);
                }
            }

            if (currWord.Length > 0)
            {
                yield return new Word(currWhitespace, whitespaceCount, currWord.ToString(), font);

                currWord.Clear();
            }
            else if (whitespaceCount > 0)
            {
                yield return new Word(currWhitespace, whitespaceCount, null, font);
            }
        }
    }
}
