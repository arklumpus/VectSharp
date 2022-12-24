---
layout: default
nav_order: 4
parent: Animations
---

# Morphing text
{: .no_toc }

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
1. TOC
{:toc}
</details>


Interesting effects can be obtained by "morphing" text, i.e. animating a text graphics action so that the text that is drawn smoothly changes between different values. This is an example of what can be achieved:

<p style="text-align: center">
    <object data="assets/images/morphingText_2.svg" style="height: 10em">
    </object>
</p>

## Animating text drawn as a path

If you try simply animating a graphics action drawing some text, you will find that the text changes abruptly in the middle of the transition (while other features such as the colour change smoothly).

<div class="code-example">
<p style="text-align: center">
    <object data="assets/images/morphingText_0.svg" style="height: 10em">
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

// Font for drawing text.
Font font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 15);

// Create a Graphics object to hold the first frame.
Graphics frame0Contents = new Graphics();
// Draw some text in green on the first frame.
frame0Contents.FillText(5, 15, "VectSharp", font, Colour.FromRgb(0, 158, 115), TextBaselines.Baseline, tag: "text");
// Create the first frame, with a duration of 1000ms.
Frame frame0 = new Frame(frame0Contents, 1000);

// Create a Graphics object to hold the second frame.
Graphics frame1Contents = new Graphics();
// Draw some different text in blue on the second frame.
frame1Contents.FillText(5, 15, "Animation", font, Colour.FromRgb(0, 115, 178), TextBaselines.Baseline, tag: "text");
// Create the second frame, also with a duration of 1000ms.
Frame frame1 = new Frame(frame1Contents, 1000);

// Create the transition between frame0 and frame1, again with a duration of 1000ms.
Transition transition0_1 = new Transition(1000);

// Create the animation object, specifying the width, height and linearisation resolution.
Animation animation = new Animation(80, 20, 0.5);
// Add the first frame to the animation.
animation.AddFrame(frame0);
// Add the second frame to the animation, with the transition we created earlier.
animation.AddFrame(frame1, transition0_1);

// Save the animation as an animated SVG file.
animation.SaveAsAnimatedSVG("animation.svg");
{% endhighlight %}
</details>

This is because text actions cannot be directly animated; however, text actions can be converted into paths, and paths can be easily animated:

<div class="code-example">
<p style="text-align: center">
    <object data="assets/images/morphingText_1.svg" style="height: 10em">
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

// Font for drawing text.
Font font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 15);

// Create a Graphics object to hold the first frame.
Graphics frame0Contents = new Graphics();
// Create a GraphicsPath containing the text for the first frame.
GraphicsPath textVectSharp = new GraphicsPath().AddText(5, 15, "VectSharp", font, TextBaselines.Baseline);
// Draw the text in green on the first frame.
frame0Contents.FillPath(textVectSharp, Colour.FromRgb(0, 158, 115),  tag: "text");
// Create the first frame, with a duration of 1000ms.
Frame frame0 = new Frame(frame0Contents, 1000);

// Create a Graphics object to hold the second frame.
Graphics frame1Contents = new Graphics();
// Create a GraphicsPath containing the text for the second frame.
GraphicsPath textAnimation = new GraphicsPath().AddText(5, 15, "Animation", font, TextBaselines.Baseline);
// Draw the text in blue on the second frame.
frame1Contents.FillPath(textAnimation, Colour.FromRgb(0, 115, 178), tag: "text");
// Create the second frame, also with a duration of 1000ms.
Frame frame1 = new Frame(frame1Contents, 1000);

// Create the transition between frame0 and frame1, again with a duration of 1000ms.
Transition transition0_1 = new Transition(1000);

// Create the animation object, specifying the width, height and linearisation resolution.
Animation animation = new Animation(80, 20, 0.5);
// Add the first frame to the animation.
animation.AddFrame(frame0);
// Add the second frame to the animation, with the transition we created earlier.
animation.AddFrame(frame1, transition0_1);

// Save the animation as an animated SVG file.
animation.SaveAsAnimatedSVG("animation.svg");
{% endhighlight %}
</details>

Now the text is indeed being smoothly changed between the two strings. However, we can obtain better results with some manual fine-tuning of the animation process.

## Manually matching figures for animation

When it is asked to morph a path into another path, VectSharp does the following:

* First of all, each "figure" (i.e., closed contour) in the first path is matched with the closest figure in the second path (based on the centre of each figure).
* Then, if one of the paths had more figures than the other, for each figure that has not been matched in the "bigger" path, a new figure is added to the "smaller" path (consisting of a single point located in the centre of the unmatched figure).
* Finally, for each pair of matched figures, additional points are added to one of the figures to ensure that all figures have the same number of points. If necessary, figures are linearised beforehand.

For example, in the animation above the single figure that makes up the `V` in `VectSharp` has been matched with the outer contour of the `A` in `Animation`; the outer contour of the `e` has been matched with the inner contour of the `A`, the inner contour of the `e` has been matched with the `n`, and so on.

To improve the results we can intervene manually matching each letter in the first string with the corresponding letter in the second string. To do this, we need to extract the figures from each path and draw them individually. The figures can be extracted from a `GraphicsPath` using the `GetFigures` method:

{% highlight CSharp %}
// Create a GraphicsPath containing the text for the first frame.
GraphicsPath textVectSharp = new GraphicsPath().AddText(5, 15, "VectSharp", font, TextBaselines.Baseline);

// Get the figures from the text.
GraphicsPath[] vectSharpFigures = textVectSharp.GetFigures().ToArray();

// The V in VectSharp is the first contour.
GraphicsPath vectSharp_V = vectSharpFigures[0];
// The e in VectSharp is made of the second and third contours.
GraphicsPath vectSharp_e = vectSharpFigures[1].AddPath(vectSharpFigures[2]);
// The c in VectSharp is the fourth contour.
GraphicsPath vectSharp_c = vectSharpFigures[3];
// The t in VectSharp is the fifth contour.
GraphicsPath vectSharp_t = vectSharpFigures[4];
// The S in VectSharp is the sixth contour.
GraphicsPath vectSharp_S = vectSharpFigures[5];
// The h in VectSharp is the seventh contour.
GraphicsPath vectSharp_h = vectSharpFigures[6];
// The a in VectSharp is made of the eigth and ninth contours.
GraphicsPath vectSharp_a = vectSharpFigures[7].AddPath(vectSharpFigures[8]);
// The r in VectSharp is the tenth contour.
GraphicsPath vectSharp_r = vectSharpFigures[9];
// The p in VectSharp is made of the eleventh and twelfth contours.
GraphicsPath vectSharp_p = vectSharpFigures[10].AddPath(vectSharpFigures[11]);

// Create a GraphicsPath containing the text for the second frame.
GraphicsPath textAnimation = new GraphicsPath().AddText(5, 15, "Animation", font, TextBaselines.Baseline);

// Get the figures from the text.
GraphicsPath[] animationFigures = textAnimation.GetFigures().ToArray();

// The A in Animation is made of the first and second contours.
GraphicsPath animation_A = animationFigures[0].AddPath(animationFigures[1]);
// The first n in Animation is the third contour.
GraphicsPath animation_n1 = animationFigures[2];
// The first i in Animation is made of the fourth and fifth contours.
GraphicsPath animation_i1 = animationFigures[3].AddPath(animationFigures[4]);
// The m in Animation is the sixth contour.
GraphicsPath animation_m = animationFigures[5];
// The a in Animation is made of the seventh and eigth contours.
GraphicsPath animation_a = animationFigures[6].AddPath(animationFigures[7]);
// The t in Animation is the ninth contour.
GraphicsPath animation_t = animationFigures[8];
// The second i in Animation is made of the tenth and eleventh contours.
GraphicsPath animation_i2 = animationFigures[9].AddPath(animationFigures[10]);
// The o in Animation is made of the twelfth and thirteenth contours.
GraphicsPath animation_o = animationFigures[11].AddPath(animationFigures[12]);
// The second n in Animation is the fourteenth contour.
GraphicsPath animation_n2 = animationFigures[13];
{% endhighlight %}

Finally, we can now draw the figures one by one, tagging them appropriately:

<div class="code-example">
<p style="text-align: center">
    <object data="assets/images/morphingText_2.svg" style="height: 10em">
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

// Font for drawing text.
Font font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 15);

// Create a Graphics object to hold the first frame.
Graphics frame0Contents = new Graphics();

// Create a GraphicsPath containing the text for the first frame.
GraphicsPath textVectSharp = new GraphicsPath().AddText(5, 15, "VectSharp", font, TextBaselines.Baseline);

// Get the figures from the text.
GraphicsPath[] vectSharpFigures = textVectSharp.GetFigures().ToArray();

// The V in VectSharp is the first contour.
GraphicsPath vectSharp_V = vectSharpFigures[0];
// The e in VectSharp is made of the second and third contours.
GraphicsPath vectSharp_e = vectSharpFigures[1].AddPath(vectSharpFigures[2]);
// The c in VectSharp is the fourth contour.
GraphicsPath vectSharp_c = vectSharpFigures[3];
// The t in VectSharp is the fifth contour.
GraphicsPath vectSharp_t = vectSharpFigures[4];
// The S in VectSharp is the sixth contour.
GraphicsPath vectSharp_S = vectSharpFigures[5];
// The h in VectSharp is the seventh contour.
GraphicsPath vectSharp_h = vectSharpFigures[6];
// The a in VectSharp is made of the eigth and ninth contours.
GraphicsPath vectSharp_a = vectSharpFigures[7].AddPath(vectSharpFigures[8]);
// The r in VectSharp is the tenth contour.
GraphicsPath vectSharp_r = vectSharpFigures[9];
// The p in VectSharp is made of the eleventh and twelfth contours.
GraphicsPath vectSharp_p = vectSharpFigures[10].AddPath(vectSharpFigures[11]);

// Draw the letters in green, one by one, on the first frame.
frame0Contents.FillPath(vectSharp_V, Colour.FromRgb(0, 158, 115), tag: "letter1");
frame0Contents.FillPath(vectSharp_e, Colour.FromRgb(0, 158, 115), tag: "letter2");
frame0Contents.FillPath(vectSharp_c, Colour.FromRgb(0, 158, 115), tag: "letter3");
frame0Contents.FillPath(vectSharp_t, Colour.FromRgb(0, 158, 115), tag: "letter4");
frame0Contents.FillPath(vectSharp_S, Colour.FromRgb(0, 158, 115), tag: "letter5");
frame0Contents.FillPath(vectSharp_h, Colour.FromRgb(0, 158, 115), tag: "letter6");
frame0Contents.FillPath(vectSharp_a, Colour.FromRgb(0, 158, 115), tag: "letter7");
frame0Contents.FillPath(vectSharp_r, Colour.FromRgb(0, 158, 115), tag: "letter8");
frame0Contents.FillPath(vectSharp_p, Colour.FromRgb(0, 158, 115), tag: "letter9");

// Create the first frame, with a duration of 1000ms.
Frame frame0 = new Frame(frame0Contents, 1000);

// Create a Graphics object to hold the second frame.
Graphics frame1Contents = new Graphics();

// Create a GraphicsPath containing the text for the second frame.
GraphicsPath textAnimation = new GraphicsPath().AddText(5, 15, "Animation", font, TextBaselines.Baseline);

// Get the figures from the text.
GraphicsPath[] animationFigures = textAnimation.GetFigures().ToArray();

// The A in Animation is made of the first and second contours.
GraphicsPath animation_A = animationFigures[0].AddPath(animationFigures[1]);
// The first n in Animation is the third contour.
GraphicsPath animation_n1 = animationFigures[2];
// The first i in Animation is made of the fourth and fifth contours.
GraphicsPath animation_i1 = animationFigures[3].AddPath(animationFigures[4]);
// The m in Animation is the sixth contour.
GraphicsPath animation_m = animationFigures[5];
// The a in Animation is made of the seventh and eigth contours.
GraphicsPath animation_a = animationFigures[6].AddPath(animationFigures[7]);
// The t in Animation is the ninth contour.
GraphicsPath animation_t = animationFigures[8];
// The second i in Animation is made of the tenth and eleventh contours.
GraphicsPath animation_i2 = animationFigures[9].AddPath(animationFigures[10]);
// The o in Animation is made of the twelfth and thirteenth contours.
GraphicsPath animation_o = animationFigures[11].AddPath(animationFigures[12]);
// The second n in Animation is the fourteenth contour.
GraphicsPath animation_n2 = animationFigures[13];

// Draw the letters in blue, one by one, on the second frame.
frame1Contents.FillPath(animation_A, Colour.FromRgb(0, 115, 178), tag: "letter1");
frame1Contents.FillPath(animation_n1, Colour.FromRgb(0, 115, 178), tag: "letter2");
frame1Contents.FillPath(animation_i1, Colour.FromRgb(0, 115, 178), tag: "letter3");
frame1Contents.FillPath(animation_m, Colour.FromRgb(0, 115, 178), tag: "letter4");
frame1Contents.FillPath(animation_a, Colour.FromRgb(0, 115, 178), tag: "letter5");
frame1Contents.FillPath(animation_t, Colour.FromRgb(0, 115, 178), tag: "letter6");
frame1Contents.FillPath(animation_i2, Colour.FromRgb(0, 115, 178), tag: "letter7");
frame1Contents.FillPath(animation_o, Colour.FromRgb(0, 115, 178), tag: "letter8");
frame1Contents.FillPath(animation_n2, Colour.FromRgb(0, 115, 178), tag: "letter9");

// Create the second frame, also with a duration of 1000ms.
Frame frame1 = new Frame(frame1Contents, 1000);

// Create the transition between frame0 and frame1, again with a duration of 1000ms.
Transition transition0_1 = new Transition(1000);

// Create the animation object, specifying the width, height and linearisation resolution.
Animation animation = new Animation(80, 20, 0.5);
// Add the first frame to the animation.
animation.AddFrame(frame0);
// Add the second frame to the animation, with the transition we created earlier.
animation.AddFrame(frame1, transition0_1);

// Save the animation as an animated SVG file.
animation.SaveAsAnimatedSVG("animation.svg");
{% endhighlight %}
</details>