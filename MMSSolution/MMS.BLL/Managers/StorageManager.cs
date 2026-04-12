using Intalio.Tools.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using MMS.BLL.Storage;
using MMS.DAL.Enumerations;
using MMS.DTO;

namespace MMS.BLL.Managers
{
	public class StorageManager
	{
		private readonly IMemoryCache _cache;
		private readonly StorageFactory _storageFactory;
		private readonly IStorage _storage;

		public StorageManager(StorageSettings storageSettings,
			StorageFactory storageFactory,
			IMemoryCache cache)
		{
			_storageFactory = storageFactory;
			_storage = _storageFactory.GetStorage();
			_cache = cache;
		}

		public async Task<bool> SaveToStorage(byte[] bytes, int? AttachmentId = null, string? fileWebRelativeUrl = null)
		{
			return await _storage.SaveToStorage(fileWebRelativeUrl, bytes);
		}

		public async Task<bool> SaveToStorage(IFormFile formFile, int? AttachmentId = null, string? fileWebRelativeUrl = null)
		{
			return await _storage.SaveToStorage(fileWebRelativeUrl, formFile);
		}

		public async Task<bool> SaveToStorage(string filePath, int? AttachmentId = null, string? fileWebRelativeUrl = null)
		{
			return await _storage.SaveToStorage(fileWebRelativeUrl, filePath);
		}

		public async Task<bool> RemoveFromStorage(int? attachmentId = null, string? fileWebRelativeUrl = null)
		{
			await _storage.RemoveFromStorage(fileWebRelativeUrl);
			return true;
		}

		public async Task<bool> Exists(int? attachmentId = null, string? fileWebRelativeUrl = null)
		{
			return _storage.Exists(fileWebRelativeUrl);
		}

		public async Task<byte[]?> GetFile(int? attachmentId = null, string? fileWebRelativeUrl = null)
		{
			return _storage.GetFile(fileWebRelativeUrl);
		}

		public async Task<byte[]?> GetFileAsPdf(string? extension, int? attachmentId = null, string? fileWebRelativeUrl = null)
		{
			return _storage.GetFileAsPdf(fileWebRelativeUrl, extension);
		}

		public async Task<byte[]?> GetFileForViewer(string? extension, string? watermarkText,
			bool isSupportedExtension, List<StampDto>? annotations, int? attachmentId = null, string? fileWebRelativeUrl = null)
		{
			return _storage.GetFileForViewer(fileWebRelativeUrl, extension, watermarkText, isSupportedExtension, annotations);
		}

		public async Task<byte[]?> GetFinalMeetingMinutesTemplate()
		{
			string cacheKey = "FinalMeetingMinutesTemplate";

			if (_cache.TryGetValue(cacheKey, out byte[] cachedBytes))
			{
				return cachedBytes;
			}

			byte[]? fileBytes = _storage.GetFile(_storageFactory.GetMeetingMinutesTemplateDirectory(AttachmentRecordTypeDbEnum.FinalMeetingMinutes));

			if (fileBytes != null)
			{
				_cache.Set(cacheKey, fileBytes, new MemoryCacheEntryOptions
				{
					SlidingExpiration = TimeSpan.FromMinutes(180)
				});
			}

			return fileBytes;
		}

		public async Task<byte[]?> GetInitialMeetingMinutesTemplate()
		{
			string cacheKey = "InitialMeetingMinutesTemplate";

			if (_cache.TryGetValue(cacheKey, out byte[] cachedBytes))
			{
				return cachedBytes;
			}

			byte[]? fileBytes = _storage.GetFile(_storageFactory.GetMeetingMinutesTemplateDirectory(AttachmentRecordTypeDbEnum.InitialMeetingMinutes));

			if (fileBytes != null)
			{
				_cache.Set(cacheKey, fileBytes, new MemoryCacheEntryOptions
				{
					SlidingExpiration = TimeSpan.FromMinutes(180)
				});
			}

			return fileBytes;
		}

		public async Task<bool> UpdateProfileImage(byte[] bytes, string userId, string extension, string imageType)
		{
			string filePath = _storageFactory.GetProfilePictureDirectory(userId) + userId + extension;
			return await _storage.SaveToStorage(filePath, bytes);
		}

		public async Task<(byte[]? bytes, string mimeType)> GetProfileImage(string userId)
		{
			string filePath = _storageFactory.GetProfilePictureDirectory(userId) + userId;
			return (_storage.GetFile(filePath), "");
		}

		public async Task<bool> UpdateDatabaseAttachment(byte[] bytes, int templateAttachmentId)
		{
			// No longer supported — database storage removed. Use file system path instead.
			return false;
		}
	}
}
