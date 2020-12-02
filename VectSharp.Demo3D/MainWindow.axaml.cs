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
using System.Diagnostics;
using VectSharp.PDF;
using System.Collections;

namespace VectSharp.Demo3D
{
    public class MainWindow : Window
    {
        Scene gpr;
        PerspectiveCamera camera;
        //OrthographicCamera camera;

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

        double maxArea = 10;
        public MainWindow()
        {
            InitializeComponent();

            gpr = new Scene();

            Colour wallColour = Colour.FromRgb(180, 180, 180);

            List<Element3D> wall1 = ObjectFactory.CreateRectangle(new Point3D(-300, -600, -300), new Point3D(-300, -600, 300), new Point3D(-300, 0, 300), new Point3D(-300, 0, -300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } });
            List<Element3D> wall2 = ObjectFactory.CreateRectangle(new Point3D(-300, -600, 300), new Point3D(300, -600, 300), new Point3D(300, 0, 300), new Point3D(-300, 0, 300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } });
            List<Element3D> wall3 = ObjectFactory.CreateRectangle(new Point3D(300, 0, -300), new Point3D(300, 0, 300), new Point3D(300, -600, 300), new Point3D(300, -600, -300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } });
            List<Element3D> wall4 = ObjectFactory.CreateRectangle(new Point3D(-300, 0, -300), new Point3D(300, 0, -300), new Point3D(300, -600, -300), new Point3D(-300, -600, -300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } });

            List<Element3D> floor = ObjectFactory.CreateRectangle(new Point3D(-300, 0, 300), new Point3D(300, 0, 300), new Point3D(300, 0, -300), new Point3D(-300, 0, -300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } });
            List<Element3D> ceiling = ObjectFactory.CreateRectangle(new Point3D(-300, -600, -300), new Point3D(300, -600, -300), new Point3D(300, -600, 300), new Point3D(-300, -600, 300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } });

            wall2 = new List<Element3D>();

            wall2.AddRange(ObjectFactory.CreateRectangle(new Point3D(-300, -600, 300), new Point3D(300, -600, 300), new Point3D(300, -450, 300), new Point3D(-300, -450, 300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } }));
            wall2.AddRange(ObjectFactory.CreateRectangle(new Point3D(-300, -150, 300), new Point3D(300, -150, 300), new Point3D(300, 0, 300), new Point3D(-300, 0, 300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } }));
            wall2.AddRange(ObjectFactory.CreateRectangle(new Point3D(-300, -450, 300), new Point3D(-150, -450, 300), new Point3D(-150, -150, 300), new Point3D(-300, -150, 300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } }));
            wall2.AddRange(ObjectFactory.CreateRectangle(new Point3D(150, -450, 300), new Point3D(300, -450, 300), new Point3D(300, -150, 300), new Point3D(150, -150, 300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } }));

            wall2.AddRange(ObjectFactory.CreateRectangle(new Point3D(-25, -450, 300), new Point3D(25, -450, 300), new Point3D(25, -150, 300), new Point3D(-25, -150, 300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } }));
            wall2.AddRange(ObjectFactory.CreateRectangle(new Point3D(-150, -325, 300), new Point3D(-25, -325, 300), new Point3D(-25, -275, 300), new Point3D(-150, -275, 300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } }));
            wall2.AddRange(ObjectFactory.CreateRectangle(new Point3D(25, -325, 300), new Point3D(150, -325, 300), new Point3D(150, -275, 300), new Point3D(25, -275, 300), new IMaterial[] { new PhongMaterial(wallColour) { SpecularReflectionCoefficient = 0 } }));

            foreach (Triangle3DElement triangle in wall1)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
            }

            foreach (Triangle3DElement triangle in wall2)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
            }

            foreach (Triangle3DElement triangle in wall3)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
            }

            foreach (Triangle3DElement triangle in wall4)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
            }

            foreach (Triangle3DElement triangle in ceiling)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
            }

            foreach (Triangle3DElement triangle in floor)
            {
                triangle.ReceivesShadow = true;
            }


            int zIndex1 = 1;

            List<Element3D> tableTop = ObjectFactory.CreateCuboid(new Point3D(225, -150, 0), 150, 20, 250, new IMaterial[] { new PhongMaterial(Colours.BurlyWood) });

            gpr.AddRange(tableTop);
            foreach (Triangle3DElement triangle in tableTop)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
                triangle.ZIndex = zIndex1;
            }

            List<Element3D> tableLeg1 = ObjectFactory.CreateCuboid(new Point3D(170, -70, 105), 20, 140, 20, new IMaterial[] { new PhongMaterial(Colours.BurlyWood) });
            gpr.AddRange(tableLeg1);
            foreach (Triangle3DElement triangle in tableLeg1)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
            }


            List<Element3D> tableLeg2 = ObjectFactory.CreateCuboid(new Point3D(280, -70, 105), 20, 140, 20, new IMaterial[] { new PhongMaterial(Colours.BurlyWood) });
            gpr.AddRange(tableLeg2);
            foreach (Triangle3DElement triangle in tableLeg2)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
            }


            List<Element3D> tableLeg3 = ObjectFactory.CreateCuboid(new Point3D(170, -70, -105), 20, 140, 20, new IMaterial[] { new PhongMaterial(Colours.BurlyWood) });
            gpr.AddRange(tableLeg3);
            foreach (Triangle3DElement triangle in tableLeg3)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
            }


            List<Element3D> tableLeg4 = ObjectFactory.CreateCuboid(new Point3D(280, -70, -105), 20, 140, 20, new IMaterial[] { new PhongMaterial(Colours.BurlyWood) });
            gpr.AddRange(tableLeg4);
            foreach (Triangle3DElement triangle in tableLeg4)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
            }


            List<Element3D> cube = ObjectFactory.CreateCuboid(new Point3D(225, -170, 0), 20, 20, 20, new IMaterial[] { new PhongMaterial(Colour.FromRgb(0, 162, 232)) });
            gpr.AddRange(cube);
            foreach (Triangle3DElement triangle in cube)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
                triangle.ZIndex = zIndex1;
            }

            List<Element3D> sphere = ObjectFactory.CreateSphere(new Point3D(245, -175, -40), 15, 16, new IMaterial[] { new PhongMaterial(Colour.FromRgb(34, 177, 76)) });
            sphere = ((Transform3D.Translate(245, -175, -40) * Transform3D.RotationAlongAxis(new NormalizedVector3D(0, -1, 0), Math.PI / 2.37) * Transform3D.Translate(-245, 175, 40)) * sphere).ToList();

            gpr.AddRange(sphere);
            foreach (Triangle3DElement triangle in sphere)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
                triangle.ZIndex = zIndex1;
            }

            List<Element3D> tetrahedron = ObjectFactory.CreateTetrahedron(new Point3D(215, -165, 40), 15, new IMaterial[] { new PhongMaterial(Colour.FromRgb(255, 127, 39)) });
            gpr.AddRange(tetrahedron);
            foreach (Triangle3DElement triangle in tetrahedron)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
                triangle.ZIndex = zIndex1;
            }


            gpr.AddRange(ObjectFactory.CreateSphere(new Point3D(0, -500, -100), 7.5, 8, new IMaterial[] { new ColourMaterial(Colour.FromRgb(255, 255, 128)) }));
            gpr.AddElement(new Line3DElement(new Point3D(0, -500, -100), new Point3D(0, -600, -100)) { Colour = Colours.Black, Thickness = 0.5 });


            Transform3D lampTransform = Transform3D.Translate(225, -250, 125) * Transform3D.RotationToAlignAWithB(new NormalizedVector3D(0, 1, 0), new NormalizedVector3D(0, 1, -1)) * Transform3D.RotationAlongAxis(new NormalizedVector3D(0, -1, 0), -Math.PI / 8) * Transform3D.Translate(0, -9, 0) * Transform3D.Translate(-225, 250, -125);
            List<Element3D> lampShade = (lampTransform * ObjectFactory.CreateTetrahedron(new Point3D(225, -250, 125), 30, new IMaterial[] { new PhongMaterial(Colour.FromRgb(220, 220, 220)) })).ToList();

            for (int i = 0; i < lampShade.Count; i++)
            {
                if (i != 3)
                {
                    ((Triangle3DElement)lampShade[i]).CastsShadow = true;
                }

                ((Triangle3DElement)lampShade[i]).ReceivesShadow = true;
                lampShade[i].ZIndex = zIndex1;
            }

            gpr.AddRange(lampShade);

            gpr.AddRange(ObjectFactory.CreateSphere(new Point3D(225, -250, 125), 6, 4, new IMaterial[] { new ColourMaterial(Colour.FromRgb(255, 255, 128)) }, zIndex: zIndex1));

            List<Element3D> lampBase = ObjectFactory.CreateCuboid(new Point3D(225, -170, 110), 50, 20, 20, new IMaterial[] { new PhongMaterial(Colour.FromRgb(220, 220, 220)) });
            gpr.AddRange(lampBase);

            foreach (Triangle3DElement triangle in lampBase)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
                triangle.ZIndex = zIndex1;
            }

            //gpr.AddElement(new Line3DElement(new Point3D(225, -258, 136), new Point3D(280, -258, 136)) { Colour = Colour.FromRgb(220, 220, 220), Thickness = 1, LineCap = LineCaps.Round, ZIndex = zIndex1 });

            List<Element3D> lampArm1 = ObjectFactory.CreateCuboid(new Point3D(252.5, -258, 136), 55, 5, 5, new IMaterial[] { new PhongMaterial(Colour.FromRgb(220, 220, 220)) });
            gpr.AddRange(lampArm1);

            foreach (Triangle3DElement triangle in lampArm1)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
                triangle.ZIndex = zIndex1;
            }


            //gpr.AddElement(new Line3DElement(new Point3D(280, -258, 136), new Point3D(280, -170, 110)) { Colour = Colour.FromRgb(220, 220, 220), Thickness = 1, LineCap = LineCaps.Round, ZIndex = zIndex1 });

            List<Element3D> lampArm2 = ObjectFactory.CreateCuboid(new Point3D(280, -214, 123), 5, 96.761, 5, new IMaterial[] { new PhongMaterial(Colour.FromRgb(220, 220, 220)) });

            lampArm2 = ((Transform3D.Translate(280, -214, 123) * Transform3D.RotationAlongAxis(new NormalizedVector3D(1, 0, 0), -Math.Atan2(26, 88)) * Transform3D.Translate(-280, 214, -123)) * lampArm2).ToList();

            gpr.AddRange(lampArm2);

            foreach (Triangle3DElement triangle in lampArm2)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
                triangle.ZIndex = zIndex1;
            }

            //gpr.AddElement(new Line3DElement(new Point3D(280, -170, 110), new Point3D(225, -170, 110)) { Colour = Colour.FromRgb(220, 220, 220), Thickness = 1, LineCap = LineCaps.Round, ZIndex = zIndex1 });

            List<Element3D> lampArm3 = ObjectFactory.CreateCuboid(new Point3D(252.5, -170, 110), 55, 5, 5, new IMaterial[] { new PhongMaterial(Colour.FromRgb(220, 220, 220)) });
            gpr.AddRange(lampArm3);

            foreach (Triangle3DElement triangle in lampArm3)
            {
                triangle.CastsShadow = true;
                triangle.ReceivesShadow = true;
                triangle.ZIndex = zIndex1;
            }


            /*  gpr.AddElement(ObjectFactory.CreatePoint(new Point3D(0, -500, -100), Colours.Orange));
              gpr.AddElement(ObjectFactory.CreatePoint(new Point3D(225, -250, 125), Colours.Orange));

              gpr.AddElement(new Point3DElement(new Point3D(500, -300, 700)) { Colour = Colour.FromRgb(220, 220, 220), Diameter = 35 });
            */


            gpr.AddRange(wall1);
            gpr.AddRange(wall2);
            gpr.AddRange(wall3);
            gpr.AddRange(wall4);

            gpr.AddRange(floor);
            gpr.AddRange(ceiling);


            /*foreach (Element3D element in gpr.SceneElements)
            {
                if (element is Triangle3DElement triangle)
                {
                    triangle.OverFill = 0.05;
                }
            }
            */

            //gpr.Replace(el => el is Triangle3DElement triangle ? new VectorRendererTriangleElement(triangle) { OverFill = 0.05 } : el);

            camera = new PerspectiveCamera(new Point3D(-450, -470, -420), new NormalizedVector3D(1, 0.35, 1), 200, new Size(200, 200), 1);
            // camera = new PerspectiveCamera(new Point3D(-450, -470, -420), new NormalizedVector3D(450, 470, 420), 200, new Size(200, 200), 1);

            GraphicsPath starPath = new GraphicsPath();

            double radius = 100;

            for (int i = 0; i < 10; i++)
            {
                if (i % 2 == 0)
                {
                    starPath.LineTo(Math.Cos(i * Math.PI / 5) * radius, Math.Sin(i * Math.PI / 5) * radius);
                }
                else
                {
                    starPath.LineTo(Math.Cos(i * Math.PI / 5) * radius * 0.5, Math.Sin(i * Math.PI / 5) * radius * 0.5);
                }
            }

            starPath.Close();

            /*  gpr = new Scene();

              gpr.AddRange(ObjectFactory.CreateTetrahedron(new Point3D(0, 0, 0), 100, new IMaterial[] { new PhongMaterial(Colour.FromRgb(0, 162, 232)) }));

              gpr.AddElement(new Point3DElement(new Point3D(0, -300, 0)) { Colour = Colour.FromRgb(255, 127, 0), Diameter = 35 });

              gpr.AddElement(new Line3DElement(new Point3D(-100, -500, 0), new Point3D(100, -500, 0)) { Colour = Colour.FromRgb(255, 127, 0), Thickness = 5, LineCap = LineCaps.Butt, LineDash = new LineDash(1, 1, 0) });
              gpr.AddElement(new Line3DElement(new Point3D(-100, -450, 0), new Point3D(100, -450, 0)) { Colour = Colour.FromRgb(255, 127, 0), Thickness = 5, LineCap = LineCaps.Round, LineDash = new LineDash(2, 2, 1) });
              gpr.AddElement(new Line3DElement(new Point3D(-100, -400, 0), new Point3D(100, -400, 0)) { Colour = Colour.FromRgb(255, 127, 0), Thickness = 5, LineCap = LineCaps.Square, LineDash = new LineDash(2, 2, 0.5) });
            gpr.AddElement(new Line3DElement(new Point3D(0, -500, -100), new Point3D(0, -600, -100)) { Colour = Colours.Black, Thickness = 0.5 });
            */

            ILightSource[] lights = new ILightSource[]
            {
                 new AmbientLightSource(0.1),

                //new ParallelLightSource(0.35 * 0.5, new NormalizedVector3D(0, 1, -1)),

                 new PointLightSource(0.35, new Point3D(350, -1000, 1000)){ DistanceAttenuationExponent = 0, CastsShadow = true },
                
                 //new ParallelLightSource(0.15 * 0.5, new NormalizedVector3D(1500, 1200, 1000)),
                //new PointLightSource(30000, new Point3D(0, -500, 0)),
                new SpotlightLightSource(40000, new Point3D(0, -500, -100), new NormalizedVector3D(0, 1, 0), Math.PI / 4, Math.PI / 1.5) { CastsShadow = true },
                
                new SpotlightLightSource(0.35, new Point3D(225, -250, 125), new NormalizedVector3D(0, 1, -1), Math.PI / 8, Math.PI / 2) { DistanceAttenuationExponent = 0, CastsShadow = true },
                
                //new PointLightSource(0.35, new Point3D(225, -250, 125)) { DistanceAttenuationExponent = 0, CastsShadow = true }
                
                //new SpotlightLightSource(50000, new Point3D(-150, -650, 0), new NormalizedVector3D(0, 1, 0), Math.PI / 6, Math.PI / 4) { DistanceAttenuationExponent = 2, AngleAttenuationExponent = 1 },

                //new PointLightSource(20000, new Point3D(-150, -450, 0))

                // new MaskedLightSource(50000, new Point3D(-200, -650, 0), new NormalizedVector3D(0, 1, 0), 100, starPath, new Vector3D(1, 0, 0), 1) { DistanceAttenuationExponent = 2, AngleAttenuationExponent = 2 }
            };


           
            VectorRenderer renderer = new VectorRenderer() { ResamplingMaxSize = maxArea, ResamplingTime = VectorRenderer.ResamplingTimes.AfterSorting, DefaultOverFill = 0.05 };
            //RasterRenderer renderer = new RasterRenderer(500, 500) { InterpolateImage = false };
            //RaycastingRenderer renderer = new RaycastingRenderer(500, 500) { InterpolateImage = true, AntiAliasing = RaycastingRenderer.AntiAliasings.None };

            Page pag = renderer.Render(gpr, lights, camera);

            Page background = new Page(pag.Width, pag.Height);

            background.Graphics.FillRectangle(0, 0, pag.Width, pag.Height, Colour.FromRgb(56, 65, 139));

            Random rnd = new Random();

            for (int i = 0; i < 150; i++)
            {
                double x = rnd.NextDouble() * pag.Width;
                double y = rnd.NextDouble() * pag.Height;
                double scale = rnd.NextDouble() * 0.9 + 0.1;
                double rotation = rnd.NextDouble();

                background.Graphics.Save();
                background.Graphics.Translate(x, y);
                background.Graphics.Scale(scale / 100 * pag.Width * 0.01, scale / 100 * pag.Width * 0.01);
                background.Graphics.Rotate(Math.PI * 2 * rotation);

                background.Graphics.FillPath(starPath, Colour.FromRgb(255, 255, 180));

                background.Graphics.Restore();
            }


            background.Graphics.FillPath(new GraphicsPath().Arc(pag.Width * 0.26, pag.Height * 0.25, pag.Width * 0.075, 0, 2 * Math.PI), Colour.FromRgb(220, 220, 220));

            double[,] craters = new double[,]
            {
                { 1.1, 0.15, 0.4 },
                { 1, 0.35, 0.6 },
            };

            for (int i = 0; i < craters.GetLength(0); i++)
            {
                double r = craters[i, 0] * (pag.Width * 0.075 - pag.Width * 0.03);
                double theta = craters[i, 1] * 2 * Math.PI;
                double size = (craters[i, 2] * 0.95 + 0.05) * pag.Width * 0.03;

                background.Graphics.FillPath(new GraphicsPath().Arc(pag.Width * 0.26 + r * Math.Cos(theta), pag.Height * 0.25 + r * Math.Sin(theta), size, 0, 2 * Math.PI), Colour.FromRgb(200, 200, 200));
            }



            background.Graphics.DrawGraphics(0, 0, pag.Graphics);




            Document doc = new Document();
            doc.Pages.Add(background);
            doc.SaveAsPDF("Test3D.pdf");


            Avalonia.Controls.Canvas can = background.PaintToCanvas();
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

                            Avalonia.Controls.Canvas can = renderer.Render(gpr, lights, camera).PaintToCanvas();
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
                        Avalonia.Controls.Canvas can = renderer.Render(gpr, lights, camera).PaintToCanvas();
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
                camera.Zoom(e.Delta.Y * 100);
                Avalonia.Controls.Canvas can = renderer.Render(gpr, lights, camera).PaintToCanvas();
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
