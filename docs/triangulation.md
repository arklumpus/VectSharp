---
layout: default
nav_order: 5
parent: Advanced topics
---

# Path triangulation

Another interesting possibility to simplify working with paths is path "triangulation". By using the `Triangulate` method of the `GraphicsPath` class, you can "triangulate" a path, i.e. obtain a collection of triangles that cover the same area the original path. This can be used for very interesting effects (e.g., VectSharp.ThreeD uses this method to create 3D objects from 2D paths).

The `Triangulate` method requires two parameters: one is the resolution at which the path is linearised before being triangulated, and the other is a `bool` indicating whether the triangles that it produces should have their vertices in clockwise or counter-clockwise order (this is useful to determine the orientation of the triangles).

The following example shows how to use the `Triangulate` method.

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/Triangulation.svg" style="height:10em">
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 25);
Graphics graphics = page.Graphics;

// Font to draw some text.
FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
Font font = new Font(family, 15);

// Original GraphicsPath containing some text.
GraphicsPath path = new GraphicsPath().AddText(15, 8, "VectSharp", font);

// Triangulate the path.
List<GraphicsPath> triangles = path.Triangulate(2, true).ToList();

// Colours to colour each triangle differently.
Colour[] colours = new Colour[]
{
    Colour.FromRgb(230, 159, 0),
    Colour.FromRgb(86, 180, 233),
    Colour.FromRgb(0, 158, 115),
    Colour.FromRgb(240, 228, 66),
    Colour.FromRgb(0, 114, 178),
    Colour.FromRgb(213, 94, 0),
    Colour.FromRgb(204, 121, 167)
};

// Draw each triangle, cycling over all the possible colours.
for (int i = 0; i < triangles.Count; i++)
{
    graphics.FillPath(triangles[i], colours[i % colours.Length]);
}

page.SaveAsSVG("Triangulation.svg");
{% endhighlight %}

The following example uses triangulation together with some basic physics to create an "explosion" effect. The VectSharp animation features are used to draw create the explosion animation, which is then saved as an animated GIF using VectSharp.Raster.ImageSharp (you could also create an animated SVG or PNG).

You could obtain a more sofisticated animation by improving the physics engine (e.g. by allowing each triangle to rotate), but this is overkill for this example.

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/TriangulationAnimation.gif" style="height:10em">
    </p>
</div>
<details markdown="block">
<summary>
    Expand source code
  </summary>
  {: .text-delta }

{% highlight CSharp %}
using VectSharp;
using VectSharp.Raster.ImageSharp;

// Font to draw some text.
FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
Font font = new Font(family, 15);

// Original GraphicsPath containing some text.
GraphicsPath path = new GraphicsPath().AddText(15, 8, "VectSharp", font);

// List to contain the vertices of the triangles.
List<List<Point>> trianglePoints = new List<List<Point>>();

// Triangulate the path and add the vertices of the triangles to the list.
foreach (GraphicsPath triangle in path.Triangulate(2, true))
{
    List<Point> currTriangle = new List<Point>();
    foreach (List<Point> pathFigure in triangle.GetPoints())
    {
        currTriangle.AddRange(pathFigure);
    }
    trianglePoints.Add(currTriangle);
}

// Origin point for the explosion.
Point explosionPoint = new Point(50, 25);

// Initial velocity for the triangles.
double initialVelocity = 10;

// Friction coefficient.
double friction = 0.25;

// Gravitational constant.
double gravity = 4;

// Will hold the initial velocities of the points.
List<List<Point>> initialVelocities = new List<List<Point>>();

// The initial velocity of the vertices of a triangle will be in the direction connecting
// the barycenter of the triangle to the explosion point, and will have modulus equal to initialVelocity.
for (int i = 0; i < trianglePoints.Count; i++)
{
    initialVelocities.Add(new List<Point>());

    // Compute the barycenter of the triangle.
    Point barycenter = new Point();
    for (int j = 0; j < trianglePoints[i].Count; j++)
    {
        barycenter = new Point(barycenter.X + trianglePoints[i][j].X, barycenter.Y + trianglePoints[i][j].Y);
    }
    barycenter = new Point(barycenter.X / trianglePoints[i].Count, barycenter.Y / trianglePoints[i].Count);

    Point direction = new Point(barycenter.X - explosionPoint.X, barycenter.Y - explosionPoint.Y);
    double r = direction.Modulus();

    double velocityModulus = initialVelocity;

    for (int j = 0; j < trianglePoints[i].Count; j++)
    {
        initialVelocities[i].Add(new Point(direction.X / r * velocityModulus, -direction.Y / r * velocityModulus));
    }
}

// Colours to colour each triangle differently.
Colour[] colours = new Colour[]
{
    Colour.FromRgb(230, 159, 0),
    Colour.FromRgb(86, 180, 233),
    Colour.FromRgb(0, 158, 115),
    Colour.FromRgb(240, 228, 66),
    Colour.FromRgb(0, 114, 178),
    Colour.FromRgb(213, 94, 0),
    Colour.FromRgb(204, 121, 167)
};

// Create the animation that will contain all the frames.
Animation animation = new Animation(132, 31, 1) { Background = Colours.White };

// Loop over time
for (double t = 0; t <= 6.5; t += 0.075)
{
    // Create a new graphics for the frame.
    Graphics graphics = new Graphics();
    graphics.Translate(16, 3);

    // Loop over the triangles.
    for (int i = 0; i < trianglePoints.Count; i++)
    {
        // Create the new path to hold the triangle.
        GraphicsPath triangle = new GraphicsPath();

        for (int j = 0; j < trianglePoints[i].Count; j++)
        {
            // Compute the current position of each point, based on initial velocity, gravity and friction.
            double currX = trianglePoints[i][j].X + initialVelocities[i][j].X / friction * (1 - Math.Exp(-friction * t));
            double currY = trianglePoints[i][j].Y + gravity * t / friction + (initialVelocities[i][j].Y / friction + gravity / (friction * friction)) * (Math.Exp(-friction * t) - 1);

            // When a point reaches y = 25, it has arrived to the "floor".
            currY = Math.Min(currY, 25);

            // Add the point to the path.
            if (j == 0)
            {
                triangle.MoveTo(currX, currY);
            }
            else
            {
                triangle.LineTo(currX, currY);
            }
        }

        // Close the triangle.
        triangle.Close();

        // Fill the triangle.
        graphics.FillPath(triangle, colours[i % colours.Length]);
    }

    // Create the frame and set the duration. We use a larger duration for the first frame.
    Frame frame;

    if (t == 0)
    {
        frame = new Frame(graphics, 250);
    }
    else
    {
        frame = new Frame(graphics, 16);
    }

    // Add the frame to the animation.
    animation.AddFrame(frame);
}


// Add the reverse animation.
for (double t = 6.5; t >= 0; t -= 0.075)
{
    // Create a new graphics for the frame.
    Graphics graphics = new Graphics();
    graphics.Translate(16, 3);

    // Loop over the triangles.
    for (int i = 0; i < trianglePoints.Count; i++)
    {
        // Create the new path to hold the triangle.
        GraphicsPath triangle = new GraphicsPath();

        for (int j = 0; j < trianglePoints[i].Count; j++)
        {
            // Compute the current position of each point, based on initial velocity, gravity and friction.
            double currX = trianglePoints[i][j].X + initialVelocities[i][j].X / friction * (1 - Math.Exp(-friction * t));
            double currY = trianglePoints[i][j].Y + gravity * t / friction + (initialVelocities[i][j].Y / friction + gravity / (friction * friction)) * (Math.Exp(-friction * t) - 1);

            // When a point reaches y = 25, it has arrived to the "floor".
            currY = Math.Min(currY, 25);

            // Add the point to the path.
            if (j == 0)
            {
                triangle.MoveTo(currX, currY);
            }
            else
            {
                triangle.LineTo(currX, currY);
            }
        }

        // Close the triangle.
        triangle.Close();

        // Fill the triangle.
        graphics.FillPath(triangle, colours[i % colours.Length]);
    }

    // Create the frame.
    Frame frame = new Frame(graphics, 16);

    // Add the frame to the animation.
    animation.AddFrame(frame);
}

// Save the animation as an animated GIF, with a scale factor of 10.
animation.SaveAsAnimatedGIF("TriangulationAnimation.gif", 10);
{% endhighlight %}
</details>