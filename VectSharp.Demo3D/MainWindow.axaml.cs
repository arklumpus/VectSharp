using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using VectSharp.ThreeD;
using VectSharp.Canvas;
using Avalonia.Input;

namespace VectSharp.Demo3D
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Create scene object
            Scene scene = new Scene();

            #region Populate scene with function surface

            // Number of sample points per coordinate
            int resolution = 20;

            // Boundary values
            double minX = -2;
            double maxX = 2;

            double minZ = -2;
            double maxZ = 2;

            // Arrays to hold point coordinate and vertex normals
            Point3D[][] points = new Point3D[resolution + 1][];
            NormalizedVector3D[][] normals = new NormalizedVector3D[resolution + 1][];

            // Compute the sample point coordinates and normals
            for (int x = 0; x <= resolution; x++)
            {
                points[x] = new Point3D[resolution + 1];
                normals[x] = new NormalizedVector3D[resolution + 1];

                // Convert sample index to coordinate
                double X = minX + x * (maxX - minX) / resolution;

                for (int z = 0; z <= resolution; z++)
                {
                    // Convert sample index to coordinate
                    double Z = minZ + z * (maxZ - minZ) / resolution;

                    // Compute function f(x, z) = 4 * x * exp(-(x^2 + z^2))
                    double funcVal = 4 * X * Math.Exp(-(X * X + Z * Z));

                    // Scale everything by 100 to avoid underflow
                    points[x][z] = new Point3D(X * 100, -funcVal * 100, Z * 100);

                    // Compute the surface normal using the gradient of the function F(x, y, z) = f(x, z) + y (note that the y axis points down)
                    double normalX = 4 * Math.Exp(-(X * X + Z * Z)) * (1 - 2 * X * X);
                    double normalY = 1;
                    double normalZ = -8 * X * Z * Math.Exp(-(X * X + Z * Z));

                    NormalizedVector3D normal = new NormalizedVector3D(normalX, normalY, normalZ);

                    normals[x][z] = normal;
                }
            }

            // Create triangles using the previously computed coordinates and normals
            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    Triangle3DElement triangle1 = new Triangle3DElement(points[x][z + 1], points[x + 1][z + 1], points[x + 1][z], normals[x][z + 1], normals[x + 1][z + 1], normals[x + 1][z]);
                    triangle1.Fill.Add(new PhongMaterial(Colours.CornflowerBlue) { SpecularReflectionCoefficient = 0 });

                    Triangle3DElement triangle2 = new Triangle3DElement(points[x][z + 1], points[x + 1][z], points[x][z], normals[x][z + 1], normals[x + 1][z], normals[x][z]);
                    triangle2.Fill.Add(new PhongMaterial(Colours.CornflowerBlue) { SpecularReflectionCoefficient = 0 });

                    scene.AddElement(triangle1);
                    scene.AddElement(triangle2);

                    // Also add the reversed triangles, otherwise the "bottom" side of the surface would be culled when looking from below
                    // Not reversing the normals, otherwise everything at the it would be dark (unless we also add another light source)

                    Triangle3DElement triangle3 = new Triangle3DElement(points[x][z + 1], points[x + 1][z], points[x + 1][z + 1], normals[x][z + 1], normals[x + 1][z], normals[x + 1][z + 1]);
                    triangle3.Fill.Add(new PhongMaterial(Colours.CornflowerBlue) { SpecularReflectionCoefficient = 0 });

                    Triangle3DElement triangle4 = new Triangle3DElement(points[x][z + 1], points[x][z], points[x + 1][z], normals[x][z + 1], normals[x][z], normals[x + 1][z]);
                    triangle4.Fill.Add(new PhongMaterial(Colours.CornflowerBlue) { SpecularReflectionCoefficient = 0 });

                    scene.AddElement(triangle3);
                    scene.AddElement(triangle4);
                }
            }

            #endregion

            // Create a camera
            PerspectiveCamera camera = new PerspectiveCamera(new Point3D(-300, -200, -400), new NormalizedVector3D(3, 2, 4), 100, new Size(115, 115), 1);

            // Generic renderer initialised as a VectorRenderer
            IRenderer renderer = new VectorRenderer() { DefaultOverFill = 0.05 };

            // Main light source
            ParallelLightSource light = new ParallelLightSource(0.75, new NormalizedVector3D(0.25, 1, 0.1));

            // Light source array
            ILightSource[] lights = new ILightSource[] { new AmbientLightSource(0.1), light };

            // Render the scene
            Page pag = renderer.Render(scene, lights, camera);

            // Display the rendered scene on the window
            Avalonia.Controls.Canvas can = pag.PaintToCanvas();
            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Add(can);
            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width = can.Width;
            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height = can.Height;

            // Function to recreate the renderer based on the options selected in the UI
            void updateRenderer()
            {
                // VectorRenderer
                if (this.FindControl<ComboBox>("RendererBox").SelectedIndex == 0)
                {
                    renderer = new VectorRenderer() { DefaultOverFill = 0.05 };

                    this.FindControl<TextBlock>("ResolutionBlock").IsVisible = false;
                    this.FindControl<TextBlock>("ResolutionHeightBlock").IsVisible = false;
                    this.FindControl<TextBlock>("ResolutionWidthBlock").IsVisible = false;

                    this.FindControl<NumericUpDown>("ResolutionWidthNUD").IsVisible = false;
                    this.FindControl<NumericUpDown>("ResolutionHeightNUD").IsVisible = false;

                    this.FindControl<CheckBox>("AntialiasingBox").IsVisible = false;
                }
                // RasterRenderer
                else if (this.FindControl<ComboBox>("RendererBox").SelectedIndex == 1)
                {
                    int width = (int)Math.Round(this.FindControl<NumericUpDown>("ResolutionWidthNUD").Value);
                    int height = (int)Math.Round(this.FindControl<NumericUpDown>("ResolutionHeightNUD").Value);

                    renderer = new RasterRenderer(width, height);

                    this.FindControl<TextBlock>("ResolutionBlock").IsVisible = true;
                    this.FindControl<TextBlock>("ResolutionHeightBlock").IsVisible = true;
                    this.FindControl<TextBlock>("ResolutionWidthBlock").IsVisible = true;

                    this.FindControl<NumericUpDown>("ResolutionWidthNUD").IsVisible = true;
                    this.FindControl<NumericUpDown>("ResolutionHeightNUD").IsVisible = true;

                    this.FindControl<CheckBox>("AntialiasingBox").IsVisible = false;
                }
                // RaycastingRenderer
                else if (this.FindControl<ComboBox>("RendererBox").SelectedIndex == 2)
                {
                    int width = (int)Math.Round(this.FindControl<NumericUpDown>("ResolutionWidthNUD").Value);
                    int height = (int)Math.Round(this.FindControl<NumericUpDown>("ResolutionHeightNUD").Value);

                    this.FindControl<TextBlock>("ResolutionBlock").IsVisible = true;
                    this.FindControl<TextBlock>("ResolutionHeightBlock").IsVisible = true;
                    this.FindControl<TextBlock>("ResolutionWidthBlock").IsVisible = true;

                    this.FindControl<NumericUpDown>("ResolutionWidthNUD").IsVisible = true;
                    this.FindControl<NumericUpDown>("ResolutionHeightNUD").IsVisible = true;

                    this.FindControl<CheckBox>("AntialiasingBox").IsVisible = true;

                    renderer = new RaycastingRenderer(width, height) { AntiAliasing = this.FindControl<CheckBox>("AntialiasingBox").IsChecked == true ? RaycastingRenderer.AntiAliasings.Bilinear4X : RaycastingRenderer.AntiAliasings.None };
                }

                // Render the scene and display it on the window
                Avalonia.Controls.Canvas can = renderer.Render(scene, lights, camera).PaintToCanvas();
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Clear();
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Add(can);
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width = can.Width;
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height = can.Height;
            }

            // Add events to controls changing rendering parameters
            this.FindControl<ComboBox>("RendererBox").SelectionChanged += (s, e) => updateRenderer();
            this.FindControl<NumericUpDown>("ResolutionWidthNUD").ValueChanged += (s, e) => updateRenderer();
            this.FindControl<NumericUpDown>("ResolutionHeightNUD").ValueChanged += (s, e) => updateRenderer();
            this.FindControl<CheckBox>("AntialiasingBox").Click += (s, e) => updateRenderer();

            // Camera reset button
            this.FindControl<Button>("ResetCameraButton").Click += (s, e) =>
            {
                camera = new PerspectiveCamera(new Point3D(-300, -200, -400), new NormalizedVector3D(3, 2, 4), 100, new Size(115, 115), 1);
                Avalonia.Controls.Canvas can = renderer.Render(scene, lights, camera).PaintToCanvas();
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Clear();
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Add(can);
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width = can.Width;
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height = can.Height;
            };


            // Code to handle mouse events moving the camera on the canvas

            bool isPressed = false;
            PointerPoint pointerPress = null;
            double lastTheta = 0;
            double lastPhi = 0;
            double lastX = 0;
            double lastY = 0;

            // Start drag
            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").PointerPressed += (s, e) =>
            {
                pointerPress = e.GetCurrentPoint(this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas"));
                lastTheta = 0;
                lastPhi = 0;
                lastX = 0;
                lastY = 0;
                isPressed = true;
            };

            // Drag end
            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").PointerReleased += (s, e) =>
            {
                isPressed = false;
            };

            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").PointerMoved += (s, e) =>
            {
                // While dragging
                if (isPressed)
                {
                    PointerPoint point = e.GetCurrentPoint(this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas"));

                    // Left button: orbit
                    if (pointerPress.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
                    {
                        double dx = (point.Position.X - pointerPress.Position.X) / this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width;
                        double dy = (point.Position.Y - pointerPress.Position.Y) / this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height;
                        if (dx >= -1 && dx <= 1 && dy >= -1 && dy <= 1)
                        {
                            double theta = Math.Asin(dx) * 2;
                            double phi = Math.Asin(dy) * 2;

                            camera.Orbit(theta - lastTheta, phi - lastPhi);

                            Avalonia.Controls.Canvas can = renderer.Render(scene, lights, camera).PaintToCanvas();
                            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Clear();
                            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Add(can);
                            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width = can.Width;
                            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height = can.Height;

                            lastTheta = theta;
                            lastPhi = phi;
                        }
                    }
                    // Right button: pan
                    else if (pointerPress.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
                    {
                        double dx = point.Position.X - pointerPress.Position.X;
                        double dy = point.Position.Y - pointerPress.Position.Y;

                        camera.Pan(dx - lastX, dy - lastY);
                        Avalonia.Controls.Canvas can = renderer.Render(scene, lights, camera).PaintToCanvas();
                        this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Clear();
                        this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Add(can);
                        this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width = can.Width;
                        this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height = can.Height;

                        lastX = dx;
                        lastY = dy;
                    }
                }
            };

            // Mouse wheel: zoom
            this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").PointerWheelChanged += (s, e) =>
            {
                camera.Zoom(e.Delta.Y * 100);
                Avalonia.Controls.Canvas can = renderer.Render(scene, lights, camera).PaintToCanvas();
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Clear();
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Children.Add(can);
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Width = can.Width;
                this.FindControl<Avalonia.Controls.Canvas>("ContainerCanvas").Height = can.Height;
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
