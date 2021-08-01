using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace VectSharp
{
    /// <summary>
    /// Represents a brush used to fill or stroke graphics elements. This could be a solid colour, or a more complicated gradient or pattern.
    /// </summary>
    public abstract class Brush
    {
        internal Brush() { }

        /// <summary>
        /// Returns a brush corresponding the current instance, with the specified <paramref name="opacity"/> multiplication applied.
        /// </summary>
        /// <param name="opacity">The value that will be used to multiply the opacity of the brush.</param>
        /// <returns>A brush corresponding the current instance, with the specified <paramref name="opacity"/> multiplication applied.</returns>
        public abstract Brush MultiplyOpacity(double opacity);

        /// <summary>
        /// Implicitly converts a <see cref="Colour"/> into a <see cref="SolidColourBrush"/>.
        /// </summary>
        /// <param name="colour">The <see cref="Colour"/> to use for the brush.</param>
        public static implicit operator Brush(Colour colour)
        {
            return new SolidColourBrush(colour);
        }
    }

    /// <summary>
    /// Represents a brush painting with a single solid colour.
    /// </summary>
    public class SolidColourBrush : Brush
    {
        /// <summary>
        /// The colour of the brush.
        /// </summary>
        public Colour Colour { get; }

        /// <summary>
        /// Red component of the colour. Range: [0, 1].
        /// </summary>
        public double R => Colour.R;

        /// <summary>
        /// Green component of the colour. Range: [0, 1].
        /// </summary>
        public double G => Colour.G;

        /// <summary>
        /// Blue component of the colour. Range: [0, 1].
        /// </summary>
        public double B => Colour.B;

        /// <summary>
        /// Alpha component of the colour. Range: [0, 1].
        /// </summary>
        public double A => Colour.A;

        /// <summary>
        /// Creates a new <see cref="SolidColourBrush"/> with the specified <paramref name="colour"/>.
        /// </summary>
        /// <param name="colour">The <see cref="Colour"/> to use for the brush.</param>
        public SolidColourBrush(Colour colour)
        {
            this.Colour = colour;
        }

        /// <inheritdoc/>
        public override Brush MultiplyOpacity(double opacity)
        {
            return new SolidColourBrush(this.Colour.WithAlpha(this.Colour.A * opacity));
        }

        /// <summary>
        /// Implicitly converts a <see cref="Colour"/> into a <see cref="SolidColourBrush"/>.
        /// </summary>
        /// <param name="colour">The <see cref="Colour"/> to use for the brush.</param>
        public static implicit operator SolidColourBrush(Colour colour)
        {
            return new SolidColourBrush(colour);
        }
    }

    /// <summary>
    /// Represents a colour stop in a gradient.
    /// </summary>
    public struct GradientStop
    {
        /// <summary>
        /// The <see cref="Colour"/> at the gradient stop.
        /// </summary>
        public Colour Colour { get; }

        /// <summary>
        /// The offset of the gradient stop. Range: [0, 1].
        /// </summary>
        public double Offset { get; }

        /// <summary>
        /// Creates a new <see cref="GradientStop"/> instance.
        /// </summary>
        /// <param name="colour">The <see cref="Colour"/> at the gradient stop.</param>
        /// <param name="offset">The offset of the gradient stop. Range: [0, 1].</param>
        public GradientStop(Colour colour, double offset)
        {
            this.Colour = colour;
            this.Offset = Math.Max(0, Math.Min(1, offset));
        }

        /// <summary>
        /// Returns a <see cref="GradientStop"/> corresponding to the current instance, whose colour's opacity has been multiplied by the specified value.
        /// </summary>
        /// <param name="opacity">The value that will be used to multiply the colour's opacity.</param>
        /// <returns>A <see cref="GradientStop"/> corresponding to the current instance, whose colour's opacity has been multiplied by the specified value.</returns>
        public GradientStop MultiplyOpacity(double opacity)
        {
            return new GradientStop(this.Colour.WithAlpha(this.Colour.A * opacity), this.Offset);
        }
    }

    /// <summary>
    /// Represents a read-only list of <see cref="GradientStop"/>s.
    /// </summary>
    public class GradientStops : IReadOnlyList<GradientStop>
    {
        /// <summary>
        /// The minimum distance that is enforced between consecutive gradient stops.
        /// </summary>
        public static readonly double StopTolerance = 1e-7;

        /// <inheritdoc/>
        public GradientStop this[int index] => gradientStops[index];

        /// <inheritdoc/>
        public int Count => gradientStops.Count;

        private ImmutableList<GradientStop> gradientStops { get; set; }

        /// <inheritdoc/>
        public IEnumerator<GradientStop> GetEnumerator()
        {
            return ((IEnumerable<GradientStop>)gradientStops).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)gradientStops).GetEnumerator();
        }

        /// <summary>
        /// Creates a new <see cref="GradientStops"/> instance containing the specified gradient stops.
        /// </summary>
        /// <param name="gradientStops">The gradient stops that will be contained in the <see cref="GradientStops"/> object.</param>
        public GradientStops(IEnumerable<GradientStop> gradientStops)
        {
            List<GradientStop> stops = (from el in gradientStops orderby el.Offset ascending select el).ToList();

            if (stops.Count == 0)
            {
                stops.Add(new GradientStop(Colour.FromRgba(0, 0, 0, 0), 0));
            }

            if (stops[0].Offset > 0)
            {
                stops.Insert(0, new GradientStop(stops[0].Colour, 0));
            }

            if (stops[stops.Count - 1].Offset < 1)
            {
                stops.Add(new GradientStop(stops[stops.Count - 1].Colour, 1));
            }

            for (int i = 1; i < stops.Count - 1; i++)
            {
                bool closeToPrevious = (stops[i].Offset - stops[i - 1].Offset < StopTolerance);
                bool closeToNext = (stops[i + 1].Offset - stops[i].Offset < StopTolerance);
                
                if (closeToPrevious && !closeToNext)
                {
                    stops[i] = new GradientStop(stops[i].Colour, stops[i - 1].Offset + StopTolerance);
                }
                else if (!closeToPrevious && closeToNext)
                {
                    stops[i] = new GradientStop(stops[i].Colour, stops[i + 1].Offset - StopTolerance);
                }
                else if (closeToPrevious && closeToNext)
                {
                    stops.RemoveAt(i);
                    i--;
                }
            }

            this.gradientStops = ImmutableList.Create(stops.ToArray());
        }

        /// <summary>
        /// Creates a new <see cref="GradientStops"/> instance containing the specified gradient stops.
        /// </summary>
        /// <param name="gradientStops">The gradient stops that will be contained in the <see cref="GradientStops"/> object.</param>
        public GradientStops(params GradientStop[] gradientStops) : this((IEnumerable<GradientStop>)gradientStops)
        {

        }
    }

    /// <summary>
    /// Represents a brush painting with a gradient.
    /// </summary>
    public abstract class GradientBrush : Brush
    {
        /// <summary>
        /// The colour stops in the gradient.
        /// </summary>
        public GradientStops GradientStops { get; protected internal set; }

        internal GradientBrush() { }
    }

    /// <summary>
    /// Represents a brush painting with a linear gradient.
    /// </summary>
    public class LinearGradientBrush : GradientBrush
    {
        /// <summary>
        /// The starting point of the gradient. Note that this is relative to the current coordinate system when the gradient is used.
        /// </summary>
        public Point StartPoint { get; }

        /// <summary>
        /// The end point of the gradient. Note that this is relative to the current coordinate system when the gradient is used.
        /// </summary>
        public Point EndPoint { get; }

        /// <summary>
        /// Creates a new <see cref="LinearGradientBrush"/> with the specified start point, end point and gradient stops.
        /// </summary>
        /// <param name="startPoint">The starting point of the gradient. Note that this is relative to the current coordinate system when the gradient is used.</param>
        /// <param name="endPoint">The ending point of the gradient. Note that this is relative to the current coordinate system when the gradient is used.</param>
        /// <param name="gradientStops">The colour stops in the gradient.</param>
        public LinearGradientBrush(Point startPoint, Point endPoint, IEnumerable<GradientStop> gradientStops)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;

            this.GradientStops = new GradientStops(gradientStops);
        }

        /// <summary>
        /// Creates a new <see cref="LinearGradientBrush"/> with the specified start point, end point and gradient stops.
        /// </summary>
        /// <param name="startPoint">The starting point of the gradient. Note that this is relative to the current coordinate system when the gradient is used.</param>
        /// <param name="endPoint">The ending point of the gradient. Note that this is relative to the current coordinate system when the gradient is used.</param>
        /// <param name="gradientStops">The colour stops in the gradient.</param>
        public LinearGradientBrush(Point startPoint, Point endPoint, params GradientStop[] gradientStops)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            List<GradientStop> stops = (from el in gradientStops orderby el.Offset ascending select el).ToList();

            if (stops.Count == 0)
            {
                stops.Add(new GradientStop(Colour.FromRgba(0, 0, 0, 0), 0));
            }

            if (stops[0].Offset > 0)
            {
                stops.Insert(0, new GradientStop(stops[0].Colour, 0));
            }

            if (stops[stops.Count - 1].Offset < 1)
            {
                stops.Add(new GradientStop(stops[stops.Count - 1].Colour, 1));
            }

            this.GradientStops = new GradientStops(gradientStops);
        }

        /// <summary>
        /// Returns a <see cref="LinearGradientBrush"/> with the same gradient stops as the current instance, whose start and end point correspond to the points of the current instance in the
        /// original reference frame of the <paramref name="referenceGraphics"/>. This involves computing the current transform matrix of the <paramref name="referenceGraphics" />, inverting it,
        /// and applying the inverse matrix to the <see cref="StartPoint"/> and <see cref="EndPoint"/> of the current instance.
        /// </summary>
        /// <param name="referenceGraphics">The <see cref="Graphics"/> whose original reference frame is to be used.</param>
        /// <returns>A <see cref="LinearGradientBrush"/> with the same gradient stops as the current instance, whose start and end point correspond to the points of the current instance in the
        /// original reference frame of the <paramref name="referenceGraphics"/>.</returns>
        public LinearGradientBrush RelativeTo(Graphics referenceGraphics)
        {
            Stack<double[,]> transformMatrix = new Stack<double[,]>();
            double[,] currMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

            for (int i = 0; i < referenceGraphics.Actions.Count; i++)
            {
                if (referenceGraphics.Actions[i] is TransformAction)
                {
                    TransformAction trf = referenceGraphics.Actions[i] as TransformAction;

                    if (trf.Delta != null)
                    {
                        currMatrix = Graphics.Multiply(currMatrix, Graphics.TranslationMatrix(trf.Delta.Value.X, trf.Delta.Value.Y));
                    }
                    else if (trf.Angle != null)
                    {
                        currMatrix = Graphics.Multiply(currMatrix, Graphics.RotationMatrix(trf.Angle.Value));
                    }
                    else if (trf.Scale != null)
                    {
                        currMatrix = Graphics.Multiply(currMatrix, Graphics.ScaleMatrix(trf.Scale.Value.Width, trf.Scale.Value.Height));
                    }
                    else if (trf.Matrix != null)
                    {
                        currMatrix = Graphics.Multiply(currMatrix, trf.Matrix);
                    }
                }
                else if (referenceGraphics.Actions[i] is StateAction)
                {
                    if (((StateAction)referenceGraphics.Actions[i]).StateActionType == StateAction.StateActionTypes.Save)
                    {
                        transformMatrix.Push(currMatrix);
                    }
                    else
                    {
                        currMatrix = transformMatrix.Pop();
                    }
                }
            }

            currMatrix = Graphics.Invert(currMatrix);

            Point p1 = Graphics.Multiply(currMatrix, this.StartPoint);
            Point p2 = Graphics.Multiply(currMatrix, this.EndPoint);

            return new LinearGradientBrush(p1, p2, this.GradientStops);
        }

        /// <inheritdoc/>
        public override Brush MultiplyOpacity(double opacity)
        {
            return new LinearGradientBrush(this.StartPoint, this.EndPoint, from el in this.GradientStops select el.MultiplyOpacity(opacity));
        }
    }

    /// <summary>
    /// Represents a brush painting with a radial gradient.
    /// </summary>
    public class RadialGradientBrush : GradientBrush
    {
        /// <summary>
        /// The focal point of the gradient (i.e. the point within the circle where the gradient starts).
        /// </summary>
        public Point FocalPoint { get; }

        /// <summary>
        /// Represents the centre of the gradient.
        /// </summary>
        public Point Centre { get; }

        /// <summary>
        /// The radius of the gradient.
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// Creates a new <see cref="RadialGradientBrush"/> with the specified focal point, centre, radius and gradient stops.
        /// </summary>
        /// <param name="focalPoint">The focal point of the gradient. Note that this is relative to the current coordinate system when the gradient is used.</param>
        /// <param name="centre">The centre of the gradient. Note that this is relative to the current coordinate system when the gradient is used.</param>
        /// <param name="radius">The radius of the gradient. Note that this is relative to the current coordinate system when the gradient is used.</param>
        /// <param name="gradientStops">The colour stops in the gradient.</param>
        public RadialGradientBrush(Point focalPoint, Point centre, double radius, params GradientStop[] gradientStops)
        {
            if (new Point(focalPoint.X - centre.X, focalPoint.Y - centre.Y).Modulus() > radius)
            {
                Point norm = new Point(focalPoint.X - centre.X, focalPoint.Y - centre.Y).Normalize();
                focalPoint = new Point(centre.X + norm.X * radius, centre.Y + norm.Y * radius);
            }

            this.FocalPoint = focalPoint;
            this.Centre = centre;
            this.Radius = radius;

            List<GradientStop> stops = (from el in gradientStops orderby el.Offset ascending select el).ToList();

            if (stops.Count == 0)
            {
                stops.Add(new GradientStop(Colour.FromRgba(0, 0, 0, 0), 0));
            }

            if (stops[0].Offset > 0)
            {
                stops.Insert(0, new GradientStop(stops[0].Colour, 0));
            }

            if (stops[stops.Count - 1].Offset < 1)
            {
                stops.Add(new GradientStop(stops[stops.Count - 1].Colour, 1));
            }

            this.GradientStops = new GradientStops(gradientStops);
        }

        /// <summary>
        /// Creates a new <see cref="RadialGradientBrush"/> with the specified focal point, centre, radius and gradient stops.
        /// </summary>
        /// <param name="focalPoint">The focal point of the gradient. Note that this is relative to the current coordinate system when the gradient is used.</param>
        /// <param name="centre">The centre of the gradient. Note that this is relative to the current coordinate system when the gradient is used.</param>
        /// <param name="radius">The radius of the gradient. Note that this is relative to the current coordinate system when the gradient is used.</param>
        /// <param name="gradientStops">The colour stops in the gradient.</param>
        public RadialGradientBrush(Point focalPoint, Point centre, double radius, IEnumerable<GradientStop> gradientStops)
        {
            if (new Point(focalPoint.X - centre.X, focalPoint.Y - centre.Y).Modulus() > radius)
            {
                Point norm = new Point(focalPoint.X - centre.X, focalPoint.Y - centre.Y).Normalize();
                focalPoint = new Point(centre.X + norm.X * radius, centre.Y + norm.Y * radius);
            }

            this.FocalPoint = focalPoint;
            this.Centre = centre;
            this.Radius = radius;

            List<GradientStop> stops = (from el in gradientStops orderby el.Offset ascending select el).ToList();

            if (stops.Count == 0)
            {
                stops.Add(new GradientStop(Colour.FromRgba(0, 0, 0, 0), 0));
            }

            if (stops[0].Offset > 0)
            {
                stops.Insert(0, new GradientStop(stops[0].Colour, 0));
            }

            if (stops[stops.Count - 1].Offset < 1)
            {
                stops.Add(new GradientStop(stops[stops.Count - 1].Colour, 1));
            }

            this.GradientStops = new GradientStops(gradientStops);
        }

        /// <inheritdoc/>
        public override Brush MultiplyOpacity(double opacity)
        {
            return new RadialGradientBrush(this.FocalPoint, this.Centre, this.Radius, from el in this.GradientStops select el.MultiplyOpacity(opacity));
        }
    }
}
