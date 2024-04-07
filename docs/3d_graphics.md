---
layout: default
nav_order: 19
---

# Creating 3D graphics

The [VectSharp.ThreeD NuGet package](https://www.nuget.org/packages/VectSharp.ThreeD) can be used to create 3D plots using the same general approach as VectSharp (naturally, with some complications due to the nature of 3D graphics, such as lights and camera positioning). The 3D scene can be rendered either as a raster image or as a vector image, which is particularly useful for plots of 3D functions.

Here are a few examples of 3D graphics produced with VectSharp.ThreeD:

* An indoors scene with moonlight and two lightbulbs.

<p style="text-align: center">
    <img src="https://raw.githubusercontent.com/arklumpus/VectSharp/master/VectSharp.ThreeD/images/Moonlight_2.svg" style="height:25em">
</p>

* The plot of a function of two variables.

<p style="text-align: center">
    <img src="https://github.com/arklumpus/VectSharp/raw/master/VectSharp.ThreeD/images/FunctionPlot.svg" style="height:25em">
</p>

* Two visualisations of a _p_ orbital of the Hydrogen atom (_n_=2, _l_=1). On the left, as a point cloud (the probability of finding a point at a certain combination of coordinates is proportional to the squared modulus of the wave function); on the right, as a set of surfaces (the squared modulus of the wave function is constant on each surface; on the outer surface it is 20% of the maximum value and it increases by 10% for each surfance until the inner surface, where it is 90% of the maximum value). The nodal plane is highlighted in grey.

<p style="text-align: center">
    <img src="https://github.com/arklumpus/VectSharp/raw/master/VectSharp.ThreeD/images/HydrogenPOrbital.svg" style="height:25em">
</p>

The following code example shows how to plot a cube using VectSharp.ThreeD:

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/Cube.svg" style="height:25em">
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.ThreeD;
using VectSharp.SVG;

// Create a scene to contain 3D objects
Scene scene = new Scene();

// The static ObjectFactory class can be used to create "complex" 3D objects, such as a cube.
// These are returned as a list of triangles that can be added to the scene.
scene.AddRange(ObjectFactory.CreateCube(new Point3D(0, 0, 0), 100,
                                        new IMaterial[] {
                                            new PhongMaterial(Colour.FromRgb(0, 158, 115))
                                        }));

// The light will be used to illuminate the object(s) in the scene and make them actually visible.
ParallelLightSource light = new ParallelLightSource(0.5, new NormalizedVector3D(1, 2, 3));

// A camera that renders the scene using a perspective projection.
PerspectiveCamera camera = new PerspectiveCamera(new Point3D(-200, -200, -300),
                                                 new NormalizedVector3D(2, 2, 3), 50,
                                                 new Size(30, 30), 1);

// A renderer to render the scene as a vector image.
VectorRenderer renderer = new VectorRenderer() { DefaultOverFill = 0.02, ResamplingMaxSize = 1 };

// Render the scene.
Page pag = renderer.Render(scene, new ILightSource[] { light }, camera);

// Save as an SVG image.
pag.SaveAsSVG("Cube.svg");
{% endhighlight %}

For more details, look at the [Readme file](https://github.com/arklumpus/VectSharp/blob/master/VectSharp.ThreeD/Readme.md) in the VectSharp.ThreeD repository.
