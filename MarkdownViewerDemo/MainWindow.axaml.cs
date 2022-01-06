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

namespace MarkdownViewerDemo
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            VectSharp.SVG.Parser.ParseImageURI = VectSharp.MuPDFUtils.ImageURIParser.Parser(VectSharp.SVG.Parser.ParseSVGURI);

            InitializeComponent();

            this.FindControl<VectSharp.MarkdownCanvas.MarkdownCanvasControl>("MarkdownCanvas").Renderer.RasterImageLoader = image => new VectSharp.MuPDFUtils.RasterImageFile(image);
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
