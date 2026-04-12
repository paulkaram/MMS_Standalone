
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Intalio.Tools.Common.Objects;
using Microsoft.AspNetCore.Http;

namespace Intalio.Tools.Common.FileKit
{
    public static class FileWord
    {
        public static bool IsWord(IFormFile file)
        {
            try
            {
                using (Stream memoryStream = file.OpenReadStream())
                {
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, false))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static byte[] CreateEmptyWordDocument(string text = "")
        {
            using (MemoryStream ms = new())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();

                    mainPart.Document = new();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        Body body = mainPart.Document.AppendChild(new Body());
                        Paragraph para = body.AppendChild(new Paragraph());
                        Run run = para.AppendChild(new Run());
                        run.AppendChild(new Text(text));
                    }
                }
                return ms.ToArray();
            }
        }

        public static byte[]? ConvertWordToPdf(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            using (MemoryStream outstream = new())
            {
                using (MemoryStream ms = new(bytes))
                {
                    Aspose.Words.Document doc = new(ms);
                    doc.Save(outstream, new Aspose.Words.Saving.PdfSaveOptions { EmbedFullFonts = true, NumeralFormat = Aspose.Words.Saving.NumeralFormat.ArabicIndic });
                    return outstream.ToArray();
                }
            }
        }

        public static byte[]? ApplyBookmarks(byte[]? sourceFile, List<FileBookmark> bookmarks)
        {
            if (sourceFile == null)
            {
                return null;
            }

            using (MemoryStream outStream = new MemoryStream())
            {
                using (MemoryStream ms = new MemoryStream(sourceFile))
                {
                    Aspose.Words.Document document = new(ms);
                    Aspose.Words.DocumentBuilder builder = new(document);
                    foreach (var bookmark in bookmarks)
                    {
                        Aspose.Words.Bookmark asposeBookmark = builder.Document.Range.Bookmarks[bookmark.Name];
                        if (asposeBookmark != null)
                        {
                            if (bookmark.IsImage)
                            {
                                asposeBookmark.Text = "";
                                if (!string.IsNullOrEmpty(bookmark.Value))
                                {
                                    builder.MoveToBookmark(bookmark.Name);
                                    builder.InsertImage(Convert.FromBase64String(bookmark.Value), Aspose.Words.Drawing.RelativeHorizontalPosition.Column, 0, Aspose.Words.Drawing.RelativeVerticalPosition.Line, 0, bookmark.Width, bookmark.Height, Aspose.Words.Drawing.WrapType.Inline);
                                }
                            }
                            else
                            {
                                
                                builder.MoveToBookmark(asposeBookmark.Name, false, true);
                                builder.InsertHtml(bookmark.Value, true);
                                
                            }
                        }
                    }

                    document.Save(outStream, Aspose.Words.SaveFormat.Docx);
                    return outStream.ToArray();
                }
            }
        }

    }
}
