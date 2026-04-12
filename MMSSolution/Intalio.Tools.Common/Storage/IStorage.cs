using Microsoft.AspNetCore.Http;
using MMS.DTO;

namespace Intalio.Tools.Common.Storage
{
	public interface IStorage
	{
		byte[]? GetFile(string filePath);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath">attachmentId for Database Storage else filePath</param>
		/// <param name="extension">including the (.)</param>
		/// <returns></returns>
		byte[]? GetFileAsPdf(string filePath, string? extension);

		byte[]? GetFileForViewer(string filePath, string? extension, string? watermarkText, bool isSupportedExtension, List<StampDto>? annotations);

		byte[]? GetFileWithStamp(string filePath, string? extension, string? watermarkText);

		Task RemoveFromStorage(string filePath);

		Task<bool> SaveToStorage(string fileWebRelativeUrl, IFormFile file);

		Task<bool> SaveToStorage(string fileWebRelativeUrl, byte[] fileBytes);

		Task<bool> SaveToStorage(string fileWebRelativeUrl, string filepath);

		bool Exists(string filePath);
	}
}
