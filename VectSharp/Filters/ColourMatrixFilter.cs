using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VectSharp.Filters
{
    /// <summary>
    /// Represents a colour transformation matrix.
    /// </summary>
    public class ColourMatrix
    {
        /// <summary>
        /// The coefficient relating the R component of the output colour to the R component of the input colour.
        /// </summary>
        public double R1 { get; set; }

        /// <summary>
        /// The coefficient relating the R component of the output colour to the G component of the input colour.
        /// </summary>
        public double R2 { get; set; }

        /// <summary>
        /// The coefficient relating the R component of the output colour to the B component of the input colour.
        /// </summary>
        public double R3 { get; set; }

        /// <summary>
        /// The coefficient relating the R component of the output colour to the A component of the input colour.
        /// </summary>
        public double R4 { get; set; }

        /// <summary>
        /// The bias (translation) applied to the R component of the output colour.
        /// </summary>
        public double R5 { get; set; }

        /// <summary>
        /// The coefficient relating the G component of the output colour to the R component of the input colour.
        /// </summary>
        public double G1 { get; set; }

        /// <summary>
        /// The coefficient relating the G component of the output colour to the G component of the input colour.
        /// </summary>
        public double G2 { get; set; }

        /// <summary>
        /// The coefficient relating the G component of the output colour to the B component of the input colour.
        /// </summary>
        public double G3 { get; set; }

        /// <summary>
        /// The coefficient relating the G component of the output colour to the A component of the input colour.
        /// </summary>
        public double G4 { get; set; }

        /// <summary>
        /// The bias (translation) applied to the R component of the output colour.
        /// </summary>
        public double G5 { get; set; }

        /// <summary>
        /// The coefficient relating the B component of the output colour to the R component of the input colour.
        /// </summary>
        public double B1 { get; set; }

        /// <summary>
        /// The coefficient relating the B component of the output colour to the G component of the input colour.
        /// </summary>
        public double B2 { get; set; }

        /// <summary>
        /// The coefficient relating the B component of the output colour to the B component of the input colour.
        /// </summary>
        public double B3 { get; set; }

        /// <summary>
        /// The coefficient relating the B component of the output colour to the A component of the input colour.
        /// </summary>
        public double B4 { get; set; }

        /// <summary>
        /// The bias (translation) applied to the B component of the output colour.
        /// </summary>
        public double B5 { get; set; }

        /// <summary>
        /// The coefficient relating the A component of the output colour to the R component of the input colour.
        /// </summary>
        public double A1 { get; set; }

        /// <summary>
        /// The coefficient relating the A component of the output colour to the G component of the input colour.
        /// </summary>
        public double A2 { get; set; }

        /// <summary>
        /// The coefficient relating the A component of the output colour to the B component of the input colour.
        /// </summary>
        public double A3 { get; set; }

        /// <summary>
        /// The coefficient relating the A component of the output colour to the A component of the input colour.
        /// </summary>
        public double A4 { get; set; }

        /// <summary>
        /// The bias (translation) applied to the A component of the output colour.
        /// </summary>
        public double A5 { get; set; }

        /// <summary>
        /// Gets or sets the requested element of the matrix. Elements of the last row of the matrix can be read, but not set.
        /// </summary>
        /// <param name="y">The row of the matrix.</param>
        /// <param name="x">The column of the matrix.</param>
        /// <returns>The requested element of the matrix.</returns>
        /// <exception cref="ArgumentOutOfRangeException">An attempt has been made to access an element out of the bounds of the matrix.</exception>
        public double this[int y, int x]
        {
            get
            {
                switch (y)
                {
                    case 0:
                        switch (x)
                        {
                            case 0:
                                return R1;
                            case 1:
                                return R2;
                            case 2:
                                return R3;
                            case 3:
                                return R4;
                            case 4:
                                return R5;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(x), x, "Coordinate out of range!");
                        }

                    case 1:
                        switch (x)
                        {
                            case 0:
                                return G1;
                            case 1:
                                return G2;
                            case 2:
                                return G3;
                            case 3:
                                return G4;
                            case 4:
                                return G5;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(x), x, "Coordinate out of range!");
                        }

                    case 2:
                        switch (x)
                        {
                            case 0:
                                return B1;
                            case 1:
                                return B2;
                            case 2:
                                return B3;
                            case 3:
                                return B4;
                            case 4:
                                return B5;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(x), x, "Coordinate out of range!");
                        }

                    case 3:
                        switch (x)
                        {
                            case 0:
                                return A1;
                            case 1:
                                return A2;
                            case 2:
                                return A3;
                            case 3:
                                return A4;
                            case 4:
                                return A5;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(x), x, "Coordinate out of range!");
                        }

                    case 4:
                        switch (x)
                        {
                            case 0:
                                return 0;
                            case 1:
                                return 0;
                            case 2:
                                return 0;
                            case 3:
                                return 0;
                            case 4:
                                return 1;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(x), x, "Coordinate out of range!");
                        }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(y), y, "Coordinate out of range!");
                }
            }

            private set
            {
                switch (y)
                {
                    case 0:
                        switch (x)
                        {
                            case 0:
                                R1 = value;
                                break;
                            case 1:
                                R2 = value;
                                break;
                            case 2:
                                R3 = value;
                                break;
                            case 3:
                                R4 = value;
                                break;
                            case 4:
                                R5 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(x), x, "Coordinate out of range!");
                        }
                        break;

                    case 1:
                        switch (x)
                        {
                            case 0:
                                G1 = value;
                                break;
                            case 1:
                                G2 = value;
                                break;
                            case 2:
                                G3 = value;
                                break;
                            case 3:
                                G4 = value;
                                break;
                            case 4:
                                G5 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(x), x, "Coordinate out of range!");
                        }
                        break;

                    case 2:
                        switch (x)
                        {
                            case 0:
                                B1 = value;
                                break;
                            case 1:
                                B2 = value;
                                break;
                            case 2:
                                B3 = value;
                                break;
                            case 3:
                                B4 = value;
                                break;
                            case 4:
                                B5 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(x), x, "Coordinate out of range!");
                        }
                        break;

                    case 3:
                        switch (x)
                        {
                            case 0:
                                A1 = value;
                                break;
                            case 1:
                                A2 = value;
                                break;
                            case 2:
                                A3 = value;
                                break;
                            case 3:
                                A4 = value;
                                break;
                            case 4:
                                A5 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(x), x, "Coordinate out of range!");
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(y), y, "Coordinate out of range!");
                }
            }
        }

        /// <summary>
        /// A <see cref="ColourMatrix"/> that whose output colour is always the same as the input colour.
        /// </summary>
        public static ColourMatrix Identity = new ColourMatrix(new double[,] { { 1, 0, 0, 0, 0 }, { 0, 1, 0, 0, 0 }, { 0, 0, 1, 0, 0 }, { 0, 0, 0, 1, 0 }, { 0, 0, 0, 0, 1 } });

        /// <summary>
        /// A <see cref="ColourMatrix"/> that transforms every colour in a shade of grey with approximately the same luminance.
        /// </summary>
        public static ColourMatrix GreyScale = new ColourMatrix(new double[,] { { 0.2126, 0.7152, 0.0722, 0, 0 }, { 0.2126, 0.7152, 0.0722, 0, 0 }, { 0.2126, 0.7152, 0.0722, 0, 0 }, { 0, 0, 0, 1, 0 }, { 0, 0, 0, 0, 1 } });

        /// <summary>
        /// A <see cref="ColourMatrix"/> producing a "pastel" (desaturation) effect.
        /// </summary>
        public static ColourMatrix Pastel = new ColourMatrix(new double[,] { { 0.75, 0.25, 0.25, 0, 0 }, { 0.25, 0.75, 0.25, 0, 0 }, { 0.25, 0.25, 0.75, 0, 0 }, { 0, 0, 0, 1, 0 }, { 0, 0, 0, 0, 1 } });

        /// <summary>
        /// A <see cref="ColourMatrix"/> that inverts every colour, leaving the alpha component intact.
        /// </summary>
        public static ColourMatrix Inversion = new ColourMatrix(new double[,] { { -1, 0, 0, 0, 1 }, { 0, -1, 0, 0, 1 }, { 0, 0, -1, 0, 1 }, { 0, 0, 0, 1, 0 }, { 0, 0, 0, 0, 1 } });

        /// <summary>
        /// A <see cref="ColourMatrix"/> that inverts the alpha component, leaving the other components intact.
        /// </summary>
        public static ColourMatrix AlphaInversion = new ColourMatrix(new double[,] { { 1, 0, 0, 0, 0 }, { 0, 1, 0, 0, 0 }, { 0, 0, 1, 0, 0 }, { 0, 0, 0, -1, 1 }, { 0, 0, 0, 0, 1 } });

        /// <summary>
        /// A <see cref="ColourMatrix"/> that shifts every colour component by an amount corresponding to the inverted alpha value. The alpha component is left intact.
        /// </summary>
        public static ColourMatrix InvertedAlphaShift = new ColourMatrix(new double[,] { { 1, 0, 0, -1, 1 }, { 0, 1, 0, -1, 1 }, { 0, 0, 1, -1, 1 }, { 0, 0, 0, 1, 0 }, { 0, 0, 0, 0, 1 } });

        /// <summary>
        /// Creates a <see cref="ColourMatrix"/> that turns every colour to which it is applied into the specified <paramref name="colour"/>.
        /// </summary>
        /// <param name="colour">The colour that will be produced by the <see cref="ColourMatrix"/>.</param>
        /// <param name="useAlpha">If this is <see langword="true"/>, all output pixels will have the same alpha value as the supplied <paramref name="colour"/>. If this is false, the alpha value of the input pixels is preserved.</param>
        /// <returns>A <see cref="ColourMatrix"/> that turns every colour to which it is applied into the specified <paramref name="colour"/>.</returns>
        public static ColourMatrix ToColour(Colour colour, bool useAlpha = false)
        {
            if (!useAlpha)
            {
                return new ColourMatrix(new double[,] { { 0, 0, 0, 0, colour.R }, { 0, 0, 0, 0, colour.G }, { 0, 0, 0, 0, colour.B }, { 0, 0, 0, 1, 0 }, { 0, 0, 0, 0, 1 } });
            }
            else
            {
                return new ColourMatrix(new double[,] { { 0, 0, 0, 0, colour.R }, { 0, 0, 0, 0, colour.G }, { 0, 0, 0, 0, colour.B }, { 0, 0, 0, 0, colour.A }, { 0, 0, 0, 0, 1 } });
            }
        }

        /// <summary>
        /// Creates a <see cref="ColourMatrix"/> that turns every colour to which it is applied into a shade of the specified <paramref name="colour"/>. The brightness of the output colour depends on the luminance of the input colour.
        /// </summary>
        /// <param name="colour">The colour whose shades will be produced by the <see cref="ColourMatrix"/>.</param>
        /// <param name="useAlpha">If this is <see langword="true"/>, the transformation will also be applied to the alpha channel. If this is false, the alpha value of the input pixels is preserved.</param>
        /// <returns>A <see cref="ColourMatrix"/> that turns every colour to which it is applied into a shade of the specified <paramref name="colour"/>.</returns>
        public static ColourMatrix LuminanceToColour(Colour colour, bool useAlpha = false)
        {
            if (!useAlpha)
            {
                return new ColourMatrix(new double[,] { { 0.2126 * colour.R, 0.7152 * colour.R, 0.0722 * colour.R, 0, 0 }, { 0.2126 * colour.G, 0.7152 * colour.G, 0.0722 * colour.G, 0, 0 }, { 0.2126 * colour.B, 0.7152 * colour.B, 0.0722 * colour.B, 0, 0 }, { 0, 0, 0, 1, 0 }, { 0, 0, 0, 0, 1 } });
            }
            else
            {
                return new ColourMatrix(new double[,] { { 0.2126 * colour.R, 0.7152 * colour.R, 0.0722 * colour.R, 0, 0 }, { 0.2126 * colour.G, 0.7152 * colour.G, 0.0722 * colour.G, 0, 0 }, { 0.2126 * colour.B, 0.7152 * colour.B, 0.0722 * colour.B, 0, 0 }, { 0.2126 * colour.A, 0.7152 * colour.A, 0.0722 * colour.A, 0, 0 }, { 0, 0, 0, 0, 1 } });
            }
        }

        /// <summary>
        /// Creates a <see cref="ColourMatrix"/> that transforms the alpha value of the colour it is applied to into a value depending on the luminance of the input colour.
        /// </summary>
        /// <param name="preserveColour">If this is <see langword="true"/>, the values of the red, green and blue components of the input colour are preserved in the output colour. If this is false, the output colour will always be black.</param>
        /// <returns>A <see cref="ColourMatrix"/> that transforms the alpha value of the colour it is applied to into a value depending on the luminance of the input colour.</returns>
        public static ColourMatrix LuminanceToAlpha(bool preserveColour = false)
        {
            if (!preserveColour)
            {
                return new ColourMatrix(new double[,] { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 }, { 0.2126, 0.7152, 0.0722, 0, 0 }, { 0, 0, 0, 0, 1 } });
            }
            else
            {
                return new ColourMatrix(new double[,] { { 1, 0, 0, 0, 0 }, { 0, 1, 0, 0, 0 }, { 0, 0, 1, 0, 0 }, { 0.2126, 0.7152, 0.0722, 0, 0 }, { 0, 0, 0, 0, 1 } });
            }
        }

        /// <summary>
        /// Creates a new <see cref="ColourMatrix"/> whose alpha coefficients are multiplied by the specified value.
        /// </summary>
        /// <param name="alpha">The value that will be used to multiply all the alpha coefficients of the <see cref="ColourMatrix"/>.</param>
        /// <returns>A new <see cref="ColourMatrix"/> whose alpha coefficients have been multiplied by the specified value.</returns>
        public ColourMatrix WithAlpha(double alpha)
        {
            return new ColourMatrix(new double[,] { { R1, R2, R3, R4, R5 }, { G1, G2, G3, G4, G5 }, { B1, B2, B3, B4, B5 }, { A1 * alpha, A2 * alpha, A3 * alpha, A4 * alpha, A5 * alpha }, { 0, 0, 0, 0, 1 } });
        }

        /// <summary>
        /// Concatenates two matrices. The resulting <see cref="ColourMatrix"/> is equivalent to first applying <paramref name="matrix2"/>, and then <paramref name="matrix1"/>.
        /// </summary>
        /// <param name="matrix1">The matrix that acts second.</param>
        /// <param name="matrix2">The matrix that acts first.</param>
        /// <returns>A <see cref="ColourMatrix"/> equivalent to first applying <paramref name="matrix2"/>, and then <paramref name="matrix1"/>.</returns>
        public static ColourMatrix operator *(ColourMatrix matrix1, ColourMatrix matrix2)
        {
            double[,] tbr = new double[5, 5];

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        tbr[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                }
            }

            return new ColourMatrix(tbr);
        }

        /// <summary>
        /// Creates a new <see cref="ColourMatrix"/> with the specified coefficients.
        /// </summary>
        /// <param name="matrix">The coefficients of the <see cref="ColourMatrix"/>.</param>
        public ColourMatrix(double[,] matrix)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    this[i, j] = matrix[i, j];
                }
            }
        }

        /// <summary>
        /// Applies the <see cref="ColourMatrix"/> to the specified <see cref="Colour"/>.
        /// </summary>
        /// <param name="colour">The <see cref="Colour"/> to which the <see cref="ColourMatrix"/> should be applied.</param>
        /// <returns>The result of applying the <see cref="ColourMatrix"/> to the specified colour.</returns>
        public Colour Apply(Colour colour)
        {
            double[] col = new double[] { colour.R, colour.G, colour.B, colour.A, 1 };

            double[] tbr = new double[5];

            for (int i = 0; i < tbr.Length; i++)
            {
                for (int j = 0; j < tbr.Length; j++)
                {
                    tbr[i] += this[i, j] * col[j];
                }
            }

            return Colour.FromRgba(tbr[0], tbr[1], tbr[2], tbr[3]);
        }

        /// <summary>
        /// Applies the <see cref="ColourMatrix"/> to the specified colour, represented as four bytes, and stores the resulting colour in the
        /// same variables as the original RGBA values.
        /// </summary>
        /// <param name="R">The R component of the input colour. After this method returns, this will contain the R component of the output colour.</param>
        /// <param name="G">The G component of the input colour. After this method returns, this will contain the G component of the output colour.</param>
        /// <param name="B">The B component of the input colour. After this method returns, this will contain the B component of the output colour.</param>
        /// <param name="A">The A component of the input colour. After this method returns, this will contain the A component of the output colour.</param>
        public void Apply(ref byte R, ref byte G, ref byte B, ref byte A)
        {
            byte r = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.R1 + G * this.R2 + B * this.R3 + A * this.R4 + this.R5 * 255)));
            byte g = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.G1 + G * this.G2 + B * this.G3 + A * this.G4 + this.G5 * 255)));
            byte b = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.B1 + G * this.B2 + B * this.B3 + A * this.B4 + this.B5 * 255)));
            byte a = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.A1 + G * this.A2 + B * this.A3 + A * this.A4 + this.A5 * 255)));

            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Applies the <see cref="ColourMatrix"/> to the specified colour, represented as three bytes, and stores the resulting colour in the
        /// same variables as the original RGB values.
        /// </summary>
        /// <param name="R">The R component of the input colour. After this method returns, this will contain the R component of the output colour.</param>
        /// <param name="G">The G component of the input colour. After this method returns, this will contain the G component of the output colour.</param>
        /// <param name="B">The B component of the input colour. After this method returns, this will contain the B component of the output colour.</param>
        public void Apply(ref byte R, ref byte G, ref byte B)
        {
            byte r = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.R1 + G * this.R2 + B * this.R3 + 255 * this.R4 + this.R5 * 255)));
            byte g = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.G1 + G * this.G2 + B * this.G3 + 255 * this.G4 + this.G5 * 255)));
            byte b = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.B1 + G * this.B2 + B * this.B3 + 255 * this.B4 + this.B5 * 255)));

            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Applies the <see cref="ColourMatrix"/> to the specified colour, represented as four bytes, and stores the resulting colour in the
        /// specified output bytes.
        /// </summary>
        /// <param name="R">The R component of the input colour.</param>
        /// <param name="G">The G component of the input colour.</param>
        /// <param name="B">The B component of the input colour.</param>
        /// <param name="A">The A component of the input colour.</param>
        /// <param name="r">After this method returns, this will contain the R component of the output colour.</param>
        /// <param name="g">After this method returns, this will contain the G component of the output colour.</param>
        /// <param name="b">After this method returns, this will contain the B component of the output colour.</param>
        /// <param name="a">After this method returns, this will contain the A component of the output colour.</param>
        public void Apply(byte R, byte G, byte B, byte A, out byte r, out byte g, out byte b, out byte a)
        {
            r = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.R1 + G * this.R2 + B * this.R3 + A * this.R4 + this.R5 * 255)));
            g = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.G1 + G * this.G2 + B * this.G3 + A * this.G4 + this.G5 * 255)));
            b = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.B1 + G * this.B2 + B * this.B3 + A * this.B4 + this.B5 * 255)));
            a = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.A1 + G * this.A2 + B * this.A3 + A * this.A4 + this.A5 * 255)));
        }

        /// <summary>
        /// Applies the <see cref="ColourMatrix"/> to the specified colour, represented as three bytes, and stores the resulting colour in the
        /// specified output bytes.
        /// </summary>
        /// <param name="R">The R component of the input colour.</param>
        /// <param name="G">The G component of the input colour.</param>
        /// <param name="B">The B component of the input colour.</param>
        /// <param name="r">After this method returns, this will contain the R component of the output colour.</param>
        /// <param name="g">After this method returns, this will contain the G component of the output colour.</param>
        /// <param name="b">After this method returns, this will contain the B component of the output colour.</param>
        public void Apply(byte R, byte G, byte B, out byte r, out byte g, out byte b)
        {
            r = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.R1 + G * this.R2 + B * this.R3 + 255 * this.R4 + this.R5 * 255)));
            g = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.G1 + G * this.G2 + B * this.G3 + 255 * this.G4 + this.G5 * 255)));
            b = (byte)Math.Min(255, Math.Max(0, Math.Round(R * this.B1 + G * this.B2 + B * this.B3 + 255 * this.B4 + this.B5 * 255)));
        }
    }

    /// <summary>
    /// Represents a filter that applies a <see cref="Filters.ColourMatrix"/> to the colours of the image.
    /// </summary>
    public class ColourMatrixFilter : ILocationInvariantFilter
    {
        /// <summary>
        /// The <see cref="Filters.ColourMatrix"/> that is applied by this filter.
        /// </summary>
        public ColourMatrix ColourMatrix { get; }

        /// <inheritdoc/>
        public Point TopLeftMargin { get; } = new Point();

        /// <inheritdoc/>
        public Point BottomRightMargin { get; } = new Point();

        /// <summary>
        /// Creates a new <see cref="ColourMatrixFilter"/> with the specified <see cref="Filters.ColourMatrix"/>.
        /// </summary>
        /// <param name="colorMatrix">The <see cref="Filters.ColourMatrix"/> that will be applied by the filter.</param>
        public ColourMatrixFilter(ColourMatrix colorMatrix)
        {
            this.ColourMatrix = colorMatrix;
        }

        /// <inheritdoc/>
        public RasterImage Filter(RasterImage image, double scale)
        {
            IntPtr tbrData = System.Runtime.InteropServices.Marshal.AllocHGlobal(image.Width * image.Height * (image.HasAlpha ? 4 : 3));
            GC.AddMemoryPressure(image.Width * image.Height * (image.HasAlpha ? 4 : 3));

            int width = image.Width;
            int height = image.Height;

            int pixelSize = image.HasAlpha ? 4 : 3;
            int stride = image.Width * pixelSize;

            int threads = Math.Min(8, Environment.ProcessorCount);

            unsafe
            {
                byte* input = (byte*)image.ImageDataAddress;
                byte* output = (byte*)tbrData;

                Action<int> yLoop;

                if (image.HasAlpha)
                {
                    yLoop = (y) =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            this.ColourMatrix.Apply(input[y * stride + x * 4], input[y * stride + x * 4 + 1], input[y * stride + x * 4 + 2], input[y * stride + x * 4 + 3], out output[y * stride + x * 4], out output[y * stride + x * 4 + 1], out output[y * stride + x * 4 + 2], out output[y * stride + x * 4 + 3]);
                        }
                    };
                }
                else
                {
                    yLoop = (y) =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            this.ColourMatrix.Apply(input[y * stride + x * 4], input[y * stride + x * 4 + 1], input[y * stride + x * 4 + 2], out output[y * stride + x * 4], out output[y * stride + x * 4 + 1], out output[y * stride + x * 4 + 2]);
                        }
                    };
                }

                if (threads == 1)
                {
                    for (int y = 0; y < height; y++)
                    {
                        yLoop(y);
                    }
                }
                else
                {
                    ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = threads };

                    Parallel.For(0, height, options, yLoop);
                }

            }

            DisposableIntPtr disp = new DisposableIntPtr(tbrData);

            return new RasterImage(ref disp, width, height, image.HasAlpha, image.Interpolate);
        }
    }
}
