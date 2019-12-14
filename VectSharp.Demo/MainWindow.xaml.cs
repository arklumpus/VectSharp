using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using VectSharp;
using VectSharp.Canvas;
using VectSharp.PDF;
using VectSharp.Raster;

namespace VectSharp.Demo
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            //Create a new document
            Document doc = new Document();

            //Add a page to the document
            doc.Pages.Add(new Page(4000, 2600));

            //Obtain the page's graphics object
            Graphics gpr = doc.Pages.Last().Graphics;

            //Text samples using default fonts
            //To use a custom font in TTF format, supply the path to the font's TTF file to the FontFamily(string) constructor
            gpr.FillText(200, 200, "Times-Roman", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesRoman), 120), Colour.FromRgb(0, 0, 0));
            gpr.FillText(200, 340, "Times-Bold", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesBold), 120), Colour.FromRgb(127, 127, 127));
            gpr.FillText(200, 480, "Times-Italic", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesItalic), 120), Colour.FromRgb(136, 0, 21));
            gpr.FillText(200, 620, "Times-BoldItalic", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesBoldItalic), 120), Colour.FromRgb(237, 28, 36));

            gpr.FillText(200, 860, "Helvetica", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 120), Colour.FromRgb(255, 174, 201));
            gpr.FillText(200, 1000, "Helvetica-Bold", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 120), Colour.FromRgb(255, 127, 39));
            gpr.FillText(200, 1140, "Helvetica-Oblique", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaOblique), 120), Colour.FromRgb(255, 242, 0));
            gpr.FillText(200, 1280, "Helvetica-BoldOblique", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBoldOblique), 120), Colour.FromRgb(181, 230, 29));

            gpr.FillText(200, 1520, "Courier", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Courier), 120), Colour.FromRgb(34, 177, 76));
            gpr.FillText(200, 1660, "Courier-Bold", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.CourierBold), 120), Colour.FromRgb(0, 162, 232));
            gpr.FillText(200, 1800, "Courier-Oblique", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.CourierOblique), 120), Colour.FromRgb(112, 146, 190));
            gpr.FillText(200, 1940, "Courier-BoldOblique", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.CourierBoldOblique), 120), Colour.FromRgb(63, 72, 204));

            gpr.FillText(200, 2180, "Σψμβολ", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Symbol), 120), Colour.FromRgb(163, 73, 204));
            gpr.FillText(200, 2320, "✺❁❐❆✤❉■❇❂❁▼▲", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.ZapfDingbats), 120), Colour.FromRgb(200, 191, 231));


            //Text metrics sample
            string testString = "VectSharp";
            Font timesBI = new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesBoldItalic), 240);
            Font.DetailedFontMetrics metrics = timesBI.MeasureTextAdvanced(testString);
            gpr.FillText(2000, 500, testString, timesBI, Colour.FromRgb(0, 0, 0), textBaseline: TextBaselines.Baseline);

            //Text baseline
            gpr.StrokePath(new GraphicsPath().MoveTo(1900, 500).LineTo(2050 + metrics.Width, 500), Colour.FromRgb(237, 28, 36), lineWidth: 5);
            gpr.FillText(1880 - gpr.MeasureText("Baseline", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50)).Width, 500, "Baseline", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50), Colour.FromRgb(237, 28, 36), textBaseline: TextBaselines.Middle);

            //Font ascent and descent
            gpr.StrokePath(new GraphicsPath().MoveTo(1900, 500 - timesBI.Ascent).LineTo(2100 + metrics.Width, 500 - timesBI.Ascent), Colour.FromRgb(255, 127, 39), lineWidth: 5);
            gpr.FillText(2120 + metrics.Width, 500 - timesBI.Ascent, "Ascent", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50), Colour.FromRgb(255, 127, 39), textBaseline: TextBaselines.Middle);

            gpr.StrokePath(new GraphicsPath().MoveTo(1900, 500 - timesBI.Descent).LineTo(2100 + metrics.Width, 500 - timesBI.Descent), Colour.FromRgb(255, 127, 39), lineWidth: 5);
            gpr.FillText(2120 + metrics.Width, 500 - timesBI.Descent, "Descent", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50), Colour.FromRgb(255, 127, 39), textBaseline: TextBaselines.Middle);


            //Text top and bottom
            gpr.StrokePath(new GraphicsPath().MoveTo(1900, 500 - metrics.Top).LineTo(2050 + metrics.Width, 500 - metrics.Top), Colour.FromRgb(0, 162, 232), lineWidth: 5);
            gpr.FillText(1880 - gpr.MeasureText("Top", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50)).Width, 500 - metrics.Top, "Top", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50), Colour.FromRgb(0, 162, 232), textBaseline: TextBaselines.Middle);

            gpr.StrokePath(new GraphicsPath().MoveTo(1900, 500 - metrics.Bottom).LineTo(2050 + metrics.Width, 500 - metrics.Bottom), Colour.FromRgb(0, 162, 232), lineWidth: 5);
            gpr.FillText(1880 - gpr.MeasureText("Bottom", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50)).Width, 500 - metrics.Bottom, "Bottom", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50), Colour.FromRgb(0, 162, 232), textBaseline: TextBaselines.Middle);

            gpr.StrokePath(new GraphicsPath().MoveTo(2065 + metrics.Width, 520 - metrics.Top).LineTo(2075 + metrics.Width, 500 - metrics.Top).LineTo(2085 + metrics.Width, 520 - metrics.Top).MoveTo(2075 + metrics.Width, 500 - metrics.Top).LineTo(2075 + metrics.Width, 500 - metrics.Bottom).MoveTo(2065 + metrics.Width, 480 - metrics.Bottom).LineTo(2075 + metrics.Width, 500 - metrics.Bottom).LineTo(2085 + metrics.Width, 480 - metrics.Bottom), Colour.FromRgb(0, 162, 232), lineWidth: 5);

            //Text width
            gpr.StrokePath(new GraphicsPath().MoveTo(2000, 425 - timesBI.Ascent).LineTo(2000, 550 - timesBI.Descent).MoveTo(2000 + metrics.Width, 425 - timesBI.Ascent).LineTo(2000 + metrics.Width, 550 - timesBI.Descent), Colour.FromRgb(63, 72, 204), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2020, 390 - timesBI.Ascent).LineTo(2000, 400 - timesBI.Ascent).LineTo(2020, 410 - timesBI.Ascent).MoveTo(2000, 400 - timesBI.Ascent).LineTo(2000 + metrics.Width, 400 - timesBI.Ascent).MoveTo(1980 + metrics.Width, 390 - timesBI.Ascent).LineTo(2000 + metrics.Width, 400 - timesBI.Ascent).LineTo(1980 + metrics.Width, 410 - timesBI.Ascent), Colour.FromRgb(63, 72, 204), lineWidth: 5);
            gpr.FillText(2000 + metrics.Width / 2 - gpr.MeasureText("Width", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50)).Width / 2, 380 - timesBI.Ascent, "Width", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50), Colour.FromRgb(63, 72, 204), textBaseline: TextBaselines.Bottom);

            //Left- and right-side bearing
            gpr.StrokePath(new GraphicsPath().MoveTo(2000 - metrics.LeftSideBearing, 450 - timesBI.Ascent).LineTo(2000 - metrics.LeftSideBearing, 550 - timesBI.Descent).MoveTo(2000 + metrics.Width + metrics.RightSideBearing, 450 - timesBI.Ascent).LineTo(2000 + metrics.Width + metrics.RightSideBearing, 550 - timesBI.Descent), Colour.FromRgb(163, 73, 204), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2000, 565 - timesBI.Descent).LineTo(2000, 585 - timesBI.Descent).MoveTo(2000, 575 - timesBI.Descent).LineTo(2000 - metrics.LeftSideBearing, 575 - timesBI.Descent).MoveTo(2000 - metrics.LeftSideBearing, 565 - timesBI.Descent).LineTo(2000 - metrics.LeftSideBearing, 585 - timesBI.Descent), Colour.FromRgb(163, 73, 204), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2000 + metrics.Width + metrics.RightSideBearing, 565 - timesBI.Descent).LineTo(2000 + metrics.Width + metrics.RightSideBearing, 585 - timesBI.Descent).MoveTo(2000 + metrics.Width + metrics.RightSideBearing, 575 - timesBI.Descent).LineTo(2000 + metrics.Width, 575 - timesBI.Descent).MoveTo(2000 + metrics.Width, 565 - timesBI.Descent).LineTo(2000 + metrics.Width, 585 - timesBI.Descent), Colour.FromRgb(163, 73, 204), lineWidth: 5);

            gpr.FillText(2000, 600 - timesBI.Descent, "Left-side bearing", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50), Colour.FromRgb(163, 73, 164));
            gpr.FillText(2000 + metrics.Width - gpr.MeasureText("Right-side bearing", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50)).Width, 600 - timesBI.Descent, "Right-side bearing", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 50), Colour.FromRgb(163, 73, 164));



            //Text metrics sample #2
            Font biggerTimes = new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesBoldItalic), 300);

            Font.DetailedFontMetrics fMetrics = biggerTimes.MeasureTextAdvanced("f");
            Font.DetailedFontMetrics qMetrics = biggerTimes.MeasureTextAdvanced("q");

            gpr.StrokePath(new GraphicsPath().MoveTo(2050, 1200).LineTo(2750 + qMetrics.Width, 1200), Colour.FromRgb(237, 28, 36), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2050, 1200 - biggerTimes.Ascent).LineTo(2750 + qMetrics.Width, 1200 - biggerTimes.Ascent), Colour.FromRgb(255, 127, 39), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2050, 1200 - biggerTimes.Descent).LineTo(2750 + qMetrics.Width, 1200 - biggerTimes.Descent), Colour.FromRgb(255, 127, 39), lineWidth: 5);

            gpr.StrokePath(new GraphicsPath().MoveTo(2050, 1200 - fMetrics.Top).LineTo(2150 + fMetrics.Width, 1200 - fMetrics.Top), Colour.FromRgb(0, 162, 232), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2050, 1200 - fMetrics.Bottom).LineTo(2150 + fMetrics.Width, 1200 - fMetrics.Bottom), Colour.FromRgb(0, 162, 232), lineWidth: 5);

            gpr.StrokePath(new GraphicsPath().MoveTo(2650, 1200 - qMetrics.Top).LineTo(2750 + qMetrics.Width, 1200 - qMetrics.Top), Colour.FromRgb(0, 162, 232), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2650, 1200 - qMetrics.Bottom).LineTo(2750 + qMetrics.Width, 1200 - qMetrics.Bottom), Colour.FromRgb(0, 162, 232), lineWidth: 5);

            gpr.StrokePath(new GraphicsPath().MoveTo(2100, 1150 - biggerTimes.Ascent).LineTo(2100, 1250 - biggerTimes.Descent), Colour.FromRgb(63, 72, 204), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2100 + fMetrics.Width, 1150 - biggerTimes.Ascent).LineTo(2100 + fMetrics.Width, 1250 - biggerTimes.Descent), Colour.FromRgb(63, 72, 204), lineWidth: 5);

            gpr.StrokePath(new GraphicsPath().MoveTo(2700, 1150 - biggerTimes.Ascent).LineTo(2700, 1250 - biggerTimes.Descent), Colour.FromRgb(63, 72, 204), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2700 + qMetrics.Width, 1150 - biggerTimes.Ascent).LineTo(2700 + qMetrics.Width, 1250 - biggerTimes.Descent), Colour.FromRgb(63, 72, 204), lineWidth: 5);

            gpr.StrokePath(new GraphicsPath().MoveTo(2100 - fMetrics.LeftSideBearing, 1150 - biggerTimes.Ascent).LineTo(2100 - fMetrics.LeftSideBearing, 1250 - biggerTimes.Descent), Colour.FromRgb(163, 73, 164), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2100 + fMetrics.Width + fMetrics.RightSideBearing, 1150 - biggerTimes.Ascent).LineTo(2100 + fMetrics.Width + fMetrics.RightSideBearing, 1250 - biggerTimes.Descent), Colour.FromRgb(163, 73, 164), lineWidth: 5);

            gpr.StrokePath(new GraphicsPath().MoveTo(2700 - qMetrics.LeftSideBearing, 1150 - biggerTimes.Ascent).LineTo(2700 - qMetrics.LeftSideBearing, 1250 - biggerTimes.Descent), Colour.FromRgb(163, 73, 164), lineWidth: 5);
            gpr.StrokePath(new GraphicsPath().MoveTo(2700 + qMetrics.Width + qMetrics.RightSideBearing, 1150 - biggerTimes.Ascent).LineTo(2700 + qMetrics.Width + qMetrics.RightSideBearing, 1250 - biggerTimes.Descent), Colour.FromRgb(163, 73, 164), lineWidth: 5);

            gpr.FillText(2100, 1200, "f", biggerTimes, Colour.FromRgb(0, 0, 0), textBaseline: TextBaselines.Baseline);
            gpr.FillText(2700, 1200, "q", biggerTimes, Colour.FromRgb(0, 0, 0), textBaseline: TextBaselines.Baseline);


            //Save graphics state (e.g. rotation, translation, scale etc.)
            gpr.Save();

            //Translate
            gpr.Translate(2000, 1000);

            //Scale
            gpr.Scale(5, 5);

            //Create a new path object
            GraphicsPath face = new GraphicsPath();
            //Figure start point
            face.MoveTo(135.484, 186.221);
            //Cubic Beziers (the start point is the current point; two control points and a destination point are provided)
            face.CubicBezierTo(119.634, 185.011, 96.762, 211.243, 89.044, 228.362);
            face.CubicBezierTo(83.104, 225.903, 76.343, 224.519, 69.141, 224.519);
            face.CubicBezierTo(61.986, 224.519, 55.267, 225.884, 49.302, 228.335);
            face.CubicBezierTo(42.919, 212.558, 18.514, 185.011, 2.664, 186.221);
            face.CubicBezierTo(-2.291, 186.6, 2.727, 212.423, 32.997, 239.883);
            face.CubicBezierTo(27.684, 246.062, 24.576, 253.702, 24.576, 262.033);
            face.CubicBezierTo(24.576, 282.245, 43.032, 296, 69.141, 296);
            face.CubicBezierTo(95.25, 296, 113.705, 282.245, 113.705, 262.033);
            face.CubicBezierTo(113.705, 253.662, 110.567, 245.988, 105.176, 239.757);
            face.CubicBezierTo(135.614, 211.926, 140.439, 186.6, 135.484, 186.221);
            //Close the path
            face.Close();
            //Fill the path
            gpr.FillPath(face, Colour.FromRgb(242, 216, 35));

            //Another path
            GraphicsPath mouth = new GraphicsPath();
            mouth.MoveTo(69.361, 288.088);
            mouth.CubicBezierTo(78.235, 288.088, 81.085, 268.837, 81.085, 268.837);
            mouth.CubicBezierTo(77.594, 269.597, 72.494, 267.158, 69.084, 267.158);
            mouth.CubicBezierTo(66.007, 267.158, 61.554, 269.144, 58.156, 268.97);
            mouth.CubicBezierTo(58.156, 268.97, 60.486, 288.088, 69.361, 288.088);
            mouth.Close();
            gpr.FillPath(mouth, Colour.FromRgb(181, 20, 24));

            //Yet another path
            GraphicsPath tongue = new GraphicsPath();
            tongue.MoveTo(60.305, 278.13);
            tongue.CubicBezierTo(61.968, 283.028, 64.795, 288.088, 69.361, 288.088);
            tongue.CubicBezierTo(73.931, 288.088, 76.896, 282.985, 78.683, 278.074);
            tongue.CubicBezierTo(72.673, 275.222, 66.57, 274.868, 60.305, 278.13);
            tongue.Close();
            gpr.FillPath(tongue, Colour.FromRgb(237, 27, 36));

            //Yet another path
            GraphicsPath leftCheek = new GraphicsPath();
            leftCheek.MoveTo(41.666, 270.34);
            leftCheek.CubicBezierTo(42.67, 275.168, 40.005, 279.806, 35.713, 280.698);
            leftCheek.CubicBezierTo(31.421, 281.591, 27.127, 278.4, 26.124, 273.571);
            leftCheek.CubicBezierTo(25.12, 268.743, 27.785, 264.105, 32.077, 263.213);
            leftCheek.CubicBezierTo(36.369, 262.32, 40.662, 265.511, 41.666, 270.34);
            leftCheek.Close();
            gpr.FillPath(leftCheek, Colour.FromRgb(235, 28, 34));

            //Yet another path
            GraphicsPath rightCheek = new GraphicsPath();
            rightCheek.MoveTo(96.503, 270.34);
            rightCheek.CubicBezierTo(95.499, 275.168, 98.164, 279.806, 102.456, 280.698);
            rightCheek.CubicBezierTo(106.748, 281.591, 111.041, 278.4, 112.045, 273.571);
            rightCheek.CubicBezierTo(113.049, 268.743, 110.384, 264.105, 106.092, 263.213);
            rightCheek.CubicBezierTo(101.8, 262.32, 97.507, 265.511, 96.503, 270.34);
            rightCheek.Close();
            gpr.FillPath(rightCheek, Colour.FromRgb(235, 28, 34));


            //Inline path syntax - mouth
            gpr.StrokePath(new GraphicsPath().MoveTo(84.683, 265.03)
            .CubicBezierTo(82.771, 272.435, 74.1, 267.158, 69.084, 267.158)
            .CubicBezierTo(64.069, 267.158, 55.398, 272.435, 53.486, 265.03)
            .MoveTo(58.182, 268.972)
            .CubicBezierTo(58.182, 268.972, 60.486, 288.088, 69.361, 288.088)
            .CubicBezierTo(78.235, 288.088, 81.059, 268.842, 81.059, 268.842)
            .MoveTo(60.31, 278.145)
            .CubicBezierTo(66.57, 274.868, 72.673, 275.222, 78.669, 278.114),
            Colour.FromRgb(0, 0, 0), lineWidth: 2.5, lineCap: LineCaps.Round);

            //Again, inline path syntax - left ear
            gpr.FillPath(new GraphicsPath().MoveTo(20.118, 192.906).CubicBezierTo(14.005, 188.531, 7.872, 185.824, 2.664, 186.221).CubicBezierTo(-1.235, 186.519, 1.041, 202.568, 16.932, 222.774).CubicBezierTo(15.793, 212.993, 14.194, 202.11, 20.118, 192.906).Close(), Colour.FromRgb(0, 0, 0));
            //Right ear
            gpr.FillPath(new GraphicsPath().MoveTo(120.095, 224.029).CubicBezierTo(137.116, 203.05, 139.479, 186.526, 135.484, 186.221).CubicBezierTo(130.041, 185.806, 123.77, 188.626, 117.569, 193.202).CubicBezierTo(122.356, 200.373, 120.978, 212.396, 120.095, 224.029).Close(), Colour.FromRgb(0, 0, 0));
            //Nose
            gpr.FillPath(new GraphicsPath().MoveTo(71.3, 261.868).CubicBezierTo(71.3, 262.361, 70.308, 262.761, 69.084, 262.761).CubicBezierTo(67.861, 262.761, 66.869, 262.361, 66.869, 261.868).CubicBezierTo(66.869, 261.375, 67.861, 260.975, 69.085, 260.975).CubicBezierTo(70.308, 260.975, 71.3, 261.375, 71.3, 261.868).Close(), Colour.FromRgb(0, 0, 0));

            GraphicsPath leftEye = new GraphicsPath();
            //Circle
            leftEye.Arc(44.875, 252.274, 8.029, 0, 2 * Math.PI);
            gpr.FillPath(leftEye, Colour.FromRgb(0, 0, 0));
            gpr.FillPath(new GraphicsPath().Arc(45.757, 249.371, 3.567, 0, 2 * Math.PI), Colour.FromRgb(255, 255, 255));

            //Right eye
            gpr.FillPath(new GraphicsPath().Arc(93.294, 252.274, 8.029, 0, 2 * Math.PI), Colour.FromRgb(0, 0, 0));
            gpr.FillPath(new GraphicsPath().Arc(92.412, 249.371, 3.567, 0, 2 * Math.PI), Colour.FromRgb(255, 255, 255));

            //Reuse the first path for a stroke           
            gpr.StrokePath(face, Colour.FromRgb(0, 0, 0), lineWidth: 2.5);

            //Restore the previous graphics state
            gpr.Restore();



            //Rotation sample

            //Save graphics state
            gpr.Save();

            string[] angles = new string[] { "0", "π/6", "π/3", "π/2", "2π/3", "5π/6", "π", "7π/6", "4π/3", "3π/2", "5π/3", "11π/6" };

            for (int i = 0; i < 12; i++)
            {
                gpr.StrokePath(new GraphicsPath().MoveTo(3400, 1150).LineTo(3550, 1150), Colour.FromRgb(0, 0, 0), lineWidth: 5);
                gpr.FillText(3600, 1150, angles[i], new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesBold), 60), Colour.FromRgb(0, 0, 0), textBaseline: TextBaselines.Middle);

                //Rotate around a point
                gpr.RotateAt(Math.PI / 6, new VectSharp.Point(3400, 1150));
            }

            //Restore graphics state
            gpr.Restore();


            //Transparency sample
            
            //Checkerboard
            for (int x = 3150; x < 3650; x += 100)
            {
                for (int y = 1825; y < 2475; y += 100)
                {
                    gpr.FillRectangle(x, y, 50, 50, Colour.FromRgb(220, 220, 220));
                    gpr.FillRectangle(x + 50, y + 50, 50, 50, Colour.FromRgb(220, 220, 220));
                }
            }

            //Transparent text
            gpr.FillText(3400 - gpr.MeasureText("Transparent", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBoldOblique), 80)).Width / 2, 1925, "Transparent", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBoldOblique), 80), Colour.FromRgba(255, 127, 39, 85), textBaseline: TextBaselines.Bottom);

            //Transparent stroke
            gpr.StrokePath(new GraphicsPath().MoveTo(3175, 1950).LineTo(3625, 1950), Colour.FromRgba(163, 73, 164, 128), lineWidth: 30);

            //Transparent fill
            gpr.FillPath(new GraphicsPath().Arc(3325, 2300, 150, 0, 2 * Math.PI), Colour.FromRgba((byte)234, (byte)28, (byte)36, (byte)85), tag: "red");
            gpr.FillPath(new GraphicsPath().Arc(3400, 2150, 150, 0, 2 * Math.PI), Colour.FromRgba((byte)34, (byte)177, (byte)76, (byte)85));
            gpr.FillPath(new GraphicsPath().Arc(3475, 2300, 150, 0, 2 * Math.PI), Colour.FromRgba((byte)0, (byte)162, (byte)232, (byte)85));


            //Text fill vs stroke
            gpr.FillText(1925, 1450, "Fill", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBoldOblique), 150), Colour.FromRgb(203, 245, 216));
            gpr.StrokeText(2325, 1450, "Stroke", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBoldOblique), 150), Colour.FromRgb(34, 177, 76), lineWidth: 6, lineJoin: LineJoins.Round);

            //Text can be also added to a graphics path.
            GraphicsPath textPath = new GraphicsPath().AddText(1925, 1650, "Fill & stroke", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBoldOblique), 150));
            gpr.FillPath(textPath, Colour.FromRgb(203, 245, 216));
            gpr.StrokePath(textPath, Colour.FromRgb(34, 177, 76), lineWidth: 6, lineJoin: LineJoins.Round);


            //Interactivity sample (Avalonia only)
            //Fill rectangle
            gpr.FillRectangle(3100, 1550, 600, 200, Colour.FromRgb(220, 220, 220), tag: "ClickMeRectangle");

            //Stroke rectangle (with dashed line)
            gpr.StrokeRectangle(3100, 1550, 600, 200, Colour.FromRgb(80, 80, 80), lineWidth: 20, lineDash: new LineDash(60, 60, 0));

            //Text
            gpr.FillText(3400 - gpr.MeasureText("Click me!", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 80)).Width / 2, 1650, "Click me!", new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 80), Colour.FromRgb(0, 0, 0), textBaseline: TextBaselines.Middle, tag: "ClickMeText");

            //Dictionary associating each tag to the action to perform on the object (Avalonia only)
            Dictionary<string, Delegate> taggedActions = new Dictionary<string, Delegate>()
            {
                {"ClickMeText", new Action<TextBlock>(block => { block.IsHitTestVisible = false; }) },
                {"ClickMeRectangle", new Action<Path>(path =>
                {
                    path.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                    path.PointerEnter += (sender, e) =>
                    {
                        path.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(180, 180, 180));
                    };

                    path.PointerLeave += (sender, e) =>
                    {
                        path.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(220, 220, 220));
                    };

                    path.PointerPressed += (sender, e) =>
                    {
                        path.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(128, 128, 128));
                    };

                    path.PointerReleased += async (sender, e) =>
                    {
                        path.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(180, 180, 180));

                        Window win = new Window() { Width = 300, Height = 150, Title = "Thanks for clicking!", WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = new TextBlock(){ Text = "Hello world!", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 40 } };
                        await win.ShowDialog(this);
                    };

                }) },
            };

            //Save the image as a PNG file
            doc.Pages.Last().SaveAsPNG(@"Sample.png");

            //Transfer the page onto an Avalonia Canvas object
            this.FindControl<Viewbox>("mainViewBox").Child = doc.Pages.Last().PaintToCanvas(taggedActions);

            //Add another page to the document (the size of each page can be different)
            doc.Pages.Add(new Page(480, 100));

            //Write some text on the second page
            doc.Pages.Last().Graphics.FillText(20, 50, "This is the second page!", new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 40), Colour.FromRgb(0, 0, 0), textBaseline: TextBaselines.Middle);

            //Create a PDF document
            doc.SaveAsPDF(@"Sample.pdf");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
