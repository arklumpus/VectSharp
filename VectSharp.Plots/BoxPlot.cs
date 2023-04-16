/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2023 Giorgio Bianchini, University of Bristol

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

namespace VectSharp.Plots
{
    /// <summary>
    /// A plot element that draws a box plot.
    /// </summary>
    public class BoxPlot : IPlotElement
    {
        /// <summary>
        /// The position of the centre (e.g., median or mean) of the box plot, in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> Position { get; set; }

        /// <summary>
        /// The direction along which the box plot is drawn, in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> Direction { get; set; }
        
        /// <summary>
        /// The distance between the centre of the box plot and the first whisker, expressed as a multiple
        /// of <see cref="Direction"/>.
        /// </summary>
        public double Whisker1 { get; set; }

        /// <summary>
        /// The distance between the centre of the box plot and the first side of the box, expressed as a multiple
        /// of <see cref="Direction"/>.
        /// </summary>
        public double Box1 { get; set; }

        /// <summary>
        /// The distance between the centre of the box plot and the second side of the box, expressed as a multiple
        /// of <see cref="Direction"/>.
        /// </summary>
        public double Box2 { get; set; }

        /// <summary>
        /// The distance between the centre of the box plot and the second whisker, expressed as a multiple
        /// of <see cref="Direction"/>.
        /// </summary>
        public double Whisker2 { get; set; }
        
        /// <summary>
        /// The width of the box plot, in data space coordinates.
        /// </summary>
        public double Width { get; set; } = 10;
        
        /// <summary>
        /// The width of the whiskers, expressed as a multiple of the <see cref="Width"/> of the box.
        /// </summary>
        public double WhiskerWidth { get; set; } = 0.5;

        /// <summary>
        /// The width of the notch, expressed as a multiple of the <see cref="Width"/> of the box.
        /// </summary>
        public double NotchWidth { get; set; } = 0.5;
        
        /// <summary>
        /// The distance between the centre of the box plot and the end of the notch, expressed as a multiple
        /// of <see cref="Direction"/>.
        /// </summary>
        public double NotchSize { get; set; } = 0;

        /// <summary>
        /// Presentation attributes for the box.
        /// </summary>
        public PlotElementPresentationAttributes BoxPresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { Fill = Colours.White };
        
        /// <summary>
        /// Presentation attributes for the whiskers.
        /// </summary>
        public PlotElementPresentationAttributes WhiskersPresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { Fill = null };

        /// <summary>
        /// Presentation attributes for the symbol drawn at the centre of the box plot.
        /// </summary>
        public PlotElementPresentationAttributes CentreSymbolPresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { Fill = null, Stroke = null };
        
        /// <summary>
        /// Symbol drawn at the centre of the box plot.
        /// </summary>
        public IDataPointElement CentreSymbol { get; set; } = new PathDataPointElement();

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<IReadOnlyList<double>> CoordinateSystem { get; set; }

        /// <summary>
        /// A tag to identify the box in the plot.
        /// </summary>
        public string Tag { get; set; }

        ICoordinateSystem IPlotElement.CoordinateSystem => this.CoordinateSystem;

        /// <summary>
        /// Create a new <see cref="BoxPlot"/> instance.
        /// </summary>
        /// <param name="position">The position of the centre (e.g., median or mean) of the box plot, in data space coordinates.</param>
        /// <param name="direction">The direction along which the box plot is drawn, in data space coordinates.</param>
        /// <param name="whisker1">The distance between the centre of the box plot and the first whisker, expressed as a multiple
        /// of <paramref name="direction"/>.</param>
        /// <param name="box1">The distance between the centre of the box plot and the first side of the box, expressed as a multiple
        /// of <paramref name="direction"/>.</param>
        /// <param name="box2">The distance between the centre of the box plot and the second side of the box, expressed as a multiple
        /// of <paramref name="direction"/>.</param>
        /// <param name="whisker2">The distance between the centre of the box plot and the second whisker, expressed as a multiple
        /// of <paramref name="direction"/>.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public BoxPlot(IReadOnlyList<double> position, IReadOnlyList<double> direction, double whisker1, double box1, double box2, double whisker2, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem)
        {
            this.Position = position;
            this.Direction = direction;
            this.Whisker1 = whisker1;
            this.Box1 = box1;
            this.Box2 = box2;
            this.Whisker2 = whisker2;
            this.CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            double[] perpDirection = new double[] { -Direction[1], Direction[0] };

            Point whisker1 = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.Whisker1, this.Position[1] + this.Direction[1] * this.Whisker1 });
            Point whisker2 = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.Whisker2, this.Position[1] + this.Direction[1] * this.Whisker2 });

            Point whisker1Left = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.Whisker1 + perpDirection[0] * this.Width * this.WhiskerWidth, this.Position[1] + this.Direction[1] * this.Whisker1 + perpDirection[1] * this.Width * this.WhiskerWidth });
            Point box1Left = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.Box1 + perpDirection[0] * this.Width, this.Position[1] + this.Direction[1] * this.Box1 + perpDirection[1] * this.Width });
            Point box2Left = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.Box2 + perpDirection[0] * this.Width, this.Position[1] + this.Direction[1] * this.Box2 + perpDirection[1] * this.Width });
            Point whisker2Left = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.Whisker2 + perpDirection[0] * this.Width * this.WhiskerWidth, this.Position[1] + this.Direction[1] * this.Whisker2 + perpDirection[1] * this.Width * this.WhiskerWidth });

            Point whisker1Right = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.Whisker1 - perpDirection[0] * this.Width * this.WhiskerWidth, this.Position[1] + this.Direction[1] * this.Whisker1 - perpDirection[1] * this.Width * this.WhiskerWidth });
            Point box1Right = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.Box1 - perpDirection[0] * this.Width, this.Position[1] + this.Direction[1] * this.Box1 - perpDirection[1] * this.Width });
            Point box2Right = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.Box2 - perpDirection[0] * this.Width, this.Position[1] + this.Direction[1] * this.Box2 - perpDirection[1] * this.Width });
            Point whisker2Right = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.Whisker2 - perpDirection[0] * this.Width * this.WhiskerWidth, this.Position[1] + this.Direction[1] * this.Whisker2 - perpDirection[1] * this.Width * this.WhiskerWidth });

            Point medianLeft = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + perpDirection[0] * this.Width, this.Position[1] + perpDirection[1] * this.Width });
            Point medianRight = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] - perpDirection[0] * this.Width, this.Position[1] - perpDirection[1] * this.Width });

            string whiskerTag = Tag;
            string boxFillTag = Tag;
            string boxStrokeTag = Tag;
            string medianTag = Tag;
            string medianSymbolTag = Tag;

            if (!string.IsNullOrEmpty(Tag) && target.UseUniqueTags)
            {
                whiskerTag += "@whiskers";
                boxStrokeTag += "@stroke";
                medianTag += "@median";
                medianSymbolTag += "@medianSymbol";
            }

            if (NotchSize != 0 && NotchWidth != 1)
            {
                medianLeft = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + perpDirection[0] * this.Width * this.NotchWidth, this.Position[1] + perpDirection[1] * this.Width * this.NotchWidth });
                medianRight = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] - perpDirection[0] * this.Width * this.NotchWidth, this.Position[1] - perpDirection[1] * this.Width * this.NotchWidth });

                Point notch2Left = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.NotchSize + perpDirection[0] * this.Width, this.Position[1] + this.Direction[1] * this.NotchSize + perpDirection[1] * this.Width });
                Point notch1Left = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] - this.Direction[0] * this.NotchSize + perpDirection[0] * this.Width, this.Position[1] - this.Direction[1] * this.NotchSize + perpDirection[1] * this.Width });

                Point notch2Right = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] + this.Direction[0] * this.NotchSize - perpDirection[0] * this.Width, this.Position[1] + this.Direction[1] * this.NotchSize - perpDirection[1] * this.Width });
                Point notch1Right = CoordinateSystem.ToPlotCoordinates(new double[] { this.Position[0] - this.Direction[0] * this.NotchSize - perpDirection[0] * this.Width, this.Position[1] - this.Direction[1] * this.NotchSize - perpDirection[1] * this.Width });


                GraphicsPath whiskers = new GraphicsPath();

                whiskers.MoveTo(whisker1Left).LineTo(whisker1Right);
                whiskers.MoveTo(whisker1).LineTo(whisker2);
                whiskers.MoveTo(whisker2Left).LineTo(whisker2Right);

                GraphicsPath box = new GraphicsPath().MoveTo(box1Left).LineTo(box1Right).LineTo(notch1Right).LineTo(medianRight).LineTo(notch2Right).LineTo(box2Right).LineTo(box2Left).LineTo(notch2Left).LineTo(medianLeft).LineTo(notch1Left).Close();
                GraphicsPath median = new GraphicsPath().MoveTo(medianLeft).LineTo(medianRight);

                if (WhiskersPresentationAttributes.Stroke != null)
                {
                    target.StrokePath(whiskers, WhiskersPresentationAttributes.Stroke, WhiskersPresentationAttributes.LineWidth, WhiskersPresentationAttributes.LineCap, WhiskersPresentationAttributes.LineJoin, WhiskersPresentationAttributes.LineDash, whiskerTag);
                }

                if (BoxPresentationAttributes.Fill != null)
                {
                    target.FillPath(box, BoxPresentationAttributes.Fill, boxFillTag);
                }

                if (BoxPresentationAttributes.Stroke != null)
                {
                    target.StrokePath(box, BoxPresentationAttributes.Stroke, BoxPresentationAttributes.LineWidth, BoxPresentationAttributes.LineCap, BoxPresentationAttributes.LineJoin, BoxPresentationAttributes.LineDash, boxStrokeTag);
                    target.StrokePath(median, BoxPresentationAttributes.Stroke, BoxPresentationAttributes.LineWidth * 2, BoxPresentationAttributes.LineCap, BoxPresentationAttributes.LineJoin, BoxPresentationAttributes.LineDash, medianTag);
                }
            }
            else
            {
                GraphicsPath whiskers = new GraphicsPath();

                whiskers.MoveTo(whisker1Left).LineTo(whisker1Right);
                whiskers.MoveTo(whisker1).LineTo(whisker2);
                whiskers.MoveTo(whisker2Left).LineTo(whisker2Right);

                GraphicsPath box = new GraphicsPath().MoveTo(box1Left).LineTo(box1Right).LineTo(box2Right).LineTo(box2Left).Close();
                GraphicsPath median = new GraphicsPath().MoveTo(medianLeft).LineTo(medianRight);

                if (WhiskersPresentationAttributes.Stroke != null)
                {
                    target.StrokePath(whiskers, WhiskersPresentationAttributes.Stroke, WhiskersPresentationAttributes.LineWidth, WhiskersPresentationAttributes.LineCap, WhiskersPresentationAttributes.LineJoin, WhiskersPresentationAttributes.LineDash, whiskerTag);
                }

                if (BoxPresentationAttributes.Fill != null)
                {
                    target.FillPath(box, BoxPresentationAttributes.Fill, boxFillTag);
                }

                if (BoxPresentationAttributes.Stroke != null)
                {
                    target.StrokePath(box, BoxPresentationAttributes.Stroke, BoxPresentationAttributes.LineWidth, BoxPresentationAttributes.LineCap, BoxPresentationAttributes.LineJoin, BoxPresentationAttributes.LineDash, boxStrokeTag);
                    target.StrokePath(median, BoxPresentationAttributes.Stroke, BoxPresentationAttributes.LineWidth * 2, LineCaps.Butt, BoxPresentationAttributes.LineJoin, BoxPresentationAttributes.LineDash, medianTag);
                }
            }

            if (CentreSymbolPresentationAttributes.Fill != null || CentreSymbolPresentationAttributes.Stroke != null)
            {
                double medianCircleRadius = Math.Min(Math.Sqrt((medianLeft.X - medianRight.X) * (medianLeft.X - medianRight.X) + (medianLeft.Y - medianRight.Y) * (medianLeft.Y - medianRight.Y)),
                Math.Min(Math.Sqrt((box1Left.X - box2Left.X) * (box1Left.X - box2Left.X) + (box1Left.Y - box2Left.Y) * (box1Left.Y - box2Left.Y)),
                Math.Sqrt((box1Right.X - box2Right.X) * (box1Right.X - box2Right.X) + (box1Right.Y - box2Right.Y) * (box1Right.Y - box2Right.Y)))) * 0.45;

                target.Save();
                target.Translate((medianLeft.X + medianRight.X) * 0.5, (medianLeft.Y + medianRight.Y) * 0.5);
                target.Scale(medianCircleRadius, medianCircleRadius);

                CentreSymbol.Plot(target, CentreSymbolPresentationAttributes, medianSymbolTag);

                target.Restore();
            }
        }
    }
}
