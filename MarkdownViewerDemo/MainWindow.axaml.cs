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
