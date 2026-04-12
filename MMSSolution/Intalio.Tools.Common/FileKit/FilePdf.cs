using Aspose.Pdf.Text;
using Aspose.Pdf;
using Intalio.Tools.Common.Objects;
using Aspose.Pdf.Facades;
using System.Drawing;

namespace Intalio.Tools.Common.FileKit
{
    public static class FilePdf
    {
        public static byte[]? StampCTS(byte[]? sourcePdfFile, string ctsExportNumber, string date)
        {
            if (sourcePdfFile == null)
            {
                return null;
            }

            using (MemoryStream ms = new(sourcePdfFile))
            {
                using (MemoryStream outStream = new())
                {
                    Aspose.Pdf.Document pdfDocument = new(ms);

                    float firstpageHeight = (float)pdfDocument.Pages[1].Rect.Height;

                    Aspose.Pdf.Facades.Stamp dateStamp = new();
                    dateStamp.BindLogo(new FormattedText("التاريخ :" + date, System.Drawing.Color.Black, System.Drawing.Color.White, Aspose.Pdf.Facades.FontStyle.Helvetica, EncodingType.Winansi, true, 12));
                    dateStamp.SetOrigin(75, firstpageHeight - 60);
                    dateStamp.Rotation = 0;
                    dateStamp.Pages = new int[] { 1 };

                    Aspose.Pdf.Facades.Stamp ctsNumberStamp = new();
                    ctsNumberStamp.BindLogo(new FormattedText("رقم القيد :" + ctsExportNumber, System.Drawing.Color.Black, System.Drawing.Color.White, Aspose.Pdf.Facades.FontStyle.Helvetica, EncodingType.Winansi, true, 12));
                    ctsNumberStamp.SetOrigin(75, firstpageHeight - 40);
                    ctsNumberStamp.Rotation = 0;
                    ctsNumberStamp.Pages = new int[] { 1 };

                    PdfFileStamp fileStamp = new(pdfDocument);
                    fileStamp.AddStamp(dateStamp);
                    fileStamp.AddStamp(ctsNumberStamp);

                    pdfDocument.Save(outStream);
                    return outStream.ToArray();
                }
            }
        }

        /// <summary>
        /// this method is from shamel
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="stamp"></param>
        /// <returns></returns>
        public static byte[]? AddStamp(byte[]? sourceFile, FileStamp stamp)
        {
            if (sourceFile == null)
            {
                return null;
            }

            using (MemoryStream outStream = new())
            {
                using (MemoryStream ms = new(sourceFile))
                {
                    using (MemoryStream imgStream = new(Convert.FromBase64String(stamp.StampBase64)))
                    {
                        Aspose.Pdf.Document pdfDocument = new(ms);
                        /*
                            in aspose, the point(0, 0) is left bottom corner
                            => apply translation to retrieve new point(x', y'),  the point(0, 0) becomes left top corner
                            X' = x
                            Y' = PageHeight - SignatureHeight - y
                         */
                        double pdfPageHeight = pdfDocument.Pages[stamp.PageNumber].Rect.Height;
                        double pdfPageWidth = pdfDocument.Pages[stamp.PageNumber].Rect.Width;

                        double scaleHeight = (pdfPageHeight + 25) / stamp.PageHeight;
                        double scaleWidth = (pdfPageWidth + 25) / stamp.PageWidth;

                        double scaleHeightPoint = pdfPageHeight / stamp.PageHeight;
                        double scaleWidthPoint = pdfPageWidth / stamp.PageWidth;

                        double newWidth = stamp.ImageWidth * scaleWidth;
                        double newHeight = stamp.ImageHeight * scaleHeight;

                        Aspose.Pdf.ImageStamp imageStamp = new(imgStream);
                        imageStamp.XIndent = stamp.ImageX * scaleHeightPoint;
                        imageStamp.YIndent = pdfPageHeight - newHeight - (stamp.ImageY * scaleWidthPoint);
                        imageStamp.Height = newHeight;
                        imageStamp.Width = newWidth;
                        imageStamp.Opacity = 1;
                        pdfDocument.Pages[stamp.PageNumber].AddStamp(imageStamp);

                        pdfDocument.Save(outStream);
                        return outStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="stamp"></param>
        /// <param name="pageNumber">page number is [1...n]</param>
        /// <param name="imageRectangle"></param>
        /// <returns></returns>
        public static byte[]? AddStamp(byte[]? sourceFile, byte[] stamp, int pageNumber, ImageRectangle imageRectangle)
        {
            if (sourceFile == null)
            {
                return null;
            }
            if (stamp == null || imageRectangle == null)
            {
                return sourceFile;
            }

            using (MemoryStream outStream = new())
            {
                using (MemoryStream ms = new(sourceFile))
                {
                    using (MemoryStream imgStream = new(stamp))
                    {
                        Aspose.Pdf.Document pdfDocument = new(ms);

                        Aspose.Pdf.ImageStamp imageStamp = new(imgStream);
                        imageStamp.XIndent = imageRectangle.X1;
                        imageStamp.YIndent = imageRectangle.Y1;
                        imageStamp.Height = imageRectangle.Y2 - imageRectangle.Y1;
                        imageStamp.Width = imageRectangle.X2 - imageRectangle.X1;
                        imageStamp.Opacity = 1;
                        pdfDocument.Pages[pageNumber].AddStamp(imageStamp);

                        pdfDocument.Save(outStream);
                        return outStream.ToArray();
                    }
                }
            }
        }

        public static byte[]? AddFreeText(byte[]? bytes, string? value, List<int> rgbColor, int fontSize, int pageNumber, ImageRectangle imageRectangle)
        {
            if (bytes == null)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(value) || imageRectangle == null)
            {
                return bytes;
            }

            using (MemoryStream outStream = new())
            {
                using (MemoryStream ms = new(bytes))
                {
                    Document pdfDocument = new(ms);
                    TextStamp textStamp = new TextStamp(value);
                    textStamp.XIndent = imageRectangle.X1;
                    textStamp.YIndent = imageRectangle.Y1;
                    textStamp.Height = imageRectangle.Y2 - imageRectangle.Y1;
                    textStamp.Width = imageRectangle.X2 - imageRectangle.X1;
                    textStamp.TextState.FontSize = fontSize;
                    textStamp.TextState.FontStyle = FontStyles.Regular;
                    Aspose.Pdf.Color color = rgbColor.Any() ? Aspose.Pdf.Color.FromRgb(System.Drawing.Color.FromArgb(rgbColor[0], rgbColor[1], rgbColor[2])) : Aspose.Pdf.Color.Black;
                    textStamp.TextState.ForegroundColor = rgbColor.Any() ? color : Aspose.Pdf.Color.Black;
                    textStamp.Opacity = 1;
                    textStamp.Background = false;
                    pdfDocument.Pages[pageNumber].AddStamp(textStamp);

                    pdfDocument.Save(outStream);
                    return outStream.ToArray();

                }
            }
        }

        public static byte[]? ApplyWatermark(byte[]? bytes, string? watermarkText)
        {
            if (bytes == null || string.IsNullOrWhiteSpace(watermarkText))
            {
                return bytes;
            }

            using (MemoryStream outstream = new())
            {
                using (MemoryStream ms = new(bytes))
                {
                    Aspose.Pdf.Document doc = new(ms);

                    foreach (Page page in doc.Pages)
                    {
                        TextStamp textStamp = new TextStamp(watermarkText);

                        textStamp.TextState.Font = FontRepository.FindFont("Tahoma");
                        textStamp.TextState.FontSize = 16.0F;
                        textStamp.TextState.FontStyle = FontStyles.Italic;
                        textStamp.TextState.ForegroundColor = Aspose.Pdf.Color.FromRgb(System.Drawing.Color.Gray);
                        textStamp.Opacity = 0.3;
                        textStamp.RotateAngle = 45;
                        textStamp.Background = false;
                        textStamp.XIndent = 20;
                        textStamp.YIndent = 20;

                        foreach (VerticalAlignment vAlign in Enum.GetValues(typeof(VerticalAlignment)))
                        {
                            if (vAlign > 0)
                            {
                                textStamp.VerticalAlignment = vAlign;
                                foreach (HorizontalAlignment hAlign in Enum.GetValues(typeof(HorizontalAlignment)))
                                {
                                    if (hAlign > 0 && (int)hAlign < 4)
                                    {
                                        textStamp.HorizontalAlignment = hAlign;
                                        page.AddStamp(textStamp);
                                    }
                                }
                            }
                        }
                    }

                    doc.Save(outstream, Aspose.Pdf.SaveFormat.Pdf);
                    return outstream.ToArray();
                }
            }
        }

        public static bool IsPdf(Stream stream)
        {
            try
            {
                return new PdfFileInfo(stream).IsPdfFile;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsPdf(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return new PdfFileInfo(ms).IsPdfFile;
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
