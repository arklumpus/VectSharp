---
layout: default
nav_order: 2
parent: Animations
---

# Easings
{: .no_toc }

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

When an object is animated during a transition between two frames, this happens by default by using a linear interpolation:

$$S\left(t \right) = S_0 \cdot \left (1 - t \right) + S_1 \cdot t$$

Where $t$ is the progress along the transition ("time"), going from $0$ to $1$, $S\left(t \right)$ is the state at time $t$ (e.g., the colour or position of an object), $S_0$ is the state at the start of the transition and $S_1$ is the state at the end of the transition. For example, if an object is moving from a point with coordinates `(0, 0)` to `(100, 84)` over a transition that lasts `1` second, after `0.5` seconds the object will be at coordinates `(50, 42)`.

This can be altered with an "easing", which makes it possible to obtain interesting effects, such as having the object accelerate at the start of the transition or slow down at its end.

An "easing" is essentially a real function $f\left (t \right)$ that is computed with an argument between `0` (start of the transition) and `1` (end of the transition) and which returns a value that is used to linearly interpolate between the start and end state of the transition:

$$S\left(t \right) =  S_0 \cdot \left (1 - f \left(t \right) \right) + S_1 \cdot f\left (t \right)$$

As mentioned, the default behaviour of VectSharp is to use a linear easing (or, no easing), i.e. $f \left( t \right) = t$; an easing function can be applied by using the optional parameters of the `Transition` constructor:

* The `IEasing easing` parameter is used to provide a single easing that will be applied to all elements in the transition.
* The `Dictionary<string, IEasing> easings` parameter is used, instead, to provide a different easing for each object being animated.

## Applying a single easing

The `IEasing` interface defines a single member: `double Ease(double value)` which applies the easing. In principle, this could be implemented using any function (even returning values smaller than `0` or greater than `1`), and this is supported when producing animated GIFs or PNGs (you will just have to create a class implementing the interface); however, because animated SVGs only support a limited set of easings, only two concrete implementations of this interface are provided by VectSharp:

* `LinearEasing`, which represents linear interpolation without any easing.
* `SplineEasing`, which represents an easing performed using a cubic Bézier curve. The constructor for this class accepts two parameters, representing the control points of the Bézier curve; again, because of limitations of the SVG format, the coordinates for these points all have to be between `0` and `1`.

The following example shows how to apply an easing to the simple animation from [Creating a simple animation]({{ site.baseurl }}{% link simple_animation.md %}) (again, the progress bar has just been added for clarity and will not be present in your output file):

<div class="code-example">
<p style="text-align: center">
    <object data="assets/images/easing_0.svg" style="height: 25em">
    </object>
</p>
</div>
{% highlight CSharp %}
// Create a Graphics object to hold the first frame.
Graphics frame0Contents = new Graphics();
// Draw a green circle on the first frame
frame0Contents.FillPath(new GraphicsPath().Arc(10, 10, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 158, 115), tag: "circle");
// Create the first frame, with a duration of 1000ms.
Frame frame0 = new Frame(frame0Contents, 1000);

// Create a Graphics object to hold the second frame.
Graphics frame1Contents = new Graphics();
// Draw a blue circle on the second frame.
frame1Contents.FillPath(new GraphicsPath().Arc(90, 90, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 114, 178), tag: "circle");
// Create the second frame, also with a duration of 1000ms.
Frame frame1 = new Frame(frame1Contents, 1000);

// Create an easing for the transition. This will cause the object to start moving slowly,
// accelerate in the middle of the transition, and slow down again at the end of the transition.
SplineEasing easing = new SplineEasing(new Point(0.75, 0), new Point(0, 0.75));

// Create the transition between frame0 and frame1, again with a duration of 1000ms.
Transition transition0_1 = new Transition(1000, easing);

// Create the animation object, specifying the width, height and linearisation resolution.
Animation animation = new Animation(100, 100, 1);
// Add the first frame to the animation.
animation.AddFrame(frame0);
// Add the second frame to the animation, with the transition we created earlier.
animation.AddFrame(frame1, transition0_1);

// Save the animation as an animated SVG file.
animation.SaveAsAnimatedSVG("animation.svg");
{% endhighlight %}

## Spline easings

A spline easing is defined by the two control points of the spline. The following interactive example shows the effect of changing the coordinates of these points on the spline and on the animation:

<iframe src="Blazor?splineEasing" style="width: 100%; height: 20em; border: 0px solid black"></iframe>

## Applying multiple easings

It is also possible to apply a different easing to each object in the transition. In this way, the start and end of the transition will happen for all objects at the same time, but each object will move in a different way.

This can be achieved by using the `easings` parameter of the `Transition` constructor, providing a `Dictionary<string, IEasing>` that associates the `tag` of each animated object with the respective easing, as demonstrated by the following example:

<div class="code-example">
<p style="text-align: center">
    <object data="assets/images/easing_1.svg" style="height: 25em">
    </object>
</p>
</div>
{% highlight CSharp %}
// Create a Graphics object to hold the first frame.
Graphics frame0Contents = new Graphics();

// Draw the first circle in green
frame0Contents.FillPath(new GraphicsPath().Arc(10, 10, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 158, 115), tag: "circle1");

// Draw the second circle in orange
frame0Contents.FillPath(new GraphicsPath().Arc(40, 10, 10, 0, 2 * Math.PI), Colour.FromRgb(214, 94, 0), tag: "circle2");

// Draw the third circle in blue
frame0Contents.FillPath(new GraphicsPath().Arc(70, 10, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 114, 178), tag: "circle3");

// Draw the fourth circle in pink
frame0Contents.FillPath(new GraphicsPath().Arc(100, 10, 10, 0, 2 * Math.PI), Colour.FromRgb(204, 121, 167), tag: "circle4");

// Draw the fifth circle in light blue
frame0Contents.FillPath(new GraphicsPath().Arc(130, 10, 10, 0, 2 * Math.PI), Colour.FromRgb(86, 180, 233), tag: "circle5");

// Create the first frame, with a duration of 1000ms.
Frame frame0 = new Frame(frame0Contents, 1000);

// Create a Graphics object to hold the second frame.
Graphics frame1Contents = new Graphics();

// Draw the first circle in green
frame1Contents.FillPath(new GraphicsPath().Arc(10, 90, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 158, 115), tag: "circle1");

// Draw the second circle in orange
frame1Contents.FillPath(new GraphicsPath().Arc(40, 90, 10, 0, 2 * Math.PI), Colour.FromRgb(214, 94, 0), tag: "circle2");

// Draw the third circle in blue
frame1Contents.FillPath(new GraphicsPath().Arc(70, 90, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 114, 178), tag: "circle3");

// Draw the fourth circle in pink
frame1Contents.FillPath(new GraphicsPath().Arc(100, 90, 10, 0, 2 * Math.PI), Colour.FromRgb(204, 121, 167), tag: "circle4");

// Draw the fifth circle in light blue
frame1Contents.FillPath(new GraphicsPath().Arc(130, 90, 10, 0, 2 * Math.PI), Colour.FromRgb(86, 180, 233), tag: "circle5");

// Create the second frame, also with a duration of 1000ms.
Frame frame1 = new Frame(frame1Contents, 1000);

// Create a different easing for each object.

// Linear easing (i.e., no easing).
LinearEasing easing1 = new LinearEasing();

// Various spline easings.
SplineEasing easing2 = new SplineEasing(new Point(0.75, 0), new Point(0.25, 1));
SplineEasing easing3 = new SplineEasing(new Point(0.75, 0), new Point(1, 0.25));
SplineEasing easing4 = new SplineEasing(new Point(0, 0.75), new Point(0.25, 1));
SplineEasing easing5 = new SplineEasing(new Point(0, 0.75), new Point(1, 0.75));

// Create a dictionary associationg each easing to an object tag.
Dictionary<string, IEasing> easings = new Dictionary<string, IEasing>() {
    {
        "circle1", easing1
    },
    {
        "circle2", easing2
    },
    {
        "circle3", easing3
    },
    {
        "circle4", easing4
    },
    {
        "circle5", easing5
    }
};

// Create the transition between frame0 and frame1, with a duration of 1000ms and applying the easings.
Transition transition0_1 = new Transition(1000, easings: easings);

// Create the animation object, specifying the width, height and linearisation resolution.
Animation animation = new Animation(140, 100, 1);
// Add the first frame to the animation.
animation.AddFrame(frame0);
// Add the second frame to the animation, with the transition we created earlier.
animation.AddFrame(frame1, transition0_1);

// Save the animation as an animated SVG file.
animation.SaveAsAnimatedSVG("animation.svg");
{% endhighlight %}