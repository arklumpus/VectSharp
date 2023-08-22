/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2022 Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

#nullable disable

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using VectSharp.SVG;

namespace MarkdownViewerDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            VectSharp.SVG.Parser.ParseImageURI = VectSharp.MuPDFUtils.ImageURIParser.Parser(VectSharp.SVG.Parser.ParseSVGURI);

            InitializeComponent();

            this.FindControl<VectSharp.MarkdownCanvas.MarkdownCanvasControl>("MarkdownCanvas").Renderer.RasterImageLoader = image => new VectSharp.MuPDFUtils.RasterImageFile(image);
            
            this.FindControl<VectSharp.MarkdownCanvas.MarkdownCanvasControl>("MarkdownCanvas").DocumentSource = @"# Markdown viewer 

This is an example of a Markdown viewer program that uses a `MarkdownCanvasControl` to display the contents of a Markdown file.

Use the button at the top to open your own Markdown source file.

Most Markdown features are supported; see the readme in the [VectSharp.Markdown GitHub repository](https://github.com/arklumpus/VectSharp/tree/master/VectSharp.Markdown) for a list of supported and unsupported features.

When the container is resized, the document is automatically reflowed in order to fill all the available horizontal space. 

External links are opened in the default web browser, while internal links cause the control to scroll to the location of the target.

Images are downloaded from the internet the first time they are required; the results are cached and reused for subsequent requests. The cache is cleared when the application exits (if the application crashes, the cache is cleared the next time the application exits successfully).

As a proof-of-concept, an interesting use case would be to display syntax-highlighted (read-only) source code:

```CSharp
using System; 
  
namespace Hello
{  
    class World
    {
        static void Main(string[] args)
        {      
            Console.WriteLine(""Hello World!""); 
        } 
    } 
} 
```";
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OpenClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog() { Title = "Open Markdown file..." };

            string[] result = await dialog.ShowAsync(this);

            if (result != null && result.Length > 0)
            {
                string markdownSource = System.IO.File.ReadAllText(result[0]);

                this.FindControl<VectSharp.MarkdownCanvas.MarkdownCanvasControl>("MarkdownCanvas").Renderer.BaseImageUri = System.IO.Path.GetDirectoryName(result[0]) + "/";
                this.FindControl<VectSharp.MarkdownCanvas.MarkdownCanvasControl>("MarkdownCanvas").Renderer.BaseLinkUri = new Uri(System.IO.Path.GetDirectoryName(result[0]) + "/");

                this.FindControl<VectSharp.MarkdownCanvas.MarkdownCanvasControl>("MarkdownCanvas").DocumentSource = markdownSource;
            }
        }
    }
}
