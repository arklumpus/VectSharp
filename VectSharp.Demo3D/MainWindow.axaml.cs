using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using VectSharp.ThreeD;
using VectSharp.Canvas;
using VectSharp.SVG;
using Avalonia.Input;
using System.Linq;
using System.Collections.Generic;

namespace VectSharp.Demo3D
{
    public class MainWindow : Window
    {
        ThreeDGraphics gpr;
        PerspectiveCamera camera;

        static Colour GetScaledColour(Colour col, double intensity)
        {
            intensity = Math.Max(Math.Min(intensity, 1), 0);

            (double L, double a, double b) = col.ToLab();

            Colour labBlack = Colour.FromLab(0, a, b);
            Colour labWhite = Colour.FromLab(1, a, b);

            (double h, double s, double l) labBlackHSL = labBlack.ToHSL();
            (double h, double s, double l) labWhiteHSL = labWhite.ToHSL();

            double totalLength = 1 + labBlackHSL.l + (1 - labWhiteHSL.l);


            /*double param = ((labBlackHSL.l + L) / totalLength - 0.5) / (Math.Pow(2, -exponent) - 0.5);
            double pos = (param * intensity * intensity + (1 - param) * intensity) * totalLength;*/

            double param = Math.Log((labBlackHSL.l + L) / totalLength) / Math.Log(0.5);

            double pos = Math.Pow(intensity, param) * totalLength;

            if (pos <= labBlackHSL.l)
            {
                return Colour.FromHSL(labBlackHSL.h, labBlackHSL.s, pos);
            }
            else if (pos >= 1 + labBlackHSL.l)
            {
                return Colour.FromHSL(labWhiteHSL.h, labWhiteHSL.s, labWhiteHSL.l + pos - 1 - labBlackHSL.l);
            }
            else
            {
                return Colour.FromLab(pos - labBlackHSL.l, a, b);
            }
        }

        static Colour GetScaledColourHSL(Colour col, double intensity)
        {
            intensity = Math.Max(Math.Min(intensity, 1), 0);

            (double h, double s, double l) = col.ToHSL();

            double param = Math.Log(l) / Math.Log(0.5);

            double pos = Math.Pow(intensity, param);

            return Colour.FromHSL(h, s, pos);
        }

        double maxArea = double.NaN;
        public MainWindow()
        {
            InitializeComponent();

            /*Colour[] colours = new Colour[] { Colours.Fuchsia.WithAlpha(0.25), Colours.Blue.WithAlpha(0.25), Colours.Red.WithAlpha(0.25), Colours.Green.WithAlpha(0.25), Colours.Orange.WithAlpha(0.25) };


            Page pag = new Page(100, 100);
            */

            Graphics image = new Graphics();

            image.StrokeRectangle(5, 5, 90, 90, Colours.Black, 0.5);
            image.StrokePath(new GraphicsPath().MoveTo(5, 35).LineTo(95, 35).MoveTo(5, 65).LineTo(95, 65).MoveTo(35, 5).LineTo(35, 95).MoveTo(65, 5).LineTo(65, 95), Colours.Black, 0.5);

            //new Point(100, 0), new Point(0, 100), new Point(0, 0)

            image.FillPath(new GraphicsPath().Arc(100, 0, 3, 0, 2 * Math.PI).Close().Linearise(0.5), Colours.Green);
            image.FillPath(new GraphicsPath().Arc(0, 100, 3, 0, 2 * Math.PI).Close().Linearise(0.5), Colours.Red);
            image.FillPath(new GraphicsPath().Arc(0, 0, 3, 0, 2 * Math.PI).Close().Linearise(0.5), Colours.Blue);

            Font fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 80);
            string txt = "e";

            image.Save();

            image.Translate(50, 50);
            image.Rotate(Math.PI / 2);

            image.FillText(-fnt.MeasureText(txt).Width * 0.5, 0, "e", fnt, Colours.Blue, TextBaselines.Middle);

            image.Restore();

            image.StrokePath(new GraphicsPath().Arc(50, 50, 40, 0, 2 * Math.PI), Colours.Red, 0.5);

            double fX = -85;
            double tX = 95 - fX;
            double tY = 95;

            //image = image.Transform(pt => new Point(pt.X, tY + (pt.Y - tY) * (pt.X - fX) / tX), 1);

            /*Graphics gpr = pag.Graphics;
            gpr.DrawGraphics(0, 0, image);*/

            /*

            GraphicsPath path1 = new GraphicsPath();
            for (int i = 0; i < 5; i++)
            {
                path1.LineTo(50 + 30 * Math.Cos(2 * Math.PI / 9 * i), 50 + 30 * Math.Sin(2 * Math.PI / 9 * i));
            }
            path1.Arc(50, 50, 30, 2 * Math.PI / 9 * 4, 2 * Math.PI);
            path1.Close();
            gpr.StrokePath(path1, Colour.FromRgb(220, 220, 220));

            int index = 0;
            foreach (GraphicsPath pth in path1.Triangulate(5, true))
            {
                gpr.FillPath(pth, colours[index++ % colours.Length]);
            }



            GraphicsPath path2 = new GraphicsPath();
            path2.MoveTo(120, 19);
            path2.LineTo(160, 20);
            path2.LineTo(150, 60);
            path2.LineTo(180, 80);
            path2.LineTo(120, 70);
            path2.LineTo(140, 60);
            path2.LineTo(110, 50);
            path2.LineTo(110, 40);
            path2.LineTo(130, 45);
            path2.Close();
            gpr.StrokePath(path2, Colour.FromRgb(220, 220, 220));
            index = 0;
            foreach (GraphicsPath pth in path2.Triangulate(5, true))
            {
                gpr.FillPath(pth, colours[index++ % colours.Length]);
            }*/



            /*List<GraphicsPath> letters = new List<GraphicsPath>();

            //GraphicsPath path3 = new GraphicsPath();
            Font fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 55);
            string text = "VectSharp";
            double x = 20;

            Font.DetailedFontMetrics stringMetrics = fnt.MeasureTextAdvanced(text);

            for (int i = 0; i < text.Length; i++)
            {
                string str = text[i].ToString();
                Font.DetailedFontMetrics metrics = fnt.MeasureTextAdvanced(str);

                if (i == 0)
                {
                    letters.Add(new GraphicsPath().AddText(x - stringMetrics.LeftSideBearing + metrics.LeftSideBearing, 20 + stringMetrics.Top, str, fnt, TextBaselines.Baseline));
                }
                else
                {
                    letters.Add(new GraphicsPath().AddText(x - stringMetrics.LeftSideBearing + metrics.LeftSideBearing, 20 + stringMetrics.Top, str, fnt, TextBaselines.Baseline));
                }

                x += fnt.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(str[0]) * fnt.FontSize / 1000;
            }

            //gpr.FillText(20, 20, text, fnt, Colours.Red);
            //gpr.FillPath(new GraphicsPath().AddText(20, 20, text, fnt), Colours.Green.WithAlpha(0.25));




            int index = 0;*/
            /*foreach (GraphicsPath letter in letters)
            {
                //gpr.StrokePath(letter.Linearise(15), Colours.Black);

                foreach (GraphicsPath pth in letter.Triangulate(5, true))
                {
                    gpr.FillPath(pth, colours[index++ % colours.Length]);

                    gpr.StrokePath(pth, Colours.Red, 0.1);
                }
            }*/


            /*foreach (GraphicsPath pth in new GraphicsPath().AddText(20, 20, text, fnt).Triangulate(5, true))
            {
                gpr.FillPath(pth, colours[index++ % colours.Length]);

                gpr.StrokePath(pth, Colours.Red, 0.1);
            }*/






            /*Point p1 = new Point(205.31, 25.49);
            Point p2 = new Point(220.7, 26.93);
            Point p3 = new Point(232.78, 25.85);*/



            /*GraphicsPath anglePath = new GraphicsPath();
            anglePath.MoveTo(p1);
            anglePath.LineTo(p2);
            anglePath.LineTo(p3);

            gpr.StrokePath(anglePath, Colours.Green);*/

            /*double angle = Math.Atan2(p1.Y - p2.Y, p1.X - p2.X) - Math.Atan2(p3.Y - p2.Y, p3.X - p2.X);
          double area = (p1.X - p2.X) * (p3.Y - p2.Y) - (p1.Y - p2.Y) * (p3.X - p2.X);

          if (angle < -Math.PI)
          {
              angle += 2 * Math.PI;
          }

          gpr.FillText(0, 10, angle.ToString(), new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 5), Colours.Orange);
          gpr.FillText(0, 0, area.ToString(), new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 5), Colours.Orange);*/



            /* Point o = new Point(40, 40);
             Point p1 = new Point(42, 60);
             Point p2 = new Point(30, 50);
             Point p3 = new Point(20, 31);



             gpr.StrokePath(new GraphicsPath().MoveTo(p1).LineTo(o), Colours.Blue);
             gpr.StrokePath(new GraphicsPath().MoveTo(p2).LineTo(o), Colours.Green);
             gpr.StrokePath(new GraphicsPath().MoveTo(p3).LineTo(o), Colours.Red);


             double angle1 = Math.Atan2(p1.Y - o.Y, p1.X - o.X);
             if (angle1 < 0)
             {
                 angle1 += 2 * Math.PI;
             }

             double angle2 = Math.Atan2(p2.Y - o.Y, p2.X - o.X);
             if (angle2 < 0)
             {
                 angle2 += 2 * Math.PI;
             }

             double angle3 = Math.Atan2(p3.Y - o.Y, p3.X - o.X);
             if (angle3 < 0)
             {
                 angle3 += 2 * Math.PI;
             }

             angle2 = angle2 - angle1;
             angle3 = angle3 - angle1;

             if (angle2 < 0)
             {
                 angle2 += 2 * Math.PI;
             }

             if (angle3 < 0)
             {
                 angle3 += 2 * Math.PI;
             }


             //gpr.FillText(0, 0, angle1.ToString(), new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 5), Colours.Orange);
             gpr.FillText(0, 6, angle2.ToString(), new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 5), Colours.Green);
             gpr.FillText(0, 12, angle3.ToString(), new Font(new FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 5), Colours.Red);

             */

            /* Avalonia.Controls.Canvas can = pag.PaintToCanvas(false);
             this.FindControl<Viewbox>("ContentBox").Child = can;*/


            /*Random rnd = new Random();

            NormalizedVector3D vect = new NormalizedVector3D(0, 0, -rnd.NextDouble());

            double[,] rotMat = Matrix3D.RotationToAlignWithZ(vect);

            Point3D originalPoint = (Point3D)(vect * 3);

            Point3D rotatedPoint = rotMat * originalPoint;*/

            gpr = new ThreeDGraphics();




            //List<IElement3D> triangles = gpr.AddCube(new Point3D(0, 0, 0), 1, 1, 1, new IMaterial[] { new PhongMaterial(Colours.CornflowerBlue) { OverFill = 0.001, DiffuseReflectionCoefficient = 1, SpecularReflectionCoefficient = 1, SpecularShininess = 1 } });

            //SimpleTexturedMaterial texture = new SimpleTexturedMaterial(image, new Point(65, 35), new Point(35, 65), new Point(35, 35)) { OverFill = 0.002 };


            gpr.AddRectangle(new Point3D(-2, -0.5, -0.5), new Point3D(-2, 0.5, -0.5), new Point3D(-2, 0.5, 0.5), new Point3D(-2, -0.5, 0.5), new IMaterial[] { new PhongMaterial(Colours.Coral) });




            /*((Triangle)triangles[9]).Fill.Add(



                 );*/

            //TextureFactory.Apply(texture, (Triangle)triangles[9], triangles);

            //TextureFactory.Apply(image.Linearise(5), new Point3D(-2, -1.5, 1.25), new NormalizedVector3D(0, 0, -1), new NormalizedVector3D(0, 1, 0), 30, 30, triangles, 0.002);

            List<IElement3D> triangles = gpr.AddSphere(new Point3D(0, 0, 0), 0.5, 8, new IMaterial[] { new PhongMaterial(Colours.Coral) { OverFill = 0.001 } });


            /*int[] sortedTriangles  = TextureFactory.Apply(image.Linearise(5), new Point3D(-2, -1.5, 1.25), new NormalizedVector3D(0, 0, -1), new NormalizedVector3D(0, 1, 0), 30, 30, triangles, 0.002);

            Page pag = new Page(100, 100);
            pag.Graphics.DrawGraphics(0, 0, image);

            Font font2 = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 4);
            Font font3 = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 6);

            for (int i = 0; i < 5; i++)
            {
                if (((Triangle)triangles[sortedTriangles[i]]).Fill.Count > 1)
                {
                    SimpleTexturedMaterial text = (SimpleTexturedMaterial)((Triangle)triangles[sortedTriangles[i]]).Fill[1];

                    pag.Graphics.StrokePath(new GraphicsPath().MoveTo(text.Point1).LineTo(text.Point2).LineTo(text.Point3).Close(), Colours.Fuchsia, lineWidth: 0.5, lineJoin: LineJoins.Round);

                    /*pag.Graphics.FillText(text.Point1, triangles[i][0].X.ToString() + ";" + triangles[i][0].Y.ToString() + ";" + triangles[i][0].Z.ToString(), font2, Colours.Green);
                    pag.Graphics.FillText(text.Point2, triangles[i][1].X.ToString() + ";" + triangles[i][1].Y.ToString() + ";" + triangles[i][1].Z.ToString(), font2, Colours.Green);
                    pag.Graphics.FillText(text.Point3, triangles[i][2].X.ToString() + ";" + triangles[i][2].Y.ToString() + ";" + triangles[i][2].Z.ToString(), font2, Colours.Green);
                }
            }

            /*for (int i = 0; i < triangles.Count; i++)
            {
                if (((Triangle)triangles[i]).Fill.Count > 1)
                {
                    SimpleTexturedMaterial text = (SimpleTexturedMaterial)((Triangle)triangles[i]).Fill[1];

                    Point meanPoint = new Point((text.Point1.X + text.Point2.X + text.Point3.X) / 3 - font3.MeasureText(i.ToString()).Width * 0.5, (text.Point1.Y + text.Point2.Y + text.Point3.Y) / 3);
                    pag.Graphics.FillText(meanPoint, i.ToString(), font2, Colours.Purple, TextBaselines.Middle);
                }
            }

            pag.Crop(new Point(-50, -50), new Size(200, 200));


            Avalonia.Controls.Canvas can = pag.PaintToCanvas(false);
            this.FindControl<Viewbox>("ContentBox").Child = can;
            /**/
            /*gpr.AddPoint(triangles[9][0], Colours.Green, zIndex: 1);
            gpr.AddPoint(triangles[9][1], Colours.Red, zIndex: 1);
            gpr.AddPoint(triangles[9][2], Colours.Blue, zIndex: 1);*/


            //GraphicsPath path = new GraphicsPath().MoveTo(-1, -1).LineTo(-1, 1).LineTo(1, 1).LineTo(1, -1).Close();
            /*string text = "VectSharp";
            Font fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 2.5);
            GraphicsPath path = new GraphicsPath().AddText(-fnt.MeasureText(text).Width * 0.5, 0, text, fnt, TextBaselines.Middle);
            */
            //gpr.AddPolygon(path, 0.1, new Point3D(0, 0, 0), new NormalizedVector3D(0, 0, -1), new NormalizedVector3D(0, 1, 0), new IMaterial[] { new PhongMaterial(Colours.CornflowerBlue) { OverFill = 0.001, DiffuseReflectionCoefficient = 1, SpecularReflectionCoefficient = 1, SpecularShininess = 1 } });

            //path = new GraphicsPath().Arc(0, 0, 1, 0, 2 * Math.PI).Close();

            //gpr.AddPrism(path, 0.1, new Point3D(0.25, 0, 0), new Point3D(-0.25, 0, 0), new NormalizedVector3D(0, 0, -1), new NormalizedVector3D(0, 1, 0), new IMaterial[] { new PhongMaterial(Colours.CornflowerBlue) { OverFill = 0.00075, DiffuseReflectionCoefficient = 1, SpecularReflectionCoefficient = 1, SpecularShininess = 1 } });

            //gpr.AddPrism(path, 1, new Point3D(0.25, 0, 0), new Point3D(-0.25, 0, 0), new NormalizedVector3D(0, 0, -1), new NormalizedVector3D(0, 1, 0), new IMaterial[] { new ColourMaterial(Colours.CornflowerBlue.WithAlpha(0.5)) });

            //gpr.AddCube(new Point3D(2, 0, 0), 1, 1, 1, new IMaterial[] { new PhongMaterial(Colours.Coral) { OverFill = 0.001 } });
            //gpr.AddSphere(new Point3D(1, 0, 0), 0.5, 32, new IMaterial[] { new PhongMaterial(Colours.CornflowerBlue) { OverFill = 0.001, DiffuseReflectionCoefficient = 0 } });
            //gpr.AddSphere(new Point3D(-1, 0, 0), 0.5, 32, new IMaterial[] { new PhongMaterial(Colours.CornflowerBlue) { OverFill = 0.001, SpecularReflectionCoefficient = 0 } });

            //gpr.AddSphere(new Point3D(0, 0, 1), 0.5, 32, new IMaterial[] { new PhongMaterial(Colours.CornflowerBlue) { OverFill = 0.001, DiffuseReflectionCoefficient = 0.5, SpecularShininess = 2 } });

            //gpr.AddSphere(new Point3D(0, 0, -1), 0.5, 32, new IMaterial[] { new PhongMaterial(Colours.CornflowerBlue) { OverFill = 0.001, DiffuseReflectionCoefficient = 1, SpecularReflectionCoefficient = 1.5, SpecularShininess = 2 } });


            camera = new PerspectiveCamera(new Point3D(-5, 0, 0), new NormalizedVector3D(5, 0, 0), 2, new Size(2, 2), 100);
            //camera = new PerspectiveCamera(new Point3D(-1.067, -0.956, -5.286), new NormalizedVector3D(1.067, 0.956, 5.286), 2, new Size(2, 2), 100);

            ILightSource[] lights = new ILightSource[]
            {
                 new AmbientLightSource(0.1),
                 new ParallelLightSource(0.5, new NormalizedVector3D(5, 3, 2))
            };


            Page pag = gpr.Render(camera, lights, maxArea);
            Avalonia.Controls.Canvas can = pag.PaintToCanvas();
            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Add(can);
            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width = can.Width;
            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height = can.Height;

            //pag.SaveAsSVG("test.svg");

            bool isPressed = false;
            PointerPoint pointerPress = null;
            double lastTheta = 0;
            double lastPhi = 0;
            double lastX = 0;
            double lastY = 0;

            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").PointerPressed += (s, e) =>
            {
                 //camera.Orbit(0, -Math.PI / 36);
                 //camera.Orbit(-Math.PI / 36, 0);
                 //camera.Orbit(0, 0);
                 //this.FindControl<Viewbox>("ContentBox").Child = gpr.Render(camera).PaintToCanvas();

                 pointerPress = e.GetCurrentPoint(this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas"));
                lastTheta = 0;
                lastPhi = 0;
                lastX = 0;
                lastY = 0;
                isPressed = true;
            };

            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").PointerReleased += (s, e) =>
            {
                isPressed = false;
            };

            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").PointerMoved += (s, e) =>
            {
                if (isPressed)
                {
                    PointerPoint point = e.GetCurrentPoint(this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas"));



                    if (pointerPress.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
                    {
                        double dx = (point.Position.X - pointerPress.Position.X) / this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width;
                        double dy = (point.Position.Y - pointerPress.Position.Y) / this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height;
                        if (dx >= -1 && dx <= 1 && dy >= -1 && dy <= 1)
                        {
                            double theta = Math.Asin(dx) * 2;
                            double phi = Math.Asin(dy) * 2;

                            camera.Orbit(theta - lastTheta, phi - lastPhi);
                            Avalonia.Controls.Canvas can = gpr.Render(camera, lights, maxArea).PaintToCanvas();
                            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Clear();
                            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Add(can);
                            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width = can.Width;
                            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height = can.Height;

                            lastTheta = theta;
                            lastPhi = phi;
                        }
                    }
                    else if (pointerPress.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
                    {
                        double dx = point.Position.X - pointerPress.Position.X;
                        double dy = point.Position.Y - pointerPress.Position.Y;

                        camera.Pan(dx - lastX, dy - lastY);
                        Avalonia.Controls.Canvas can = gpr.Render(camera, lights, maxArea).PaintToCanvas();
                        this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Clear();
                        this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Add(can);
                        this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width = can.Width;
                        this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height = can.Height;

                        lastX = dx;
                        lastY = dy;
                    }
                }
            };


            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").PointerWheelChanged += (s, e) =>
            {
                camera.Zoom(e.Delta.Y);
                Avalonia.Controls.Canvas can = gpr.Render(camera, lights, maxArea).PaintToCanvas();
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Clear();
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Add(can);
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width = can.Width;
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height = can.Height;
            };/**/
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
