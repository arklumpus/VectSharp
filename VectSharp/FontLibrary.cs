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
using System.Linq;
using System.IO;

namespace VectSharp
{
    /// <summary>
    /// Represents a font library with methods to create <see cref="FontFamily"/> objects from a string or from <see cref="FontFamily.StandardFontFamilies"/>.
    /// </summary>
    public interface IFontLibrary
    {
        /// <summary>
        /// Create a new font family from the specified family name or true type file. If the family name or the true type file are not valid, an exception might be raised.
        /// </summary>
        /// <param name="fontFamily">The name of the font family to create, or the path to a TTF file.</param>
        /// <returns>If the font family name or the true type file is valid, a <see cref="FontFamily"/> object corresponding to the specified font family.</returns>
        FontFamily ResolveFontFamily(string fontFamily);

        /// <summary>
        /// Create a new font family from the specified family name or true type file. If the family name or the true type file are not valid, try to instantiate the font family using
        /// the <paramref name="fallback"/>. If none of the fallback family names or true type files are valid, an exception might be raised.
        /// </summary>
        /// <param name="fontFamily">The name of the font family to create, or the path to a TTF file.</param>
        /// <param name="fallback">Names of additional font families or TTF files, which will be tried if the first <paramref name="fontFamily"/> is not valid.</param>
        /// <returns>A <see cref="FontFamily"/> object corresponding to the first of the specified font families that is valid.</returns>
        FontFamily ResolveFontFamily(string fontFamily, params string[] fallback);

        /// <summary>
        /// Create a new font family from the specified standard font family name.
        /// </summary>
        /// <param name="standardFontFamily">The standard name of the font family.</param>
        /// <returns>A <see cref="FontFamily"/> object corresponding to the specified font family.</returns>
        FontFamily ResolveFontFamily(FontFamily.StandardFontFamilies standardFontFamily);

        /// <summary>
        /// Create a new font family from the specified family name or true type file. If the family name or the true type file are not valid, try to instantiate the font family using
        /// the <paramref name="fallback"/>. If none of the fallback family names or true type files are valid, instantiate a standard font family using the <paramref name="finalFallback"/>.
        /// </summary>
        /// <param name="fontFamily">The name of the font family to create, or the path to a TTF file.</param>
        /// <param name="fallback">Names of additional font families or TTF files, which will be tried if the first <paramref name="fontFamily"/> is not valid.</param>
        /// <param name="finalFallback">The standard name of the font family that will be used if none of the fallback families are valid.</param>
        /// <returns>A <see cref="FontFamily"/> object corresponding to the first of the specified font families that is valid.</returns>
        FontFamily ResolveFontFamily(string fontFamily, FontFamily.StandardFontFamilies finalFallback, params string[] fallback);
    }

    /// <summary>
    /// Abstract class with a default implementation of font family fallbacks.
    /// </summary>
    public abstract class FontLibrary : IFontLibrary
    {
        /// <inheritdoc/>
        public abstract FontFamily ResolveFontFamily(string fontFamily);

        /// <inheritdoc/>
        public abstract FontFamily ResolveFontFamily(FontFamily.StandardFontFamilies standardFontFamily);
        
        /// <inheritdoc/>
        public virtual FontFamily ResolveFontFamily(string fontFamily, params string[] fallback)
        {
            bool found = false;

            FontFamily tbr = null;

            try
            {
                tbr = ResolveFontFamily(fontFamily);
                if (tbr.TrueTypeFile != null)
                {
                    found = true;
                }
            }
            catch
            {
                tbr = null;
                found = false;
            }

            if (!found)
            {
                for (int i = 0; i < fallback.Length; i++)
                {
                    try
                    {
                        tbr = ResolveFontFamily(fontFamily);
                        if (tbr.TrueTypeFile != null)
                        {
                            found = true;
                        }
                    }
                    catch
                    {
                        tbr = null;
                        found = false;
                    }

                    if (found)
                    {
                        return tbr;
                    }
                }
            }
            else
            {
                return tbr;
            }

            throw new FontFamilyCreationException(fontFamily);
        }

        /// <inheritdoc/>
        public virtual FontFamily ResolveFontFamily(string fontFamily, FontFamily.StandardFontFamilies finalFallback, params string[] fallback)
        {
            bool found = false;

            FontFamily tbr = null;

            try
            {
                tbr = ResolveFontFamily(fontFamily);
                if (tbr.TrueTypeFile != null)
                {
                    found = true;
                }
            }
            catch
            {
                tbr = null;
                found = false;
            }

            if (!found)
            {
                for (int i = 0; i < fallback.Length; i++)
                {
                    try
                    {
                        tbr = ResolveFontFamily(fontFamily);
                        if (tbr.TrueTypeFile != null)
                        {
                            found = true;
                        }
                    }
                    catch
                    {
                        tbr = null;
                        found = false;
                    }

                    if (found)
                    {
                        return tbr;
                    }
                }
            }
            else
            {
                return tbr;
            }

            return ResolveFontFamily(finalFallback);
        }
    }

    /// <summary>
    /// A font library that can be used to cache and resolve font family names.
    /// </summary>
    public class SimpleFontLibrary : FontLibrary
    {
        private Dictionary<string, string> KnownFonts = new Dictionary<string, string>();
        private Dictionary<string, string> NotLoadedFonts = new Dictionary<string, string>();
        private Dictionary<string, FontFamily> LoadedFonts = new Dictionary<string, FontFamily>();
        private Dictionary<FontFamily.StandardFontFamilies, FontFamily> Fallbacks = new Dictionary<FontFamily.StandardFontFamilies, FontFamily>();
        private DefaultFontLibrary defaultLibrary = new DefaultFontLibrary();

        /// <summary>
        /// Create a new <see cref="SimpleFontLibrary"/> instance.
        /// </summary>
        /// <param name="standardFontLibrary">An existing font library that will be used to resolve the standard font families.</param>
        public SimpleFontLibrary(IFontLibrary standardFontLibrary)
        {
            for (int i = 0; i < 14; i++)
            {
                FontFamily.StandardFontFamilies stdFF = (FontFamily.StandardFontFamilies)i;
                FontFamily resolved = standardFontLibrary.ResolveFontFamily(stdFF);

                Fallbacks[stdFF] = resolved;
                LoadedFonts[FontFamily.StandardFamilies[i]] = resolved;
                KnownFonts[FontFamily.StandardFamilies[i]] = FontFamily.StandardFamilies[i];
                KnownFonts[resolved.TrueTypeFile.GetFontFamilyName()] = FontFamily.StandardFamilies[i];
                KnownFonts[resolved.TrueTypeFile.GetFontName()] = FontFamily.StandardFamilies[i];
            }
        }

        /// <summary>
        /// Create a new <see cref="SimpleFontLibrary"/> instance, using the default font library to resolve the standard font families.
        /// </summary>
        public SimpleFontLibrary() : this(FontFamily.DefaultFontLibrary)
        {

        }

        /// <summary>
        /// Create a new <see cref="SimpleFontLibrary"/> instance, with the specified replacements for the standard font families.
        /// </summary>
        /// <param name="timesRoman">The font family to use for the Times-Roman standard font.</param>
        /// <param name="timesBold">The font family to use for the Times-Bold standard font.</param>
        /// <param name="timesItalic">The font family to use for the Times-Italic standard font.</param>
        /// <param name="timesBoldItalic">The font family to use for the Times-BoldItalic standard font.</param>
        /// <param name="helvetica">The font family to use for the Helvetica standard font.</param>
        /// <param name="helveticaBold">The font family to use for the Helvetica-Bold standard font.</param>
        /// <param name="helveticaOblique">The font family to use for the Helvetica-Oblique standard font.</param>
        /// <param name="helveticaBoldOblique">The font family to use for the Helvetica-BoldOblique standard font.</param>
        /// <param name="courier">The font family to use for the Courier standard font.</param>
        /// <param name="courierBold">The font family to use for the Courier-Bold standard font.</param>
        /// <param name="courierOblique">The font family to use for the Courier-Oblique standard font.</param>
        /// <param name="courierBoldOblique">The font family to use for the Courier-BoldOblique standard font.</param>
        /// <param name="symbol">The font family to use for the Symbol standard font.</param>
        /// <param name="zapfdingbats">The font family to use for the Zapfdingbats standard font.</param>
        public SimpleFontLibrary(FontFamily timesRoman, FontFamily timesBold, FontFamily timesItalic, FontFamily timesBoldItalic,
            FontFamily helvetica, FontFamily helveticaBold, FontFamily helveticaOblique, FontFamily helveticaBoldOblique,
            FontFamily courier, FontFamily courierBold, FontFamily courierOblique, FontFamily courierBoldOblique,
            FontFamily symbol, FontFamily zapfdingbats)
        {
            Fallbacks[FontFamily.StandardFontFamilies.TimesRoman] = timesRoman;
            Fallbacks[FontFamily.StandardFontFamilies.TimesBold] = timesBold;
            Fallbacks[FontFamily.StandardFontFamilies.TimesItalic] = timesItalic;
            Fallbacks[FontFamily.StandardFontFamilies.TimesBoldItalic] = timesBoldItalic;

            Fallbacks[FontFamily.StandardFontFamilies.Helvetica] = helvetica;
            Fallbacks[FontFamily.StandardFontFamilies.HelveticaBold] = helveticaBold;
            Fallbacks[FontFamily.StandardFontFamilies.HelveticaOblique] = helveticaOblique;
            Fallbacks[FontFamily.StandardFontFamilies.HelveticaBoldOblique] = helveticaBoldOblique;

            Fallbacks[FontFamily.StandardFontFamilies.Courier] = courier;
            Fallbacks[FontFamily.StandardFontFamilies.CourierBold] = courierBold;
            Fallbacks[FontFamily.StandardFontFamilies.CourierOblique] = courierOblique;
            Fallbacks[FontFamily.StandardFontFamilies.CourierBoldOblique] = courierBoldOblique;

            Fallbacks[FontFamily.StandardFontFamilies.Symbol] = symbol;
            Fallbacks[FontFamily.StandardFontFamilies.ZapfDingbats] = zapfdingbats;

            for (int i = 0; i < 14; i++)
            {
                FontFamily.StandardFontFamilies stdFF = (FontFamily.StandardFontFamilies)i;
                FontFamily resolved = Fallbacks[stdFF];
                resolved.IsStandardFamily = true;
                resolved.FileName = FontFamily.StandardFamilies[i];

                LoadedFonts[FontFamily.StandardFamilies[i]] = resolved;
                KnownFonts[FontFamily.StandardFamilies[i]] = FontFamily.StandardFamilies[i];
                KnownFonts[resolved.TrueTypeFile.GetFontFamilyName()] = FontFamily.StandardFamilies[i];
                KnownFonts[resolved.TrueTypeFile.GetFontName()] = FontFamily.StandardFamilies[i];
            }
        }


        /// <summary>
        /// Create a new <see cref="SimpleFontLibrary"/> instance, with the specified replacements for the standard font families.
        /// </summary>
        /// <param name="timesRoman">The font family to use for the Times-Roman standard font.</param>
        /// <param name="timesBold">The font family to use for the Times-Bold standard font.</param>
        /// <param name="timesItalic">The font family to use for the Times-Italic standard font.</param>
        /// <param name="timesBoldItalic">The font family to use for the Times-BoldItalic standard font.</param>
        /// <param name="helvetica">The font family to use for the Helvetica standard font.</param>
        /// <param name="helveticaBold">The font family to use for the Helvetica-Bold standard font.</param>
        /// <param name="helveticaOblique">The font family to use for the Helvetica-Oblique standard font.</param>
        /// <param name="helveticaBoldOblique">The font family to use for the Helvetica-BoldOblique standard font.</param>
        /// <param name="courier">The font family to use for the Courier standard font.</param>
        /// <param name="courierBold">The font family to use for the Courier-Bold standard font.</param>
        /// <param name="courierOblique">The font family to use for the Courier-Oblique standard font.</param>
        /// <param name="courierBoldOblique">The font family to use for the Courier-BoldOblique standard font.</param>
        /// <param name="symbol">The font family to use for the Symbol standard font.</param>
        /// <param name="zapfdingbats">The font family to use for the Zapfdingbats standard font.</param>
        public SimpleFontLibrary(string timesRoman, string timesBold, string timesItalic, string timesBoldItalic,
            string helvetica, string helveticaBold, string helveticaOblique, string helveticaBoldOblique,
            string courier, string courierBold, string courierOblique, string courierBoldOblique,
            string symbol, string zapfdingbats)
        {
            Fallbacks[FontFamily.StandardFontFamilies.TimesRoman] = FontFamily.DefaultFontLibrary.ResolveFontFamily(timesRoman);
            Fallbacks[FontFamily.StandardFontFamilies.TimesBold]= FontFamily.DefaultFontLibrary.ResolveFontFamily(timesBold);
            Fallbacks[FontFamily.StandardFontFamilies.TimesItalic]= FontFamily.DefaultFontLibrary.ResolveFontFamily(timesItalic);
            Fallbacks[FontFamily.StandardFontFamilies.TimesBoldItalic]= FontFamily.DefaultFontLibrary.ResolveFontFamily(timesBoldItalic);

            Fallbacks[FontFamily.StandardFontFamilies.Helvetica]= FontFamily.DefaultFontLibrary.ResolveFontFamily(helvetica);
            Fallbacks[FontFamily.StandardFontFamilies.HelveticaBold]= FontFamily.DefaultFontLibrary.ResolveFontFamily(helveticaBold);
            Fallbacks[FontFamily.StandardFontFamilies.HelveticaOblique]= FontFamily.DefaultFontLibrary.ResolveFontFamily(helveticaOblique);
            Fallbacks[FontFamily.StandardFontFamilies.HelveticaBoldOblique]= FontFamily.DefaultFontLibrary.ResolveFontFamily(helveticaBoldOblique);

            Fallbacks[FontFamily.StandardFontFamilies.Courier]= FontFamily.DefaultFontLibrary.ResolveFontFamily(courier);
            Fallbacks[FontFamily.StandardFontFamilies.CourierBold]= FontFamily.DefaultFontLibrary.ResolveFontFamily(courierBold);
            Fallbacks[FontFamily.StandardFontFamilies.CourierOblique]= FontFamily.DefaultFontLibrary.ResolveFontFamily(courierOblique);
            Fallbacks[FontFamily.StandardFontFamilies.CourierBoldOblique]= FontFamily.DefaultFontLibrary.ResolveFontFamily(courierBoldOblique);

            Fallbacks[FontFamily.StandardFontFamilies.Symbol]= FontFamily.DefaultFontLibrary.ResolveFontFamily(symbol);
            Fallbacks[FontFamily.StandardFontFamilies.ZapfDingbats]= FontFamily.DefaultFontLibrary.ResolveFontFamily(zapfdingbats);

            for (int i = 0; i < 14; i++)
            {
                FontFamily.StandardFontFamilies stdFF = (FontFamily.StandardFontFamilies)i;
                FontFamily resolved = Fallbacks[stdFF];
                resolved.IsStandardFamily = true;
                resolved.FileName = FontFamily.StandardFamilies[i];

                LoadedFonts[FontFamily.StandardFamilies[i]] = resolved;
                KnownFonts[FontFamily.StandardFamilies[i]] = FontFamily.StandardFamilies[i];
                KnownFonts[resolved.TrueTypeFile.GetFontFamilyName()] = FontFamily.StandardFamilies[i];
                KnownFonts[resolved.TrueTypeFile.GetFontName()] = FontFamily.StandardFamilies[i];
            }
        }


        /// <summary>
        /// Add the specified font family to the library with the specified name.
        /// </summary>
        /// <param name="fontFamilyName">The name of the font family.</param>
        /// <param name="fontFamily">The font family to add.</param>
        public void Add(string fontFamilyName, FontFamily fontFamily)
        {
            this.LoadedFonts[fontFamilyName] = fontFamily;
            this.KnownFonts[fontFamilyName] = fontFamilyName;

            if (fontFamily.TrueTypeFile != null)
            {
                this.KnownFonts[fontFamily.TrueTypeFile.GetFontFamilyName()] = fontFamilyName;
                this.KnownFonts[fontFamily.TrueTypeFile.GetFontName()] = fontFamilyName;
            }
        }

        /// <summary>
        /// Add the specified font family to the library.
        /// </summary>
        /// <param name="fontFamily">The font family to add.</param>
        public void Add(FontFamily fontFamily)
        {
            if (fontFamily.TrueTypeFile != null)
            {
                string familyName = fontFamily.TrueTypeFile.GetFontFamilyName();

                this.LoadedFonts[familyName] = fontFamily;
                this.KnownFonts[familyName] = familyName;
                this.KnownFonts[fontFamily.TrueTypeFile.GetFontName()] = familyName;
            }
        }

        /// <summary>
        /// Add the font family contained in the specified True Type Font file to the library.
        /// </summary>
        /// <param name="fileName">The path to the TTF file containing the font family.</param>
        public void Add(string fileName)
        {
            FontFamily fontFamily = FontFamily.DefaultFontLibrary.ResolveFontFamily(fileName);

            if (fontFamily.TrueTypeFile != null)
            {
                string familyName = fontFamily.TrueTypeFile.GetFontFamilyName();

                this.LoadedFonts[familyName] = fontFamily;
                this.KnownFonts[familyName] = familyName;
                this.KnownFonts[fontFamily.TrueTypeFile.GetFontName()] = familyName;
            }
        }

        /// <summary>
        /// Add the font family contained in the specified True Type Font file to the library, with the specified name. The font family is not loaded until it is requested for the first time.
        /// </summary>
        /// <param name="fontFamily">The name of the font family.</param>
        /// <param name="fileName">The path to the TTF file containing the font family.</param>
        public void Add(string fontFamily, string fileName)
        {
            this.KnownFonts[fontFamily] = fontFamily;
            this.NotLoadedFonts[fontFamily] = fileName;
        }

        /// <inheritdoc/>
        public override FontFamily ResolveFontFamily(FontFamily.StandardFontFamilies standardFontFamily)
        {
            return Fallbacks[standardFontFamily];
        }
        
        /// <inheritdoc/>
        public override FontFamily ResolveFontFamily(string fontFamily)
        {
            if (KnownFonts.TryGetValue(fontFamily, out string knownFontName))
            {
                if (LoadedFonts.TryGetValue(knownFontName, out FontFamily tbr))
                {
                    return tbr;
                }
                else
                {
                    if (NotLoadedFonts.TryGetValue(knownFontName, out string ttfFile))
                    {
                        tbr = defaultLibrary.ResolveFontFamily(ttfFile);

                        if (tbr.TrueTypeFile != null)
                        {
                            string familyName = tbr.TrueTypeFile.GetFontFamilyName();

                            this.LoadedFonts[familyName] = tbr;
                            this.KnownFonts[familyName] = familyName;
                            this.KnownFonts[tbr.TrueTypeFile.GetFontName()] = familyName;
                        }

                        return tbr;
                    }
                    else
                    {
                        return defaultLibrary.ResolveFontFamily(fontFamily);
                    }
                }
            }
            else
            {
                return defaultLibrary.ResolveFontFamily(fontFamily);
            }
        }
    }

    /// <summary>
    /// An exception that occurs while creating a <see cref="FontFamily"/>.
    /// </summary>
    public class FontFamilyCreationException : Exception
    {
        /// <summary>
        /// The name of the font family that was being created.
        /// </summary>
        public string FontFamily { get; }

        /// <summary>
        /// Create a new <see cref="FontFamilyCreationException"/> instance.
        /// </summary>
        /// <param name="fontFamily">The name of the font family that was being created.</param>
        public FontFamilyCreationException(string fontFamily) : base("The font family \"" + fontFamily + "\" could not be created!")
        {
            this.FontFamily = fontFamily;
        }
    }

    /// <summary>
    /// A default font library that resolves standard families using the embedded fonts.
    /// </summary>
    public class DefaultFontLibrary : FontLibrary
    {
        /// <inheritdoc/>
        public override FontFamily ResolveFontFamily(string fontFamily)
        {
            lock (FontFamily.fontFamilyLock)
            {
                bool isStandardFamily;

                if (FontFamily.StandardFamilies.Contains(fontFamily))
                {
                    isStandardFamily = true;
                }
                else
                {
                    isStandardFamily = false;
                }

                if (isStandardFamily)
                {
                    Stream ttfStream = FontFamily.GetManifestResourceStream(FontFamily.StandardFontFamilyResources[Array.IndexOf(FontFamily.StandardFamilies, fontFamily)]);

                    FontFamily tbr = new FontFamily(ttfStream);
                    tbr.IsStandardFamily = true;
                    tbr.FileName = fontFamily;

                    if (fontFamily == "Times-Italic" || fontFamily == "Times-BoldItalic" || fontFamily == "Helvetica-Oblique" || fontFamily == "Helvetica-BoldOblique" || fontFamily == "Courier-Oblique" || fontFamily == "Courier-BoldOblique")
                    {
                        tbr.IsItalic = true;
                        tbr.IsOblique = (fontFamily == "Courier-Oblique" || fontFamily == "Courier-BoldOblique");
                    }
                    else
                    {
                        tbr.IsItalic = false;
                        tbr.IsOblique = false;
                    }

                    return tbr;
                }
                else
                {
                    try
                    {
                        FontFamily tbr = new FontFamily(TrueTypeFile.CreateTrueTypeFile(fontFamily));
                        tbr.FileName = fontFamily;
                        return tbr;
                    }
                    catch
                    {
                        FontFamily tbr = new FontFamily();
                        tbr.FileName = fontFamily;
                        return tbr;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override FontFamily ResolveFontFamily(FontFamily.StandardFontFamilies standardFontFamily)
        {
            lock (FontFamily.fontFamilyLock)
            {
                Stream ttfStream = FontFamily.GetManifestResourceStream(FontFamily.StandardFontFamilyResources[(int)standardFontFamily]);
                FontFamily tbr = new FontFamily(ttfStream);


                tbr.IsStandardFamily = true;

                tbr.FileName = FontFamily.StandardFamilies[(int)standardFontFamily];

                if (tbr.FileName == "Times-Italic" || tbr.FileName == "Times-BoldItalic" || tbr.FileName == "Helvetica-Oblique" || tbr.FileName == "Helvetica-BoldOblique" || tbr.FileName == "Courier-Oblique" || tbr.FileName == "Courier-BoldOblique")
                {
                    tbr.IsItalic = true;
                    tbr.IsOblique = (tbr.FileName == "Courier-Oblique" || tbr.FileName == "Courier-BoldOblique");
                }
                else
                {
                    tbr.IsItalic = false;
                    tbr.IsOblique = false;
                }

                return tbr;
            }
        }
    }
}
