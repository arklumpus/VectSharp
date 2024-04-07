/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2022 Giorgio Bianchini, University of Bristol

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
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using VectSharp.Filters;

namespace VectSharp
{
    internal interface IAnimation
    {
        IGraphicsAction StartValue { get; }
        IGraphicsAction EndValue { get; }
        IGraphicsAction Interpolate(double position, Dictionary<string, IEasing> easings);
    }

    internal abstract class IAnimation<T> : IAnimation where T : IGraphicsAction
    {
        public abstract T StartValue { get; }
        public abstract T EndValue { get; }

        IGraphicsAction IAnimation.StartValue => (IGraphicsAction)StartValue;

        IGraphicsAction IAnimation.EndValue => (IGraphicsAction)EndValue;

        public abstract T Interpolate(double position, Dictionary<string, IEasing> easings);

        IGraphicsAction IAnimation.Interpolate(double position, Dictionary<string, IEasing> easings)
        {
            return (IGraphicsAction)Interpolate(position, easings);
        }
    }

    internal class ConstantAnimation<T> : IAnimation<T> where T : IGraphicsAction
    {
        public override T StartValue { get; }

        public override T EndValue => StartValue;

        public ConstantAnimation(T action)
        {
            this.StartValue = action;
        }

        public override T Interpolate(double position, Dictionary<string, IEasing> easings)
        {
            return this.StartValue;
        }
    }

    internal class StateAnimation : IAnimation<StateAction>
    {
        public override StateAction StartValue { get; }

        public override StateAction EndValue { get; }

        public StateAnimation(StateAction startValue, StateAction endValue)
        {
            this.StartValue = startValue;
            this.EndValue = endValue;
        }

        public override StateAction Interpolate(double position, Dictionary<string, IEasing> easings)
        {
            if (easings != null && !string.IsNullOrEmpty(StartValue.Tag) && easings.TryGetValue(StartValue.Tag, out IEasing easing))
            {
                position = easing.Ease(position);
            }

            if (position < 0.5)
            {
                return StartValue;
            }
            else
            {
                return EndValue;
            }
        }
    }

    internal static class InterpolationUtils
    {
        public static Point InterpolatePoint(Point start, Point end, double position)
        {
            return new Point(start.X * (1 - position) + end.X * position, start.Y * (1 - position) + end.Y * position);
        }

        public static Point InterpolatePoint(Point? start, Point? end, double position)
        {
            return InterpolatePoint(start ?? new Point(), end ?? new Point(), position);
        }

        public static Size InterpolateSize(Size start, Size end, double position)
        {
            return new Size(start.Width * (1 - position) + end.Width * position, start.Height * (1 - position) + end.Height * position);
        }

        public static Size InterpolateSize(Size? start, Size? end, double position)
        {
            return InterpolateSize(start ?? new Size(), end ?? new Size(), position);
        }

        public static double InterpolateDouble(double start, double end, double position)
        {
            return start * (1 - position) + end * position;
        }

        public static double InterpolateDouble(double? start, double? end, double position)
        {
            return start * (1 - position) + end * position ?? 0;
        }

        public static Colour InterpolateColour(Colour start, Colour end, double position)
        {
            return Colour.FromRgba(InterpolateDouble(start.R, end.R, position),
                    InterpolateDouble(start.G, end.G, position),
                    InterpolateDouble(start.B, end.B, position),
                    InterpolateDouble(start.A, end.A, position));
        }

        private static HashSet<double> GetAnchors(Brush start, Brush end)
        {
            HashSet<double> anchors = new HashSet<double>();

            if (start is GradientBrush gradientStart)
            {
                for (int i = 0; i < gradientStart.GradientStops.Count; i++)
                {
                    anchors.Add(gradientStart.GradientStops[i].Offset);
                }
            }

            if (end is GradientBrush gradientEnd)
            {
                for (int i = 0; i < gradientEnd.GradientStops.Count; i++)
                {
                    anchors.Add(gradientEnd.GradientStops[i].Offset);
                }
            }

            return anchors;
        }

        private static void AddGradientStop(List<GradientStop> stops)
        {
            int minIndex = -1;
            double maxInterval = double.MinValue;

            for (int i = 1; i < stops.Count; i++)
            {
                double interval = stops[i].Offset - stops[i - 1].Offset;

                if (interval > maxInterval)
                {
                    maxInterval = interval;
                    minIndex = i;
                }
            }

            if (minIndex > 0)
            {
                stops.Insert(minIndex, new GradientStop(Colour.FromRgba((stops[minIndex - 1].Colour.R + stops[minIndex].Colour.R) * 0.5, (stops[minIndex - 1].Colour.G + stops[minIndex].Colour.G) * 0.5, (stops[minIndex - 1].Colour.B + stops[minIndex].Colour.B) * 0.5, (stops[minIndex - 1].Colour.A + stops[minIndex].Colour.A) * 0.5), stops[minIndex - 1].Offset + maxInterval * 0.5));
            }
        }

        public static Brush InterpolateBrush(Brush start, Brush end, double position)
        {
            if (position <= 0)
            {
                return start;
            }
            else if (position >= 1)
            {
                return end;
            }
            else
            {
                HashSet<double> anchors = GetAnchors(start, end);

                if (start is SolidColourBrush solidStart && end is SolidColourBrush solidEnd)
                {
                    return new SolidColourBrush(InterpolateColour(solidStart.Colour, solidEnd.Colour, position));
                }
                else if (start is LinearGradientBrush linearStart && end is LinearGradientBrush linearEnd)
                {
                    List<GradientStop> startStops = linearStart.GradientStops.ToList();
                    List<GradientStop> endStops = linearEnd.GradientStops.ToList();

                    while (startStops.Count < endStops.Count)
                    {
                        AddGradientStop(startStops);
                    }

                    while (startStops.Count > endStops.Count)
                    {
                        AddGradientStop(endStops);
                    }

                    List<GradientStop> interpolatedStops = new List<GradientStop>();

                    for (int i = 0; i < startStops.Count; i++)
                    {
                        interpolatedStops.Add(new GradientStop(InterpolateColour(startStops[i].Colour, endStops[i].Colour, position), InterpolateDouble(startStops[i].Offset, endStops[i].Offset, position)));
                    }

                    return new LinearGradientBrush(InterpolatePoint(linearStart.StartPoint, linearEnd.StartPoint, position), InterpolatePoint(linearStart.EndPoint, linearEnd.EndPoint, position), interpolatedStops);
                }
                else if (start is SolidColourBrush solidStart2 && end is LinearGradientBrush linearEnd2)
                {
                    List<GradientStop> gradientStops = new List<GradientStop>(anchors.Count);

                    foreach (double anchor in anchors)
                    {
                        Colour startColour = solidStart2.Colour;
                        Colour endColour = linearEnd2.GradientStops.GetColourAt(anchor);

                        gradientStops.Add(new GradientStop(InterpolateColour(startColour, endColour, position), anchor));
                    }

                    return new LinearGradientBrush(linearEnd2.StartPoint, linearEnd2.EndPoint, gradientStops);
                }
                else if (start is LinearGradientBrush linearStart2 && end is SolidColourBrush solidEnd2)
                {
                    List<GradientStop> gradientStops = new List<GradientStop>(anchors.Count);

                    foreach (double anchor in anchors)
                    {
                        Colour startColour = linearStart2.GradientStops.GetColourAt(anchor);
                        Colour endColour = solidEnd2.Colour;

                        gradientStops.Add(new GradientStop(InterpolateColour(startColour, endColour, position), anchor));
                    }

                    return new LinearGradientBrush(linearStart2.StartPoint, linearStart2.EndPoint, gradientStops);
                }
                else if (start is RadialGradientBrush radialStart && end is RadialGradientBrush radialEnd)
                {
                    List<GradientStop> startStops = radialStart.GradientStops.ToList();
                    List<GradientStop> endStops = radialEnd.GradientStops.ToList();

                    while (startStops.Count < endStops.Count)
                    {
                        AddGradientStop(startStops);
                    }

                    while (startStops.Count > endStops.Count)
                    {
                        AddGradientStop(endStops);
                    }

                    List<GradientStop> interpolatedStops = new List<GradientStop>();

                    for (int i = 0; i < startStops.Count; i++)
                    {
                        interpolatedStops.Add(new GradientStop(InterpolateColour(startStops[i].Colour, endStops[i].Colour, position), InterpolateDouble(startStops[i].Offset, endStops[i].Offset, position)));
                    }

                    return new RadialGradientBrush(InterpolatePoint(radialStart.FocalPoint, radialEnd.FocalPoint, position), InterpolatePoint(radialStart.Centre, radialEnd.Centre, position), InterpolateDouble(radialStart.Radius, radialEnd.Radius, position), interpolatedStops);
                }
                else if (start is SolidColourBrush solidStart3 && end is RadialGradientBrush radialEnd2)
                {
                    List<GradientStop> gradientStops = new List<GradientStop>(anchors.Count);

                    foreach (double anchor in anchors)
                    {
                        Colour startColour = solidStart3.Colour;
                        Colour endColour = radialEnd2.GradientStops.GetColourAt(anchor);

                        gradientStops.Add(new GradientStop(InterpolateColour(startColour, endColour, position), anchor));
                    }

                    return new RadialGradientBrush(radialEnd2.FocalPoint, radialEnd2.Centre, radialEnd2.Radius, gradientStops);
                }
                else if (start is RadialGradientBrush radialStart2 && end is SolidColourBrush solidEnd3)
                {
                    List<GradientStop> gradientStops = new List<GradientStop>(anchors.Count);

                    foreach (double anchor in anchors)
                    {
                        Colour startColour = radialStart2.GradientStops.GetColourAt(anchor);
                        Colour endColour = solidEnd3.Colour;

                        gradientStops.Add(new GradientStop(InterpolateColour(startColour, endColour, position), anchor));
                    }

                    return new RadialGradientBrush(radialStart2.FocalPoint, radialStart2.Centre, radialStart2.Radius, gradientStops);
                }
                else if (start is LinearGradientBrush linearStart3 && end is RadialGradientBrush radialEnd3)
                {
                    Point linearVector = new Point(linearStart3.EndPoint.X - linearStart3.StartPoint.X, linearStart3.EndPoint.Y - linearStart3.StartPoint.Y);

                    double modulus = linearVector.Modulus();
                    linearVector = new Point(linearVector.X / modulus, linearVector.Y / modulus);

                    double radius = radialEnd3.Radius / position;// radialEnd3.Radius * (1 + 100 * (1 - position));

                    Point target = new Point(linearStart3.EndPoint.X - linearVector.X * radius, linearStart3.EndPoint.Y - linearVector.Y * radius);

                    List<GradientStop> gradientStops = new List<GradientStop>(anchors.Count);

                    double lOverR = (modulus + (radialEnd3.Radius - modulus) * position) / radialEnd3.Radius;

                    foreach (double anchor in anchors)
                    {
                        double offset = (1 - lOverR * position) + anchor * position * lOverR;

                        Colour startColour = linearStart3.GradientStops.GetColourAt(anchor);
                        Colour endColour = radialEnd3.GradientStops.GetColourAt(anchor);
                        gradientStops.Add(new GradientStop(InterpolateColour(startColour, endColour, position), offset));
                    }

                    return new RadialGradientBrush(InterpolatePoint(target, radialEnd3.FocalPoint, position), InterpolatePoint(target, radialEnd3.Centre, position), InterpolateDouble(radius, radialEnd3.Radius, position), gradientStops);
                }
                else if (start is RadialGradientBrush && end is LinearGradientBrush)
                {
                    return InterpolateBrush(end, start, 1 - position);
                }
                else
                {
                    if (position < 0.5)
                    {
                        return start;
                    }
                    else
                    {
                        return end;
                    }
                }
            }
        }

        public static int InterpolateInt(int start, int end, double position)
        {
            return (int)Math.Round(start * (1 - position) + end * position);
        }

        public static Font InterpolateFont(Font start, Font end, double position)
        {
            Font tbr = new Font(position < 0.5 ? start.FontFamily : end.FontFamily, InterpolateDouble(start.FontSize, end.FontSize, position), start.Underline != null || end.Underline != null);

            if (start.Underline != null && end.Underline == null)
            {
                tbr.Underline.FollowItalicAngle = start.Underline.FollowItalicAngle;
                tbr.Underline.LineCap = start.Underline.LineCap;
                tbr.Underline.Position = start.Underline.Position;
                tbr.Underline.SkipDescenders = start.Underline.SkipDescenders;
                tbr.Underline.Thickness = InterpolateDouble(start.Underline.Thickness, 0, position);
            }
            else if (start.Underline == null && end.Underline != null)
            {
                tbr.Underline.FollowItalicAngle = end.Underline.FollowItalicAngle;
                tbr.Underline.LineCap = end.Underline.LineCap;
                tbr.Underline.Position = end.Underline.Position;
                tbr.Underline.SkipDescenders = end.Underline.SkipDescenders;
                tbr.Underline.Thickness = InterpolateDouble(0, end.Underline.Thickness, position);
            }
            else if (start.Underline != null && end.Underline != null)
            {
                tbr.Underline.FollowItalicAngle = position < 0.5 ? start.Underline.FollowItalicAngle : end.Underline.FollowItalicAngle;
                tbr.Underline.LineCap = position < 0.5 ? start.Underline.LineCap : end.Underline.LineCap;
                tbr.Underline.Position = InterpolateDouble(start.Underline.Position, end.Underline.Position, position);
                tbr.Underline.SkipDescenders = position < 0.5 ? start.Underline.SkipDescenders : end.Underline.SkipDescenders;
                tbr.Underline.Thickness = InterpolateDouble(start.Underline.Thickness, 0, position);
            }

            return tbr;
        }
    }

    internal class TransformAnimation : IAnimation<TransformAction>
    {
        public override TransformAction StartValue { get; }

        public override TransformAction EndValue { get; }

        public TransformAnimation(TransformAction startValue, TransformAction endValue)
        {
            this.StartValue = startValue;
            this.EndValue = endValue;
        }
        public override TransformAction Interpolate(double position, Dictionary<string, IEasing> easings)
        {
            if (easings != null && !string.IsNullOrEmpty(StartValue.Tag) && easings.TryGetValue(StartValue.Tag, out IEasing easing))
            {
                position = easing.Ease(position);
            }

            if (position <= 0)
            {
                return this.StartValue;
            }
            else if (position >= 1)
            {
                return this.EndValue;
            }
            else
            {
                if (this.StartValue.Delta != null && this.EndValue.Delta != null)
                {
                    return new TransformAction(InterpolationUtils.InterpolatePoint(this.StartValue.Delta, this.EndValue.Delta, position), position < 0.5 ? this.StartValue.Tag : this.EndValue.Tag);
                }
                else if (this.StartValue.Scale != null && this.EndValue.Scale != null)
                {
                    return new TransformAction(InterpolationUtils.InterpolateSize(this.StartValue.Scale, this.EndValue.Scale, position), position < 0.5 ? this.StartValue.Tag : this.EndValue.Tag);
                }
                else if (this.StartValue.Angle != null && this.EndValue.Angle != null)
                {
                    return new TransformAction(InterpolationUtils.InterpolateDouble(this.StartValue.Angle, this.EndValue.Angle, position), position < 0.5 ? this.StartValue.Tag : this.EndValue.Tag);
                }
                else
                {
                    double[,] startMatrix = this.StartValue.GetMatrix();
                    double[,] endMatrix = this.EndValue.GetMatrix();

                    double[,] tbrMatrix = new double[startMatrix.GetLength(0), startMatrix.GetLength(1)];

                    for (int i = 0; i < startMatrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < startMatrix.GetLength(1); j++)
                        {
                            tbrMatrix[i, j] = startMatrix[i, j] * (1 - position) + endMatrix[i, j] * position;
                        }
                    }

                    return new TransformAction(tbrMatrix, position < 0.5 ? this.StartValue.Tag : this.EndValue.Tag);
                }
            }
        }
    }

    internal abstract class PrintableActionAnimation<T> : IAnimation<T> where T : IPrintableAction, IGraphicsAction
    {
        public override T StartValue { get; }
        public override T EndValue { get; }

        protected abstract T InterpolateConcrete((double, LineJoins, LineCaps, LineDash, Brush, Brush) commonElements, double position, Dictionary<string, IEasing> easings);

        public PrintableActionAnimation(T startValue, T endValue)
        {
            this.StartValue = startValue;
            this.EndValue = endValue;
        }

        public override T Interpolate(double position, Dictionary<string, IEasing> easings)
        {
            string tag = (StartValue as IPrintableAction)?.Tag ?? (StartValue as IGraphicsAction)?.Tag;

            if (easings != null && !string.IsNullOrEmpty(tag) && easings.TryGetValue(tag, out IEasing easing))
            {
                position = easing.Ease(position);
            }

            if (position <= 0)
            {
                return StartValue;
            }
            else if (position >= 1)
            {
                return EndValue;
            }
            else
            {
                T tbr = InterpolateConcrete(InterpolateCommon(position), position, easings);
                ((IGraphicsAction)tbr).Tag = ((IGraphicsAction)this.StartValue).Tag;
                return tbr;
            }
        }

        protected (double, LineJoins, LineCaps, LineDash, Brush, Brush) InterpolateCommon(double position)
        {
            double lineWidth = InterpolationUtils.InterpolateDouble(this.StartValue.LineWidth, this.EndValue.LineWidth, position);
            LineJoins lineJoin = position < 0.5 ? this.StartValue.LineJoin : this.EndValue.LineJoin;
            LineCaps lineCap = position < 0.5 ? this.StartValue.LineCap : this.EndValue.LineCap;
            LineDash lineDash = new LineDash(InterpolationUtils.InterpolateDouble(this.StartValue.LineDash.UnitsOn, this.EndValue.LineDash.UnitsOn, position),
                InterpolationUtils.InterpolateDouble(this.StartValue.LineDash.UnitsOff, this.EndValue.LineDash.UnitsOff, position),
                InterpolationUtils.InterpolateDouble(this.StartValue.LineDash.Phase, this.EndValue.LineDash.Phase, position));

            Brush stroke = null;

            if (this.StartValue.Stroke != null && this.EndValue.Stroke != null)
            {
                stroke = InterpolationUtils.InterpolateBrush(this.StartValue.Stroke, this.EndValue.Stroke, position);
            }

            Brush fill = null;

            if (this.StartValue.Fill != null && this.EndValue.Fill != null)
            {
                fill = InterpolationUtils.InterpolateBrush(this.StartValue.Fill, this.EndValue.Fill, position);
            }

            return (lineWidth, lineJoin, lineCap, lineDash, stroke, fill);
        }
    }

    internal class RectangleActionAnimation : PrintableActionAnimation<RectangleAction>
    {
        public RectangleActionAnimation(RectangleAction startValue, RectangleAction endValue) : base(startValue, endValue) { }

        protected override RectangleAction InterpolateConcrete((double, LineJoins, LineCaps, LineDash, Brush, Brush) commonElements, double position, Dictionary<string, IEasing> easings)
        {
            (double lineWidth, LineJoins lineJoin, LineCaps lineCap, LineDash lineDash, Brush stroke, Brush fill) = commonElements;

            return new RectangleAction(InterpolationUtils.InterpolatePoint(this.StartValue.TopLeft, this.EndValue.TopLeft, position),
                          InterpolationUtils.InterpolateSize(this.StartValue.Size, this.EndValue.Size, position),
                          fill, stroke, lineWidth, lineCap, lineJoin, lineDash, position < 0.5 ? this.StartValue.Tag : this.EndValue.Tag);
        }
    }

    internal class RasterImageActionAnimation : PrintableActionAnimation<RasterImageAction>
    {
        public RasterImageActionAnimation(RasterImageAction startValue, RasterImageAction endValue) : base(startValue, endValue) { }

        protected override RasterImageAction InterpolateConcrete((double, LineJoins, LineCaps, LineDash, Brush, Brush) commonElements, double position, Dictionary<string, IEasing> easings)
        {
            return new RasterImageAction(InterpolationUtils.InterpolateInt(this.StartValue.SourceX, this.EndValue.SourceX, position),
                InterpolationUtils.InterpolateInt(this.StartValue.SourceY, this.EndValue.SourceY, position),
                InterpolationUtils.InterpolateInt(this.StartValue.SourceWidth, this.EndValue.SourceWidth, position),
                InterpolationUtils.InterpolateInt(this.StartValue.SourceHeight, this.EndValue.SourceHeight, position),
                InterpolationUtils.InterpolateDouble(this.StartValue.DestinationX, this.EndValue.DestinationX, position),
                InterpolationUtils.InterpolateDouble(this.StartValue.DestinationY, this.EndValue.DestinationY, position),
                InterpolationUtils.InterpolateDouble(this.StartValue.DestinationWidth, this.EndValue.DestinationWidth, position),
                InterpolationUtils.InterpolateDouble(this.StartValue.DestinationHeight, this.EndValue.DestinationHeight, position), position < 0.5 ? this.StartValue.Image : this.EndValue.Image
                , position < 0.5 ? this.StartValue.Tag : this.EndValue.Tag);
        }
    }

    internal class TextActionAnimation : PrintableActionAnimation<TextAction>
    {
        public TextActionAnimation(TextAction startValue, TextAction endValue) : base(startValue, endValue) { }

        protected override TextAction InterpolateConcrete((double, LineJoins, LineCaps, LineDash, Brush, Brush) commonElements, double position, Dictionary<string, IEasing> easings)
        {
            (double lineWidth, LineJoins lineJoin, LineCaps lineCap, LineDash lineDash, Brush stroke, Brush fill) = commonElements;

            return new TextAction(InterpolationUtils.InterpolatePoint(this.StartValue.Origin, this.EndValue.Origin, position),
                position < 0.5 ? this.StartValue.Text : this.EndValue.Text,
                InterpolationUtils.InterpolateFont(this.StartValue.Font, this.EndValue.Font, position),
                position < 0.5 ? this.StartValue.TextBaseline : this.EndValue.TextBaseline,
                fill, stroke, lineWidth, lineCap, lineJoin, lineDash, position < 0.5 ? this.StartValue.Tag : this.EndValue.Tag);
        }
    }

    internal class PathActionAnimation : PrintableActionAnimation<PathAction>
    {
        public override PathAction StartValue { get; }
        public override PathAction EndValue { get; }

        private bool AreTheyTheSame(GraphicsPath pth1, GraphicsPath pth2)
        {
            if (pth1.Segments.Count != pth2.Segments.Count)
            {
                return false;
            }

            for (int i = 0; i < pth1.Segments.Count; i++)
            {
                if (pth1.Segments[i].Type != pth2.Segments[i].Type)
                {
                    return false;
                }
            }

            return true;
        }

        private Point GetCenter(GraphicsPath path)
        {
            List<Point> points = path.GetPoints().ToList().Aggregate(new List<Point>(), (a, b) => { a.AddRange(b); return a; });

            double cX = 0;
            double cY = 0;

            for (int i = 0; i < points.Count; i++)
            {
                cX += points[i].X;
                cY += points[i].Y;
            }

            return new Point(cX / points.Count, cY / points.Count);
        }

        private double ComparePaths(GraphicsPath path1, GraphicsPath path2)
        {
            Point c1 = GetCenter(path1);
            Point c2 = GetCenter(path2);

            return (c1.X - c2.X) * (c1.X - c2.X) + (c1.Y - c2.Y) * (c1.Y - c2.Y);
        }

        private List<(int, int)> GetBestAssignment(List<GraphicsPath> figures1, List<GraphicsPath> figures2)
        {
            List<(int, int)> bestAssignment = new List<(int, int)>();

            double[,] distMat = new double[figures1.Count, figures2.Count];

            for (int i = 0; i < figures1.Count; i++)
            {
                for (int j = 0; j < figures2.Count; j++)
                {
                    distMat[i, j] = ComparePaths(figures1[i], figures2[j]);
                }
            }

            List<int> missingIndices1 = Enumerable.Range(0, figures1.Count).ToList();
            List<int> missingIndices2 = Enumerable.Range(0, figures2.Count).ToList();

            if (figures1.Count > figures2.Count)
            {
                while (missingIndices2.Count > 0)
                {
                    double minValue = double.MaxValue;
                    (int, int) bestPair = (-1, -1);

                    for (int i = 0; i < missingIndices1.Count; i++)
                    {
                        for (int j = 0; j < missingIndices2.Count; j++)
                        {
                            if (distMat[missingIndices1[i], missingIndices2[j]] < minValue)
                            {
                                minValue = distMat[missingIndices1[i], missingIndices2[j]];
                                bestPair = (missingIndices1[i], missingIndices2[j]);
                            }
                        }
                    }

                    bestAssignment.Add(bestPair);
                    missingIndices1.Remove(bestPair.Item1);
                    missingIndices2.Remove(bestPair.Item2);
                }
            }
            else
            {
                while (missingIndices1.Count > 0)
                {
                    double minValue = double.MaxValue;
                    (int, int) bestPair = (-1, -1);

                    for (int i = 0; i < missingIndices1.Count; i++)
                    {
                        for (int j = 0; j < missingIndices2.Count; j++)
                        {
                            if (distMat[missingIndices1[i], missingIndices2[j]] < minValue)
                            {
                                minValue = distMat[missingIndices1[i], missingIndices2[j]];
                                bestPair = (missingIndices1[i], missingIndices2[j]);
                            }
                        }
                    }

                    bestAssignment.Add(bestPair);
                    missingIndices1.Remove(bestPair.Item1);
                    missingIndices2.Remove(bestPair.Item2);
                }
            }

            return bestAssignment;
        }

        private void IncreasePoints(List<Point> points, bool isClosed)
        {
            if (points.Count > 1)
            {
                double maxLength = double.MinValue;
                int maxIndex = -1;

                double target = isClosed ? points.Count : points.Count - 1;

                for (int i = 0; i < target; i++)
                {
                    double length = (points[i].X - points[(i + 1) % points.Count].X) * (points[i].X - points[(i + 1) % points.Count].X) + (points[i].Y - points[(i + 1) % points.Count].Y) * (points[i].Y - points[(i + 1) % points.Count].Y);

                    if (length > maxLength)
                    {
                        maxLength = length;
                        maxIndex = i;
                    }
                }

                points.Insert(maxIndex + 1, new Point((points[maxIndex].X + points[(maxIndex + 1) % points.Count].X) * 0.5, (points[maxIndex].Y + points[(maxIndex + 1) % points.Count].Y) * 0.5));
            }
            else
            {
                points.Add(points[0]);
            }

        }

        public PathActionAnimation(PathAction startValue, PathAction endValue, double linearisationResolution) : base(startValue, endValue)
        {
            GraphicsPath startPath = startValue.Path.ConvertArcsToBeziers();
            GraphicsPath endPath = endValue.Path.ConvertArcsToBeziers();

            if (AreTheyTheSame(startPath, endPath))
            {
                this.StartValue = new PathAction(startPath, startValue.Fill, startValue.Stroke, startValue.LineWidth, startValue.LineCap, startValue.LineJoin, startValue.LineDash, startValue.Tag, startValue.FillRule, startValue.IsClipping);
                this.EndValue = new PathAction(endPath, endValue.Fill, endValue.Stroke, endValue.LineWidth, endValue.LineCap, endValue.LineJoin, endValue.LineDash, endValue.Tag, endValue.FillRule, endValue.IsClipping);
            }
            else
            {
                List<GraphicsPath> startFigures = startPath.GetFigures().ToList();
                List<GraphicsPath> endFigures = endPath.GetFigures().ToList();

                List<(int, int)> correspondences = GetBestAssignment(startFigures, endFigures);

                HashSet<int> startFiguresIndices = new HashSet<int>(Enumerable.Range(0, startFigures.Count));
                HashSet<int> endFiguresIndices = new HashSet<int>(Enumerable.Range(0, endFigures.Count));

                for (int i = 0; i < correspondences.Count; i++)
                {
                    startFiguresIndices.Remove(correspondences[i].Item1);
                    endFiguresIndices.Remove(correspondences[i].Item2);
                }

                foreach (int i in startFiguresIndices)
                {
                    Point center = GetCenter(startFigures[i]);

                    GraphicsPath path = new GraphicsPath().MoveTo(center).LineTo(center);
                    if (startFigures[i].Segments[startFigures[i].Segments.Count - 1].Type == SegmentType.Close)
                    {
                        path.Close();
                    }

                    endFigures.Add(path);
                    correspondences.Add((i, endFigures.Count - 1));
                }


                foreach (int i in endFiguresIndices)
                {
                    Point center = GetCenter(endFigures[i]);

                    GraphicsPath path = new GraphicsPath().MoveTo(center).LineTo(center);
                    if (endFigures[i].Segments[endFigures[i].Segments.Count - 1].Type == SegmentType.Close)
                    {
                        path.Close();
                    }

                    startFigures.Add(path);
                    correspondences.Add((startFigures.Count - 1, i));
                }


                startPath = new GraphicsPath();
                endPath = new GraphicsPath();

                for (int i = 0; i < startFigures.Count; i++)
                {
                    double startLength = startFigures[correspondences[i].Item1].MeasureLength();
                    double endLength = endFigures[correspondences[i].Item2].MeasureLength();

                    double maxLength = Math.Max(startLength, endLength);
                    int numPoints = (int)Math.Ceiling(maxLength / linearisationResolution);

                    double startResolution = startLength / maxLength * linearisationResolution;
                    double endResolution = endLength / maxLength * linearisationResolution;

                    if (startResolution <= 0)
                    {
                        startResolution = 1;
                    }

                    if (endResolution <= 0)
                    {
                        endResolution = 1;
                    }

                    List<Point> startFigure = startFigures[correspondences[i].Item1].Linearise(startResolution).GetPoints().First();
                    bool startIsClosed = startFigures[correspondences[i].Item1].Segments[startFigures[correspondences[i].Item1].Segments.Count - 1].Type == SegmentType.Close;

                    List<Point> endFigure = endFigures[correspondences[i].Item2].Linearise(endResolution).GetPoints().First();
                    bool endIsClosed = endFigures[correspondences[i].Item2].Segments[endFigures[correspondences[i].Item2].Segments.Count - 1].Type == SegmentType.Close;

                    while (startFigure.Count < numPoints)
                    {
                        IncreasePoints(startFigure, startIsClosed);
                    }

                    while (endFigure.Count < numPoints)
                    {
                        IncreasePoints(endFigure, endIsClosed);
                    }


                    double minVariance = double.MaxValue;
                    int bestShift = -1;

                    for (int j = 0; j < numPoints; j++)
                    {
                        double average = 0;
                        //double averageSq = 0;

                        for (int k = 0; k < numPoints; k++)
                        {
                            double dist = (startFigure[k].X - endFigure[(k + j) % numPoints].X) * (startFigure[k].X - endFigure[(k + j) % numPoints].X) + (startFigure[k].Y - endFigure[(k + j) % numPoints].Y) * (startFigure[k].Y - endFigure[(k + j) % numPoints].Y);
                            average += dist;
                            //averageSq += dist * dist;
                        }

                        //double variance = averageSq / numPoints - (average / numPoints) * (average / numPoints);

                        if (average < minVariance)
                        {
                            minVariance = average;
                            bestShift = j;
                        }
                    }

                    startPath.MoveTo(startFigure[0]);
                    endPath.MoveTo(endFigure[bestShift % numPoints]);

                    for (int j = 0; j < numPoints; j++)
                    {
                        startPath.LineTo(startFigure[j]);
                        endPath.LineTo(endFigure[(j + bestShift) % numPoints]);
                    }

                    if (startIsClosed && endIsClosed)
                    {
                        startPath.Close();
                        endPath.Close();
                    }
                    else if (startIsClosed && !endIsClosed)
                    {
                        startPath.LineTo(startFigure[0]);
                        endPath.LineTo(endFigure[endFigure.Count - 1]);
                    }
                    else if (!startIsClosed && endIsClosed)
                    {
                        startPath.LineTo(startFigure[startFigure.Count - 1]);
                        endPath.LineTo(endFigure[0]);
                    }
                }

                this.StartValue = new PathAction(startPath, startValue.Fill, startValue.Stroke, startValue.LineWidth, startValue.LineCap, startValue.LineJoin, startValue.LineDash, startValue.Tag, startValue.FillRule, startValue.IsClipping);
                this.EndValue = new PathAction(endPath, endValue.Fill, endValue.Stroke, endValue.LineWidth, endValue.LineCap, endValue.LineJoin, endValue.LineDash, endValue.Tag, endValue.FillRule, endValue.IsClipping);

            }
        }

        private static Segment InterpolateSegment(Segment start, Segment end, double position)
        {
            if (start.Type == end.Type)
            {
                if (start.Type == SegmentType.Arc)
                {
                    ArcSegment startA = start as ArcSegment;
                    ArcSegment endA = end as ArcSegment;

                    return new ArcSegment(InterpolationUtils.InterpolatePoint(startA.Points[0], endA.Points[0], position), InterpolationUtils.InterpolateDouble(startA.Radius, endA.Radius, position), InterpolationUtils.InterpolateDouble(startA.StartAngle, endA.StartAngle, position), InterpolationUtils.InterpolateDouble(startA.EndAngle, endA.EndAngle, position));
                }
                else if (start.Type == SegmentType.Move)
                {
                    return new MoveSegment(InterpolationUtils.InterpolatePoint(start.Point, end.Point, position));
                }
                else if (start.Type == SegmentType.Line)
                {
                    return new LineSegment(InterpolationUtils.InterpolatePoint(start.Point, end.Point, position));
                }
                else if (start.Type == SegmentType.Close)
                {
                    return new CloseSegment();
                }
                else if (start.Type == SegmentType.CubicBezier)
                {
                    CubicBezierSegment startC = start as CubicBezierSegment;
                    CubicBezierSegment endC = end as CubicBezierSegment;

                    return new CubicBezierSegment(InterpolationUtils.InterpolatePoint(startC.Points[0], endC.Points[0], position),
                        InterpolationUtils.InterpolatePoint(startC.Points[1], endC.Points[1], position),
                        InterpolationUtils.InterpolatePoint(startC.Points[2], endC.Points[2], position));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                return position < 0.5 ? start : end;
            }
        }

        protected override PathAction InterpolateConcrete((double, LineJoins, LineCaps, LineDash, Brush, Brush) commonElements, double position, Dictionary<string, IEasing> easings)
        {
            (double lineWidth, LineJoins lineJoin, LineCaps lineCap, LineDash lineDash, Brush stroke, Brush fill) = commonElements;

            GraphicsPath path = new GraphicsPath();

            for (int i = 0; i < this.StartValue.Path.Segments.Count; i++)
            {
                path.Segments.Add(InterpolateSegment(this.StartValue.Path.Segments[i], this.EndValue.Path.Segments[i], position));
            }

            return new PathAction(path, fill, stroke, lineWidth, lineCap, lineJoin, lineDash, position < 0.5 ? this.StartValue.Tag : this.EndValue.Tag, position < 0.5 ? this.StartValue.FillRule : this.EndValue.FillRule, position < 0.5 ? this.StartValue.IsClipping : this.EndValue.IsClipping);
        }
    }

    internal class FilteredGraphicsAnimation : PrintableActionAnimation<FilteredGraphicsAction>
    {
        private IGraphicsAnimation GraphicsAnimation { get; }
        private IGraphicsAnimation MaskAnimation { get; }

        public FilteredGraphicsAnimation(FilteredGraphicsAction startValue, FilteredGraphicsAction endValue, double linearisationResolution) : base(startValue, endValue)
        {
            if (startValue.Content == endValue.Content)
            {
                this.GraphicsAnimation = new ConstantGraphicsAnimation(startValue.Content);
            }
            else
            {
                this.GraphicsAnimation = new GraphicsAnimation(startValue.Content, endValue.Content, linearisationResolution);
            }

            if (startValue.Filter is MaskFilter startM && endValue.Filter is MaskFilter endM)
            {
                if (startM.Mask == endM.Mask)
                {
                    this.MaskAnimation = new ConstantGraphicsAnimation(startM.Mask);
                }
                else
                {
                    this.MaskAnimation = new GraphicsAnimation(startM.Mask, endM.Mask, linearisationResolution);
                }
            }
        }

        protected override FilteredGraphicsAction InterpolateConcrete((double, LineJoins, LineCaps, LineDash, Brush, Brush) commonElements, double position, Dictionary<string, IEasing> easings)
        {
            Graphics gpr = this.GraphicsAnimation.Interpolate(position, easings);

            if (this.StartValue.Filter.GetType() == this.EndValue.Filter.GetType())
            {
                if (this.StartValue.Filter is BoxBlurFilter startBBF && this.EndValue.Filter is BoxBlurFilter endBBF)
                {
                    BoxBlurFilter filter = new BoxBlurFilter(InterpolationUtils.InterpolateDouble(startBBF.BoxRadius, endBBF.BoxRadius, position));

                    return new FilteredGraphicsAction(gpr, filter);
                }
                else if (this.StartValue.Filter is ColourMatrixFilter startCMF && this.EndValue.Filter is ColourMatrixFilter endCMF)
                {
                    double[,] colourMatrix = new double[5, 5];

                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            colourMatrix[i, j] = InterpolationUtils.InterpolateDouble(startCMF.ColourMatrix[i, j], endCMF.ColourMatrix[i, j], position);
                        }
                    }

                    ColourMatrixFilter filter = new ColourMatrixFilter(new ColourMatrix(colourMatrix));

                    return new FilteredGraphicsAction(gpr, filter);
                }
                else if (this.StartValue.Filter is ConvolutionFilter startConvo && this.EndValue.Filter is ConvolutionFilter endConvo)
                {
                    if (startConvo.Kernel.GetLength(0) == endConvo.Kernel.GetLength(0) && startConvo.Kernel.GetLength(1) == endConvo.Kernel.GetLength(1))
                    {
                        double[,] kernel = new double[startConvo.Kernel.GetLength(0), startConvo.Kernel.GetLength(1)];

                        for (int i = 0; i < kernel.GetLength(0); i++)
                        {
                            for (int j = 0; j < kernel.GetLength(1); j++)
                            {
                                kernel[i, j] = InterpolationUtils.InterpolateDouble(startConvo.Kernel[i, j], endConvo.Kernel[i, j], position);
                            }
                        }

                        ConvolutionFilter filter = new ConvolutionFilter(kernel, InterpolationUtils.InterpolateDouble(startConvo.Scale, endConvo.Scale, position), position < 0.5 ? startConvo.PreserveAlpha : endConvo.PreserveAlpha, InterpolationUtils.InterpolateDouble(startConvo.Normalisation, endConvo.Normalisation, position), InterpolationUtils.InterpolateDouble(startConvo.Bias, endConvo.Bias, position));
                        return new FilteredGraphicsAction(gpr, filter);
                    }
                    else
                    {
                        return position < 0.5 ? this.StartValue : this.EndValue;
                    }
                }
                else if (this.StartValue.Filter is GaussianBlurFilter startGauss && this.EndValue.Filter is GaussianBlurFilter endGauss)
                {
                    GaussianBlurFilter filter = new GaussianBlurFilter(InterpolationUtils.InterpolateDouble(startGauss.StandardDeviation, endGauss.StandardDeviation, position));
                    return new FilteredGraphicsAction(gpr, filter);
                }
                else if (this.StartValue.Filter is MaskFilter startMask && this.EndValue.Filter is MaskFilter endMask)
                {
                    Graphics mask = MaskAnimation.Interpolate(position, easings);
                    MaskFilter filter = new MaskFilter(mask);
                    return new FilteredGraphicsAction(gpr, filter);
                }
                else
                {
                    return position < 0.5 ? this.StartValue : this.EndValue;
                }
            }
            else
            {
                return position < 0.5 ? this.StartValue : this.EndValue;
            }
        }
    }

    internal interface IGraphicsAnimation
    {
        Graphics StartValue { get; }
        Graphics EndValue { get; }
        Graphics Interpolate(double position, Dictionary<string, IEasing> easings);
    }

    internal class ConstantGraphicsAnimation : IGraphicsAnimation
    {
        public Graphics StartValue { get; }
        public Graphics EndValue => StartValue;
        public Graphics Interpolate(double position, Dictionary<string, IEasing> easings) => this.StartValue;

        public ConstantGraphicsAnimation(Graphics startValue)
        {
            this.StartValue = startValue;
        }
    }

    internal class GraphicsAnimation : IGraphicsAnimation
    {
        public Graphics StartValue { get; }
        public Graphics EndValue { get; }

        private List<IAnimation> StartAnimations { get; }
        private List<IAnimation> EndAnimations { get; }

        public GraphicsAnimation(Graphics startValue, Graphics endValue, double linearisationResolution)
        {
            this.StartValue = startValue;
            this.EndValue = endValue;

            Dictionary<string, IGraphicsAction> startTaggedActions = new Dictionary<string, IGraphicsAction>();

            foreach (IGraphicsAction action in startValue.Actions)
            {
                if (!string.IsNullOrEmpty(action.Tag))
                {
                    startTaggedActions[action.Tag] = action;
                }
            }

            Dictionary<string, IGraphicsAction> endTaggedActions = new Dictionary<string, IGraphicsAction>();

            foreach (IGraphicsAction action in endValue.Actions)
            {
                if (!string.IsNullOrEmpty(action.Tag))
                {
                    endTaggedActions[action.Tag] = action;
                }
            }

            List<IAnimation> startAnimations = new List<IAnimation>();

            Dictionary<string, IAnimation> computedAnimations = new Dictionary<string, IAnimation>();

            for (int i = 0; i < this.StartValue.Actions.Count; i++)
            {
                IGraphicsAction startAction = this.StartValue.Actions[i];

                if (!string.IsNullOrEmpty(this.StartValue.Actions[i].Tag) && endTaggedActions.TryGetValue(this.StartValue.Actions[i].Tag, out IGraphicsAction endAction) && startAction.GetType() == endAction.GetType())
                {
                    if (startAction is TransformAction startT && endAction is TransformAction endT)
                    {
                        IAnimation animation = new TransformAnimation(startT, endT);

                        computedAnimations[this.StartValue.Actions[i].Tag] = animation;

                        startAnimations.Add(animation);
                    }
                    else if (startAction is StateAction startS && endAction is StateAction endS)
                    {
                        IAnimation animation = new StateAnimation(startS, endS);
                        computedAnimations[this.StartValue.Actions[i].Tag] = animation;
                        startAnimations.Add(animation);
                    }
                    else if (startAction is TextAction startX && endAction is TextAction endX)
                    {
                        IAnimation animation = new TextActionAnimation(startX, endX);
                        computedAnimations[this.StartValue.Actions[i].Tag] = animation;
                        startAnimations.Add(animation);
                    }
                    else if (startAction is RectangleAction startR && endAction is RectangleAction endR)
                    {
                        IAnimation animation = new RectangleActionAnimation(startR, endR);
                        computedAnimations[this.StartValue.Actions[i].Tag] = animation;
                        startAnimations.Add(animation);
                    }
                    else if (startAction is PathAction startP && endAction is PathAction endP)
                    {
                        IAnimation animation = new PathActionAnimation(startP, endP, linearisationResolution);
                        computedAnimations[this.StartValue.Actions[i].Tag] = animation;
                        startAnimations.Add(animation);
                    }
                    else if (startAction is RasterImageAction startI && endAction is RasterImageAction endI)
                    {
                        IAnimation animation = new RasterImageActionAnimation(startI, endI);
                        computedAnimations[this.StartValue.Actions[i].Tag] = animation;
                        startAnimations.Add(animation);
                    }
                    else if (startAction is FilteredGraphicsAction startF && endAction is FilteredGraphicsAction endF)
                    {
                        IAnimation animation = new FilteredGraphicsAnimation(startF, endF, linearisationResolution);
                        computedAnimations[this.StartValue.Actions[i].Tag] = animation;
                        startAnimations.Add(animation);
                    }
                    else
                    {
                        startAnimations.Add(new ConstantAnimation<IGraphicsAction>(startAction));
                    }
                }
                else
                {
                    startAnimations.Add(new ConstantAnimation<IGraphicsAction>(startAction));
                }
            }


            List<IAnimation> endAnimations = new List<IAnimation>();

            for (int i = 0; i < this.EndValue.Actions.Count; i++)
            {
                IGraphicsAction endAction = this.EndValue.Actions[i];

                if (!string.IsNullOrEmpty(this.EndValue.Actions[i].Tag) && startTaggedActions.TryGetValue(this.EndValue.Actions[i].Tag, out IGraphicsAction startAction) && startAction.GetType() == endAction.GetType())
                {
                    endAnimations.Add(computedAnimations[endAction.Tag]);
                }
                else
                {
                    endAnimations.Add(new ConstantAnimation<IGraphicsAction>(endAction));
                }
            }

            this.StartAnimations = startAnimations;
            this.EndAnimations = endAnimations;
        }

        public Graphics Interpolate(double position, Dictionary<string, IEasing> easings)
        {
            if (position <= 0)
            {
                return this.StartValue;
            }
            else if (position >= 1)
            {
                return this.EndValue;
            }
            else
            {
                Graphics gpr = new Graphics();

                if (position < 0.5)
                {
                    for (int i = 0; i < StartAnimations.Count; i++)
                    {
                        gpr.Actions.Add(StartAnimations[i].Interpolate(position, easings));
                    }
                }
                else
                {
                    for (int i = 0; i < EndAnimations.Count; i++)
                    {
                        gpr.Actions.Add(EndAnimations[i].Interpolate(position, easings));
                    }
                }

                return gpr;
            }
        }
    }

    /// <summary>
    /// A key frame for an animation.
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// The duration of the frame, in milliseconds.
        /// </summary>
        public double Duration { get; }

        /// <summary>
        /// The contents of the frame.
        /// </summary>
        public Graphics Graphics { get; }

        /// <summary>
        /// Creates a new <see cref="Frame"/> with the specified contents and duration.
        /// </summary>
        /// <param name="graphics">The contents of the frame.</param>
        /// <param name="duration">The duration of the frame, in milliseconds.</param>
        public Frame(Graphics graphics, double duration)
        {
            this.Graphics = graphics;
            this.Duration = duration;
        }
    }

    /// <summary>
    /// Describes a function used to transform the transition speed.
    /// </summary>
    public interface IEasing
    {
        /// <summary>
        /// Applies the easing to the specified transition offset.
        /// </summary>
        /// <param name="value">The transition offset (ranging from 0 to 1).</param>
        /// <returns>The eased transition offset value.</returns>
        [Pure]
        double Ease(double value);
    }

    /// <summary>
    /// Describes an easing defined by a Cubic Bezier curve.
    /// </summary>
    public class SplineEasing : IEasing
    {
        /// <summary>
        /// The first control point of the curve.
        /// </summary>
        public Point ControlPoint1 { get; }

        /// <summary>
        /// The second control point of the curve.
        /// </summary>
        public Point ControlPoint2 { get; }
        private double[] SampledEasings { get; }

        /// <summary>
        /// Creates a new <see cref="SplineEasing"/> with the specified control points. The start point is always (0, 0) and the end point is always (1, 1).
        /// </summary>
        /// <param name="controlPoint1">The first control point of the curve. Both X and Y must be between 0 and 1, inclusive.</param>
        /// <param name="controlPoint2">The second control point of the curve. Both X and Y must be between 0 and 1, inclusive.</param>
        /// <exception cref="ArgumentException">This exception is thrown if any coordinate of the control points is &lt; 0 or &gt; 1.</exception>
        public SplineEasing(Point controlPoint1, Point controlPoint2)
        {
            if (controlPoint1.X < 0 || controlPoint1.Y < 0 || controlPoint2.X > 1 || controlPoint2.Y > 1)
            {
                throw new ArgumentException("The control point coordinates are out of range! All coordinates must be within 0 and 1 (inclusive).");
            }

            this.ControlPoint1 = controlPoint1;
            this.ControlPoint2 = controlPoint2;

            Point[] pts = new Point[51];

            for (int i = 0; i <= 50; i++)
            {
                double t = i / 50.0;

                pts[i] = new Point(3 * (1 - t) * (1 - t) * t * controlPoint1.X + 3 * (1 - t) * t * t * controlPoint2.X + t * t * t, 3 * (1 - t) * (1 - t) * t * controlPoint1.Y + 3 * (1 - t) * t * t * controlPoint2.Y + t * t * t);
            }

            double[] sampledEasings = new double[51];


            double currX = 0;
            double currY = pts[0].Y;

            int nextSample = 1;
            sampledEasings[0] = pts[0].Y;

            for (int i = 1; i < sampledEasings.Length; i++)
            {
                double targetX = nextSample / 50.0;

                double newX = pts[i].X;

                while (currX <= targetX && newX >= targetX)
                {
                    sampledEasings[nextSample] = currY + (targetX - currX) / (newX - currX) * (pts[i].Y - currY);
                    nextSample++;
                    targetX = nextSample / 50.0;
                }

                currX = newX;
                currY = pts[i].Y;
            }

            this.SampledEasings = sampledEasings;
        }

        /// <inheritdoc/>
        [Pure]
        public double Ease(double value)
        {
            if (value <= 0)
            {
                return 0;
            }
            else if (value >= 1)
            {
                return 1;
            }
            else
            {
                int lowIndex = (int)Math.Floor(value * 50);
                int highIndex = (int)Math.Ceiling(value * 50);

                return SampledEasings[lowIndex] + (SampledEasings[highIndex] - SampledEasings[lowIndex]) * (value * 50 - lowIndex);
            }
        }
    }

    /// <summary>
    /// Describes a linear easing (i.e., no easing).
    /// </summary>
    public class LinearEasing : IEasing
    {
        /// <summary>
        /// Creates a new <see cref="LinearEasing"/>.
        /// </summary>
        public LinearEasing()
        {

        }

        /// <inheritdoc/>
        [Pure]
        public double Ease(double value) => value;
    }

    /// <summary>
    /// Describes the transition between two successive <see cref="Frame"/>s.
    /// </summary>
    public class Transition
    {
        /// <summary>
        /// The duration of the transition, in milliseconds.
        /// </summary>
        public double Duration { get; }

        /// <summary>
        /// The <see cref="IEasing"/> to apply to all elements for which another easing is not specified. Set to null to use the default linear easing.
        /// </summary>
        public IEasing OverallEasing { get; } = null;

        /// <summary>
        /// A dictionary associating graphic action tags to the corresponding <see cref="IEasing"/>.
        /// </summary>
        public Dictionary<string, IEasing> Easings { get; } = null;

        /// <summary>
        /// Creates a new <see cref="Transition"/> with the specified duration and easings.
        /// </summary>
        /// <param name="duration">The duration of the transition, in milliseconds.</param>
        /// <param name="easing">The <see cref="IEasing"/> to apply to all elements for which another easing is not specified. Set to null to use the default linear easing.</param>
        /// <param name="easings">A dictionary associating graphic action tags to the corresponding <see cref="IEasing"/>.</param>
        public Transition(double duration, IEasing easing = null, Dictionary<string, IEasing> easings = null)
        {
            if (easing == null)
            {
                easing = new LinearEasing();
            }

            this.Duration = duration;
            this.OverallEasing = easing;
            this.Easings = easings;
        }
    }

    /// <summary>
    /// Describes an animation constituted by a number of frames and transitions between them.
    /// </summary>
    public class Animation
    {
        /// <summary>
        /// The key frames of the animation.
        /// </summary>
        public ImmutableList<Frame> Frames { get; private set; } = ImmutableList<Frame>.Empty;

        /// <summary>
        /// The transitions between successive frames of the animation. This array always contains one fewer element than <see cref="Frames"/>.
        /// </summary>
        public ImmutableList<Transition> Transitions { get; private set; } = ImmutableList<Transition>.Empty;

        private ImmutableList<GraphicsAnimation> Animations { get; set; } = ImmutableList<GraphicsAnimation>.Empty;


        /// <summary>
        /// The width of the animation.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The height of the animation.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// The background colour of the animation.
        /// </summary>
        public Colour Background { get; set; } = Colour.FromRgba(255, 255, 255, 0);

        /// <summary>
        /// The absolute length between successive samples to use when linearising <see cref="GraphicsPath"/>s.
        /// </summary>
        public double LinearisationResolution { get; }

        /// <summary>
        /// The total duration of the animation (not including the number of repeats).
        /// </summary>
        public double Duration { get; private set; } = 0;

        /// <summary>
        /// The number of times that the animation should repeat.
        /// </summary>
        public int RepeatCount { get; set; } = 0;

        /// <summary>
        /// Creates a new <see cref="Animation"/> with the specified width, height and linearisation resolution.
        /// </summary>
        /// <param name="width">The width of the animation.</param>
        /// <param name="height">The height of the animation.</param>
        /// <param name="linearisationResolution">The absolute length between successive samples to use when linearising <see cref="GraphicsPath"/>s.</param>
        public Animation(double width, double height, double linearisationResolution)
        {
            this.Width = width;
            this.Height = height;
            this.LinearisationResolution = linearisationResolution;
        }

        /// <summary>
        /// Obtains the (interpolated) frame that should be displayed after the specified time has passed since the start of the animation.
        /// </summary>
        /// <param name="time">The time since the start of the animation (in milliseconds).</param>
        /// <returns>A <see cref="Page"/> containing the interpolated frame that should be displayed after the specified time has passed since the start of the animation.</returns>
        public Page GetFrameAtAbsolute(double time)
        {
            if (time > Duration)
            {
                if (this.RepeatCount <= 0)
                {
                    time = time % this.Duration;
                }
                else
                {
                    time = Math.Max(time % this.Duration, time - this.Duration * RepeatCount);
                }
            }

            if (time <= 0)
            {
                Page pag = new Page(this.Width, this.Height) { Background = this.Background };
                pag.Graphics.DrawGraphics(0, 0, this.Frames[0].Graphics);
                return pag;
            }
            else if (time > this.Duration)
            {
                Page pag = new Page(this.Width, this.Height) { Background = this.Background };
                pag.Graphics.DrawGraphics(0, 0, this.Frames[this.Frames.Count - 1].Graphics);
                return pag;
            }
            else
            {
                double currTime = 0;

                for (int i = 0; i < Frames.Count; i++)
                {
                    if (i > 0)
                    {
                        double newTime = currTime + Transitions[i - 1].Duration;

                        if (currTime <= time && newTime >= time)
                        {
                            Frame previousFrame = Frames[i - 1];

                            Frame nextFrame = Frames[i];

                            if (previousFrame != null && nextFrame != null)
                            {
                                double position = Transitions[i - 1].OverallEasing.Ease((time - currTime) / (newTime - currTime));

                                Graphics gpr = Animations[i - 1].Interpolate(position, Transitions[i - 1].Easings);
                                Page pag = new Page(this.Width, this.Height) { Background = this.Background };
                                pag.Graphics.DrawGraphics(0, 0, gpr);
                                return pag;
                            }
                            else if (previousFrame == null && nextFrame != null)
                            {
                                Page pag = new Page(this.Width, this.Height) { Background = this.Background };
                                pag.Graphics.DrawGraphics(0, 0, nextFrame.Graphics);
                                return pag;
                            }
                            else if (previousFrame != null && nextFrame == null)
                            {
                                Page pag = new Page(this.Width, this.Height) { Background = this.Background };
                                pag.Graphics.DrawGraphics(0, 0, previousFrame.Graphics);
                                return pag;
                            }
                            else
                            {
                                Page pag = new Page(this.Width, this.Height) { Background = this.Background };
                                return pag;
                            }
                        }

                        currTime = newTime;
                    }


                    {
                        double newTime = currTime + Frames[i].Duration;

                        if (currTime <= time && newTime >= time)
                        {
                            Page pag = new Page(this.Width, this.Height) { Background = this.Background };
                            pag.Graphics.DrawGraphics(0, 0, Frames[i].Graphics);
                            return pag;
                        }

                        currTime = newTime;
                    }

                }


                {
                    Page pag = new Page(this.Width, this.Height) { Background = this.Background };
                    pag.Graphics.DrawGraphics(0, 0, this.Frames[this.Frames.Count - 1].Graphics);
                    return pag;
                }
            }
        }

        /// <summary>
        /// Obtains the (interpolated) frame that should be displayed after the specified relative time has passed since the start of the animation.
        /// </summary>
        /// <param name="relativeTime">The time since the start of the animation (ranging from 0 for the start of the animation, to 1 for the end of the animation).</param>
        /// <returns>A <see cref="Page"/> containing the interpolated frame that should be displayed after the specified time has passed since the start of the animation.</returns>
        public Page GetFrameAtRelative(double relativeTime)
        {
            return GetFrameAtAbsolute(this.Duration * relativeTime);
        }

        /// <summary>
        /// Adds a new frame to the animation, with the specified transition.
        /// </summary>
        /// <param name="frame">The new frame to add to the animation.</param>
        /// <param name="transition">The transition that should be applied between the previous frame and the new frame. This parameter is ignored for the first frame. If this is <see langword="null"/>, the animation will abruptly change from one frame to the next.</param>
        public void AddFrame(Frame frame, Transition transition = null)
        {
            this.Frames = this.Frames.Add(frame);
            this.Duration += frame.Duration;

            if (this.Frames.Count > 1)
            {
                if (transition == null)
                {
                    transition = new Transition(0, new LinearEasing());
                }

                this.Transitions = this.Transitions.Add(transition);
                this.Animations = this.Animations.Add(new GraphicsAnimation(this.Frames[this.Frames.Count - 2].Graphics, this.Frames[this.Frames.Count - 1].Graphics, this.LinearisationResolution));
                this.Duration += transition.Duration;
            }
        }

        /// <summary>
        /// Removes the last frame from the animation (and the corresponding transition).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if there are no frames in the animation.</exception>
        public void RemoveLastFrame()
        {
            if (this.Frames.Count > 0)
            {
                if (this.Frames.Count > 1)
                {
                    double lostDuration = this.Frames[this.Frames.Count - 1].Duration + this.Transitions[this.Transitions.Count - 1].Duration;

                    this.Frames = this.Frames.RemoveAt(this.Frames.Count - 1);
                    this.Animations = this.Animations.RemoveAt(this.Animations.Count - 1);
                    this.Transitions = this.Transitions.RemoveAt(this.Transitions.Count - 1);

                    this.Duration -= lostDuration;
                }
                else
                {
                    this.Frames = ImmutableList<Frame>.Empty;
                    this.Duration = 0;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("There are no frames in the animation!");
            }
        }
    }
}
