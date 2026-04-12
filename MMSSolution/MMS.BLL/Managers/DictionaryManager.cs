using MapsterMapper;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Dictionary;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class DictionaryManager
	{

		private readonly ISettingsUnitOfWork _settingsUnitOfWork;
		public DictionaryManager( ISettingsUnitOfWork settingsUnitOfWork)
		{
			_settingsUnitOfWork = settingsUnitOfWork;
		}

		public async Task CreateDictionaryAsync(DictionaryDto dictionaryObject)
		{
			Dictionary dictionary = new()
			{
				Keyword = dictionaryObject.Keyword,
				Ar = dictionaryObject.Ar,
				En = dictionaryObject.En,

			};
			await _settingsUnitOfWork.Dictionary.AddAsync(dictionary);
			await _settingsUnitOfWork.SaveChangesAsync();
		}

		public async Task DeleteDictionaryAsync(int dictionaryId)
		{
			var riskType = await _settingsUnitOfWork.Dictionary.GetAsync(x => x.Id == dictionaryId);
			if (riskType != null)
			{
				_settingsUnitOfWork.Dictionary.Remove(riskType);
				await _settingsUnitOfWork.SaveChangesAsync();
			}
		}

		public async Task UpdateDictionaryAsync(int dictionaryId, DictionaryDto dictionaryObject)
		{
			var dictionary = await _settingsUnitOfWork.Dictionary.GetAsync(x => x.Id == dictionaryId);
			if (dictionary != null)
			{
				dictionary.Keyword = dictionaryObject.Keyword;
				dictionary.En = dictionaryObject.En;
				dictionary.Ar = dictionaryObject.Ar;
				await _settingsUnitOfWork.SaveChangesAsync();
			}
		}

		public async Task<List<Dictionary>> ListDictionaryAsync()
		{
			var dictionary = await _settingsUnitOfWork.Dictionary.ListAsync();
			return dictionary.ToList();
		}

		public async Task<Dictionary> GetByKey(string key)
		{
			Dictionary? item = await _settingsUnitOfWork.Dictionary.GetAsync(x => x.Keyword == key);
			if (item == null)
			{
				item = new Dictionary() {Id=0, Keyword = key, Ar = key, En = key };
			}
			return item;
		}
		public async Task<string> GetByKeyTranslated(string key,LanguageDbEnum language)
		{
			Dictionary? item = await _settingsUnitOfWork.Dictionary.GetAsync(x => x.Keyword == key);
			if (item == null)
			{
				return key;
			}
			return language == LanguageDbEnum.Arabic ? item.Ar : item.En;
		}
	}
}
