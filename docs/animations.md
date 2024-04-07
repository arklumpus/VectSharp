---
layout: default
nav_order: 20
has_children: true
---

# Animations

VectSharp can be also be used to create animations, which can be saved as animated GIF, PNG or SVG files. Each format has its own advantages and disadvantages:

* Animated GIFs are widely supported by most software and are generally small in size. However, they are limited to a 50 fps framerate, and they can only store up to 255 colours plus a single transparent colour for each frame (better compression can be achieved by using the same 255+1 colours for all frames). These can be created by using the VectSharp.Raster.ImageSharp NuGet package.

* Animated PNGs enjoy more limited support, though most modern web browsers (e.g. Chrome, Firefox, Edge) appear to display them correctly. They are generally larger in size than an equivalent animated GIF, but they have a maxium framerate of 90fps and can store full 32bpp (24bpp colour + 8bpp alpha) images. Animated PNGs can be create using both VectSharp.Raster and VectSharp.Raster.ImageSharp. As usual, VectSharp.Raster is faster than VectSharp.Raster.ImageSharp, but it requires a native dependency. Additionally, if you have another way of creating the individual frames of the animation and storing them as raw pixel data, you can use the base VectSharp NuGet package to create an animated PNG out of those frames.

* Animated SVGs are also supported by most web browsers and are usually small in size. They have no intrinsic framerate (which means that the framerate is determined by the program used to view them), and, unlike GIFs and PNGs, display the animations as vector images (which means e.g. that they can be resized without losing quality). As these normally use native features of SVG/SMIL, there are a few effects that cannot be recreated properly in animated SVGs (e.g., transitions between a linear brush and a gradient brush). To work around this issue, it is also possible to export the animated SVG as a series of frames; however, this will cause the size of the output file to increase dramatically. Finally, animated SVGs can include javascript-based controls that affect the playback of the animation (e.g. play/pause). Animated SVGs can be created by using the VectSharp.SVG NuGet package.

* Animated `Canvas`es can also be created using VectSharp.Canvas and displayed in Avalonia applications.

In short, here is a quick guide on deciding the best animation format:

* If the target supports viewing animated SVGs and your animation does not use any unsupported effects, use a regular animated SVG.
    * If the target supports animated SVGs, but your animation uses unsupported effects and file size is not an issue, use an animated SVG with individual frames. 
    * Otherwise, if you need a small file size, use an animated GIF.
* If you need or prefer a raster animation and the target supports animated PNGs, create both an animated PNG and an animated GIF and compare the file sizes.
    * If the file size of the animated PNG is not an issue, use that, otherwise fall back to the GIF.

Here are two examples of the kind of animations that can be created:

<p style="text-align: center">
    <object data="assets/images/VectSharp_Animation.svg" style="width: 90%">
    </object>
</p>


<p style="text-align: center">
    <object data="assets/images/bouncingBall_4.svg" style="width: 90%">
    </object>
</p>
