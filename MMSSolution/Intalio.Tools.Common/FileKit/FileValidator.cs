using Intalio.Tools.Common.Enumerations;
using Microsoft.AspNetCore.Http;

namespace Intalio.Tools.Common.FileKit
{
	public static class FileValidator
	{
		public static FileStatusEnum ValidateFile(IFormFile file, List<string> extensions)
		{
			var validName= IsValidFileName(file.FileName);
			if (!validName)
			{
				return FileStatusEnum.InvalidExtension;
			}
			string uploadedExtension = Path.GetExtension(file.FileName).ToLower();
			if (extensions.Contains(uploadedExtension))
			{
				switch (uploadedExtension.ToLower())
				{
					case ".docx":
						if (!FileWord.IsWord(file))
						{
							return FileStatusEnum.InvalidFile;
						}
						break;
					case ".pdf":
						if (!FilePdf.IsPdf(file.OpenReadStream()))
						{
							return FileStatusEnum.InvalidFile;
						}
						break;
					case ".jpg":
					case ".jpeg":
						if (!FileImage.IsJpeg(file))
						{
							return FileStatusEnum.InvalidFile;
						}
						break;
					case ".png":
						if (!FileImage.IsPNG(file))
						{
							return FileStatusEnum.InvalidFile;
						}
						break;
					case ".xlsx":
						if (!FileExcel.IsExcel(file))
						{
							return FileStatusEnum.InvalidFile;
						}
						break;
					case ".xml":
						if (!FileXml.IsXml(file))
						{
							return FileStatusEnum.InvalidFile;
						}
						break;
					// Extensions without specific content validation - allow if in the allowed list
					case ".pptx":
					case ".ppt":
					case ".doc":
					case ".xls":
					case ".txt":
					case ".mp4":
					case ".gif":
					case ".rar":
					case ".zip":
						// These extensions are allowed without deep content validation
						break;
					default:
						return FileStatusEnum.UnknownFile;
				}
			}
			else
			{
				return FileStatusEnum.InvalidExtension;
			}

			return FileStatusEnum.Valid;
		}

		private static bool IsValidFileName(string fileName)
		{
			// Remove null bytes and other potentially harmful characters
			var length= fileName.Length;
			fileName = fileName.Replace("\0", string.Empty).Replace("%00", string.Empty);

			// Strip additional extensions (only keep the first one)
			var parts = fileName.Split('.');
			if (parts.Length > 2)
			{
				fileName = string.Join(".", parts.Take(2));
			}

			return fileName.Length== length;
		}
		public static (FileStatusEnum Status, int IndexOfCorruptedFile) ValidateFiles(IFormFileCollection files, List<string> extensions)
		{
			for (int i = 0; i < files.Count; i++)
			{
				IFormFile file = files[i];
				var status = ValidateFile(file, extensions);
				if (status != FileStatusEnum.Valid)
				{
					return (status, i);
				}
			}

			return (FileStatusEnum.Valid, -1);
		}

		public static (FileStatusEnum Status, int IndexOfCorruptedFile) ValidateFiles(List<IFormFile> files, List<string> extensions)
		{
			for (int i = 0; i < files.Count; i++)
			{
				var status = ValidateFile(files[i], extensions);
				if (status != FileStatusEnum.Valid)
				{
					return (status, i);
				}
			}

			return (FileStatusEnum.Valid, -1);
		}

	}
}
