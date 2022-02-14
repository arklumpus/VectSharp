---
layout: default
nav_order: 7
---

# Drawing text
{: .no_toc }

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

Drawing text is another very common operation that you may want to perform. This is achieved by using the `FillText` and `StrokeText` methods (as usual, separate methods are used to fill and stroke). Both of these methods require a `Font` parameter, that determines the font that is used to draw the text.

> **Note**: the text features in VectSharp have sort of a Western bias, in the sense that they were designed with a left-to-right text layout in mind and expecting a Latin script. They _might_ work fine with other writing systems/scripts, but unexpected issues may occur at any point.
>
> Unfortunately, I am not familiar with other writing systems, and as a result I cannot provide a reliable implementation (essentially, I would not be able to recognise if something is wrong!). However, if you have any suggestions or comments on specific situations, feel free to open an [issue in the GitHub repository](https://github.com/arklumpus/VectSharp/issues)!

## Fonts and font families

A `Font` object can be created by providing a `FontFamily` and specifying a font size:

{% highlight CSharp %}
// Font family (see below).
FontFamily family = ...

// Font size (in graphics units).
double fontSize = 10;

// Create a new font object.
Font font = new Font(family, fontSize);
{% endhighlight %}


A `FontFamily`, such as `Times Roman` or `Helvetica Bold`, contains information about the shape of the letters (or, "glyphs") that will be drawn, which is scaled using the font size. Note that VectSharp treats version of the same font that differ in style or weight (e.g., `Helvetica`, `Helvetica Bold` and `Helvetica Oblique`) as completely different entities.

A `FontFamily` can be initialised either by providing the path to a [TTF file](https://en.wikipedia.org/wiki/TrueType), or by leveraging one of the fourteen "standard fonts" embedded within VectSharp. These correspond to the standard fonts of the PDF format and are available in the `FontFamily.StandardFontFamilies` enum. They include:

* `Helvetica`, `HelveticaBold`, `HelveticaOblique` and `HelveticaBoldOblique`, representing a sans-serif font.
* `TimesRoman`, `TimesBold`, `TimesItalic` and `TimesBoldItalic`, representing a serif font.
* `Courier`, `CourierBold`, `CourierOblique` and `CourierBoldOblique`, representing a monospaced font.
* `Symbol`, which contains symbols such as greek letters.
* `ZapfDingbats`, which contains various ornamental symbols.

VectSharp embeds an open-source metrics-compatible version of each of these fonts.

When creating a PDF document using VectSharp.PDF, these standard fonts are not embedded within the document (unlike fonts specified via a TTF file), which results in a reduced file size; when the file is opened with a PDF viewer, the program uses its own variants of the standard fonts to typeset the text. Since these fonts all have the same metrics (i.e., glyph width), the result should always look more or less the same.

When an SVG image is created with VectSharp.SVG, instead, a subset of each font that has been used is embedded within the document, including only the glyphs that have been actually used. This is also done for PDF documents with font families that have been specified using a TTF file. This ensures that the text will actually look the same when the file is opened.

This behaviour can be changed by providing a value for the `textOption` parameter when exporting the SVG/PDF. This will be explored in more detail later.

A `FontFamily` object can be created by invoking the `FontFamily.ResolveFontFamily` static method. This method accepts a single parameter, which can be:
* The path to a TTF file (as a `string`), e.g. `/path/to/font.ttf`.
* The name of a standard font as a `string`, e.g. `Helvetica` or `Times Roman`.
* An element of the `FontFamily.StandardFontFamilies` enum, e.g. `FontFamily.StandardFontFamilies.HelveticaBold`.

{% highlight CSharp %}
// Create a FontFamily from a TTF file.
FontFamily family = FontFamily.ResolveFontFamily("/path/to/font.ttf");

// Create a standard FontFamily from a string.
FontFamily family = FontFamily.ResolveFontFamily("Helvetica");

// Create a standard FontFamily from a FontFamily.StandardFontFamilies.
FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
{% endhighlight %}

Note that successive calls to the `FontFamily.ResolveFontFamily` method with the same argument will return the _same object_ (i.e. `FontFamily.ResolveFontFamily("Helvetica") == FontFamily.ResolveFontFamily("Helvetica")` is `true`).

[Back to top](#){: .btn }

## Filling and stroking text

Once you have a `FontFamily` and you have used it to create a `Font`, you are ready to draw text using the `FillText` or `StrokeText` methods. In addition to the usual parameters to specify the fill and stroke colours and the stroke options, these methods have the following specific parameters:

* The coordinates of the origin of the text, specified either as a two `double`s, or as a single `Point`.
* The text to draw (as a `string`).
* The `Font` to use to draw the text.

The following example demonstrates how a simple text string can be drawn:

<div class="code-example">
    <iframe src="Blazor?basicText" style="width: 100%; height: 21em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 100);
Graphics graphics = page.Graphics;

// Create the FontFamily.
FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);

// Create the Font.
Font font = new Font(family, 15);

// The position of the text.
Point position = new Point(15, 40);

// The text to draw.
string text = "VectSharp";

// Stroke the text.
graphics.StrokeText(position, text, font, Colour.FromRgb(0, 80, 44), lineJoin: LineJoins.Round);

// Fill the text.
graphics.FillText(position, text, font, Colour.FromRgb(0, 178, 115));

page.SaveAsSVG("BasicText.svg");
{% endhighlight %}

[Back to top](#){: .btn }

## Positioning text

Precisely positioning text is not an easy task. Normally, when you use the `StrokeText` or `FillText` methods, the origin of the text (i.e., the point that you specify) is interpreted as the top-left corner of the bounding box of the text as it is drawn, including all ascenders, descenders, and bearings.

This can be inconvenient if you wish to draw multiple text strings one after the other. For example, if you draw a lowercase `c` next to an upper case `T` providing points with the same y coordinate, you will notice that they are not aligned:

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/TAndc.svg" style="height: 5em" />
    </p>
</div>
<details markdown="block">
  <summary>
    Expand code
  </summary>
  {: .text-delta }

{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 100);
Graphics graphics = page.Graphics;

FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
Font font = new Font(family, 80);

// The position of the T.
Point positionT = new Point(5, 25);

// The position of the c.
Point positionc = new Point(60, 25);

// Fill the T.
graphics.FillText(positionT, "T", font, Colours.Green);

// Fill the c.
graphics.FillText(positionc, "c", font, Colours.Green);

page.SaveAsSVG("TAndc.svg");
{% endhighlight %}
</details>

You can change this behaviour by supplying the optional `textBaseline` parameter. This is a value of the `TextBaselines` enum, which includes:

* `TextBaselines.Top`, which specifies the default behaviour.
* `TextBaselines.Bottom`, which specifies that the y coordinate of the text position should correspond to the bottom of the text (including any descenders).
* `TextBaselines.Middle`, which specifies that the y coordinate of the text position should correspond to the vertical midpoint of the text (i.e., halfway between `TextBaselines.Top` and `TextBaselines.Bottom`).
* `TextBaselines.Baseline`, which specifies that the y coordinate of the text position should correspond to the baseline of the text, i.e. the line along which the text is drawn.

The following image shows the distinction between these baselines.

<p style="text-align: center">
    <img src="assets/tutorials/Baselines.svg" style="height: 5em" />
</p>

The following example shows how to use `TextBaselines.Baseline` to correctly align the `T` and `c` from the previous example:

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/TAndcFixed.svg" style="height: 5em" />
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 100);
Graphics graphics = page.Graphics;

FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
Font font = new Font(family, 80);

// The position of the T.
Point positionT = new Point(5, 80);

// The position of the c.
Point positionc = new Point(60, 80);

// Fill the T.
graphics.FillText(positionT, "T", font, Colours.Green, textBaseline: TextBaselines.Baseline);

// Fill the c.
graphics.FillText(positionc, "c", font, Colours.Green, textBaseline: TextBaselines.Baseline);

page.SaveAsSVG("TAndcFixed.svg");
{% endhighlight %}

### Precise text positioning

By leveraging the `textBaseline` parameter, you can specify the position of one of four "anchors" on the text: the top-left corner, the bottom-left corner, the left-midpoint, and the left-baseline. What all these points have in common is that they refer to the _left_ side of the text.

If you want to have more control over the horizontal position of the text, you can use the `MeasureText` method of the `Font` object; this method returns a `Size` containing the width and height that the text will have when rendered:

{% highlight CSharp %}
Font font = ...

Size textSize = font.MeasureText("Test text");
{% endhighlight %}

You can then use the `Width` property of the `Size` to align the text.

{% highlight CSharp %}
// Default: the left side of the text is at x = 50.
graphics.FillText(50, 50, "Test text", font, Colours.Black);

// The right side of the text is at x = 50.
graphics.FillText(50 - textSize.Width, 50, "Test text", font, Colours.Black);

// The centre of the text is at x = 50.
graphics.FillText(50 - textSize.Width * 0.5, 50, "Test text", font, Colours.Black);
{% endhighlight %}

To position the text even more precisely, you can use the `MeasureTextAdvanced` method of the `Font` object instead. This returns a `Font.DetailedFontMetrics` object with a number of interesting properties:

{% highlight CSharp %}
Font.DetailedFontMetrics metrics = font.MeasureTextAdvanced("Test text");
{% endhighlight %}

* `Width` and `Height` are the same as returned by the `MeasureText` method - the overall width and height of the rendered text.

* `Top` is the distance between the baseline of the text and the highest point of the tallest glyph in the text (i.e., the distance between the green `Baseline` and the blue `Top` in the figure above). This is normally $> 0$.

* `Bottom` is the distance between the baseline of the text and the lowest point of the longest descender in the text (i.e., the distance between the green `Baseline` and the orange `Bottom` in the figure above). For text containing glyphs that extend below the baseline (such as the `p` in `VectSharp`), this is $< 0$.

  As a result, given the vertical position of the baseline $b$, you can use these values to compute the position of the top $t$, bottom $u$ and middle $m$ of the text:

  $$t = b - \mathrm{Top}$$

  $$u = b - \mathrm{Bottom}$$

  $$m = \frac{t + u}{2} = b - \frac{\mathrm{Top} + \mathrm{Bottom}}{2}$$

The `LeftSideBearing`, `RightSideBearing` and `AdvanceWidth` properties are a bit more complicated, and explaining their meaning requires a bit more of an introduction about how text is typeset. They are described in the section below.

<details markdown="block">
<summary>
Details
</summary>
{: .text-delta }

A text string consists of a number of "glyphs". Conceptually, you can image each glyph as corresponding to a letter. Drawing a text string means positioning the various glyphs on the image one next to the other.

For example, assuming a left-to-right writing direction, to write the string `VectSharp` you would:

* Starting at the specified point, write the `V`.
* Move a bit to the right and write the `e`.
* Move again to the right and write the `c`.
* Etc...

_How much_ should you move to the right? Unless you are using a monospaced font, this varies with each glyph (e.g., an `i` is narrower than an `m`) and is the "advance width" of the glyph. The `AdvanceWidth` property of the `Font.DetailedFontMetrics` object is simply the sum of all the advance widths of the glyphs in the string[^1].

[^1]: Actually, it is a bit more complicated than this, because [kerning](https://en.wikipedia.org/wiki/Kerning) is also applied; however, this does not really matter, unless you are trying to manually typeset the text - which you should not really need to do!

_However_, the actual width of a glyph may be larger than its advance width. As a result, part of the glyph will "hang" over the previous glyph (on the left) or the next glyph (on the right). The amount of glyph that hangs to the left is the "left-side bearing", while the amount that hangs to the right is the "right-side bearing".

The `LeftSideBearing` property of the `Font.DetailedFontMetrics` object corresponds to the left-side bearing of the first glyph in the text string (which determines the left-side bearing of the whole string). If the first glyph overhangs to the left (e.g. an italic letter such as _f_), this has a positive value.

Similarly, the `RightSideBearing` property corresponds to the right-side bearing of the last glyph in the text string. Again, this has a positive value if the glyph overhangs to the right.
</details>

[Back to top](#){: .btn }

## Drawing underlined text

The text you want to draw may need to be underlined. Unlike bold and italic variants, underlined fonts are not specified as separate files; instead, the `Font` constructor takes an optional `bool` argument that determines whether the text is underlined or not.

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/BasicUnderline.svg" style="height: 5em" />
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 25);
Graphics graphics = page.Graphics;

FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);

// Determines whether text is underlined or not.
bool underline = true;

// Create the Font.
Font font = new Font(family, 15, underline);

Point position = new Point(15, 8);
string text = "VectSharp";

// Stroke the text, including the underline.
graphics.StrokeText(position, text, font, Colour.FromRgb(0, 80, 44), lineJoin: LineJoins.Round);

// Fill the text, including the underline.
graphics.FillText(position, text, font, Colour.FromRgb(0, 178, 115));

page.SaveAsSVG("BasicUnderline.svg");
{% endhighlight %}

All `Font` objects have an `Underline` property, which is a `Font.FontUnderline` describing how the text is underlined. If the font was initialised without an underline, this property will be `null`. Instead, if you created the font like above, you can access the properties of the `Underline` object. These include:

* `Position` and `Thickness`, which specify the position and thickness of the underline as a multiple of the font size (e.g., a thickness of `0.0625` will result in an underline that is `1` unit thick with a font size of `16`, and `2` units thick with a font size of `32`). The default values for these properties are determined from the font file.
* `LineCap`, which determines the shape of the ends of the underline (butt, round or square).
* `SkipDescenders`, which is a `bool` that determines whether the underline is drawn as a continuous line, or if it breaks in order not to overlap glyphs that fall below the baseline (which is the default).
* `FollowItalicAngle`: if this is true, for italic fonts the ends of the underline are slanted by the same angle as the font (try it in the example below).

The following example illustrates the effect of these settings.

<div class="code-example">
    <iframe src="Blazor?underline" style="width: 100%; height: 18em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 25);
Graphics graphics = page.Graphics;

FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBoldOblique);
Font font = new Font(family, 15, true);

// Set the position of the underline.
font.Underline.Position = 0.106;

// Set the thickness of the underline.
font.Underline.Thickness = 0.073;

// Set the line cap.
font.Underline.LineCap = LineCaps.Round;

// Do not skip descenders.
font.Underline.SkipDescenders = false;

// Follow the italic angle.
font.Underline.FollowItalicAngle = true;

Point position = new Point(15, 8);
string text = "VectSharp";

// Fill the text, including the underline.
graphics.FillText(position, text, font, Colours.Black);

page.SaveAsSVG("Underline.svg");
{% endhighlight %}

If you want to have finer control over the appearance of the underline, you can draw the text and its underline separately. For example, this makes it possible to have an underline of a different colour than the text.

To do this, you need to create two `Font`s: one with the underline, and one without the underline. Then, you draw the text using the `FillText`/`StrokeText` method with the font without the underline, and use the `FillTextUnderline`/`StrokeTextUnderline` method to only draw the underline. The following example shows how to do this.

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/ColouredUnderline.svg" style="height: 5em" />
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 25);
Graphics graphics = page.Graphics;

FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBoldOblique);

// Font without the underline.
Font notUnderlined = new Font(family, 15, false);

// Font with the underline.
Font underlined = new Font(family, 15, true);

Point position = new Point(15, 8);
string text = "VectSharp";

// Fill the text without the underline.
graphics.FillText(position, text, notUnderlined, Colours.Black);

// Fill the underline without the text.
graphics.FillTextUnderline(position, text, underlined, Colours.Green);

page.SaveAsSVG("ColouredUnderline.svg");
{% endhighlight %}

[Back to top](#){: .btn }

## Drawing text along a path

Normally, text is drawn on a horizontal line. However, using VectSharp you can also make the text flow along a `GraphicsPath` instead. This is achieved with the `FillTextOnPath` and `StrokeTextOnPath` methods.

Instead of a `Point`, the first parameter of these methods is the `GraphicsPath` on which the text is drawn. These methods also have two optional parameters that determine the alignment of the text on the path:

* `anchor` determines the anchoring of the text to the path (i.e. whether the anchor is on the left side of the text, on the right side, or at the centre).
* `reference` is a `double` that determines the position of the anchor with respect to the `GraphicsPath`. This value ranges from `0` to `1`, where `0` represents the start of the path and `1` represents the end of the path.

For example, setting the `anchor` to `TextAnchors.Left` and the `reference` to `0` (which is the default) will align the left side of the text with the start of the path. Instead, setting `anchor` to `TextAnchors.Center` and the `reference` to `0.5` will align the centre of the text with the centre of the path. Finally, an `anchor` of `TextAnchors.Right` and a `reference` of `1` will align the right side of the text with the end of the path.

The following example shows how to draw text along a path.

<div class="code-example">
    <iframe src="Blazor?textOnPath" style="width: 100%; height: 20em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 100);
Graphics graphics = page.Graphics;

FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
Font font = new Font(family, 15);

// Text to draw.
string text = "VectSharp";

// Create the GraphicsPath object.
GraphicsPath path = new GraphicsPath().MoveTo(20, 30).CubicBezierTo(50, 20, 80, 40, 80, 70);

// Reference to the middle of the path.
double reference = 0.5;

// Anchor the centre of the text.
TextAnchors anchor = TextAnchors.Center;

// Fill the text flowing on the path.
graphics.FillTextOnPath(path, text, font, Colours.Black, reference: reference, anchor: anchor,
                        textBaseline: TextBaselines.Baseline);

page.SaveAsSVG("TextOnPath.svg");
{% endhighlight %}

Note that if you try to make the text flow on a path with very narrow curves or cusps, the results might be not very appealing. Therefore, this feature is best used with gentle paths.

[Back to top](#){: .btn }

## Drawing formatted text

In addition to drawing simple text strings, you can also draw text with more complicated formatting. This is achieved by using the overloads of the `FillText` and `StrokeText` methods that take a `IEnumerable<FormattedText>` parameter for the `text` instead of a `string`.

A `FormattedText` object represents a single piece of text with a uniform formatting. An `IEnumerable` of these objects thus represents a collection of text spans with different formattings. The easiest way to create such a collection is to use the `FormattedText.Format` method; this method takes a `string` as an parameter and parses formatting information in order to produce an `IEnumerable<FormattedText>`.

When you supply a string to `FormattedText.Format`, you can specify the format of the string using HTML-like tags. For example, `"This text is <b>bold</b>"` would be transformed in a collection of two items: one with text `"This text is "` and a regular font, and one with text `"bold"` and a bold font.

The following format specifiers are currently supported:

* `<b></b>` or `<strong></strong>` produces bold text, e.g. `This text is <b>bold</b>` becomes: "This text is **bold**".
* `<i></i>` or `<em></em>` produces italic text, e.g. `This text is in <i>italics</i>` becomes: "This text is in _italics_".
* `<u></u>` produces underlined text, e.g. `This text is <u>underlined</u>` becomes: "This text is <span style="text-decoration: underline">underlined</span>".
* `<sup></sup>` produces a superscript, e.g. `This text is <sup>superscript</sup>` becomes: "This text is <sup>superscript</sup>".
* `<sub></sub>` produces a subscript, e.g. `This text is <sub>subscript</sub>` becomes: "This text is <sub>subscript</sub>".
* `<#col></#>`, where `col` is a CSS colour specification (e.g. `rgb(0, 178, 115)` or `009E73`) produces coloured text, e.g. `This text is <#009E73>coloured</#>` becomes "This text is <span style="color: #009E73">coloured</span>".

Naturally, you can nest multiple tags; for example `This text is <b>bold and <#009E73>coloured</#></b>` becomes: "This text is <strong>bold and <span style="color: #009E73">coloured</span></strong>".

The `FormattedText.Format` method has two overloads. The first, in addition to the `string` to format, takes a standard font family name as an additional parameter. VectSharp then internally maps the standard font family to its bold and italic variants. This overload also requires the font size as a parameter. Alternatively, the other overload of this method takes four `Font`s as parameter: these will be used to render, respectively, regular text, bold text, italic text and bold-italic text. This is useful if you wish to use a non-standard font collection.

Both parameters have two optional arguments: one is a boolean that determines whether the text is initially underlined (this is roughly equivalent to wrapping the whole string between `<u></u>`, and the other is the colour that is used to draw text that does not specify a colour through the `<#col></#>` tags (if this is not provided, it is determined by the colour provided in the `FillText`/`StrokeText` call).

You can determine the size that a `IEnumerable<FormattedText>` would have when rendered by using the `Measure` extension method provided on this collection. This produces a `Font.DetailedFontMetrics` object that is analogous to the one produced by the `Font.MeasureTextAdvanced` method for a simple string.

The following example shows how to render some formatted text.

<div class="code-example">
    <iframe src="Blazor?formattedText" style="width: 100%; height: 15em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(300, 30);
Graphics graphics = page.Graphics;

// String that needs to be formatted.
string text = "This text is <b>bold, <#009E73>coloured, and <u>underlined</u>!</#></b>";

// Format the text
IEnumerable<FormattedText> formattedText = FormattedText.Format(text,
                                                                FontFamily.StandardFontFamilies.Helvetica,
                                                                14);

// Measure the size of the formatted text.
Font.DetailedFontMetrics metrics = formattedText.Measure();

// Fill the formatted text, centering it in the page.
graphics.FillText(150 - metrics.Width * 0.5, 15, formattedText, Colours.Black, TextBaselines.Middle);

page.SaveAsSVG("FormattedText.svg");
{% endhighlight %}

[Back to top](#){: .btn }

## Adding text to a `GraphicsPath`

Finally, in addition to directly drawing the text on the `Graphics` object, you may also want to add it to a `GraphicsPath` instead. To do this, you can use the `AddText`, `AddTextOnPath` and `AddTextUnderline` methods of the `GraphicsPath` object. These work in the same way as the `FillText`, `FillTextOnPath` and `FillTextUnderline` methods, and take the same arguments (of course, they do not require parameters to determine the fill or stroke colour or the stroke options).

Adding the text to a `GraphicsPath` can be useful if you wish to perform some post-processing using the advanced path functions, which are described in [another section]({% link advanced_transformations.md %}). It can also be necessary in order to use the text as a clipping path (again, described in [another section]({% link clipping.md %})).

[Back to top](#){: .btn }

## Exporting documents containing text

When you use the `SaveAsSVG` or `SaveAsPDF` methods to create PDF or SVG documents, you can use the optional `textOption` parameter to determine what should happen to text that has been drawn on the page.

### VectSharp.SVG

For the `SaveAsSVG` method, the `SVGContextInterpreter.TextOptions` enum has four possible values:

* `EmbedFonts` specifies that the whole TTF file of the font will be embedded within the SVG file.
* `SubsetFonts` specifies that only a subset of the TTF file will be embedded, containing the glyphs that have actually been used in the image. This is the default.
* `DoNotEmbed` specifies that no font file should be embedded in the document. When you open the SVG file, if you have the font installed in your system everything should be fine; otherwise the text may be drawn using a different font.
* `ConvertIntoPaths` converts all text elements into paths, thus obviating the need for an embedded font file.

If you plan to edit the text included in the SVG file later, it might be appropriate to use the `EmbedFonts` option, so that the full font is available and you do not have any missing glyphs. On the other end, if you wish to edit the SVG file without modifying the text, using the `ConvertIntoPaths` options might be better, because some editors (such as Inkscape) do not support embedded font files.

The default setting (`SubsetFonts`) is recommended if you do not plan to alter the SVG image after creating it with VectSharp.

### VectSharp.PDF

For the `SaveAsPDF` method, the `PDFContextInterpreter.TextOptions` enum has two possible values, `SubsetFonts` (the default) and `ConvertIntoPaths`, which are equivalent to the options with the same name in VectSharp.SVG.

However, an important note is that if you use the standard fonts, the font file will not be embedded within the PDF, even if `SubsetFonts` is selected. This is because a metrics-compatible version of the standard fonts is supposed to be available with any PDF viewing program. If you wish to make sure that the document looks exactly the same in any viewer, you may want to use the `ConvertIntoPaths` option instead.

An important note is that for both VectSharp.SVG and VectSharp.PDF, using the `ConvertIntoPaths` option means that the text is not recognised as such by viewer programs any more: therefore, it will not be possible e.g. to search, highlight or copy text from the document.

It is also possible to [force the PDF document to include a standard font file](#forcing-a-pdf-document-to-embed-the-standard-fonts).

### VectSharp.Canvas

The `PaintToCanvas` and `PaintToSKCanvas` methods of VectSharp.Canvas also have an optional `textOption` parameter.

The `AvaloniaContextInterpreter.TextOptions` enum has three possible values:

* `AlwaysConvert` specifies that text should always be converted into paths before being drawn on the Avalonia `Canvas`.
* `NeverConvert` specifies that text should never be converted into paths when it is drawn on the `Canvas`.
* `ConvertIfNecessary` (the default) specifies that text should only be converted into paths if it is necessary to preserve its appearance.

For the `PaintToCanvas` method, converting the text is necessary if it is drawn using a non-standard font. If you use the `NeverConvert` option in this case, the text may be drawn by Avalonia using a default font instead. If you use the `AlwaysConvert` option when text is only drawn using standard fonts, this will result in reduced performance.

For the `PaintToSKCanvas` method, converting the text is only necessary on Linux (because the SkiaSharp method to draw text produces a really ugly result).

[Back to top](#){: .btn }

## Custom fonts and font libraries

In addition to using the 14 standard fonts, you may also want to create text using your own custom fonts. As explained above, this can be achieved by providing the `FontFamily.ResolveFontFamily` method with the path to a TTF file; alternatively, you can also create a `FontFamily` from a TTF file that has been loaded in a `Stream`. This is useful, for example, if you wish to include your font as an embedded resource, or if you want to force the PDF exporter to embed one of the standard font files.

> **Note**: always make sure that the font files you are using are in TTF format and do not use any OpenType features that are not available in TrueType (except for kerning information in the `GPOS` table). If you download a variable font file from Google Fonts, use the "static" font files instead (there are available in the `static` subfolder when you download the font family).
>
> It is also a good idea to try opening the files you created on a system where the fonts you used are not installed, before publishing them.
>
> In any case, **always** make sure that you comply with the terms of each font's license!

To create a `FontFamily` from a `Stream`, you can use the constructor that takes a `Stream` as its only parameter (unlike other constructors, this one is not deprecated):

{% highlight CSharp %}
using VectSharp;

// Load the TTF stream from somewhere (e.g. an embedded resource).
Stream ttfStream = ...

// Create the FontFamily from the stream.
FontFamily family = new FontFamily(ttfStream);

// Create the Font as normal.
Font font = new Font(family, 15);

// ...
{% endhighlight %}

However, if you export a PDF document with a font family that was created in this way, you may find out that the font is not included, and it is replaced with a default font instead. To avoid this, you need to create a new font library to replace the default font library, and add the new font family to it.

A font library is an instance of a class implementing the `IFontLibrary` interface. This interface defines a number of overloads for the `ResolveFontFamily` method; two of these simply resolve a font family from a `string` or a `FontFamily.StandardFontFamilies` enum, while the others make it possible to resolve a font family providing fallback options. A default implementation for the latter is provided by the abstract class `FontLibrary` (in short, if you wish to create a new `IFontLibrary` implementation, it is easier to inherit from `FontLibrary` than to directly implement `IFontLibrary`).

VectSharp currently contains two implementations of the `IFontLibrary` interface: the `DefaultFontLibrary` and the `SimpleFontLibrary`.

The `DefaultFontLibrary` class (which, unsurprisingly, is the default font library when nothing else is specified) implements a font library that resolves the standard font families using the font files embedded with VectSharp. It can also create a `FontFamily` from a TTF file. However, this library cannot be extended, in the sense that you cannot add to this library a `FontFamily` that was created in other ways (e.g. from a `Stream`).

The `SimpleFontLibrary` class, instead, provides this possibility. Once you add a `FontFamily` to this font library, you can then resolve it using its name (e.g., `"Open Sans"`). 

> **Note**: if you add multiple fonts from the same family (e.g., Open Sans Regular and Open Sans Bold), you will need to use the full name of the font family to ensure that you get the correct variation! If you are in doubt, you can check the `FontFamily`'s `FamilyName` property to determine the name that you should use.

The following example shows how to use a `SimpleFontLibrary` to use custom fonts.

{% highlight CSharp %}
using VectSharp;

// Create a new SimpleFontLibrary.
SimpleFontLibrary fontLibrary = new SimpleFontLibrary();

// Set the new font library as the default. This will re-route
// all calls to FontFamily.ResolveFontFamily to our new fontLibrary.
FontFamily.DefaultFontLibrary = fontLibrary;

// Load the TTFs from somewhere (e.g. an embedded resource).
// Pretend that we are loading the Open Sans Regular and Bold fonts.
Stream ttfStreamRegular = ...
Stream ttfStreamBold = ...

// Create the FontFamilies from the streams.
FontFamily familyRegular = new FontFamily(ttfStreamRegular);
FontFamily familyBold = new FontFamily(ttfStreamBold);
// Note that familyRegular.FamilyName is "Open Sans Regular" and
// familyBold.FamilyName is "Open Sans Bold".

// Add the new font families to the font library.
fontLibrary.Add(familyRegular);
fontLibrary.Add(familyBold);

// Create the Fonts as normal.
Font fontRegular = new Font(familyRegular, 15);
Font fontBold = new Font(familyBold, 15);

// You can now resolve the font families using their names.
FontFamily familyRegular2 = FontFamily.ResolveFontFamily("Open Sans Regular");
FontFamily familyBold2 = FontFamily.ResolveFontFamily("Open Sans Bold");
// Note that familyRegular2 == familyRegular and familyBold2 == familyBold.

// However, make sure you use an unambiguous family name!
FontFamily familyAmbiguous = FontFamily.ResolveFontFamily("Open Sans");
// Here, familyAmbiguous == familyBold!
{% endhighlight %}

Normally, you will want to create the `SimpleFontLibrary` and load all the custom fonts at the start of your program. Then, when you actually need the font families, you invoke the `FontFamily.ResolveFontFamily` method with the font family name.

Using font libraries like this opens up a few interesting possibilities. For example, the [VectSharp.Fonts.Nimbus](https://www.nuget.org/packages/VectSharp.Fonts.Nimbus/) package provides replacements for the 14 standard fonts (these were the fonts that were included in the original GPL release of VectSharp; since the library moved to LGPL, a different set of fonts had to be used). To use these fonts, after installing the NuGet package, you need to set the Nimbus font library as default:

{% highlight CSharp %}
using VectSharp;

FontFamily.DefaultFontLibrary = VectSharp.Fonts.Nimbus.Library;

{% endhighlight %}

Other possibilities for future development include creating a font library that resolves font families from Google Fonts and downloads them as necessary, or one that loads fonts that are installed on the system (provided that there is a cross-platform way to access them).

[Back to top](#){: .btn }

## Forcing a PDF document to embed the standard fonts

Normally, when you create a PDF document, the 14 standard fonts are not embedded, and the PDF viewer program that is used to open the file uses its own standard fonts to display the text. However, you may wish instead to force the library to embed the standard fonts. 

The way to do this is to "trick" the library into thinking that you are not using one of the standard fonts. You can achieve this by creating a new font family using the stream of one of the standard fonts. Note that you will also have to follow the font library pattern described above.

The following example shows how to access the stream of one of the standard fonts and how to use it to create a new `FontFamily`.

{% highlight CSharp %}
using VectSharp;

// Create the font library and set it as default.
SimpleFontLibrary fontLibrary = new SimpleFontLibrary();
FontFamily.DefaultFontLibrary = fontLibrary;

// Get the name of the embedded resource containing the standard font.
string helveticaResourceName = FontFamily.StandardFontFamilyResources[(int)FontFamily.StandardFontFamilies.Helvetica];

// Get the corresponding stream.
Stream helveticaStream = typeof(FontFamily).Assembly.GetManifestResourceStream(helveticaResourceName);

// Create the FontFamily.
FontFamily helveticaForced = new FontFamily(helveticaStream);

// Add the FontFamily to the library.
fontLibrary.Add(helveticaForced);

// Create the Font as normal.
Font font = new Font(helveticaForced, 15);
{% endhighlight %}

[Back to top](#){: .btn }

---
