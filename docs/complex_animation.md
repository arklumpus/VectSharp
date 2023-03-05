---
layout: default
nav_order: 3
parent: Animations
---

# A complex animation
{: .no_toc }

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

By combining multiple key frames and easings, VectSharp makes it possible to create rather complex animations. For example, let us say we want to animate a "bouncing ball". This is what the end result should look like:

<p style="text-align: center">
    <object data="assets/images/bouncingBall_4.svg" style="height: 22em">
    </object>
</p>

## Animating transforms

We can start by creating the start and end point of each "bounce":

<div class="code-example">
<p style="text-align: center">
    <object data="assets/images/bouncingBall_0.svg" style="height: 22em">
    </object>
</p>
</div>
<details markdown="block">
<summary>
    Expand source code
  </summary>
  {: .text-delta }


{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

// Create the animation object, specifying the width, height and linearisation resolution.
Animation animation = new Animation(200, 100, 1);

// Starting X and Y position of the ball.
double startingX = 10;
double startingY = 10;

// Y coordinate of the "floor".
double floorY = 90;

// Amount of energy conserved in each bounce.
double damping = 0.75;

// Speed of the ball on the X axis.
double xSpeed = 20;

// Current time in the animation.
double currTime = 0;

// Create the frames in a loop.
for (int i = 0; i < 20; i++)
{
    // Create a Graphics object to hold the frame.
    Graphics frameContents = new Graphics();

    // Coordinates of the ball in the current frame.
    double x, y;

    // The ball moves along the X axis at a constant speed.
    x = startingX + xSpeed * currTime;

    // In even frames, the ball will be in the air.
    if (i % 2 == 0)
    {
        // The height of each bounce can be computed using the damping factor.
        y = startingY + (floorY - startingY) * (1 - Math.Pow(damping, i / 2));
    }
    // In odd frames, the ball is on the floor.
    else
    {
        y = floorY;
    }

    // Draw the ball at the appropriate coordinates, in green.
    frameContents.FillPath(new GraphicsPath().Arc(x, y, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 158, 115), tag: "ball");

    // Create a new Frame, with a duration of 0s (each bounce is istantaneous).
    Frame frame = new Frame(frameContents, 0);

    // The amount of time between bounces (i.e. the duration of the transition) depends on how much the ball
    // needs to travel vertically.
    double duration = Math.Pow(damping, i / 2);

    // Transition between the previous frame and the current frame.
    Transition transition = null;

    // Create a new transition, with the right duration (except for the first frame).
    if (i > 0)
    {
        transition = new Transition(duration * 1000);
    }

    // Add the frame to the animation.
    animation.AddFrame(frame, transition);

    // Increase the current time.
    currTime += duration;
}

// Save the animation as an animated SVG file.
animation.SaveAsAnimatedSVG("animation.svg");
{% endhighlight %}
</details>

The ball is bouncing, but its movement does not look very natural. Recall from high-school physics, that when an object falls towards the ground and bounces (neglecting air friction), its trajectory looks like a parabola; instead, in our animation the ball moves in straight-line segments.

## Using easings to approximate motion

To address this, we can use easings. We can decompose the motion of the bouncing bool in two motions: one along the X axis, which happens at a constant speed (linear easing), and one along the Y axis, which happens at a constant acceleration (which we can simulate using spline easings).

To do this in VectSharp, instead of drawing the ball at coordinates `x` and `y`, we can draw the ball at `(0, 0)`, and apply two translation transforms to move it. Then, we do not apply any easing to the horizontal translation, and instead apply a spline easing to the vertical translation.

Unfortunately, to do this in an animated SVG, we need to save the animation using the `SaveAsAnimatedSVGWithFrames` method, as animated transforms are not properly supported in SVG files otherwise. This is shown in the following example:

<div class="code-example">
<p style="text-align: center">
    <object data="assets/images/bouncingBall_1.svg" style="height: 22em">
    </object>
</p>
</div>
<details markdown="block">
<summary>
    Expand source code
  </summary>
  {: .text-delta }
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

// Create the animation object, specifying the width, height and linearisation resolution.
Animation animation = new Animation(200, 100, 1);

// Starting X and Y position of the ball.
double startingX = 10;
double startingY = 10;

// Y coordinate of the "floor".
double floorY = 90;

// Amount of energy conserved in each bounce.
double damping = 0.75;

// Speed of the ball on the X axis.
double xSpeed = 20;

// Current time in the animation.
double currTime = 0;

// Create the frames in a loop.
for (int i = 0; i < 20; i++)
{
    // Create a Graphics object to hold the frame.
    Graphics frameContents = new Graphics();

    // Coordinates of the ball in the current frame.
    double x, y;

    // The ball moves along the X axis at a constant speed.
    x = startingX + xSpeed * currTime;

    // In even frames, the ball will be in the air.
    if (i % 2 == 0)
    {
        // The height of each bounce can be computed using the damping factor.
        y = startingY + (floorY - startingY) * (1 - Math.Pow(damping, i / 2));
    }
    // In odd frames, the ball is on the floor.
    else
    {
        y = floorY;
    }

    // Apply the translation to the X axis.
    frameContents.Translate(x, 0, tag: "xTranslation");

    // Apply the translation to the Y axis.
    frameContents.Translate(0, y, tag: "yTranslation");

    // Draw the ball at 0, 0, in green.
    frameContents.FillPath(new GraphicsPath().Arc(0, 0, 10, 0, 2 * Math.PI), Colour.FromRgb(0, 158, 115), tag: "ball");

    // Create a new Frame, with a duration of 0s (each bounce is istantaneous).
    Frame frame = new Frame(frameContents, 0);

    // The amount of time between bounces (i.e. the duration of the transition) depends on how much the ball
    // needs to travel vertically.
    double duration = Math.Pow(damping, i / 2);

    // Create the easing for the Y axis translation.
    Dictionary<string, IEasing> easings = new Dictionary<string, IEasing>();
    
    // When i is odd, the ball is falling down: start slow and accelerate.
    if (i % 2 == 1)
    {
        easings.Add("yTranslation", new SplineEasing(new Point(0.5414, 0.0353), new Point(0.8650, 0.7570)));
    }
    // When i is even, the ball is rising up: start fast and slow down.
    else
    {
        easings.Add("yTranslation", new SplineEasing(new Point(0.1350, 0.2430), new Point(0.4585, 0.9647)));
    }
    

    // Transition between the previous frame and the current frame.
    Transition transition = null;

    // Create a new transition, with the right duration (except for the first frame).
    if (i > 0)
    {
        transition = new Transition(duration * 1000, easings: easings);
    }

    // Add the frame to the animation.
    animation.AddFrame(frame, transition);

    // Increase the current time.
    currTime += duration;
}

// Save the animation as an animated SVG file. Note that we need to save the animation
// using the SaveAsAnimatedSVGWithFrames method, as transform animations are not fully
// supported in SVG animations otherwise.
animation.SaveAsAnimatedSVGWithFrames("animation.svg");
{% endhighlight %}
</details>

## Adding depth with a gradient

The bouncing of the ball looks much more natural now! We can also add "depth" to the ball and make it look more like a sphere, by adding a shading to it. This can be achieved by using a `RadialGradientBrush`:

<div class="code-example">
<p style="text-align: center">
    <object data="assets/images/bouncingBall_2.svg" style="height: 22em">
    </object>
</p>
</div>
<details markdown="block">
<summary>
    Expand source code
  </summary>
  {: .text-delta }
{% highlight CSharp %}
// ...

// Create a radial gradient to shade the ball.
RadialGradientBrush ballGradient = new RadialGradientBrush(new Point(-4, -4), new Point(-2, -2), 20,
    new GradientStop(Colour.FromRgb(118, 242, 195), 0),  // Highlight
    new GradientStop(Colour.FromRgb(0, 158, 115), 0.35), // Base colour
    new GradientStop(Colour.FromRgb(0, 0, 0), 1));       // Shadow

// Draw the ball at 0, 0 using the gradient.
frameContents.FillPath(new GraphicsPath().Arc(0, 0, 10, 0, 2 * Math.PI), ballGradient, tag: "ball");

// ...
{% endhighlight %}
</details>

## Adding a shadow

We can further improve the impression of depth by adding a shadow to the ball. To make it look more natural, we can use a skew transform to warp the shape of the shadow:

<div class="code-example">
<p style="text-align: center">
    <object data="assets/images/bouncingBall_3.svg" style="height: 22em">
    </object>
</p>
</div>
<details markdown="block">
<summary>
    Expand source code
  </summary>
  {: .text-delta }
{% highlight CSharp %}
// Create the animation object, specifying the width, height and linearisation resolution.
Animation animation = new Animation(200, 100, 1);

// Starting X and Y position of the ball.
double startingX = 10;
double startingY = 10;

// Y coordinate of the "floor".
double floorY = 90;

// Amount of energy conserved in each bounce.
double damping = 0.75;

// Speed of the ball on the X axis.
double xSpeed = 20;

// Current time in the animation.
double currTime = 0;

// Create the frames in a loop.
for (int i = 0; i < 20; i++)
{
    // Create a Graphics object to hold the frame.
    Graphics frameContents = new Graphics();

    // Coordinates of the ball in the current frame.
    double x, y;

    // The ball moves along the X axis at a constant speed.
    x = startingX + xSpeed * currTime;

    // In even frames, the ball will be in the air.
    if (i % 2 == 0)
    {
        // The height of each bounce can be computed using the damping factor.
        y = startingY + (floorY - startingY) * (1 - Math.Pow(damping, i / 2));
    }
    // In odd frames, the ball is on the floor.
    else
    {
        y = floorY;
    }

    // Apply the translation to the X axis.
    frameContents.Translate(x, 0, tag: "xTranslation");

    // Apply the translation to the Y axis.
    frameContents.Translate(0, y, tag: "yTranslation");

    // Save the graphics context.
    frameContents.Save();

    // Apply the skew. On odd frames, the shadow should be aligned with the ball,
    // as both are on the floor.
    if (i % 2 == 1)
    {
        frameContents.Transform(1, // Scale X
            0,  // Skew X
            -2, // Skew Y
            1,  // Scale Y
            20, // Translate X
            0,  // Translate Y
            tag: "skew");
    }
    // On even frames, the shadow should be further away from the ball, as the shadow
    // is still on the floor, while the ball is in the air. The shadow should also be
    // larger.
    else
    {
        frameContents.Transform(1 + 0.5 * Math.Pow(damping, i / 2), // Scale X
            0, // Skew X
            -2 * (1 + Math.Pow(damping, i / 2)), // Skew Y
            1 + 0.5 * Math.Pow(damping, i / 2), // Scale Y
            20 * (1 + Math.Pow(damping, i / 2)) + 50 * Math.Pow(damping, i / 2), // Translate X
            -5 * Math.Pow(damping, i / 2), // Translate Y
            tag: "skew");
    }

    // Draw the shadow.
    frameContents.FillPath(new GraphicsPath().Arc(0, 0, 10, 0, 2 * Math.PI), Colour.FromRgb(128, 128, 128), tag: "shadow");

    // Remove the skew transform.
    frameContents.Restore();

    // Create a radial gradient to shade the ball.
    RadialGradientBrush ballGradient = new RadialGradientBrush(new Point(-4, -4), new Point(-2, -2), 20,
        new GradientStop(Colour.FromRgb(118, 242, 195), 0),  // Highlight
        new GradientStop(Colour.FromRgb(0, 158, 115), 0.35), // Base colour
        new GradientStop(Colour.FromRgb(0, 0, 0), 1));       // Shadow

    // Draw the ball at 0, 0 using the gradient.
    frameContents.FillPath(new GraphicsPath().Arc(0, 0, 10, 0, 2 * Math.PI), ballGradient, tag: "ball");

    // Create a new Frame, with a duration of 0s (each bounce is istantaneous).
    Frame frame = new Frame(frameContents, 0);

    // The amount of time between bounces (i.e. the duration of the transition) depends on how much the ball
    // needs to travel vertically.
    double duration = Math.Pow(damping, i / 2);

    // Create the easing for the Y axis translation.
    Dictionary<string, IEasing> easings = new Dictionary<string, IEasing>();

    // When i is odd, the ball is falling down: start slow and accelerate.
    if (i % 2 == 1)
    {
        easings.Add("yTranslation", new SplineEasing(new Point(0.5414, 0.0353), new Point(0.8650, 0.7570)));
        // As the amount of skew depends on the position on the Y axis, we need to apply the same easing
        // to the skew transformation as well.
        easings.Add("skew", new SplineEasing(new Point(0.5414, 0.0353), new Point(0.8650, 0.7570)));
    }
    // When i is even, the ball is rising up: start fast and slow down.
    else
    {
        easings.Add("yTranslation", new SplineEasing(new Point(0.1350, 0.2430), new Point(0.4585, 0.9647)));
        // As the amount of skew depends on the position on the Y axis, we need to apply the same easing
        // to the skew transformation as well.
        easings.Add("skew", new SplineEasing(new Point(0.1350, 0.2430), new Point(0.4585, 0.9647)));
    }


    // Transition between the previous frame and the current frame.
    Transition transition = null;

    // Create a new transition, with the right duration (except for the first frame).
    if (i > 0)
    {
        transition = new Transition(duration * 1000, easings: easings);
    }

    // Add the frame to the animation.
    animation.AddFrame(frame, transition);

    // Increase the current time.
    currTime += duration;
}

// Save the animation as an animated SVG file. Note that we need to save the animation
// using the SaveAsAnimatedSVGWithFrames method, as transform animations are not fully
// supported in SVG animations otherwise.
animation.SaveAsAnimatedSVGWithFrames("animation.svg");
{% endhighlight %}
</details>

## Blurring the shadow

Finally, for the shadow to feel more natural, it should be crisper when it is closer to the object, and more blurred when it is further away. We can achieve this effect by using and animating a Gaussian blur filter:

<div class="code-example">
<p style="text-align: center">
    <object data="assets/images/bouncingBall_4.svg" style="height: 22em">
    </object>
</p>
</div>
<details markdown="block">
<summary>
    Expand source code
  </summary>
  {: .text-delta }
{% highlight CSharp %}
// Create the animation object, specifying the width, height and linearisation resolution.
Animation animation = new Animation(200, 100, 1);

// Starting X and Y position of the ball.
double startingX = 10;
double startingY = 10;

// Y coordinate of the "floor".
double floorY = 90;

// Amount of energy conserved in each bounce.
double damping = 0.75;

// Speed of the ball on the X axis.
double xSpeed = 20;

// Current time in the animation.
double currTime = 0;

// Create the frames in a loop.
for (int i = 0; i < 20; i++)
{
    // Create a Graphics object to hold the frame.
    Graphics frameContents = new Graphics();

    // Coordinates of the ball in the current frame.
    double x, y;

    // The ball moves along the X axis at a constant speed.
    x = startingX + xSpeed * currTime;

    // In even frames, the ball will be in the air.
    if (i % 2 == 0)
    {
        // The height of each bounce can be computed using the damping factor.
        y = startingY + (floorY - startingY) * (1 - Math.Pow(damping, i / 2));
    }
    // In odd frames, the ball is on the floor.
    else
    {
        y = floorY;
    }

    // Apply the translation to the X axis.
    frameContents.Translate(x, 0, tag: "xTranslation");

    // Apply the translation to the Y axis.
    frameContents.Translate(0, y, tag: "yTranslation");

    // Save the graphics context.
    frameContents.Save();

    // Apply the skew. On odd frames, the shadow should be aligned with the ball,
    // as both are on the floor.
    if (i % 2 == 1)
    {
        frameContents.Transform(1, // Scale X
            0,  // Skew X
            -2, // Skew Y
            1,  // Scale Y
            20, // Translate X
            0,  // Translate Y
            tag: "skew");
    }
    // On even frames, the shadow should be further away from the ball, as the shadow
    // is still on the floor, while the ball is in the air. The shadow should also be
    // larger.
    else
    {
        frameContents.Transform(1 + 0.5 * Math.Pow(damping, i / 2), // Scale X
            0, // Skew X
            -2 * (1 + Math.Pow(damping, i / 2)), // Skew Y
            1 + 0.5 * Math.Pow(damping, i / 2), // Scale Y
            20 * (1 + Math.Pow(damping, i / 2)) + 50 * Math.Pow(damping, i / 2), // Translate X
            -5 * Math.Pow(damping, i / 2), // Translate Y
            tag: "skew");
    }

    // Create a new Graphics object to hold the shadow that will be blurred.
    Graphics shadow = new Graphics();

    // Draw the shadow.
    shadow.FillPath(new GraphicsPath().Arc(0, 0, 10, 0, 2 * Math.PI), Colour.FromRgb(128, 128, 128), tag: "shadow");

    // On odd frames, the shadow should be crisper, as it is close to the ball.
    if (i % 2 == 1)
    {
        frameContents.DrawGraphics(0, 0, shadow, new GaussianBlurFilter(0.5), tag: "shadowFilter");
    }
    // On even frames, the shadow should be more blurred, as it is further away from the ball.
    // The distance between the ball and the shadow depends on the height of the bounce.
    else
    {
        frameContents.DrawGraphics(0, 0, shadow, new GaussianBlurFilter(0.5 + 10 * Math.Pow(damping, i / 2)), tag: "shadowFilter");
    }

    // Remove the skew transform.
    frameContents.Restore();

    // Create a radial gradient to shade the ball.
    RadialGradientBrush ballGradient = new RadialGradientBrush(new Point(-4, -4), new Point(-2, -2), 20,
        new GradientStop(Colour.FromRgb(118, 242, 195), 0),  // Highlight
        new GradientStop(Colour.FromRgb(0, 158, 115), 0.35), // Base colour
        new GradientStop(Colour.FromRgb(0, 0, 0), 1));       // Shadow

    // Draw the ball at 0, 0 using the gradient.
    frameContents.FillPath(new GraphicsPath().Arc(0, 0, 10, 0, 2 * Math.PI), ballGradient, tag: "ball");

    // Create a new Frame, with a duration of 0s (each bounce is istantaneous).
    Frame frame = new Frame(frameContents, 0);

    // The amount of time between bounces (i.e. the duration of the transition) depends on how much the ball
    // needs to travel vertically.
    double duration = Math.Pow(damping, i / 2);

    // Create the easing for the Y axis translation.
    Dictionary<string, IEasing> easings = new Dictionary<string, IEasing>();

    // When i is odd, the ball is falling down: start slow and accelerate.
    if (i % 2 == 1)
    {
        easings.Add("yTranslation", new SplineEasing(new Point(0.5414, 0.0353), new Point(0.8650, 0.7570)));
        // As the amount of skew depends on the position on the Y axis, we need to apply the same easing
        // to the skew transformation as well.
        easings.Add("skew", new SplineEasing(new Point(0.5414, 0.0353), new Point(0.8650, 0.7570)));
        // As the amount of blurring depends on the position on the Y axis, we need to apply the same easing
        // to the blur filter transformation as well.
        easings.Add("shadowFilter", new SplineEasing(new Point(0.5414, 0.0353), new Point(0.8650, 0.7570)));
    }
    // When i is even, the ball is rising up: start fast and slow down.
    else
    {
        easings.Add("yTranslation", new SplineEasing(new Point(0.1350, 0.2430), new Point(0.4585, 0.9647)));
        // As the amount of skew depends on the position on the Y axis, we need to apply the same easing
        // to the skew transformation as well.
        easings.Add("skew", new SplineEasing(new Point(0.1350, 0.2430), new Point(0.4585, 0.9647)));
        // As the amount of blurring depends on the position on the Y axis, we need to apply the same easing
        // to the blur filter transformation as well.
        easings.Add("shadowFilter", new SplineEasing(new Point(0.1350, 0.2430), new Point(0.4585, 0.9647)));
    }


    // Transition between the previous frame and the current frame.
    Transition transition = null;

    // Create a new transition, with the right duration (except for the first frame).
    if (i > 0)
    {
        transition = new Transition(duration * 1000, easings: easings);
    }

    // Add the frame to the animation.
    animation.AddFrame(frame, transition);

    // Increase the current time.
    currTime += duration;
}

// Save the animation as an animated SVG file. Note that we need to save the animation
// using the SaveAsAnimatedSVGWithFrames method, as transform animations are not fully
// supported in SVG animations otherwise.
animation.SaveAsAnimatedSVGWithFrames("animation.svg");
{% endhighlight %}
</details>