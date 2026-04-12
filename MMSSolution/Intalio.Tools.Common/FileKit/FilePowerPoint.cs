using Aspose.Slides;
using Aspose.Slides.Export;

namespace Intalio.Tools.Common.FileKit
{
    public static class FilePowerPoint
    {
        /// <summary>
        /// Converts a PowerPoint file (PPTX/PPT) to PDF
        /// </summary>
        /// <param name="inputStream">The PowerPoint file stream</param>
        /// <returns>PDF file as byte array</returns>
        public static byte[] ConvertToPdf(Stream inputStream)
        {
            using var presentation = new Presentation(inputStream);
            using var outputStream = new MemoryStream();

            presentation.Save(outputStream, SaveFormat.Pdf);

            return outputStream.ToArray();
        }

        /// <summary>
        /// Converts a PowerPoint file (PPTX/PPT) to PDF
        /// </summary>
        /// <param name="inputBytes">The PowerPoint file bytes</param>
        /// <returns>PDF file as byte array</returns>
        public static byte[] ConvertToPdf(byte[] inputBytes)
        {
            using var inputStream = new MemoryStream(inputBytes);
            return ConvertToPdf(inputStream);
        }

        /// <summary>
        /// Checks if a file is a valid PowerPoint file
        /// </summary>
        public static bool IsPowerPoint(Stream inputStream)
        {
            try
            {
                using var presentation = new Presentation(inputStream);
                return presentation.Slides.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the number of slides in a PowerPoint file
        /// </summary>
        public static int GetSlideCount(Stream inputStream)
        {
            try
            {
                using var presentation = new Presentation(inputStream);
                return presentation.Slides.Count;
            }
            catch
            {
                return 0;
            }
        }
    }
}
