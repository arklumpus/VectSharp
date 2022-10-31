/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2022 Giorgio Bianchini, University of Bristol

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>
*/

using Avalonia;
using Avalonia.Controls;
using System.Linq;
using System.Reactive.Linq;
using VectSharp.Canvas;
using VectSharp.Filters;
using VectSharp.Raster.ImageSharp;
using VectSharp.SVG;

namespace VectSharp.DemoAnimation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadDocument();
        }

        private void LoadDocument()
        {
            // Load the VectSharp logo from an embedded SVG file.
            Page vectSharpLogo;
            using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("VectSharp.DemoAnimation.icon.svg"))
            {
                vectSharpLogo = Parser.FromStream(stream);
            }

            // Load another version of the VectSharp logo from an embedded SVG file (this only contains the # symbol without the arrow).
            Page vectSharpLogoOnlySharp;
            using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("VectSharp.DemoAnimation.icon_sharp.svg"))
            {
                vectSharpLogoOnlySharp = Parser.FromStream(stream);
            }

            // Create fonts.
            Font font230 = new Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 230);
            Font font200 = new Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 200);

            // Create a Graphics object contaning the "VectSharp" text. We will need this to blur and mask it.
            Graphics vectSharpTextGraphics = new Graphics();
            // Draw the text.
            vectSharpTextGraphics.FillText(420, 200, "VectSharp", font230, Colours.Black, TextBaselines.Middle);

            // Create a Graphics object to hold the blurred text.
            Graphics blurredVectSharpTextGraphics = new Graphics();
            // Draw the vectSharpTextGraphics object (which contains the text) using a gaussian blur filter.
            // The tag will be necessary for the animation to "connect" the blurred text with the un-blurred text.
            blurredVectSharpTextGraphics.DrawGraphics(0, 0, vectSharpTextGraphics, new GaussianBlurFilter(10), tag: "text_VectSharp");

            // Create a Graphics object to hold the un-blurred text.
            Graphics unBlurredVectSharpTextGraphics = new Graphics();
            // Draw the vectSharpTextGraphics object (which contains the text) using a gaussian blur filter
            // with a very small standard deviation (i.e. practically unblurred). The animation will interpolate the
            // standard deviation between the two values.
            // Again, we are using the same tag, so that the animation can "connect" the blurred text with the un-blurred text.
            unBlurredVectSharpTextGraphics.DrawGraphics(0, 0, vectSharpTextGraphics, new GaussianBlurFilter(1e-5), "text_VectSharp");

            // Create a Graphics object to hold the mask for the text. This initial mask hides all the text.
            Graphics initialMask = new Graphics();
            // Fill a rectangle with a linear gradient going from white (visible) to black (hidden).
            // The tag is necessary for the animation to "connect" the initial mask with the final mask.
            initialMask.FillRectangle(0, 0, 1500, 400, new LinearGradientBrush(new Point(300, 200), new Point(400, 200), new GradientStop(Colours.White, 0), new GradientStop(Colours.White, 0.25), new GradientStop(Colours.Black, 1)), tag: "mask");

            // Create a Graphics object to hold the mask for the text. This final mask shows the whole text.
            Graphics finalMask = new Graphics();
            // Fill a rectangle with a linear gradient going from white (visible) to black (hidden).
            // The animation will interpolate between the two linear gradients. Again, the tag allows the
            // animation to "connect" the two masks.
            finalMask.FillRectangle(0, 0, 1500, 400, new LinearGradientBrush(new Point(300, 200), new Point(1600, 200), new GradientStop(Colours.White, 0), new GradientStop(Colours.White, 0.9375), new GradientStop(Colours.Black, 1)), tag: "mask");

            // Create the first frame. This holds the blurred and hidden text.
            Graphics frame0 = new Graphics();
            // Draw the blurred text with the mask hiding it. Again, the tag will allow the animation to connect this frame with the next.
            frame0.DrawGraphics(0, 0, blurredVectSharpTextGraphics, new MaskFilter(initialMask), "text");

            // Create the second frame. This holds the un-blurred and un-hidden text.
            Graphics frame1 = new Graphics();
            // Draw un-blurred text with the mask not hiding it. Again, the tag will allow the animation to connect this frame with the previous
            // one and to interpolate between the two.
            frame1.DrawGraphics(0, 0, unBlurredVectSharpTextGraphics, new MaskFilter(finalMask), "text");

            // Create the third frame. This holds the "VectSharp" text and the start of the line.
            Graphics frame2 = new Graphics();
            // Draw the text (since this is not blurred or masked anymore, we can just draw the text now, instead of using the Graphics
            // object we created before).
            frame2.FillText(420, 200, "VectSharp", font230, Colours.Black, TextBaselines.Middle);
            // Draw the line. Initially, the line connects the starting point on the right to itself (i.e., it is invisible). Note the tag.
            frame2.StrokePath(new GraphicsPath().MoveTo(1480, 330).LineTo(1480, 330), Colour.FromRgb(5, 147, 12), 15, tag: "line");

            // Create the fourth frame. This holds the "VectSharp" text and the full line.
            Graphics frame3 = new Graphics();
            // Draw the text, same as before. No tag is used for this, because no interpolation needs to take place.
            frame3.FillText(420, 200, "VectSharp", font230, Colours.Black, TextBaselines.Middle);
            // Draw the full line. Thanks to the tag, the animation will interpolate between the previous frame and this frame.
            frame3.StrokePath(new GraphicsPath().MoveTo(1480, 330).LineTo(420, 330), Colour.FromRgb(5, 147, 12), 15, tag: "line");

            // Create the fifth frame, holding the "VectSharp" text, the full line, and the "#" logo hidden by a clipping path.
            Graphics frame4 = new Graphics();
            // Draw the text.
            frame4.FillText(420, 200, "VectSharp", font230, Colours.Black, TextBaselines.Middle);
            // Draw the full line (again, no tag because this is not going to be interpolated).
            frame4.StrokePath(new GraphicsPath().MoveTo(1480, 330).LineTo(420, 330), Colour.FromRgb(5, 147, 12), 15);
            // Set the clipping path. Note the tag, which will again allow the animation to interpolate between the frames.
            frame4.SetClippingPath(400, 20, 0, 380, "clipping");
            // Draw the "#" logo at the right size and in the right place.
            frame4.Save();
            frame4.Translate(20, 20);
            frame4.Scale(380 / vectSharpLogoOnlySharp.Width, 380 / vectSharpLogoOnlySharp.Height);
            frame4.DrawGraphics(0, 0, vectSharpLogoOnlySharp.Graphics);
            frame4.Restore();

            // Create the sixth frame, holding the "VectSharp" text, the full line, and the unveiled "#" logo.
            Graphics frame5 = new Graphics();
            // Draw the text.
            frame5.FillText(420, 200, "VectSharp", font230, Colours.Black, TextBaselines.Middle);
            // Draw the line.
            frame5.StrokePath(new GraphicsPath().MoveTo(1480, 330).LineTo(420, 330), Colour.FromRgb(5, 147, 12), 15);
            // We still need a clipping path, so that the animation can interpolate between the clipping path in the
            // previous frame and the new clipping path (which does not hide anything).
            frame5.SetClippingPath(110, 20, 305, 380, "clipping");
            // Draw the "#" logo at the same size and position as before.
            frame5.Save();
            frame5.Translate(20, 20);
            frame5.Scale(380 / vectSharpLogoOnlySharp.Width, 380 / vectSharpLogoOnlySharp.Height);
            frame5.DrawGraphics(0, 0, vectSharpLogoOnlySharp.Graphics);
            frame5.Restore();

            // Create the seventh frame, holding the "VectSharp" text, the full line, the "#" logo, and the full
            // logo hidden by a clipping path.
            Graphics frame6 = new Graphics();
            // Draw the text.
            frame6.FillText(420, 200, "VectSharp", font230, Colours.Black, TextBaselines.Middle);
            // Draw the line.
            frame6.StrokePath(new GraphicsPath().MoveTo(1480, 330).LineTo(420, 330), Colour.FromRgb(5, 147, 12), 15);
            // Draw the "#" logo at the same size and position as before.
            frame6.Save();
            frame6.Translate(20, 20);
            frame6.Scale(380 / vectSharpLogoOnlySharp.Width, 380 / vectSharpLogoOnlySharp.Height);
            frame6.DrawGraphics(0, 0, vectSharpLogoOnlySharp.Graphics);
            frame6.Restore();
            // Set the clipping path for the full logo (in this frame, the clipping path completely hides the logo).
            frame6.SetClippingPath(new GraphicsPath().MoveTo(0, 380).LineTo(0, 380).LineTo(0, 380).LineTo(0, 380).LineTo(0, 380).Close(), tag: "clipping2");
            // Draw the full logo at the same size and position as the "#" logo.
            frame6.Save();
            frame6.Translate(20, 20);
            frame6.Scale(380 / vectSharpLogo.Width, 380 / vectSharpLogo.Height);
            frame6.DrawGraphics(0, 0, vectSharpLogo.Graphics);
            frame6.Restore();

            // Create the eighth frame, holding the "VectSharp" text, the full line, the "#" logo and the unveiled
            // full logo.
            Graphics frame7 = new Graphics();
            // Draw the text.
            frame7.FillText(420, 200, "VectSharp", font230, Colours.Black, TextBaselines.Middle);
            // Draw the line.
            frame7.StrokePath(new GraphicsPath().MoveTo(1480, 330).LineTo(420, 330), Colour.FromRgb(5, 147, 12), 15);
            // Draw the "#" logo at the same size and position as before. We still need this even though the full logo is
            // unveiled in this frame, as it needs to be present during the interpolation.
            frame7.Save();
            frame7.Translate(20, 20);
            frame7.Scale(380 / vectSharpLogoOnlySharp.Width, 380 / vectSharpLogoOnlySharp.Height);
            frame7.DrawGraphics(0, 0, vectSharpLogoOnlySharp.Graphics);
            frame7.Restore();
            // Set the clipping path for the full logo (in this frame, the clipping path does not hide anything).
            frame7.SetClippingPath(new GraphicsPath().MoveTo(20, 380).LineTo(20, 20).LineTo(280, -80).LineTo(480, 120).LineTo(380, 380).Close(), tag: "clipping2");
            // Draw the full logo at the same size and position as before.
            frame7.Save();
            frame7.Translate(20, 20);
            frame7.Scale(380 / vectSharpLogo.Width, 380 / vectSharpLogo.Height);
            frame7.DrawGraphics(0, 0, vectSharpLogo.Graphics);
            frame7.Restore();

            // Create a GraphicsPath containing the shapes of the letters that make up the "VectSharp" text
            // (at exactly the same position). We will use this to "morph" the "VectSharp" text into a different text string.
            GraphicsPath vectSharpTextPath = new GraphicsPath().AddText(420, 200, "VectSharp", font230, TextBaselines.Middle);
            // Get the figure that make up the text (i.e. the contours of each letter).
            GraphicsPath[] vectSharpFigures = vectSharpTextPath.GetFigures().ToArray();

            // The contour of the "V".
            GraphicsPath VPath = vectSharpFigures[0];
            // The "e" has two contours - one for the outer shape, and one for the "hole".
            GraphicsPath ePath = vectSharpFigures[1].AddPath(vectSharpFigures[2]);
            // The contour of the "c".
            GraphicsPath cPath = vectSharpFigures[3];
            // The contour of the "t".
            GraphicsPath tPath = vectSharpFigures[4];
            // The contour of the "S".
            GraphicsPath SPath = vectSharpFigures[5];
            // The contour of the "h".
            GraphicsPath hPath = vectSharpFigures[6];
            // The "a" also has two contours - one for the outer shape, and one for the "hole".
            GraphicsPath aPath = vectSharpFigures[7].AddPath(vectSharpFigures[8]);
            // The contour of the "r".
            GraphicsPath rPath = vectSharpFigures[9];
            // The "p" also has two contours - one for the outer shape, and one for the "hole".
            GraphicsPath pPath = vectSharpFigures[10].AddPath(vectSharpFigures[11]);

            // Create another GraphicsPath containing the text we want to draw in the end.
            GraphicsPath animationTextPath = new GraphicsPath().AddText(420, 230, "Animation", font200, TextBaselines.Top);
            // Get the figure that make up the text (i.e. the contours of each letter).
            GraphicsPath[] animationFigures = animationTextPath.GetFigures().ToArray();

            // The "A" has two contours - one for the outer shape, and one for the "hole".
            GraphicsPath APath = animationFigures[0].AddPath(animationFigures[1]);
            // The contour of the "n".
            GraphicsPath nPath = animationFigures[2];
            // The "i" also has two contours - one for the main body, and one for the dot.
            GraphicsPath iPath = animationFigures[3].AddPath(animationFigures[4]);
            // The contour of the "m".
            GraphicsPath mPath = animationFigures[5];
            // The "a" also has two contours - one for the outer shape, and one for the "hole".
            GraphicsPath aPath2 = animationFigures[6].AddPath(animationFigures[7]);
            // The contour of the "t".
            GraphicsPath tPath2 = animationFigures[8];
            // Again, the second "i" also has two contours - one for the main body, and one for the dot.
            GraphicsPath iPath2 = animationFigures[9].AddPath(animationFigures[10]);
            // The "o" also has two contours - one for the outer shape, and one for the "hole".
            GraphicsPath oPath = animationFigures[11].AddPath(animationFigures[12]);
            // The contour of the second "n".
            GraphicsPath nPath2 = animationFigures[13];

            // Add additional contours to some letters, to ensure that every letter in "VectSharp" has the same number of
            // contours as the letter of "Animation" in which it is going to morph.
            // The "V" (one contour) will become an "A" (two contours), hence we need to add another contour to the "V".
            // This is done by adding a single line segment connecting the center of the "V" to itself.
            VPath.AddPath(new GraphicsPath().MoveTo(VPath.GetBounds().Centre).LineTo(VPath.GetBounds().Centre).Close());
            // The "e" (two contours) will become an "n" (one contour), hence we need to add another contour to the "n".
            nPath.AddPath(new GraphicsPath().MoveTo(nPath.GetBounds().Centre).LineTo(nPath.GetBounds().Centre).Close());
            // The "c" (one contour) will become an "i" (two contours), hence we need to add another contour to the "c".
            cPath.AddPath(new GraphicsPath().MoveTo(cPath.GetBounds().Centre).LineTo(cPath.GetBounds().Centre).Close());
            // The "S" (one contour) will become an "a" (two contours), hence we need to add another contour to the "S".
            SPath.AddPath(new GraphicsPath().MoveTo(SPath.GetBounds().Centre).LineTo(SPath.GetBounds().Centre).Close());
            // The "r" (one contour) will become an "o" (two contours), hence we need to add another contour to the "r".
            rPath.AddPath(new GraphicsPath().MoveTo(rPath.GetBounds().Centre).LineTo(rPath.GetBounds().Centre).Close());
            // The "p" (two contours) will become an "n" (one contour), hence we need to add another contour to the "n".
            nPath2.AddPath(new GraphicsPath().MoveTo(nPath2.GetBounds().Centre).LineTo(nPath2.GetBounds().Centre).Close());

            // Create the ninth frame, holding the "VectSharp" text, the line, the full logo, and another copy of the
            // "VectSharp" text drawn letter by letter, which will be interpolated and morphed into "Animation" in the
            // next frame.
            Graphics frame8 = new Graphics();
            // Draw the text. This is tagged because we will be changing its size and position in the next frame.
            frame8.FillText(420, 200, "VectSharp", font230, Colours.Black, TextBaselines.Middle, tag: "textVectSharp");
            // Draw again the text, letter by letter, with a different tag for each letter.
            frame8.FillPath(VPath, Colours.Black, tag: "text_V");
            frame8.FillPath(ePath, Colours.Black, tag: "text_e");
            frame8.FillPath(cPath, Colours.Black, tag: "text_c");
            frame8.FillPath(tPath, Colours.Black, tag: "text_t");
            frame8.FillPath(SPath, Colours.Black, tag: "text_S");
            frame8.FillPath(hPath, Colours.Black, tag: "text_h");
            frame8.FillPath(aPath, Colours.Black, tag: "text_a");
            frame8.FillPath(rPath, Colours.Black, tag: "text_r");
            frame8.FillPath(pPath, Colours.Black, tag: "text_p");
            // Draw the line.
            frame8.StrokePath(new GraphicsPath().MoveTo(1480, 330).LineTo(420, 330), Colour.FromRgb(5, 147, 12), 15, tag: "separatorLine");
            // Draw the full logo in the same position as before.
            frame8.Save();
            frame8.Translate(20, 20);
            frame8.Scale(380 / vectSharpLogo.Width, 380 / vectSharpLogo.Height);
            frame8.DrawGraphics(0, 0, vectSharpLogo.Graphics);
            frame8.Restore();

            // Create the tenth frame, holding the "VectSharp" text, the "Animation text" (drawn in green letter by letter),
            // the line, and the full logo.
            Graphics frame9 = new Graphics();
            // Draw the "VectSharp" text, with a smaller font and in a different position than before.
            frame9.FillText(420, 100, "VectSharp", font200, Colours.Black, TextBaselines.Middle, tag: "textVectSharp");
            // Draw the "Animation" text, letter by letter, using the same tags that we used for the "VectSharp" text above.
            frame9.FillPath(APath, Colour.FromRgb(5, 147, 12), tag: "text_V");
            frame9.FillPath(nPath, Colour.FromRgb(5, 147, 12), tag: "text_e");
            frame9.FillPath(iPath, Colour.FromRgb(5, 147, 12), tag: "text_c");
            frame9.FillPath(mPath, Colour.FromRgb(5, 147, 12), tag: "text_t");
            frame9.FillPath(aPath2, Colour.FromRgb(5, 147, 12), tag: "text_S");
            frame9.FillPath(tPath2, Colour.FromRgb(5, 147, 12), tag: "text_h");
            frame9.FillPath(iPath2, Colour.FromRgb(5, 147, 12), tag: "text_a");
            frame9.FillPath(oPath, Colour.FromRgb(5, 147, 12), tag: "text_r");
            frame9.FillPath(nPath2, Colour.FromRgb(5, 147, 12), tag: "text_p");
            // Draw the line.
            frame9.StrokePath(new GraphicsPath().MoveTo(1330, 210).LineTo(420, 210), Colour.FromRgb(5, 147, 12), 15, tag: "separatorLine");
            // Draw the full logo in the same position as before.
            frame9.Save();
            frame9.Translate(20, 20);
            frame9.Scale(380 / vectSharpLogo.Width, 380 / vectSharpLogo.Height);
            frame9.DrawGraphics(0, 0, vectSharpLogo.Graphics);
            frame9.Restore();

            // Create the eleventh frame, holding the "VectSharp" text, the "Animation" text (drawn in black, still letter by letter),
            // the line, and the full logo.
            Graphics frame10 = new Graphics();
            // Draw the "VectSharp" text, same as the previous frame.
            frame10.FillText(420, 100, "VectSharp", font200, Colours.Black, TextBaselines.Middle);
            // Draw the "Animation" text, with the same tag as in the previous frame, but in black rather than in green.
            // The animation will interpolate between the two colours.
            frame10.FillPath(APath, Colours.Black, tag: "text_V");
            frame10.FillPath(nPath, Colours.Black, tag: "text_e");
            frame10.FillPath(iPath, Colours.Black, tag: "text_c");
            frame10.FillPath(mPath, Colours.Black, tag: "text_t");
            frame10.FillPath(aPath2, Colours.Black, tag: "text_S");
            frame10.FillPath(tPath2, Colours.Black, tag: "text_h");
            frame10.FillPath(iPath2, Colours.Black, tag: "text_a");
            frame10.FillPath(oPath, Colours.Black, tag: "text_r");
            frame10.FillPath(nPath2, Colours.Black, tag: "text_p");
            // Draw the line.
            frame10.StrokePath(new GraphicsPath().MoveTo(1330, 210).LineTo(420, 210), Colour.FromRgb(5, 147, 12), 15);
            // Draw the full logo at the same size and in the same position as before.
            frame10.Save();
            frame10.Translate(20, 20);
            frame10.Scale(380 / vectSharpLogo.Width, 380 / vectSharpLogo.Height);
            frame10.DrawGraphics(0, 0, vectSharpLogo.Graphics);
            frame10.Restore();

            // Create a new Graphics object to hold the "shadow" for the "VectSharp" and "Animation" text.
            Graphics animationShadow = new Graphics();
            // Draw the "VectSharp" text with a semi-transparent brush, in the same position as before.
            animationShadow.FillText(420, 100, "VectSharp", font200, Colours.Black.WithAlpha(0.25), TextBaselines.Middle);
            // Draw the "Animation" text with a semi-transparent brush, in the same position as before.
            animationShadow.FillText(420, 230, "Animation", font200, Colours.Black.WithAlpha(0.25), TextBaselines.Top);

            // Create the twelfth frame, holding the "VectSharp" and "Animation" text, the line, the full logo, and the
            // shadows, currently not blurred and perfectly hidden by the actual text.
            Graphics frame11 = new Graphics();
            // Draw the shadow, in the same position as the text, and using a Gaussian blur filter with a very small
            // standard deviation. This way, the shadows will be perfectly aligned with the text itself and will not
            // be visible. The animation will interpolate the position of the shadow and its blurriness.
            frame11.DrawGraphics(0, 0, animationShadow, new GaussianBlurFilter(1e-5), tag: "animationShadow");
            // Draw the "VectSharp" text.
            frame11.FillText(420, 100, "VectSharp", font200, Colours.Black, TextBaselines.Middle);
            // Draw the "Animation" text.
            frame11.FillText(420, 230, "Animation", font200, Colours.Black, TextBaselines.Top);
            // Draw the line.
            frame11.StrokePath(new GraphicsPath().MoveTo(1330, 210).LineTo(420, 210), Colour.FromRgb(5, 147, 12), 15);
            // Draw the full logo at the same size and in the same position as before.
            frame11.Save();
            frame11.Translate(20, 20);
            frame11.Scale(380 / vectSharpLogo.Width, 380 / vectSharpLogo.Height);
            frame11.DrawGraphics(0, 0, vectSharpLogo.Graphics);
            frame11.Restore();

            // Create the thirteenth frame, holding the final result: the "VectSharp" and "Animation" text with shadows,
            // the line, and the full logo.
            Graphics frame12 = new Graphics();
            // Draw the shadows, blurred and shifted with respect to the original text.
            frame12.DrawGraphics(5, 10, animationShadow, new GaussianBlurFilter(10), tag: "animationShadow");
            // Draw the "VectSharp" text.
            frame12.FillText(420, 100, "VectSharp", font200, Colours.Black, TextBaselines.Middle);
            // Draw the "Animation" text.
            frame12.FillText(420, 230, "Animation", font200, Colours.Black, TextBaselines.Top);
            // Draw the line.
            frame12.StrokePath(new GraphicsPath().MoveTo(1330, 210).LineTo(420, 210), Colour.FromRgb(5, 147, 12), 15);
            // Draw the full logo at the same size and in the same position as before.
            frame12.Save();
            frame12.Translate(20, 20);
            frame12.Scale(380 / vectSharpLogo.Width, 380 / vectSharpLogo.Height);
            frame12.DrawGraphics(0, 0, vectSharpLogo.Graphics);
            frame12.Restore();

            // Create the Animation object, with the specified width, height and linearisation resolution.
            // A smaller value for the linearisation resolution (third parameter) produces more accurate path
            // interpolations, but it can increase the size of the output files and the amount of time necessary
            // to produce them.
            Animation animation = new Animation(1500, 400, 5)
            {
                // Repeat the animation just once. If you set this to 0, the animation will loop indefinitely; if
                // you set it to any other number, the animation will restart the specified number of times.
                RepeatCount = 1,
                // Background colour for the animation.
                Background = Colours.White
            };

            // Add the first frame; wait 100ms before proceeding to the next frame.
            animation.AddFrame(new Frame(frame0, 100));
            // Add the second frame; spend 1000ms (1s) transitioning between the first and the second frame,
            // then proceed immediately to the following frame.
            animation.AddFrame(new Frame(frame1, 0), new Transition(1000));
            // Add the third frame, without any transition between the second and third frame.
            animation.AddFrame(new Frame(frame2, 0));
            // Add the fourth frame; spend 1000ms (1s) transitioning between the third and the fourth frame,
            // then proceed immediately to the following frame. Use the specified easing to interpolate elements
            // between the third and fourth frame.
            animation.AddFrame(new Frame(frame3, 0), new Transition(1000, new SplineEasing(new Point(0.75, 0), new Point(1, 1))));
            // Add the fifth frame, without any transition between the fourth and fifth frame.
            animation.AddFrame(new Frame(frame4, 0));
            // Add the sixth frame; spend 350ms transitioning between the fifth and the sixth frame,
            // then proceed immediately to the following frame. Use the specified easing to interpolate elements
            // between the fifth and sixth frame.
            animation.AddFrame(new Frame(frame5, 0), new Transition(350, new SplineEasing(new Point(0, 0), new Point(0.75, 1))));
            // Add the seventh frame, without any transition between the sixth and seventh frame.
            animation.AddFrame(new Frame(frame6, 0));
            // Add the eighth frame; spend 1000ms (1s) transitioning between the seventh and the eighth frame,
            // then proceed immediately to the following frame. Use the specified easing to interpolate elements
            // between the seventh and eighth frame.
            animation.AddFrame(new Frame(frame7, 0), new Transition(1000, new SplineEasing(new Point(0, 0.25), new Point(0.25, 1))));
            // Add the ninth frame, without any transition between the eighth and ninth frame.
            animation.AddFrame(new Frame(frame8, 0));
            // Add the tenth frame; spend 1000ms (1s) transitioning between the ninth and the tenth frame,
            // then proceed immediately to the following frame.
            animation.AddFrame(new Frame(frame9, 0), new Transition(1000));
            // Add the eleventh frame; spend 500ms transitioning between the tenth and the eleventh frame,
            // then wait 200ms before proceeding to the following frame.
            animation.AddFrame(new Frame(frame10, 200), new Transition(500));
            // Add the twelfth frame, without any transition between the eleventh and twelfth frame.
            animation.AddFrame(new Frame(frame11, 0));
            // Add the thirteenth frame; spend 1000ms (1s) transitioning between the twelfth and the thirteenth frame,
            // then proceed immediately to the following frame (i.e. the first frame, if the animation is being looped).
            // Use the specified easing to interpolate elements between the twelfth and thirteenth frame.
            animation.AddFrame(new Frame(frame12, 0), new Transition(1000, new SplineEasing(new Point(0, 0.25), new Point(0.25, 1))));

            // Save the animation as an animated SVG file, using SVG animations (SMIL) to animate the plot elements.
            // Also include some playback controls to play, pause and change the position of the animation.
            animation.SaveAsAnimatedSVG("AnimationSample.svg", true);

            // Save the animation as an animated SVG file at the default frame rate (60fps), encoding each frame as a separate
            // SVG sub-image. This will create a much larger file, but it can produce more faithful results when the animation
            // uses some features.
            animation.SaveAsAnimatedSVGWithFrames("AnimationSample_frames.svg", true);

            // Save the animation as an animated PNG file. Use a web browser to view this, as most image viewer programs do not
            // expect animated PNGs. This uses VectSharp.Raster; you can also use VectSharp.Raster.ImageSharp to obtain the same
            // result, but it will be much slower and require more RAM.
            VectSharp.Raster.Raster.SaveAsAnimatedPNG(animation, "AnimationSample.png");

            // Save the animation as an animated GIF file. This should have a smaller size and wider support than an animated PNG,
            // but it is limited to 256 colours per frame and to 1-bit-per-pixel alpha.
            animation.SaveAsAnimatedGIF("AnimationSample.gif");

            // Transver the animation onto an Avalonia-compatible AnimatedCanvas, at the default frame rate (60fps).
            AnimatedCanvas animatedCanvas = animation.PaintToAnimatedCanvas();
            // Add the animatedCanvas to the Window, by placing it inside the ViewBox.
            this.mainViewBox.Child = animatedCanvas;

            // Set up some minimal playback controls.
            // The slider shows the current frame of the animation. Here we set the maximum value for the slider (i.e., the number of
            // frames in the rendered animation - 1).
            this.progressSlider.Maximum = animatedCanvas.FrameCount - 1;
            // Create a two-way binding between the value of the slider and the current frame of the animation. This will automatically
            // update the slider as the animation plays, and change the position of the animation when the slider's position is manually changed.
            this.progressSlider.Bind(ProgressBar.ValueProperty, animatedCanvas.GetObservable(AnimatedCanvas.CurrentFrameProperty).Select(x => (double)x));
            animatedCanvas.Bind(AnimatedCanvas.CurrentFrameProperty, progressSlider.GetObservable(ProgressBar.ValueProperty).Select(x => (int)x));

            // Create a binding between the "IsPlaying" property of the AnimatedCanvas and the contents of the play/pause button.
            // When IsPlaying is true, the button will contain the text "Pause", while when IsPlaying is false, it will contain the text "Play".
            this.playPauseButton.Bind(Button.ContentProperty, animatedCanvas.GetObservable(AnimatedCanvas.IsPlayingProperty).Select(x => x ? "Pause" : "Play"));
            // Add an event handler for the play/pause button.
            this.playPauseButton.Click += (s, e) =>
            {
                // If the animation is playing.
                if (animatedCanvas.IsPlaying)
                {
                    // Pause the animation.
                    animatedCanvas.IsPlaying = false;
                }
                else
                // If the animation is not playing.
                {
                    // If the animation should loop indefinitely and it is not playing, it means that it has been paused. Just restart it.
                    if (animation.RepeatCount == 0)
                    {
                        animatedCanvas.IsPlaying = true;
                    }
                    else
                    // If the animation should loop a finite amount of times, we need to check...
                    {
                        // If the animation is stopped because it reached the end...
                        if (animatedCanvas.CurrentFrame >= (animatedCanvas.FrameCount - 1) * animation.RepeatCount)
                        {
                            // Reset it to the beginning...
                            animatedCanvas.CurrentFrame = 0;
                            // And start it.
                            animatedCanvas.IsPlaying = true;
                        }
                        else
                        // If the animation has been paused before the end, just restart it.
                        {
                            animatedCanvas.IsPlaying = true;
                        }
                    }
                }
            };
        }
    }
}
