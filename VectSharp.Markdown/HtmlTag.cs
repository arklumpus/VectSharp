using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VectSharp.Markdown
{
    internal class HtmlTag
    {
        public Dictionary<string, string> Attributes { get; }
        public string Tag { get; }

        private HtmlTag(string tag, Dictionary<string, string> attributes)
        {
            this.Tag = tag;
            this.Attributes = attributes;
        }

        private static HtmlTag ParseTag(StringReader reader)
        {
            StringBuilder tagBuilder = new StringBuilder();

            int character = reader.Read();

            while (character >= 0 && (char)character != '<')
            {
                character = reader.Read();
            }

            if ((char)character == '<')
            {
                character = reader.Read();
            }

            while (character >= 0 && char.IsWhiteSpace((char)character))
            {
                character = reader.Read();
            }

            while (character >= 0 && !char.IsWhiteSpace((char)character) && (char)character != '>')
            {
                tagBuilder.Append((char)character);
                character = reader.Read();
            }

            string tag = tagBuilder.ToString();

            Dictionary<string, string> attributes = new Dictionary<string, string>();

            (string, string)? attribute = ReadAttribute(reader, ref character);

            while (attribute != null && character >= 0)
            {
                attributes[attribute.Value.Item1] = attribute.Value.Item2;
                attribute = ReadAttribute(reader, ref character);
            }

            return new HtmlTag(tag, attributes);
        }

        public static IEnumerable<HtmlTag> ParseTagsUntil(StringReader reader, string targetTag)
        {
            HtmlTag tag = ParseTag(reader);

            while (tag.Tag != targetTag && reader.Peek() >= 0)
            {
                if (tag.Tag.Equals("p", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (HtmlTag nestedTag in ParseTagsUntil(reader, "/p"))
                    {
                        if (nestedTag.Tag != "/p")
                        {
                            foreach (KeyValuePair<string, string> kvp in tag.Attributes)
                            {
                                if (!nestedTag.Attributes.ContainsKey(kvp.Key))
                                {
                                    nestedTag.Attributes[kvp.Key] = kvp.Value;
                                }
                            }

                            yield return nestedTag;
                        }
                    }
                }
                else
                {
                    yield return tag;
                }


                tag = ParseTag(reader);
            }

            yield return tag;
        }

        public static IEnumerable<HtmlTag> Parse(string html)
        {
            using (StringReader reader = new StringReader(html))
            {
                while (reader.Peek() >= 0)
                {
                    HtmlTag tag = ParseTag(reader);

                    if (tag.Tag.Equals("p", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (HtmlTag nestedTag in ParseTagsUntil(reader, "/p"))
                        {
                            if (nestedTag.Tag != "/p")
                            {
                                foreach (KeyValuePair<string, string> kvp in tag.Attributes)
                                {
                                    if (!nestedTag.Attributes.ContainsKey(kvp.Key))
                                    {
                                        nestedTag.Attributes[kvp.Key] = kvp.Value;
                                    }
                                }

                                yield return nestedTag;
                            }
                        }
                    }
                    else
                    {
                        yield return tag;
                    }
                }
            }
        }

        private static (string, string)? ReadAttribute(StringReader reader, ref int character)
        {
            while (character >= 0 && char.IsWhiteSpace((char)character) && (char)character != '>')
            {
                character = reader.Read();
            }

            if ((char)character == '>')
            {
                return null;
            }
            else
            {
                StringBuilder attributeNameBuilder = new StringBuilder();

                while (character >= 0 && !char.IsWhiteSpace((char)character) && (char)character != '>' && (char)character != '=')
                {
                    attributeNameBuilder.Append((char)character);
                    character = reader.Read();
                }

                string attributeName = attributeNameBuilder.ToString();

                while (character >= 0 && char.IsWhiteSpace((char)character) && (char)character != '>' && (char)character != '=')
                {
                    character = reader.Read();
                }

                if ((char)character == '=')
                {
                    character = reader.Read();

                    while (character >= 0 && char.IsWhiteSpace((char)character) && (char)character != '>')
                    {
                        character = reader.Read();
                    }

                    if ((char)character == '>')
                    {
                        return (attributeName, null);
                    }
                    else
                    {
                        bool quoted = (char)character == '"' || (char)character == '\'';

                        if (quoted)
                        {
                            char quoteChar = (char)character;

                            character = reader.Read();

                            StringBuilder attributeValueBuilder = new StringBuilder();

                            bool isEscaped = (char)character == '\\';

                            while (character >= 0 && ((char)character != quoteChar || isEscaped))
                            {
                                attributeValueBuilder.Append((char)character);
                                character = reader.Read();
                                isEscaped = (char)character == '\\' && !isEscaped;
                            }

                            string attributeValue = attributeValueBuilder.ToString();

                            return (attributeName, attributeValue);
                        }
                        else
                        {
                            StringBuilder attributeValueBuilder = new StringBuilder();

                            while (character >= 0 && !char.IsWhiteSpace((char)character) && (char)character != '>' && (char)character != '=')
                            {
                                attributeValueBuilder.Append((char)character);
                                character = reader.Read();
                            }

                            string attributeValue = attributeValueBuilder.ToString();

                            return (attributeName, attributeValue);
                        }
                    }
                }
                else
                {
                    return (attributeName, null);
                }
            }
        }

    }

    /// <summary>
    /// Contains utilities to resolve absolute and relative URIs.
    /// </summary>
    public static class HTTPUtils
    {
        /// <summary>
        /// Determines whether every file that is downloaded should be logged to the standard error stream.
        /// </summary>
        public static bool LogDownloads { get; set; } = true;

        /// <summary>
        /// Resolves an image Uri, by downloading the image file if necessary. It also takes care of ensuring that the file extension matches the format of the file.
        /// </summary>
        /// <param name="uri">The address of the image.</param>
        /// <param name="baseUriString">The base uri to use for relative uris.</param>
        /// <returns>A tuple containing the local path of the image file (either the original image, or a local copy of a remote file) and a boolean value indicating whether the image was fetched from a remote location and should be deleted after the program is done with it.</returns>
        public static (string path, bool wasDownloaded) ResolveImageURI(string uri, string baseUriString)
        {
            if (File.Exists(Path.Combine(baseUriString, uri)))
            {
                return (Path.Combine(baseUriString, uri), false);
            }
            else if (File.Exists(uri))
            {
                return (uri, false);
            }
            else
            {
                Uri absoluteUri;
                bool validUri;

                if (Uri.TryCreate(baseUriString, UriKind.Absolute, out Uri baseUri))
                {
                    validUri = Uri.TryCreate(baseUri, uri, out absoluteUri);
                }
                else
                {
                    validUri = Uri.TryCreate(uri, UriKind.Absolute, out absoluteUri);
                }

                if (validUri)
                {
                    string tempFile = Path.GetTempFileName();
                    File.Delete(tempFile);
                    Directory.CreateDirectory(tempFile);

                    string fileDest = Path.Combine(tempFile, Path.GetFileName(absoluteUri.LocalPath));

                    try
                    {
                        if (LogDownloads)
                        {
                            Console.Error.WriteLine();
                            Console.Error.Write("Downloading {0}...", absoluteUri);
                        }
                        
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(absoluteUri, fileDest);
                        }

                        if (LogDownloads)
                        {
                            Console.Error.WriteLine(" Done.");
                        }

                        string newName = FixFileExtensionBasedOnContent(fileDest);

                        File.Move(fileDest, newName);
                        fileDest = newName;

                        return (fileDest, true);
                    }
                    catch (Exception ex)
                    {
                        if (LogDownloads)
                        {
                            Console.Error.WriteLine(" Failed!");
                            Console.Error.WriteLine(ex.Message);
                        }

                        Directory.Delete(tempFile, true);
                        return (null, false);
                    }
                }
                else
                {
                    return (null, false);
                }
            }
        }

        private static string FixFileExtensionBasedOnContent(string fileName)
        {
            using (FileStream fileStream = File.OpenRead(fileName))
            {
                bool isSvg = false;

                try
                {
                    using (var xmlReader = System.Xml.XmlReader.Create(fileStream))
                    {
                        isSvg = xmlReader.MoveToContent() == System.Xml.XmlNodeType.Element && "svg".Equals(xmlReader.Name, StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch
                {
                    isSvg = false;
                }

                if (isSvg)
                {
                    return fileName + ".svg";
                }
                else
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    byte[] header = new byte[8];

                    for (int i = 0; i < header.Length; i++)
                    {
                        header[i] = (byte)fileStream.ReadByte();
                    }

                    if (header[0] == 0x42 && header[1] == 0x4D)
                    {
                        return fileName + ".bmp";
                    }
                    else if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
                    {
                        return fileName + ".gif";
                    }
                    else if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF && (header[3] == 0xDB || header[3] == 0xE0 || header[3] == 0xEE || header[3] == 0xE1))
                    {
                        return fileName + ".jpg";
                    }
                    else if (header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46 && header[4] == 0x2D)
                    {
                        return fileName + ".pdf";
                    }
                    else if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                    {
                        return fileName + ".png";
                    }
                    else if ((header[0] == 0x49 && header[1] == 0x49 && header[2] == 0x2A && header[3] == 0x00) || (header[0] == 0x4D && header[1] == 0x4D && header[2] == 0x00 && header[3] == 0x2A))
                    {
                        return fileName + ".tif";
                    }
                    else
                    {
                        return fileName;
                    }
                }
            }
        }
    }
}
