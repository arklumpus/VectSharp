using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.IO;

namespace VectSharp.MarkdownCanvas
{
    internal static class ImageCache
    {
        private const string imageCacheId = "bb5a724e-93f7-431d-87c3-53f31d8da16e";

        private static string imageCacheFolder;

        private static Dictionary<string, string> imageCache;

        static ImageCache()
        {
            imageCacheFolder = Path.Combine(Path.GetTempPath(), imageCacheId);
            Directory.CreateDirectory(imageCacheFolder);
            imageCache = new Dictionary<string, string>();
        }

        private static bool exitHandlerSet = false;
        public static void SetExitEventHandler()
        {
            if (!exitHandlerSet)
            {
                if (Avalonia.Application.Current.ApplicationLifetime is IControlledApplicationLifetime lifetime)
                {
                    lifetime.Exit += (s, e) =>
                    {
                        try
                        {
                            Directory.Delete(imageCacheFolder, true);
                        }
                        catch
                        {
                            try
                            {
                                foreach (string sr in Directory.GetFiles(imageCacheFolder, "*.*"))
                                {
                                    try
                                    {
                                        File.Delete(sr);
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                        }
                    };
                }
                exitHandlerSet = true;
            }
        }

        public static (string, bool) ImageUriResolver(string imageUri, string baseUriString)
        {
            if (!imageCache.TryGetValue(baseUriString + "|||" + imageUri, out string cachedImage))
            {
                (string imagePath, bool wasDownloaded) = VectSharp.Markdown.HTTPUtils.ResolveImageURI(imageUri, baseUriString);

                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    string id = Guid.NewGuid().ToString();

                    cachedImage = Path.Combine(imageCacheFolder, id + Path.GetExtension(imagePath));

                    if (wasDownloaded)
                    {
                        if (!Directory.Exists(imageCacheFolder))
                        {
                            Directory.CreateDirectory(imageCacheFolder);
                        }

                        File.Move(imagePath, cachedImage);
                        Directory.Delete(Path.GetDirectoryName(imagePath));
                    }
                    else
                    {
                        File.Copy(imagePath, cachedImage);
                    }

                    imageCache[baseUriString + "|||" + imageUri] = cachedImage;
                }
                else
                {
                    cachedImage = null;
                }
            }

            return (cachedImage, false);
        }
    }
}
