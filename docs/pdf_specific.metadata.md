---
layout: default
nav_order: 4
parent: PDF-specific features
---

# Document metadata

PDF documents can include metadata describing the document's title, the author, the program that created the document, and so on. Metadata can be customised by providing a `PDFMetadata` object as the `metadata` optional parameter of the `SaveAsPDF` method.

The `PDFMetadata` class defines the following properties:

* `string Title`: the title of the document. Default: empty.
* `string Subject`: the subject of the document. Default: empty.
* `string Author`: the name of the person who created the document. Default: `Environment.UserName`.
* `string Keywords`: keywords for the document. Default: empty.
* `string Creator`: the program that created the document. Default: `"VectSharp vX.Y.Z"`.
* `string Producer`: the program that took the document and converted it to PDF format. Default: `"VectSharp.PDF vX.Y.Z"`.
* `DateTime CreationDate`: date of creation of the document, expressed locally to the `CreationDateTimeZone`. Default: `DateTime.Now`.
* `TimeZoneInfo CreationDateTimeZone`: time zone for the `CreationDate`. Default: `TimeZoneInfo.Local`.
* `DateTime ModificationDate`: date of the last modification of the document, expressed locally to the `ModificationDateTimeZone`. Default: `DateTime.Now`.
* `TimeZoneInfo ModificationDateTimeZone`: time zone for the `ModificationDate`. Default: `TimeZoneInfo.Local`.
* `Dictionary<string, object> CustomProperties`: you can use this dictionary to define custom document metadata properties. Default: `null`.
* `bool ExcludeMetadata`: if you do not wish to include any metadata in the document, set this to `true`. Default: `false`.

The following example shows how to create a PDF document with customised metadata.

<div class="code-example">
    <p style="text-align: center">
        <a href="assets/tutorials/Metadata.pdf" download>Download the example PDF</a>
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.PDF;

// Create the document.
Document doc = new Document();

// Create the page and add it to the document.
Page pag = new Page(595, 842);
doc.Pages.Add(pag);

// Draw something on the page.
pag.Graphics.FillRectangle(20, 20, 555, 802, Colours.Red);

// Create the metadata object.
PDFMetadata pdfMetadata = new PDFMetadata()
{
    Title = "Document title",
    Subject = "Document subject",
    Author = "Your name",
    Keywords = "keyword1;keyword2",

    Creator = "Program that created the document",
    Producer = "Program that created the PDF",
    
    CreationDate = DateTime.Today,
    CreationDateTimeZone = TimeZoneInfo.Local,

    ModificationDate = DateTime.Today,
    ModificationDateTimeZone = TimeZoneInfo.Local
};

// Save the PDF document.
doc.SaveAsPDF("Metadata.pdf", metadata: pdfMetadata);
{% endhighlight %}

If you do not provide the `metadata` parameter to the `SaveAsPDF` method (or if it is set to `null`), the default values are used. If you wish to explicitly exclude metadata from the document, you need to provide a `PDFMetadata` object whose `ExcludeMetadata` property is `false`. The following example shows how to create a PDF document without any metadata.

{% highlight CSharp %}
using VectSharp;
using VectSharp.PDF;

// Create the document.
Document doc = new Document();

// Create the page and add it to the document.
Page pag = new Page(595, 842);
doc.Pages.Add(pag);

// Draw something on the page.
pag.Graphics.FillRectangle(20, 20, 555, 802, Colours.Red);

// Save the PDF document.
doc.SaveAsPDF("Metadata.pdf", metadata: new PDFMetadata() { ExcludeMetadata = true });
{% endhighlight %}
