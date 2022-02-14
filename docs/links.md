---
layout: default
nav_order: 15
---

# Adding links

Documents in PDF or SVG format can include links; these can be either "local" links to a different part of the document, or links to some URL on the internet.

To create a link in VectSharp, you can use the `tag` feature. You may have noticed that most drawing calls (e.g. `FillRectangle`, `StrokeRectangle`, `FillPath`, `StrokePath`, etc.) accept an optional `tag` parameter, which is a `string` whose default value is `null`. This parameter is used to identify elements in the plot. When you export the image using the `SaveAsSVG` or `SaveAsPDF` methods, you can supply an optional `Dictionary<string, string>` parameter (`linkDestinations`), which associates the tags to the corresponding link.

If a tag is associated with a link that starts with a hash (`#`) symbol, the link is interpreted as being an internal link to the object whose tag corresponds to the linked tag; otherwise, the link is interpreted as an external link.

The following example shows how to create an external link in an SVG image:

<div class="code-example">
    <p style="text-align: center">
        <svg xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 100 25" version="1.1" style="font-synthesis: none;" xmlns="http://www.w3.org/2000/svg">
  <style>
		@font-face
		{			font-family: "Arimo-cf5b2ac1-6998-4f5b-83b3-a7432b4da14a";
			src: url("data:font/ttf;charset=utf-8;base64,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ADAAMgAwACAAVABoAGUAIABBAHIAaQBtAG8AIABQAHIAbwBqAGUAYwB0ACAAQQB1AHQAaABvAHIAcwAgACgAaAB0AHQAcABzADoALwAvAGcAaQB0AGgAdQBiAC4AYwBvAG0ALwBnAG8AbwBnAGwAZQBmAG8AbgB0AHMALwBhAHIAaQBtAG8AKQBBAHIAaQBtAG8AUgBlAGcAdQBsAGEAcgAxAC4AMwAzADsARwBPAE8ARwA7AEEAcgBpAG0AbwAtAFIAZQBnAHUAbABhAHIAQQByAGkAbQBvACAAUgBlAGcAdQBsAGEAcgBWAGUAcgBzAGkAbwBuACAAMQAuADMAMwBBAHIAaQBtAG8ALQBSAGUAZwB1AGwAYQByAEEAcgBpAG0AbwAgAGkAcwAgAGEAIAB0AHIAYQBkAGUAbQBhAHIAawAgAG8AZgAgAEcAbwBvAGcAbABlACAASQBuAGMALgBNAG8AbgBvAHQAeQBwAGUAIABJAG0AYQBnAGkAbgBnACAASQBuAGMALgBTAHQAZQB2AGUAIABNAGEAdAB0AGUAcwBvAG4AQQByAGkAbQBvACAAdwBhAHMAIABkAGUAcwBpAGcAbgBlAGQAIABiAHkAIABTAHQAZQB2AGUAIABNAGEAdAB0AGUAcwBvAG4AIABhAHMAIABhAG4AIABpAG4AbgBvAHYAYQB0AGkAdgBlACwAIAByAGUAZgByAGUAcwBoAGkAbgBnACAAcwBhAG4AcwAgAHMAZQByAGkAZgAgAGQAZQBzAGkAZwBuACAAdABoAGEAdAAgAGkAcwAgAG0AZQB0AHIAaQBjAGEAbABsAHkAIABjAG8AbQBwAGEAdABpAGIAbABlACAAdwBpAHQAaAAgAEEAcgBpAGEAbAAoAHQAbQApAC4AIABBAHIAaQBtAG8AIABvAGYAZgBlAHIAcwAgAGkAbQBwAHIAbwB2AGUAZAAgAG8AbgAtAHMAYwByAGUAZQBuACAAcgBlAGEAZABhAGIAaQBsAGkAdAB5ACAAYwBoAGEAcgBhAGMAdABlAHIAaQBzAHQAaQBjAHMAIABhAG4AZAAgAHQAaABlACAAcABhAG4ALQBFAHUAcgBvAHAAZQBhAG4AIABXAEcATAAgAGMAaABhAHIAYQBjAHQAZQByACAAcwBlAHQAIABhAG4AZAAgAHMAbwBsAHYAZQBzACAAdABoAGUAIABuAGUAZQBkAHMAIABvAGYAIABkAGUAdgBlAGwAbwBwAGUAcgBzACAAbABvAG8AawBpAG4AZwAgAGYAbwByACAAdwBpAGQAdABoAC0AYwBvAG0AcABhAHQAaQBiAGwAZQAgAGYAbwBuAHQAcwAgAHQAbwAgAGEAZABkAHIAZQBzAHMAIABkAG8AYwB1AG0AZQBuAHQAIABwAG8AcgB0AGEAYgBpAGwAaQB0AHkAIABhAGMAcgBvAHMAcwAgAHAAbABhAHQAZgBvAHIAbQBzAC4AaAB0AHQAcAA6AC8ALwB3AHcAdwAuAGcAbwBvAGcAbABlAC4AYwBvAG0ALwBnAGUAdAAvAG4AbwB0AG8ALwBoAHQAdABwADoALwAvAHcAdwB3AC4AbQBvAG4AbwB0AHkAcABlAC4AYwBvAG0ALwBzAHQAdQBkAGkAbwBUAGgAaQBzACAARgBvAG4AdAAgAFMAbwBmAHQAdwBhAHIAZQAgAGkAcwAgAGwAaQBjAGUAbgBzAGUAZAAgAHUAbgBkAGUAcgAgAHQAaABlACAAUwBJAEwAIABPAHAAZQBuACAARgBvAG4AdAAgAEwAaQBjAGUAbgBzAGUALAAgAFYAZQByAHMAaQBvAG4AIAAxAC4AMQAuACAAVABoAGkAcwAgAEYAbwBuAHQAIABTAG8AZgB0AHcAYQByAGUAIABpAHMAIABkAGkAcwB0AHIAaQBiAHUAdABlAGQAIABvAG4AIABhAG4AIAAiAEEAUwAgAEkAUwAiACAAQgBBAFMASQBTACwAIABXAEkAVABIAE8AVQBUACAAVwBBAFIAUgBBAE4AVABJAEUAUwAgAE8AUgAgAEMATwBOAEQASQBUAEkATwBOAFMAIABPAEYAIABBAE4AWQAgAEsASQBOAEQALAAgAGUAaQB0AGgAZQByACAAZQB4AHAAcgBlAHMAcwAgAG8AcgAgAGkAbQBwAGwAaQBlAGQALgAgAFMAZQBlACAAdABoAGUAIABTAEkATAAgAE8AcABlAG4AIABGAG8AbgB0ACAATABpAGMAZQBuAHMAZQAgAGYAbwByACAAdABoAGUAIABzAHAAZQBjAGkAZgBpAGMAIABsAGEAbgBnAHUAYQBnAGUALAAgAHAAZQByAG0AaQBzAHMAaQBvAG4AcwAgAGEAbgBkACAAbABpAG0AaQB0AGEAdABpAG8AbgBzACAAZwBvAHYAZQByAG4AaQBuAGcAIAB5AG8AdQByACAAdQBzAGUAIABvAGYAIAB0AGgAaQBzACAARgBvAG4AdAAgAFMAbwBmAHQAdwBhAHIAZQAuAGgAdAB0AHAAOgAvAC8AcwBjAHIAaQBwAHQAcwAuAHMAaQBsAC4AbwByAGcALwBPAEYATABBAHIAaQBtAG8AUgBvAG0AYQBuAFcAZQBpAGcAaAB0AEEAcgBpAG0AbwAtAEIAbwBsAGQASQB0AGEAbABpAGMAUgBvAG0AYQBuAAMAAAAAAAD/JwCWAAAAAAAAAAAAAAAAAAAAAAAAAAC4Af+FsASNAA==");
		}
</style>
  <defs />
  <path d="M 0 0 L 100 0 L 100 25 L 0 25 Z " stroke="none" fill="#FFFFFF" fill-opacity="0" transform="matrix(1,0,0,1,0,0)" />
  <a href="https://github.com/arklumpus/VectSharp">
    <text stroke="none" fill="#00B273" fill-opacity="1" transform="matrix(1,0,0,1,14.94,18.86)" x="0" y="0" font-size="15" font-family="Arimo-cf5b2ac1-6998-4f5b-83b3-a7432b4da14a, sans-serif" font-weight="regular" font-style="normal" id="link"><tspan>V</tspan><tspan dx="-0.82763671875">ectSharp</tspan></text>
  </a>
  <a href="https://github.com/arklumpus/VectSharp">
    <path d="M 15 21.54798828125 L 15 20.44935546875 L 75.67447265625 20.44935546875 L 75.67447265625 21.54798828125 Z M 79.19009765625 21.54798828125 L 79.19009765625 20.44935546875 L 83.48736328125001 20.44935546875 L 83.48736328125001 21.54798828125 L 79.19009765625 21.54798828125 Z " stroke="none" fill="#00B273" fill-opacity="1" transform="matrix(1,0,0,1,0,0)" id="link" />
  </a>
</svg>
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 25);
Graphics graphics = page.Graphics;

FontFamily family = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
Font font = new Font(family, 15, true);

// Draw the text, tagging it as "link".
graphics.FillText(15, 8, "VectSharp", font, Colour.FromRgb(0, 178, 115), tag: "link");

// Dictionary containing the links.
Dictionary<string, string> links = new Dictionary<string, string>
{
    { "link", "https://github.com/arklumpus/VectSharp" }
};

page.SaveAsSVG("ExternalLink.svg", linkDestinations: links);
{% endhighlight %}

The following example, instead, shows how to create a PDF file with an internal link. Note that internal links are supported in SVG as well, but they are only useful if the SVG image is large enough that it does not fit on the screen. In a PDF document, instead, links can be made to point at different pages.

<div class="code-example">
    <p style="text-align: center">
        <a href="assets/tutorials/InternalLinks.pdf" download>Download the example PDF</a>
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.PDF;

// Create the document.
Document doc = new Document();

Font fontNotUnderlined = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica),
                                  24);
Font fontUnderlined = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica),
                               24, true);

// Create an A4 page.
Page page1 = new Page(595, 842);

// Draw the target for the link at page 2.
page1.Graphics.FillText(50, 50, "Target for link 2", fontNotUnderlined, Colours.Black, tag: "target2");

// Draw the link to page 2.
page1.Graphics.FillText(50, 100, "Link 1", fontUnderlined, Colours.Black, tag: "link1");

// Add the page to the document.
doc.Pages.Add(page1);

// Create another A4 page.
Page page2 = new Page(595, 842);

// Draw the target for the link at page 2.
page2.Graphics.FillText(50, 50, "Target for link 1", fontNotUnderlined, Colours.Black, tag: "target1");

// Draw the link to page 2.
page2.Graphics.FillText(50, 100, "Link 2", fontUnderlined, Colours.Black, tag: "link2");

// Add the page to the document.
doc.Pages.Add(page2);

// Dictionary containing the links.
Dictionary<string, string> links = new Dictionary<string, string>
{
    { "link1", "#target1" },
    { "link2", "#target2" }
};

// Save the document as a PDF.
doc.SaveAsPDF("InternalLinks.pdf", linkDestinations: links);
{% endhighlight %}