---
layout: default
nav_order: 17
has_children: true
---

# PDF-specific features

The optional parameters of the `Document.SaveAsPDF` method provided by VectSharp.PDF can be used to implement PDF-specific features. These include:

* A [document outline]({{ site.baseurl }}{% link pdf_specific.outline.md %}) (also known as a table of contents or a set fo bookmarks). This provides an overview of the structure of the document (e.g. chapters, subchapters, etc.) and can be used to navigate it.
* [Optional content groups]({{ site.baseurl }}{% link pdf_specific.ocg.md %}) (also known as layers), which allow users to conditionally hide or show parts of the PDF document.
* [Specifying the appearance of links in the document]({{ site.baseurl }}{% link pdf_specific.annotation_appearance.md %}) (e.g., whether they should be highlighted by a rectangle and what colour should the rectangle be).
* [Document metadata]({{ site.baseurl }}{% link pdf_specific.metadata.md %}), which include information such as the author of the document, the program used to create it, etc.

While all of these features are defined in the PDF format specification, not all PDF viewers completely support all of them. Before using any of these, you should therefore decide if/how the (lack of) support of each feature will impact your use case. Specific examples of viewers that support or do not support each feature are provided in the feature pages.