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
using System.Diagnostics.Contracts;

namespace VectSharp
{
    /// <summary>
    /// Represents an RGB colour.
    /// </summary>
    public partial struct Colour : IEquatable<Colour>
    {
        /// <summary>
        /// Red component of the colour. Range: [0, 1].
        /// </summary>
        public double R;

        /// <summary>
        /// Green component of the colour. Range: [0, 1].
        /// </summary>
        public double G;

        /// <summary>
        /// Blue component of the colour. Range: [0, 1].
        /// </summary>
        public double B;

        /// <summary>
        /// Alpha component of the colour. Range: [0, 1].
        /// </summary>
        public double A;

        private Colour(double r, double g, double b, double a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Create a new colour from RGB (red, green and blue) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 1].</param>
        /// <param name="g">The green component of the colour. Range: [0, 1].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 1].</param>
        /// <returns>A <see cref="Colour"/> struct with the specified components and an alpha component of 1.</returns>
        public static Colour FromRgb(double r, double g, double b)
        {
            return new Colour(r, g, b, 1);
        }

        /// <summary>
        /// Create a new colour from RGB (red, green and blue) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <returns>A <see cref="Colour"/> struct with the specified components and an alpha component of 1.</returns>
        public static Colour FromRgb(byte r, byte g, byte b)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, 1);
        }

        /// <summary>
        /// Create a new colour from RGB (red, green and blue) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <returns>A <see cref="Colour"/> struct with the specified components and an alpha component of 1.</returns>
        public static Colour FromRgb(int r, int g, int b)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, 1);
        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 1].</param>
        /// <param name="g">The green component of the colour. Range: [0, 1].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 1].</param>
        /// <param name="a">The alpha component of the colour. Range: [0, 1].</param>
        /// <returns>A <see cref="Colour"/> struct with the specified components.</returns>
        public static Colour FromRgba(double r, double g, double b, double a)
        {
            return new Colour(r, g, b, a);
        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <param name="a">The alpha component of the colour. Range: [0, 255].</param>
        /// <returns>A <see cref="Colour"/><see cref="Colour"/> struct with the specified components.</returns>
        public static Colour FromRgba(byte r, byte g, byte b, byte a)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <param name="a">The alpha component of the colour. Range: [0, 1].</param>
        /// <returns>A <see cref="Colour"/> struct with the specified components.</returns>
        public static Colour FromRgba(byte r, byte g, byte b, double a)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, a);
        }
        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <param name="a">The alpha component of the colour. Range: [0, 255].</param>
        /// <returns>A <see cref="Colour"/> struct with the specified components.</returns>
        public static Colour FromRgba(int r, int g, int b, int a)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <param name="a">The alpha component of the colour. Range: [0, 1].</param>
        /// <returns>A <see cref="Colour"/> struct with the specified components.</returns>
        public static Colour FromRgba(int r, int g, int b, double a)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, a);
        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="colour">A <see cref="ValueTuple{Int32, Int32, Int32, Double}"/> containing component information for the colour. For r, g, and b, range: [0, 255]; for a, range: [0, 1].</param>
        /// <returns>A <see cref="Colour"/> struct with the specified components.</returns>
        public static Colour FromRgba((int r, int g, int b, double a) colour)
        {
            return new Colour(colour.r / 255.0, colour.g / 255.0, colour.b / 255.0, colour.a);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Colour))
            {
                return false;
            }
            else
            {
                return this.Equals((Colour)obj);
            }
        }

        /// <inheritdoc/>
        public bool Equals(Colour col)
        {
            return col.R == this.R && col.G == this.G && col.B == this.B && col.A == this.A;
        }

        /// <inheritdoc/>
        public static bool operator ==(Colour col1, Colour col2)
        {
            return col1.R == col2.R && col1.G == col2.G && col1.B == col2.B && col1.A == col2.A;
        }

        /// <inheritdoc/>
        public static bool operator !=(Colour col1, Colour col2)
        {
            return col1.R != col2.R || col1.G != col2.G || col1.B != col2.B || col1.A != col2.A;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (int)(this.R * 255 + this.G * 255 * 255 + this.B * 255 * 255 * 255 + this.A * 255 * 255 * 255 * 255);
        }

        /// <summary>
        /// Convert the <see cref="Colour"/> object into a hex string that is constituted by a "#" followed by two-digit hexadecimal representations of the red, green and blue components of the colour (in the range 0x00 - 0xFF).
        /// Optionally also includes opacity (alpha channel) data.
        /// </summary>
        /// <param name="includeAlpha">Whether two additional hex digits representing the colour's opacity (alpha channel) should be included in the string.</param>
        /// <returns>A hex colour string.</returns>
        public string ToCSSString(bool includeAlpha)
        {
            if (includeAlpha)
            {
                return "#" + ((int)Math.Round(this.R * 255)).ToString("X2") + ((int)Math.Round(this.G * 255)).ToString("X2") + ((int)Math.Round(this.B * 255)).ToString("X2") + ((int)Math.Round(this.A * 255)).ToString("X2");
            }
            else
            {
                return "#" + ((int)Math.Round(this.R * 255)).ToString("X2") + ((int)Math.Round(this.G * 255)).ToString("X2") + ((int)Math.Round(this.B * 255)).ToString("X2");
            }
        }

        /// <summary>
        /// Convert a CSS colour string into a <see cref="Colour"/> object.
        /// </summary>
        /// <param name="cssString">The CSS colour string. In addition to 148 standard colour names (case-insensitive), #RGB, #RGBA, #RRGGBB and #RRGGBBAA hex strings and rgb(r, g, b) and rgba(r, g, b, a) functional colour notations are supported.</param>
        /// <returns></returns>
        public static Colour? FromCSSString(string cssString)
        {
            if (cssString.StartsWith("#"))
            {
                cssString = cssString.Substring(1);

                try
                {
                    if (cssString.Length == 3)
                    {
                        byte r = byte.Parse(cssString.Substring(0, 1) + cssString.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                        byte g = byte.Parse(cssString.Substring(1, 1) + cssString.Substring(1, 1), System.Globalization.NumberStyles.HexNumber);
                        byte b = byte.Parse(cssString.Substring(2, 1) + cssString.Substring(2, 1), System.Globalization.NumberStyles.HexNumber);

                        return Colour.FromRgb(r, g, b);
                    }
                    else if (cssString.Length == 4)
                    {
                        byte r = byte.Parse(cssString.Substring(0, 1) + cssString.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                        byte g = byte.Parse(cssString.Substring(1, 1) + cssString.Substring(1, 1), System.Globalization.NumberStyles.HexNumber);
                        byte b = byte.Parse(cssString.Substring(2, 1) + cssString.Substring(2, 1), System.Globalization.NumberStyles.HexNumber);
                        byte a = byte.Parse(cssString.Substring(3, 1) + cssString.Substring(3, 1), System.Globalization.NumberStyles.HexNumber);

                        return Colour.FromRgba(r, g, b, a);
                    }
                    else if (cssString.Length == 6)
                    {
                        byte r = byte.Parse(cssString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        byte g = byte.Parse(cssString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        byte b = byte.Parse(cssString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

                        return Colour.FromRgb(r, g, b);
                    }
                    else if (cssString.Length == 8)
                    {
                        byte r = byte.Parse(cssString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        byte g = byte.Parse(cssString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        byte b = byte.Parse(cssString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                        byte a = byte.Parse(cssString.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

                        return Colour.FromRgba(r, g, b, a);
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
            else if (cssString.StartsWith("rgb(") || cssString.StartsWith("rgba("))
            {
                try
                {
                    cssString = cssString.Substring(cssString.IndexOf("(") + 1).Replace(")", "").Replace(" ", "");
                    string[] splitCssString = cssString.Split(',');

                    double R = ParseColourValueOrPercentage(splitCssString[0]);
                    double G = ParseColourValueOrPercentage(splitCssString[1]);
                    double B = ParseColourValueOrPercentage(splitCssString[2]);

                    double A = 1;

                    if (splitCssString.Length == 4)
                    {
                        A = double.Parse(splitCssString[3], System.Globalization.CultureInfo.InvariantCulture);
                    }

                    return Colour.FromRgba(R, G, B, A);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (StandardColours.TryGetValue(cssString, out Colour tbr))
                {
                    return tbr;
                }
                else
                {
                    return null;
                }
            }
        }

        private static double ParseColourValueOrPercentage(string value)
        {
            if (int.TryParse(value, out int tbr))
            {
                return tbr / 255.0;
            }
            else if (value.Contains("%"))
            {
                return double.Parse(value.Replace("%", ""), System.Globalization.CultureInfo.InvariantCulture) / 100.0;
            }
            else
            {
                return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Create a new <see cref="Colour"/> with the same RGB components as the <paramref name="original"/> <see cref="Colour"/>, but with the specified <paramref name="alpha"/>.
        /// </summary>
        /// <param name="original">The original <see cref="Colour"/> from which the RGB components will be taken.</param>
        /// <param name="alpha">The alpha component of the new <see cref="Colour"/>.</param>
        /// <returns>A <see cref="Colour"/> struct with the same RGB components as the <paramref name="original"/> <see cref="Colour"/> and the specified <paramref name="alpha"/>.</returns>
        public static Colour WithAlpha(Colour original, double alpha)
        {
            return Colour.FromRgba(original.R, original.G, original.B, alpha);
        }

        /// <summary>
        /// Create a new <see cref="Colour"/> with the same RGB components as the <paramref name="original"/> <see cref="Colour"/>, but with the specified <paramref name="alpha"/>.
        /// </summary>
        /// <param name="original">The original <see cref="Colour"/> from which the RGB components will be taken.</param>
        /// <param name="alpha">The alpha component of the new <see cref="Colour"/>.</param>
        /// <returns>A <see cref="Colour"/> struct with the same RGB components as the <paramref name="original"/> <see cref="Colour"/> and the specified <paramref name="alpha"/>.</returns>
        public static Colour WithAlpha(Colour original, byte alpha)
        {
            return Colour.FromRgba(original.R, original.G, original.B, (double)alpha / 255.0);
        }

        /// <summary>
        /// Create a new <see cref="Colour"/> with the same RGB components as the current <see cref="Colour"/>, but with the specified <paramref name="alpha"/>.
        /// </summary>
        /// <param name="alpha">The alpha component of the new <see cref="Colour"/>.</param>
        /// <returns>A <see cref="Colour"/> struct with the same RGB components as the current <see cref="Colour"/> and the specified <paramref name="alpha"/>.</returns>
        [Pure]
        public Colour WithAlpha(double alpha)
        {
            return Colour.FromRgba(this.R, this.G, this.B, alpha);
        }

        /// <summary>
        /// Create a new <see cref="Colour"/> with the same RGB components as the current <see cref="Colour"/>, but with the specified <paramref name="alpha"/>.
        /// </summary>
        /// <param name="alpha">The alpha component of the new <see cref="Colour"/>.</param>
        /// <returns>A <see cref="Colour"/> struct with the same RGB components as the current <see cref="Colour"/> and the specified <paramref name="alpha"/>.</returns>
        [Pure]
        public Colour WithAlpha(byte alpha)
        {
            return Colour.FromRgba(this.R, this.G, this.B, (double)alpha / 255.0);
        }

        /// <summary>
        /// Converts a <see cref="Colour"/> to the CIE XYZ colour space.
        /// </summary>
        /// <returns>A <see cref="ValueTuple"/> containing the X, Y and Z components of the <see cref="Colour"/>.</returns>
        public (double X, double Y, double Z) ToXYZ()
        {
            double r, g, b;

            if (R <= 0.04045)
            {
                r = 25 * R / 323;
            }
            else
            {
                r = Math.Pow((200 * R + 11) / 211, 2.4);
            }

            if (G <= 0.04045)
            {
                g = 25 * G / 323;
            }
            else
            {
                g = Math.Pow((200 * G + 11) / 211, 2.4);
            }

            if (B <= 0.04045)
            {
                b = 25 * B / 323;
            }
            else
            {
                b = Math.Pow((200 * B + 11) / 211, 2.4);
            }

            double x = 0.41239080 * r + 0.35758434 * g + 0.18048079 * b;
            double y = 0.21263901 * r + 0.71516868 * g + 0.07219232 * b;
            double z = 0.01933082 * r + 0.11919478 * g + 0.95053215 * b;

            return (x, y, z);
        }

        /// <summary>
        /// Creates a <see cref="Colour"/> from CIE XYZ coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <returns>An sRGB <see cref="Colour"/> created from the specified components.</returns>
        public static Colour FromXYZ(double x, double y, double z)
        {
            double r = +3.24096994 * x - 1.53738318 * y - 0.49861076 * z;
            double g = -0.96924364 * x + 1.8759675 * y + 0.04155506 * z;
            double b = 0.05563008 * x - 0.20397696 * y + 1.05697151 * z;

            if (r <= 0.0031308)
            {
                r = 323 * r / 25;
            }
            else
            {
                r = (211 * Math.Pow(r, 1 / 2.4) - 11) / 200;
            }

            if (g <= 0.0031308)
            {
                g = 323 * g / 25;
            }
            else
            {
                g = (211 * Math.Pow(g, 1 / 2.4) - 11) / 200;
            }

            if (b <= 0.0031308)
            {
                b = 323 * b / 25;
            }
            else
            {
                b = (211 * Math.Pow(b, 1 / 2.4) - 11) / 200;
            }

            r = Math.Min(Math.Max(0, r), 1);
            g = Math.Min(Math.Max(0, g), 1);
            b = Math.Min(Math.Max(0, b), 1);

            return Colour.FromRgb(r, g, b);
        }

        /// <summary>
        /// Converts a <see cref="Colour"/> to the CIE Lab colour space (under Illuminant D65).
        /// </summary>
        /// <returns>A <see cref="ValueType"/> containing the L*, a* and b* components of the <see cref="Colour"/>.</returns>
        public (double L, double a, double b) ToLab()
        {
            double f(double t)
            {
                const double d = 6.0 / 29;

                if (t > d * d * d)
                {
                    return Math.Pow(t, 1.0 / 3);
                }
                else
                {
                    return t / (3 * d * d) + 4.0 / 29;
                }
            }

            const double xN = 0.950489;
            const double yN = 1;
            const double zN = 1.088840;

            (double x, double y, double z) = this.ToXYZ();

            double fY = f(y / yN);

            double l = 1.16 * fY - 0.16;
            double a = 5 * (f(x / xN) - fY);
            double b = 2 * (fY - f(z / zN));

            return (l, a, b);
        }

        /// <summary>
        /// Creates a <see cref="Colour"/> from CIE Lab coordinates (under Illuminant D65).
        /// </summary>
        /// <param name="L">The L* component.</param>
        /// <param name="a">The a* component.</param>
        /// <param name="b">The b* component.</param>
        /// <returns>An sRGB <see cref="Colour"/> created from the specified components.</returns>
        public static Colour FromLab(double L, double a, double b)
        {
            double f(double t)
            {
                const double d = 6.0 / 29;

                if (t > d)
                {
                    return t * t * t;
                }
                else
                {
                    return 3 * d * d * (t - 4.0 / 29);
                }
            }

            const double xN = 0.950489;
            const double yN = 1;
            const double zN = 1.088840;

            double x = xN * f((L + 0.16) / 1.16 + a / 5);
            double y = yN * f((L + 0.16) / 1.16);
            double z = zN * f((L + 0.16) / 1.16 - b / 2);

            return Colour.FromXYZ(x, y, z);
        }

        /// <summary>
        /// Converts a <see cref="Colour"/> to the HSL colour space.
        /// </summary>
        /// <returns>A <see cref="ValueType"/> containing the H, S and L components of the <see cref="Colour"/>. Each component has range [0, 1].</returns>
        public (double H, double S, double L) ToHSL()
        {
            double xMax = Math.Max(Math.Max(R, G), B);
            double xMin = Math.Min(Math.Min(R, G), B);

            double l = (xMax + xMin) * 0.5;

            double h;

            if (xMax == xMin)
            {
                h = 0;
            }
            else if (xMax == R)
            {
                h = (G - B) / (xMax - xMin) / 6;
            }
            else if (xMax == G)
            {
                h = (2 + (B - R) / (xMax - xMin)) / 6;
            }
            else
            {
                h = (4 + (R - G) / (xMax - xMin)) / 6;
            }

            double s;

            if (l == 0 || l == 1)
            {
                s = 0;
            }
            else
            {
                s = (xMax - l) / Math.Min(l, 1 - l);
            }

            return (h, s, l);
        }

        /// <summary>
        /// Creates a <see cref="Colour"/> from HSL coordinates.
        /// </summary>
        /// <param name="h">The H component. Should be in range [0, 1].</param>
        /// <param name="s">The S component. Should be in range [0, 1].</param>
        /// <param name="l">The L component. Should be in range [0, 1].</param>
        /// <returns>A <see cref="Colour"/> created from the specified components.</returns>
        public static Colour FromHSL(double h, double s, double l)
        {
            double c = (1 - Math.Abs(2 * l - 1)) * s;

            double hp = h * 6;

            double x = c * (1 - Math.Abs((hp % 2) - 1));

            double r1, g1, b1;

            if (hp <= 1)
            {
                r1 = c;
                g1 = x;
                b1 = 0;
            }
            else if (hp <= 2)
            {
                r1 = x;
                g1 = c;
                b1 = 0;
            }
            else if (hp <= 3)
            {
                r1 = 0;
                g1 = c;
                b1 = x;
            }
            else if (hp <= 4)
            {
                r1 = 0;
                g1 = x;
                b1 = c;
            }
            else if (hp <= 5)
            {
                r1 = x;
                g1 = 0;
                b1 = c;
            }
            else if (hp <= 6)
            {
                r1 = c;
                g1 = 0;
                b1 = x;
            }
            else
            {
                r1 = 0;
                g1 = 0;
                b1 = 0;
            }

            double m = l - c / 2;

            return Colour.FromRgb(r1 + m, g1 + m, b1 + m);
        }

    }
}
