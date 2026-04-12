using Intalio.Tools.Common.Extensions.StringExtensions;
using Intalio.Tools.Common.FileKit;
using Intalio.Tools.Common.Objects;
using Microsoft.AspNetCore.Http;
using MMS.DTO;

namespace Intalio.Tools.Common.Storage
{
    public class FileSystemStorage : IStorage
    {
        private readonly string _storagePath;

        public FileSystemStorage(string storagePath)
        {
            _storagePath = storagePath.SuffixSlash();

        }

        public async Task<bool> SaveToStorage(string fileWebRelativeUrl, IFormFile file)
        {
            CreatePath(fileWebRelativeUrl);

            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                await File.WriteAllBytesAsync(_storagePath + fileWebRelativeUrl, ms.ToArray());
            }
            return true;
        }

        public async Task<bool> SaveToStorage(string fileWebRelativeUrl, byte[] fileBytes)
        {
            CreatePath(fileWebRelativeUrl);

            await File.WriteAllBytesAsync(_storagePath + fileWebRelativeUrl, fileBytes);
            return true;
        }

        public async Task<bool> SaveToStorage(string fileWebRelativeUrl, string filepath)
        {
            CreatePath(fileWebRelativeUrl);

            await File.WriteAllBytesAsync(_storagePath + fileWebRelativeUrl, File.ReadAllBytes(filepath));
            return true;
        }

        public async Task RemoveFromStorage(string filePath)
        {
            File.Delete(_storagePath + filePath);
            await Task.FromResult(0);
        }

        public byte[]? GetFile(string filePath)
        {
            return File.ReadAllBytes(_storagePath + filePath);
        }

        public byte[]? GetFileAsPdf(string filePath, string? extension)
        {
            byte[]? bytes = File.ReadAllBytes(_storagePath + filePath);

            if (extension?.ToLower() == ".docx" || extension?.ToLower() == ".doc")
            {
                bytes = FileWord.ConvertWordToPdf(bytes);
            }

            return bytes;
        }

        public byte[]? GetFileForViewer(string filePath, string? extension, string? watermarkText, bool isSupportedExtension, List<StampDto>? annotations)
        {
            if (isSupportedExtension)
            {
                var fileBytes = GetFileForViewer(filePath, extension?.ToLower() ?? "");
                if (annotations != null && annotations.Any())
                {
                    annotations.ForEach(x =>
                    {
                        // Use StampType if set, otherwise fall back to AnnotationType
                        var annotationType = x.StampType ?? (int)x.AnnotationType;
                        switch (annotationType)
                        {
                            case (int)AnnotationTypeEnum.Stamp:
                            case (int)AnnotationTypeEnum.Draw:
                            case (int)AnnotationTypeEnum.Signature:
                                var stampData = x.StampData.GetFromCleanBase64();
                                if (stampData != null)
                                {
                                    fileBytes = FilePdf.AddStamp(fileBytes, stampData, x.PageIndex + 1, new ImageRectangle(x.Rect));
                                }
                                break;
                            case (int)AnnotationTypeEnum.Text:
                                fileBytes = FilePdf.AddFreeText(fileBytes, x.Value, x.Color, x.FontSize, x.PageIndex + 1, new ImageRectangle(x.Rect));
                                break;
                        }
                    });
                }
                return FilePdf.ApplyWatermark(fileBytes, watermarkText);
            }

            return GetFile(filePath);
        }

        public byte[]? GetFileWithStamp(string filePath, string? extension, string? watermarkText)
        {
            return FilePdf.ApplyWatermark(File.ReadAllBytes(_storagePath + filePath), watermarkText);
        }

        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        private void CreatePath(string path)
        {
            string directory = _storagePath + System.IO.Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private byte[]? GetFileForViewer(string filePath, string extension)
        {
            byte[]? bytes = File.ReadAllBytes(_storagePath + filePath);

            switch (extension)
            {
                case ".docx":
                case ".doc":
                    bytes = FileWord.ConvertWordToPdf(bytes);
                    break;
                case ".png":
                case ".jpeg":
                case ".jpg":
                    bytes = FileImage.ConvertImageToPDF(bytes);
                    break;
                default:
                    break;
            }
            return bytes;
        }

	}
}
