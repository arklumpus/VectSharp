---
layout: default
nav_order: 1
parent: Animations
---

# Creating a simple animation
{: .no_toc }

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

## `Frame`s and `Transition`s

Animations in VectSharp are created by defining a number of "keyframes" that determine the status of the animation at a specific time. Each frame has a duration (i.e., the amount of time for which it will be displayed), and between consecutive frames a "transition" happens, with its own duration.

The following code shows an example of how to create an animation with two key frames:

{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

// Create a Graphics object to hold the first frame.
Graphics frame0Contents = new Graphics();
// Draw a green circle on the first frame
frame0Contents.FillPath(new GraphicsPath().Arc(10, 10, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 158, 115));
// Create the first frame, with a duration of 1000ms.
Frame frame0 = new Frame(frame0Contents, 1000);

// Create a Graphics object to hold the second frame.
Graphics frame1Contents = new Graphics();
// Draw a blue circle on the second frame.
frame1Contents.FillPath(new GraphicsPath().Arc(90, 90, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 114, 178));
// Create the second frame, also with a duration of 1000ms.
Frame frame1 = new Frame(frame1Contents, 1000);

// Create the transition between frame0 and frame1, again with a duration of 1000ms.
Transition transition0_1 = new Transition(1000);

// Create the animation object, specifying the width, height and linearisation resolution.
Animation animation = new Animation(100, 100, 1);
// Add the first frame to the animation.
animation.AddFrame(frame0);
// Add the second frame to the animation, with the transition we created earlier.
animation.AddFrame(frame1, transition0_1);

// Save the animation as an animated SVG file.
animation.SaveAsAnimatedSVG("animation.svg");
{% endhighlight %}

The final parameter in the `Animation` constructor (the linearisation resolution) is used when morphing a path into a different path; this is not the case here, so any value will work. For more information on this, see [Linearising paths]({{ site.baseurl }}{% link linearising.md %}).

The result should be similar to the following (the progress bar at the top has been added for the sake of this example, and will not be present in your output file):

<p style="text-align: center">
    <object data="assets/images/simple_animation_0.svg" style="height: 25em">
    </object>
</p>

## Tagging objects

As you will notice, the animation does not move smoothly between the two frames, but rather changes "abruptly" upon the end of the transition. This is because at this stage VectSharp does not have any information on what should be "morphed" between one frame and the next one; we can provide this information by adding a `tag` while drawing the circle:

{% highlight CSharp %}
// ...

frame0Contents.FillPath(new GraphicsPath().Arc(10, 10, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 158, 115), tag: "circle");

// ...

frame1Contents.FillPath(new GraphicsPath().Arc(90, 90, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 114, 178), tag: "circle");

// ...
{% endhighlight %}

When creating the animation, VectSharp will identify elements with the same `tag`, and automatically morph them during the transition between consecutive frames. The resulting animation should be similar to the following (again, the progress bar has just been added for clarity and will not be present in your output file):

<p style="text-align: center">
    <object data="assets/images/simple_animation_1.svg" style="height: 25em">
    </object>
</p>

As you will notice, the circle now moves smoothly between the starting and end position, changing colour in the process.

Most plot elements can be morphed into other elements of the same kind:

* Rectangles can be morphed into other rectangles (attributes such as size, fill colour, stroke colour, line width etc. will morph continuously).
* Paths can be morphed into other paths. If the start and end path have the same number of figures (closed contours), and each figure in the start path has the same number and type of segments as the corresponding figure in the end path, the points will simply be interpolated; otherwise the paths will have to be linearised to create this correspondence (this is where the "linearisation resolution" parameter of the `Animation` constructor comes into play).
* Text can be morphed into other text. Note that attributes such as the position or the fill colour will change smoothly between the starting and end position, while other things (such as the text itself) will change abruptly. If you wish to morph text, you should draw it as a path (see [Morphing text]({{ site.baseurl }}{% link morphing_text.md %})).
* Transforms can be morphed into other transforms. Note that depending on the kind of transform, the transition may take "unexpected" paths (e.g., a rotation starting at angle 0 and ending at angle $\frac{3}{2}\pi$ may actually go in the opposite direction as the one you would expect - if this is a problem, you should add more key frames so that every rotation has to be animated by less than $\pi$).
* When drawing objects, brushes can be morphed into other brushes, both of the same kind and of a different kind. For example, a `SolidColourBrush` can be morphed into a `LinearGradientBrush` or into a `RadialGradientBrush`. However, because of limitations in SVG animations, morphing a `LinarGradientBrush` into a `RadialGradientBrush` may produce unexpected results - if you need this kind of effect, you should save the animation as an animated GIF or PNG, or as an animated SVG with frames.
* Filters can be morphed into filters of the same kind (e.g., a `GaussianBlurFilter` can be morphed into another `GaussianBlurFilter` with a different standard deviation).

In most instances, you just have to make sure to use the same `tag` for objects that need to be morphed into one another, and the library will take care of performing the right interpolation. Note that for complex paths this might take a while, so you may want to "help" it, e.g. by splitting a single complex path into smaller elements (more on this in the [Morphing text]({{ site.baseurl }}{% link morphing_text.md %}) page).

## Saving the animation in different formats

VectSharp can save an animation in various different formats.

### Animated GIFs

Animated GIFs are one of the most established animated image formats, despite their limitations (maximum framerate of 50fps and 255 colours + 1 transparent colour for each frame). To create an animated GIF, you will need the VectSharp.Raster.ImageSharp NuGet package:

{% highlight CSharp %}
using VectSharp;
using VectSharp.Raster.ImageSharp;

// ...

animation.SaveAsAnimatedGIF("animation.gif");
{% endhighlight %}

The `SaveAsAnimatedGIF` method has three of overloads (one saving the image to a file, one saving the image to a `Stream` and one saving it to an ImageSharp `Image` object), and a number of optional parameters:

* `double scale`: the scale at which the raster image will be rendered.
* `double frameRate`: the framerate of the animation, in frames per second (fps). The maximum value for this is 50, values higher than 50 will be capped at this value.
* `double durationScaling`: this parameter is used to multiply the duration of the animation - for example, an animation that has a total duration of 10 seconds with a `durationScaling` of `2` will actually last 20 seconds.
* `GifColorTableMode colorTableMode`: this parameter determines whether a different colour table is used for each frame (which produces bigger but better-looking files), or whether a single colour table is used for all frames (which produces smaller files).

### Animated PNGs

Animated PNGs unfortunately are not as widely supported as GIFs, though they produce better-looking images (since there is no 256-colour restriction and full 8-bit alpha transparency can be used) and have a higher maximum framerate (90 fps). To create an animated PNG, you can use the VectSharp.Raster or VectSharp.Raster.ImageSharp NuGet packages (VectSharp.Raster is faster, but it has a native dependency; on the other hand, VectSharp.Raster.ImageSharp is slower and requires more RAM).

{% highlight CSharp %}
using VectSharp;
using VectSharp.Raster.ImageSharp;
// or using VectSharp.Raster;

// ...

animation.SaveAsAnimatedPNG("animation.png");

{% endhighlight %}

The `SaveAsAnimatedPNG` method has two overloads, saving to a file or to a `Stream`, and a few optional parameters:

* `double scale`: the scale at which the raster image will be rendered.
* `double frameRate`: the framerate of the animation, in frames per second (fps). The maximum value for this is 90, values higher than 90 will be capped at this value.
* `double durationScaling`: this parameter is used to multiply the duration of the animation - for example, an animation that has a total duration of 10 seconds with a `durationScaling` of `2` will actually last 20 seconds.
* `InterframeCompression interframeCompression`: this parameter determines what kind of inter-frame compression is used. If this is set to `InterframeCompression.None`, no inter-frame compression is used. If this is set to `InterframeCompression.First`, frames are compressed by encoding only the difference between each frame and the first frame of the animation. If this is set to `InterframeCompression.Previous`, frames are compressed  by only encoding the difference between each frame and the previous frame. These settings have no effect on the appearance of the image, but only on the file size (`None` produces the largest files, while `Previous` produces the smallest). Note that `Previous` requires all frames to be rendered in memory, which will increase RAM usage. Note also that if the animation has a transparent background, no compression can be performed, and this parameter is ignored.

### Animated SVGs

Animated SVGs are supported by most major browsers, and can generally produce high-quality animations with small file sizes. However, there are a few small features that are not supported by animated SVGs (such as transitions between a `LinearGradientBrush` and a `RadialGradientBrush`). To create an animated SVG, you will need the VectSharp.SVG NuGet package:

{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

// ...

animation.SaveAsAnimatedSVG("animation.svg");

{% endhighlight %}

The `SaveAsAnimatedSVG` method has three overloads (saving to a file, to a `Stream` or to an `XmlDocument`) and a few optional parameters:

* `bool includeControls`: if this is `true`, javascript-based playback controls are included in the output file, which can be used to pause and change the position of the animation (as if it were a proper "video"). Otherwise, these controls are not included.
* `double durationScaling`: this parameter is used to multiply the duration of the animation - for example, an animation that has a total duration of 10 seconds with a `durationScaling` of `2` will actually last 20 seconds.
* `textOption`, `linkDestinations` and `filterOption`: these parameters have the same function as for regular static SVG images.

Furthermore, the `SaveAsAnimatedSVGWithFrames` method can be used to create an animated SVG that actually encodes every frame, instead of relying on SVG transitions (SMIL):

{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

// ...

animation.SaveAsAnimatedSVGWithFrames("animation.svg");

{% endhighlight %}

This produces larger files, but may work around some of the compatibility issues with certain features (such as transitions between `LinearGradientBrush`es and `RadialGradientBrush`es). This method also has three overloads saving to a file, to a `Stream` or to an `XmlDocument`, and the same optional parameters as the previous method, with the addition of the `frameRate` parameter, which determines the frame rate of the animation in fps (no maximum value, but naturally the higher this is, the larger the output file will be).

### Animated Canvas

When using VectSharp.Canvas to display graphics in an Avalonia application, it is also possible to create an animated canvas out of an `Animation` object:

{% highlight CSharp %}
using VectSharp;
using VectSharp.Canvas;

// ...

AnimatedCanvas canvas = animation.PaintToAnimatedCanvas();

{% endhighlight %}

You can then place the `AnimatedCanvas` object (which is a `Control`) e.g. in a `Window` or in a `Viewbox`.

The `PaintToAnimatedCanvas` method has a number of optional parameters:

* `double frameRate`: the framerate of the animation, in frames per second (fps). This determines how often the animated canvas is redrawn (invalidated).
* `double durationScaling`: this parameter is used to multiply the duration of the animation - for example, an animation that has a total duration of 10 seconds with a `durationScaling` of `2` will actually last 20 seconds.
* `textOption` and `filterOption`: these parameters have the same meaning as for normal static canvases.

The `AnimatedCanvas` class also has some interesting properties:

* `int CurrentFrame`: gets/sets the current frame of the animation (between `0` and `FrameCount - 1`).
* `int FrameCount`: gets the total number of frames in the animation.
* `double FrameRate`: gets the framerate of the animation.
* `bool IsPlaying`: can be get/set to determine whether the animation is playing or has been paused.

You can use these properties to control playback of the animation (e.g., you can set `IsPlaying` to false and manually update the `CurrentFrame`) or to implement something like animation controls.