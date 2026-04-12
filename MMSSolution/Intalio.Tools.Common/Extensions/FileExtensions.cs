using Intalio.Tools.Common.Enumerations;
using Microsoft.AspNetCore.Http;

namespace Intalio.Tools.Common.Extensions.FileExtensions
{
	public static class FileExtensions
	{

		public static string GetFileMime(this string filename)
		{
			return Path.GetExtension(filename).ToLower() switch
			{
				".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
				".doc" => "application/msword",
				".pdf" => "application/pdf",
				_ => "application/octet-stream",
			};
		}

		public static FileTypeEnum GetFileType(this string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				return FileTypeEnum.File;
			}

			return Path.GetExtension(filename).ToLower() switch
			{
				".docx" or ".doc" => FileTypeEnum.Word,
				".pdf" => FileTypeEnum.Pdf,
				".xls" or ".xlsx" => FileTypeEnum.Excel,
				".png" or ".jpg" or ".jpeg" => FileTypeEnum.Image,
				_ => FileTypeEnum.File
			};
		}

		public static bool IsExtensionSupportedByViewer(this string filename)
		{
			if (!string.IsNullOrEmpty(filename))
			{
				List<string> supportedExtensions = new() { ".docx", ".doc", ".pdf", ".png", ".jpg", ".jpeg" };
				return supportedExtensions.Contains(Path.GetExtension(filename).ToLower());
			}
			return true;
		}

		public static string GetFilename(this string webUrl)
		{
			if (!string.IsNullOrEmpty(webUrl))
			{
				return Path.GetFileName(webUrl);
			}
			return "";
		}

        public static string GetExplicitTableName(this string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
				switch (tableName.ToLower())
				{
					case "user":
						return "[user]";
					default:
                        return tableName;
                }
				
            }
            return "";
        }

        public static byte[] ToBytes(this IFormFile file)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				file.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}
	}
}
