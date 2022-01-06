/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2022 Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, version 3.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

namespace VectSharp.Fonts
{
    /// <summary>
    /// Contains an <see cref="IFontLibrary"/> providing access to the Nimbus family of standard fonts (used e.g. by MuPDF).
    /// </summary>
    public class Nimbus
    {
        /// <summary>
        /// The font library.
        /// </summary>
        public static IFontLibrary Library { get; }

        static Nimbus()
        {
            Library = new SimpleFontLibrary(
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusRomNo9L-Reg.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Roman No9 L"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusRomNo9L-Med.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Roman No9 L"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusRomNo9L-RegIta.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Roman No9 L"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusRomNo9L-MedIta.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Roman No9 L"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusSanL-Reg.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Sans L"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusSanL-Bol.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Sans L"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusSanL-RegIta.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Sans L"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusSanL-BolIta.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Sans L"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusMono-Regular.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Mono"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusMono-Bold.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Mono"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusMono-Oblique.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Mono"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.NimbusMono-BoldOblique.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#Nimbus Mono"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.StandardSymbolsPS.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#StandardSymbolsPS"),
                new ResourceFontFamily(typeof(Nimbus).Assembly.GetManifestResourceStream("VectSharp.Fonts.Nimbus.Nimbus.D050000L.ttf"), "resm:VectSharp.Fonts.Nimbus.Nimbus.?assembly=VectSharp.Fonts.Nimbus#D050000L"));
        }
    }
}
