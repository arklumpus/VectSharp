using System;
using System.Collections.Generic;
using System.Text;

namespace VectSharp
{
    public partial struct Colour
    {
        private static Dictionary<string, Colour> StandardColours = new Dictionary<string, Colour>(StringComparer.OrdinalIgnoreCase)
        {
            { "Black", Colour.FromRgb(0, 0, 0) },
            { "Navy", Colour.FromRgb(0, 0, 128) },
            { "DarkBlue", Colour.FromRgb(0, 0, 139) },
            { "MediumBlue", Colour.FromRgb(0, 0, 205) },
            { "Blue", Colour.FromRgb(0, 0, 255) },
            { "DarkGreen", Colour.FromRgb(0, 100, 0) },
            { "Green", Colour.FromRgb(0, 128, 0) },
            { "Teal", Colour.FromRgb(0, 128, 128) },
            { "DarkCyan", Colour.FromRgb(0, 139, 139) },
            { "DeepSkyBlue", Colour.FromRgb(0, 191, 255) },
            { "DarkTurquoise", Colour.FromRgb(0, 206, 209) },
            { "MediumSpringGreen", Colour.FromRgb(0, 250, 154) },
            { "Lime", Colour.FromRgb(0, 255, 0) },
            { "SpringGreen", Colour.FromRgb(0, 255, 127) },
            { "Aqua", Colour.FromRgb(0, 255, 255) },
            { "Cyan", Colour.FromRgb(0, 255, 255) },
            { "MidnightBlue", Colour.FromRgb(25, 25, 112) },
            { "DodgerBlue", Colour.FromRgb(30, 144, 255) },
            { "LightSeaGreen", Colour.FromRgb(32, 178, 170) },
            { "ForestGreen", Colour.FromRgb(34, 139, 34) },
            { "SeaGreen", Colour.FromRgb(46, 139, 87) },
            { "DarkSlateGray", Colour.FromRgb(47, 79, 79) },
            { "DarkSlateGrey", Colour.FromRgb(47, 79, 79) },
            { "LimeGreen", Colour.FromRgb(50, 205, 50) },
            { "MediumSeaGreen", Colour.FromRgb(60, 179, 113) },
            { "Turquoise", Colour.FromRgb(64, 224, 208) },
            { "RoyalBlue", Colour.FromRgb(65, 105, 225) },
            { "SteelBlue", Colour.FromRgb(70, 130, 180) },
            { "DarkSlateBlue", Colour.FromRgb(72, 61, 139) },
            { "MediumTurquoise", Colour.FromRgb(72, 209, 204) },
            { "Indigo", Colour.FromRgb(75, 0, 130) },
            { "DarkOliveGreen", Colour.FromRgb(85, 107, 47) },
            { "CadetBlue", Colour.FromRgb(95, 158, 160) },
            { "CornflowerBlue", Colour.FromRgb(100, 149, 237) },
            { "RebeccaPurple", Colour.FromRgb(102, 51, 153) },
            { "MediumAquaMarine", Colour.FromRgb(102, 205, 170) },
            { "DimGray", Colour.FromRgb(105, 105, 105) },
            { "DimGrey", Colour.FromRgb(105, 105, 105) },
            { "SlateBlue", Colour.FromRgb(106, 90, 205) },
            { "OliveDrab", Colour.FromRgb(107, 142, 35) },
            { "SlateGray", Colour.FromRgb(112, 128, 144) },
            { "SlateGrey", Colour.FromRgb(112, 128, 144) },
            { "LightSlateGray", Colour.FromRgb(119, 136, 153) },
            { "LightSlateGrey", Colour.FromRgb(119, 136, 153) },
            { "MediumSlateBlue", Colour.FromRgb(123, 104, 238) },
            { "LawnGreen", Colour.FromRgb(124, 252, 0) },
            { "Chartreuse", Colour.FromRgb(127, 255, 0) },
            { "Aquamarine", Colour.FromRgb(127, 255, 212) },
            { "Maroon", Colour.FromRgb(128, 0, 0) },
            { "Purple", Colour.FromRgb(128, 0, 128) },
            { "Olive", Colour.FromRgb(128, 128, 0) },
            { "Gray", Colour.FromRgb(128, 128, 128) },
            { "Grey", Colour.FromRgb(128, 128, 128) },
            { "SkyBlue", Colour.FromRgb(135, 206, 235) },
            { "LightSkyBlue", Colour.FromRgb(135, 206, 250) },
            { "BlueViolet", Colour.FromRgb(138, 43, 226) },
            { "DarkRed", Colour.FromRgb(139, 0, 0) },
            { "DarkMagenta", Colour.FromRgb(139, 0, 139) },
            { "SaddleBrown", Colour.FromRgb(139, 69, 19) },
            { "DarkSeaGreen", Colour.FromRgb(143, 188, 143) },
            { "LightGreen", Colour.FromRgb(144, 238, 144) },
            { "MediumPurple", Colour.FromRgb(147, 112, 219) },
            { "DarkViolet", Colour.FromRgb(148, 0, 211) },
            { "PaleGreen", Colour.FromRgb(152, 251, 152) },
            { "DarkOrchid", Colour.FromRgb(153, 50, 204) },
            { "YellowGreen", Colour.FromRgb(154, 205, 50) },
            { "Sienna", Colour.FromRgb(160, 82, 45) },
            { "Brown", Colour.FromRgb(165, 42, 42) },
            { "DarkGray", Colour.FromRgb(169, 169, 169) },
            { "DarkGrey", Colour.FromRgb(169, 169, 169) },
            { "LightBlue", Colour.FromRgb(173, 216, 230) },
            { "GreenYellow", Colour.FromRgb(173, 255, 47) },
            { "PaleTurquoise", Colour.FromRgb(175, 238, 238) },
            { "LightSteelBlue", Colour.FromRgb(176, 196, 222) },
            { "PowderBlue", Colour.FromRgb(176, 224, 230) },
            { "FireBrick", Colour.FromRgb(178, 34, 34) },
            { "DarkGoldenRod", Colour.FromRgb(184, 134, 11) },
            { "MediumOrchid", Colour.FromRgb(186, 85, 211) },
            { "RosyBrown", Colour.FromRgb(188, 143, 143) },
            { "DarkKhaki", Colour.FromRgb(189, 183, 107) },
            { "Silver", Colour.FromRgb(192, 192, 192) },
            { "MediumVioletRed", Colour.FromRgb(199, 21, 133) },
            { "IndianRed", Colour.FromRgb(205, 92, 92) },
            { "Peru", Colour.FromRgb(205, 133, 63) },
            { "Chocolate", Colour.FromRgb(210, 105, 30) },
            { "Tan", Colour.FromRgb(210, 180, 140) },
            { "LightGray", Colour.FromRgb(211, 211, 211) },
            { "LightGrey", Colour.FromRgb(211, 211, 211) },
            { "Thistle", Colour.FromRgb(216, 191, 216) },
            { "Orchid", Colour.FromRgb(218, 112, 214) },
            { "GoldenRod", Colour.FromRgb(218, 165, 32) },
            { "PaleVioletRed", Colour.FromRgb(219, 112, 147) },
            { "Crimson", Colour.FromRgb(220, 20, 60) },
            { "Gainsboro", Colour.FromRgb(220, 220, 220) },
            { "Plum", Colour.FromRgb(221, 160, 221) },
            { "BurlyWood", Colour.FromRgb(222, 184, 135) },
            { "LightCyan", Colour.FromRgb(224, 255, 255) },
            { "Lavender", Colour.FromRgb(230, 230, 250) },
            { "DarkSalmon", Colour.FromRgb(233, 150, 122) },
            { "Violet", Colour.FromRgb(238, 130, 238) },
            { "PaleGoldenRod", Colour.FromRgb(238, 232, 170) },
            { "LightCoral", Colour.FromRgb(240, 128, 128) },
            { "Khaki", Colour.FromRgb(240, 230, 140) },
            { "AliceBlue", Colour.FromRgb(240, 248, 255) },
            { "HoneyDew", Colour.FromRgb(240, 255, 240) },
            { "Azure", Colour.FromRgb(240, 255, 255) },
            { "SandyBrown", Colour.FromRgb(244, 164, 96) },
            { "Wheat", Colour.FromRgb(245, 222, 179) },
            { "Beige", Colour.FromRgb(245, 245, 220) },
            { "WhiteSmoke", Colour.FromRgb(245, 245, 245) },
            { "MintCream", Colour.FromRgb(245, 255, 250) },
            { "GhostWhite", Colour.FromRgb(248, 248, 255) },
            { "Salmon", Colour.FromRgb(250, 128, 114) },
            { "AntiqueWhite", Colour.FromRgb(250, 235, 215) },
            { "Linen", Colour.FromRgb(250, 240, 230) },
            { "LightGoldenRodYellow", Colour.FromRgb(250, 250, 210) },
            { "OldLace", Colour.FromRgb(253, 245, 230) },
            { "Red", Colour.FromRgb(255, 0, 0) },
            { "Fuchsia", Colour.FromRgb(255, 0, 255) },
            { "Magenta", Colour.FromRgb(255, 0, 255) },
            { "DeepPink", Colour.FromRgb(255, 20, 147) },
            { "OrangeRed", Colour.FromRgb(255, 69, 0) },
            { "Tomato", Colour.FromRgb(255, 99, 71) },
            { "HotPink", Colour.FromRgb(255, 105, 180) },
            { "Coral", Colour.FromRgb(255, 127, 80) },
            { "DarkOrange", Colour.FromRgb(255, 140, 0) },
            { "LightSalmon", Colour.FromRgb(255, 160, 122) },
            { "Orange", Colour.FromRgb(255, 165, 0) },
            { "LightPink", Colour.FromRgb(255, 182, 193) },
            { "Pink", Colour.FromRgb(255, 192, 203) },
            { "Gold", Colour.FromRgb(255, 215, 0) },
            { "PeachPuff", Colour.FromRgb(255, 218, 185) },
            { "NavajoWhite", Colour.FromRgb(255, 222, 173) },
            { "Moccasin", Colour.FromRgb(255, 228, 181) },
            { "Bisque", Colour.FromRgb(255, 228, 196) },
            { "MistyRose", Colour.FromRgb(255, 228, 225) },
            { "BlanchedAlmond", Colour.FromRgb(255, 235, 205) },
            { "PapayaWhip", Colour.FromRgb(255, 239, 213) },
            { "LavenderBlush", Colour.FromRgb(255, 240, 245) },
            { "SeaShell", Colour.FromRgb(255, 245, 238) },
            { "Cornsilk", Colour.FromRgb(255, 248, 220) },
            { "LemonChiffon", Colour.FromRgb(255, 250, 205) },
            { "FloralWhite", Colour.FromRgb(255, 250, 240) },
            { "Snow", Colour.FromRgb(255, 250, 250) },
            { "Yellow", Colour.FromRgb(255, 255, 0) },
            { "LightYellow", Colour.FromRgb(255, 255, 224) },
            { "Ivory", Colour.FromRgb(255, 255, 240) },
            { "White", Colour.FromRgb(255, 255, 255) },
        };
    }

    /// <summary>
    /// Standard colours.
    /// </summary>
    public static class Colours
    {
        /// <summary>
        /// Black #000000
        /// </summary>
        public static Colour Black = Colour.FromRgb(0, 0, 0);
        /// <summary>
        /// Navy #000080
        /// </summary>
        public static Colour Navy = Colour.FromRgb(0, 0, 128);
        /// <summary>
        /// DarkBlue #00008B
        /// </summary>
        public static Colour DarkBlue = Colour.FromRgb(0, 0, 139);
        /// <summary>
        /// MediumBlue #0000CD
        /// </summary>
        public static Colour MediumBlue = Colour.FromRgb(0, 0, 205);
        /// <summary>
        /// Blue #0000FF
        /// </summary>
        public static Colour Blue = Colour.FromRgb(0, 0, 255);
        /// <summary>
        /// DarkGreen #006400
        /// </summary>
        public static Colour DarkGreen = Colour.FromRgb(0, 100, 0);
        /// <summary>
        /// Green #008000
        /// </summary>
        public static Colour Green = Colour.FromRgb(0, 128, 0);
        /// <summary>
        /// Teal #008080
        /// </summary>
        public static Colour Teal = Colour.FromRgb(0, 128, 128);
        /// <summary>
        /// DarkCyan #008B8B
        /// </summary>
        public static Colour DarkCyan = Colour.FromRgb(0, 139, 139);
        /// <summary>
        /// DeepSkyBlue #00BFFF
        /// </summary>
        public static Colour DeepSkyBlue = Colour.FromRgb(0, 191, 255);
        /// <summary>
        /// DarkTurquoise #00CED1
        /// </summary>
        public static Colour DarkTurquoise = Colour.FromRgb(0, 206, 209);
        /// <summary>
        /// MediumSpringGreen #00FA9A
        /// </summary>
        public static Colour MediumSpringGreen = Colour.FromRgb(0, 250, 154);
        /// <summary>
        /// Lime #00FF00
        /// </summary>
        public static Colour Lime = Colour.FromRgb(0, 255, 0);
        /// <summary>
        /// SpringGreen #00FF7F
        /// </summary>
        public static Colour SpringGreen = Colour.FromRgb(0, 255, 127);
        /// <summary>
        /// Aqua #00FFFF
        /// </summary>
        public static Colour Aqua = Colour.FromRgb(0, 255, 255);
        /// <summary>
        /// Cyan #00FFFF
        /// </summary>
        public static Colour Cyan = Colour.FromRgb(0, 255, 255);
        /// <summary>
        /// MidnightBlue #191970
        /// </summary>
        public static Colour MidnightBlue = Colour.FromRgb(25, 25, 112);
        /// <summary>
        /// DodgerBlue #1E90FF
        /// </summary>
        public static Colour DodgerBlue = Colour.FromRgb(30, 144, 255);
        /// <summary>
        /// LightSeaGreen #20B2AA
        /// </summary>
        public static Colour LightSeaGreen = Colour.FromRgb(32, 178, 170);
        /// <summary>
        /// ForestGreen #228B22
        /// </summary>
        public static Colour ForestGreen = Colour.FromRgb(34, 139, 34);
        /// <summary>
        /// SeaGreen #2E8B57
        /// </summary>
        public static Colour SeaGreen = Colour.FromRgb(46, 139, 87);
        /// <summary>
        /// DarkSlateGray #2F4F4F
        /// </summary>
        public static Colour DarkSlateGray = Colour.FromRgb(47, 79, 79);
        /// <summary>
        /// DarkSlateGrey #2F4F4F
        /// </summary>
        public static Colour DarkSlateGrey = Colour.FromRgb(47, 79, 79);
        /// <summary>
        /// LimeGreen #32CD32
        /// </summary>
        public static Colour LimeGreen = Colour.FromRgb(50, 205, 50);
        /// <summary>
        /// MediumSeaGreen #3CB371
        /// </summary>
        public static Colour MediumSeaGreen = Colour.FromRgb(60, 179, 113);
        /// <summary>
        /// Turquoise #40E0D0
        /// </summary>
        public static Colour Turquoise = Colour.FromRgb(64, 224, 208);
        /// <summary>
        /// RoyalBlue #4169E1
        /// </summary>
        public static Colour RoyalBlue = Colour.FromRgb(65, 105, 225);
        /// <summary>
        /// SteelBlue #4682B4
        /// </summary>
        public static Colour SteelBlue = Colour.FromRgb(70, 130, 180);
        /// <summary>
        /// DarkSlateBlue #483D8B
        /// </summary>
        public static Colour DarkSlateBlue = Colour.FromRgb(72, 61, 139);
        /// <summary>
        /// MediumTurquoise #48D1CC
        /// </summary>
        public static Colour MediumTurquoise = Colour.FromRgb(72, 209, 204);
        /// <summary>
        /// Indigo #4B0082
        /// </summary>
        public static Colour Indigo = Colour.FromRgb(75, 0, 130);
        /// <summary>
        /// DarkOliveGreen #556B2F
        /// </summary>
        public static Colour DarkOliveGreen = Colour.FromRgb(85, 107, 47);
        /// <summary>
        /// CadetBlue #5F9EA0
        /// </summary>
        public static Colour CadetBlue = Colour.FromRgb(95, 158, 160);
        /// <summary>
        /// CornflowerBlue #6495ED
        /// </summary>
        public static Colour CornflowerBlue = Colour.FromRgb(100, 149, 237);
        /// <summary>
        /// RebeccaPurple #663399
        /// </summary>
        public static Colour RebeccaPurple = Colour.FromRgb(102, 51, 153);
        /// <summary>
        /// MediumAquaMarine #66CDAA
        /// </summary>
        public static Colour MediumAquaMarine = Colour.FromRgb(102, 205, 170);
        /// <summary>
        /// DimGray #696969
        /// </summary>
        public static Colour DimGray = Colour.FromRgb(105, 105, 105);
        /// <summary>
        /// DimGrey #696969
        /// </summary>
        public static Colour DimGrey = Colour.FromRgb(105, 105, 105);
        /// <summary>
        /// SlateBlue #6A5ACD
        /// </summary>
        public static Colour SlateBlue = Colour.FromRgb(106, 90, 205);
        /// <summary>
        /// OliveDrab #6B8E23
        /// </summary>
        public static Colour OliveDrab = Colour.FromRgb(107, 142, 35);
        /// <summary>
        /// SlateGray #708090
        /// </summary>
        public static Colour SlateGray = Colour.FromRgb(112, 128, 144);
        /// <summary>
        /// SlateGrey #708090
        /// </summary>
        public static Colour SlateGrey = Colour.FromRgb(112, 128, 144);
        /// <summary>
        /// LightSlateGray #778899
        /// </summary>
        public static Colour LightSlateGray = Colour.FromRgb(119, 136, 153);
        /// <summary>
        /// LightSlateGrey #778899
        /// </summary>
        public static Colour LightSlateGrey = Colour.FromRgb(119, 136, 153);
        /// <summary>
        /// MediumSlateBlue #7B68EE
        /// </summary>
        public static Colour MediumSlateBlue = Colour.FromRgb(123, 104, 238);
        /// <summary>
        /// LawnGreen #7CFC00
        /// </summary>
        public static Colour LawnGreen = Colour.FromRgb(124, 252, 0);
        /// <summary>
        /// Chartreuse #7FFF00
        /// </summary>
        public static Colour Chartreuse = Colour.FromRgb(127, 255, 0);
        /// <summary>
        /// Aquamarine #7FFFD4
        /// </summary>
        public static Colour Aquamarine = Colour.FromRgb(127, 255, 212);
        /// <summary>
        /// Maroon #800000
        /// </summary>
        public static Colour Maroon = Colour.FromRgb(128, 0, 0);
        /// <summary>
        /// Purple #800080
        /// </summary>
        public static Colour Purple = Colour.FromRgb(128, 0, 128);
        /// <summary>
        /// Olive #808000
        /// </summary>
        public static Colour Olive = Colour.FromRgb(128, 128, 0);
        /// <summary>
        /// Gray #808080
        /// </summary>
        public static Colour Gray = Colour.FromRgb(128, 128, 128);
        /// <summary>
        /// Grey #808080
        /// </summary>
        public static Colour Grey = Colour.FromRgb(128, 128, 128);
        /// <summary>
        /// SkyBlue #87CEEB
        /// </summary>
        public static Colour SkyBlue = Colour.FromRgb(135, 206, 235);
        /// <summary>
        /// LightSkyBlue #87CEFA
        /// </summary>
        public static Colour LightSkyBlue = Colour.FromRgb(135, 206, 250);
        /// <summary>
        /// BlueViolet #8A2BE2
        /// </summary>
        public static Colour BlueViolet = Colour.FromRgb(138, 43, 226);
        /// <summary>
        /// DarkRed #8B0000
        /// </summary>
        public static Colour DarkRed = Colour.FromRgb(139, 0, 0);
        /// <summary>
        /// DarkMagenta #8B008B
        /// </summary>
        public static Colour DarkMagenta = Colour.FromRgb(139, 0, 139);
        /// <summary>
        /// SaddleBrown #8B4513
        /// </summary>
        public static Colour SaddleBrown = Colour.FromRgb(139, 69, 19);
        /// <summary>
        /// DarkSeaGreen #8FBC8F
        /// </summary>
        public static Colour DarkSeaGreen = Colour.FromRgb(143, 188, 143);
        /// <summary>
        /// LightGreen #90EE90
        /// </summary>
        public static Colour LightGreen = Colour.FromRgb(144, 238, 144);
        /// <summary>
        /// MediumPurple #9370DB
        /// </summary>
        public static Colour MediumPurple = Colour.FromRgb(147, 112, 219);
        /// <summary>
        /// DarkViolet #9400D3
        /// </summary>
        public static Colour DarkViolet = Colour.FromRgb(148, 0, 211);
        /// <summary>
        /// PaleGreen #98FB98
        /// </summary>
        public static Colour PaleGreen = Colour.FromRgb(152, 251, 152);
        /// <summary>
        /// DarkOrchid #9932CC
        /// </summary>
        public static Colour DarkOrchid = Colour.FromRgb(153, 50, 204);
        /// <summary>
        /// YellowGreen #9ACD32
        /// </summary>
        public static Colour YellowGreen = Colour.FromRgb(154, 205, 50);
        /// <summary>
        /// Sienna #A0522D
        /// </summary>
        public static Colour Sienna = Colour.FromRgb(160, 82, 45);
        /// <summary>
        /// Brown #A52A2A
        /// </summary>
        public static Colour Brown = Colour.FromRgb(165, 42, 42);
        /// <summary>
        /// DarkGray #A9A9A9
        /// </summary>
        public static Colour DarkGray = Colour.FromRgb(169, 169, 169);
        /// <summary>
        /// DarkGrey #A9A9A9
        /// </summary>
        public static Colour DarkGrey = Colour.FromRgb(169, 169, 169);
        /// <summary>
        /// LightBlue #ADD8E6
        /// </summary>
        public static Colour LightBlue = Colour.FromRgb(173, 216, 230);
        /// <summary>
        /// GreenYellow #ADFF2F
        /// </summary>
        public static Colour GreenYellow = Colour.FromRgb(173, 255, 47);
        /// <summary>
        /// PaleTurquoise #AFEEEE
        /// </summary>
        public static Colour PaleTurquoise = Colour.FromRgb(175, 238, 238);
        /// <summary>
        /// LightSteelBlue #B0C4DE
        /// </summary>
        public static Colour LightSteelBlue = Colour.FromRgb(176, 196, 222);
        /// <summary>
        /// PowderBlue #B0E0E6
        /// </summary>
        public static Colour PowderBlue = Colour.FromRgb(176, 224, 230);
        /// <summary>
        /// FireBrick #B22222
        /// </summary>
        public static Colour FireBrick = Colour.FromRgb(178, 34, 34);
        /// <summary>
        /// DarkGoldenRod #B8860B
        /// </summary>
        public static Colour DarkGoldenRod = Colour.FromRgb(184, 134, 11);
        /// <summary>
        /// MediumOrchid #BA55D3
        /// </summary>
        public static Colour MediumOrchid = Colour.FromRgb(186, 85, 211);
        /// <summary>
        /// RosyBrown #BC8F8F
        /// </summary>
        public static Colour RosyBrown = Colour.FromRgb(188, 143, 143);
        /// <summary>
        /// DarkKhaki #BDB76B
        /// </summary>
        public static Colour DarkKhaki = Colour.FromRgb(189, 183, 107);
        /// <summary>
        /// Silver #C0C0C0
        /// </summary>
        public static Colour Silver = Colour.FromRgb(192, 192, 192);
        /// <summary>
        /// MediumVioletRed #C71585
        /// </summary>
        public static Colour MediumVioletRed = Colour.FromRgb(199, 21, 133);
        /// <summary>
        /// IndianRed #CD5C5C
        /// </summary>
        public static Colour IndianRed = Colour.FromRgb(205, 92, 92);
        /// <summary>
        /// Peru #CD853F
        /// </summary>
        public static Colour Peru = Colour.FromRgb(205, 133, 63);
        /// <summary>
        /// Chocolate #D2691E
        /// </summary>
        public static Colour Chocolate = Colour.FromRgb(210, 105, 30);
        /// <summary>
        /// Tan #D2B48C
        /// </summary>
        public static Colour Tan = Colour.FromRgb(210, 180, 140);
        /// <summary>
        /// LightGray #D3D3D3
        /// </summary>
        public static Colour LightGray = Colour.FromRgb(211, 211, 211);
        /// <summary>
        /// LightGrey #D3D3D3
        /// </summary>
        public static Colour LightGrey = Colour.FromRgb(211, 211, 211);
        /// <summary>
        /// Thistle #D8BFD8
        /// </summary>
        public static Colour Thistle = Colour.FromRgb(216, 191, 216);
        /// <summary>
        /// Orchid #DA70D6
        /// </summary>
        public static Colour Orchid = Colour.FromRgb(218, 112, 214);
        /// <summary>
        /// GoldenRod #DAA520
        /// </summary>
        public static Colour GoldenRod = Colour.FromRgb(218, 165, 32);
        /// <summary>
        /// PaleVioletRed #DB7093
        /// </summary>
        public static Colour PaleVioletRed = Colour.FromRgb(219, 112, 147);
        /// <summary>
        /// Crimson #DC143C
        /// </summary>
        public static Colour Crimson = Colour.FromRgb(220, 20, 60);
        /// <summary>
        /// Gainsboro #DCDCDC
        /// </summary>
        public static Colour Gainsboro = Colour.FromRgb(220, 220, 220);
        /// <summary>
        /// Plum #DDA0DD
        /// </summary>
        public static Colour Plum = Colour.FromRgb(221, 160, 221);
        /// <summary>
        /// BurlyWood #DEB887
        /// </summary>
        public static Colour BurlyWood = Colour.FromRgb(222, 184, 135);
        /// <summary>
        /// LightCyan #E0FFFF
        /// </summary>
        public static Colour LightCyan = Colour.FromRgb(224, 255, 255);
        /// <summary>
        /// Lavender #E6E6FA
        /// </summary>
        public static Colour Lavender = Colour.FromRgb(230, 230, 250);
        /// <summary>
        /// DarkSalmon #E9967A
        /// </summary>
        public static Colour DarkSalmon = Colour.FromRgb(233, 150, 122);
        /// <summary>
        /// Violet #EE82EE
        /// </summary>
        public static Colour Violet = Colour.FromRgb(238, 130, 238);
        /// <summary>
        /// PaleGoldenRod #EEE8AA
        /// </summary>
        public static Colour PaleGoldenRod = Colour.FromRgb(238, 232, 170);
        /// <summary>
        /// LightCoral #F08080
        /// </summary>
        public static Colour LightCoral = Colour.FromRgb(240, 128, 128);
        /// <summary>
        /// Khaki #F0E68C
        /// </summary>
        public static Colour Khaki = Colour.FromRgb(240, 230, 140);
        /// <summary>
        /// AliceBlue #F0F8FF
        /// </summary>
        public static Colour AliceBlue = Colour.FromRgb(240, 248, 255);
        /// <summary>
        /// HoneyDew #F0FFF0
        /// </summary>
        public static Colour HoneyDew = Colour.FromRgb(240, 255, 240);
        /// <summary>
        /// Azure #F0FFFF
        /// </summary>
        public static Colour Azure = Colour.FromRgb(240, 255, 255);
        /// <summary>
        /// SandyBrown #F4A460
        /// </summary>
        public static Colour SandyBrown = Colour.FromRgb(244, 164, 96);
        /// <summary>
        /// Wheat #F5DEB3
        /// </summary>
        public static Colour Wheat = Colour.FromRgb(245, 222, 179);
        /// <summary>
        /// Beige #F5F5DC
        /// </summary>
        public static Colour Beige = Colour.FromRgb(245, 245, 220);
        /// <summary>
        /// WhiteSmoke #F5F5F5
        /// </summary>
        public static Colour WhiteSmoke = Colour.FromRgb(245, 245, 245);
        /// <summary>
        /// MintCream #F5FFFA
        /// </summary>
        public static Colour MintCream = Colour.FromRgb(245, 255, 250);
        /// <summary>
        /// GhostWhite #F8F8FF
        /// </summary>
        public static Colour GhostWhite = Colour.FromRgb(248, 248, 255);
        /// <summary>
        /// Salmon #FA8072
        /// </summary>
        public static Colour Salmon = Colour.FromRgb(250, 128, 114);
        /// <summary>
        /// AntiqueWhite #FAEBD7
        /// </summary>
        public static Colour AntiqueWhite = Colour.FromRgb(250, 235, 215);
        /// <summary>
        /// Linen #FAF0E6
        /// </summary>
        public static Colour Linen = Colour.FromRgb(250, 240, 230);
        /// <summary>
        /// LightGoldenRodYellow #FAFAD2
        /// </summary>
        public static Colour LightGoldenRodYellow = Colour.FromRgb(250, 250, 210);
        /// <summary>
        /// OldLace #FDF5E6
        /// </summary>
        public static Colour OldLace = Colour.FromRgb(253, 245, 230);
        /// <summary>
        /// Red #FF0000
        /// </summary>
        public static Colour Red = Colour.FromRgb(255, 0, 0);
        /// <summary>
        /// Fuchsia #FF00FF
        /// </summary>
        public static Colour Fuchsia = Colour.FromRgb(255, 0, 255);
        /// <summary>
        /// Magenta #FF00FF
        /// </summary>
        public static Colour Magenta = Colour.FromRgb(255, 0, 255);
        /// <summary>
        /// DeepPink #FF1493
        /// </summary>
        public static Colour DeepPink = Colour.FromRgb(255, 20, 147);
        /// <summary>
        /// OrangeRed #FF4500
        /// </summary>
        public static Colour OrangeRed = Colour.FromRgb(255, 69, 0);
        /// <summary>
        /// Tomato #FF6347
        /// </summary>
        public static Colour Tomato = Colour.FromRgb(255, 99, 71);
        /// <summary>
        /// HotPink #FF69B4
        /// </summary>
        public static Colour HotPink = Colour.FromRgb(255, 105, 180);
        /// <summary>
        /// Coral #FF7F50
        /// </summary>
        public static Colour Coral = Colour.FromRgb(255, 127, 80);
        /// <summary>
        /// DarkOrange #FF8C00
        /// </summary>
        public static Colour DarkOrange = Colour.FromRgb(255, 140, 0);
        /// <summary>
        /// LightSalmon #FFA07A
        /// </summary>
        public static Colour LightSalmon = Colour.FromRgb(255, 160, 122);
        /// <summary>
        /// Orange #FFA500
        /// </summary>
        public static Colour Orange = Colour.FromRgb(255, 165, 0);
        /// <summary>
        /// LightPink #FFB6C1
        /// </summary>
        public static Colour LightPink = Colour.FromRgb(255, 182, 193);
        /// <summary>
        /// Pink #FFC0CB
        /// </summary>
        public static Colour Pink = Colour.FromRgb(255, 192, 203);
        /// <summary>
        /// Gold #FFD700
        /// </summary>
        public static Colour Gold = Colour.FromRgb(255, 215, 0);
        /// <summary>
        /// PeachPuff #FFDAB9
        /// </summary>
        public static Colour PeachPuff = Colour.FromRgb(255, 218, 185);
        /// <summary>
        /// NavajoWhite #FFDEAD
        /// </summary>
        public static Colour NavajoWhite = Colour.FromRgb(255, 222, 173);
        /// <summary>
        /// Moccasin #FFE4B5
        /// </summary>
        public static Colour Moccasin = Colour.FromRgb(255, 228, 181);
        /// <summary>
        /// Bisque #FFE4C4
        /// </summary>
        public static Colour Bisque = Colour.FromRgb(255, 228, 196);
        /// <summary>
        /// MistyRose #FFE4E1
        /// </summary>
        public static Colour MistyRose = Colour.FromRgb(255, 228, 225);
        /// <summary>
        /// BlanchedAlmond #FFEBCD
        /// </summary>
        public static Colour BlanchedAlmond = Colour.FromRgb(255, 235, 205);
        /// <summary>
        /// PapayaWhip #FFEFD5
        /// </summary>
        public static Colour PapayaWhip = Colour.FromRgb(255, 239, 213);
        /// <summary>
        /// LavenderBlush #FFF0F5
        /// </summary>
        public static Colour LavenderBlush = Colour.FromRgb(255, 240, 245);
        /// <summary>
        /// SeaShell #FFF5EE
        /// </summary>
        public static Colour SeaShell = Colour.FromRgb(255, 245, 238);
        /// <summary>
        /// Cornsilk #FFF8DC
        /// </summary>
        public static Colour Cornsilk = Colour.FromRgb(255, 248, 220);
        /// <summary>
        /// LemonChiffon #FFFACD
        /// </summary>
        public static Colour LemonChiffon = Colour.FromRgb(255, 250, 205);
        /// <summary>
        /// FloralWhite #FFFAF0
        /// </summary>
        public static Colour FloralWhite = Colour.FromRgb(255, 250, 240);
        /// <summary>
        /// Snow #FFFAFA
        /// </summary>
        public static Colour Snow = Colour.FromRgb(255, 250, 250);
        /// <summary>
        /// Yellow #FFFF00
        /// </summary>
        public static Colour Yellow = Colour.FromRgb(255, 255, 0);
        /// <summary>
        /// LightYellow #FFFFE0
        /// </summary>
        public static Colour LightYellow = Colour.FromRgb(255, 255, 224);
        /// <summary>
        /// Ivory #FFFFF0
        /// </summary>
        public static Colour Ivory = Colour.FromRgb(255, 255, 240);
        /// <summary>
        /// White #FFFFFF
        /// </summary>
        public static Colour White = Colour.FromRgb(255, 255, 255);
    }
}
