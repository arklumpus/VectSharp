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
using Avalonia.Media;
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
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OpenClicked(object sender, RoutedEventArgs e)
        {
            System.Collections.Generic.IReadOnlyList<Avalonia.Platform.Storage.IStorageFile> result = await this.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions()
            {
                 AllowMultiple = false,
                 Title = "Open Markdown file..."
            });

            if (result != null && result.Count == 1)
            {
                await using System.IO.Stream fileStream = await result[0].OpenReadAsync();
                using System.IO.StreamReader sr = new System.IO.StreamReader(fileStream);

                string markdownSource = sr.ReadToEnd();

                this.FindControl<VectSharp.MarkdownCanvas.MarkdownCanvasControl>("MarkdownCanvas").Renderer.BaseImageUri = System.IO.Path.GetDirectoryName(result[0].Path.ToString()) + "/";
                this.FindControl<VectSharp.MarkdownCanvas.MarkdownCanvasControl>("MarkdownCanvas").Renderer.BaseLinkUri = new Uri(System.IO.Path.GetDirectoryName(result[0].Path.ToString()) + "/");

                this.FindControl<VectSharp.MarkdownCanvas.MarkdownCanvasControl>("MarkdownCanvas").DocumentSource = markdownSource;
            }
        }
    }
}
