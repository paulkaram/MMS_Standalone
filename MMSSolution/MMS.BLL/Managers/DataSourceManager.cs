using Intalio.Tools.Common.Encryptions;
using MapsterMapper;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Models.MMS;
using MMS.DTO.DataSources;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class DataSourceManager
	{
		private readonly IMapper _mapper;

		private readonly ISettingsUnitOfWork _settingsUnitOfWork;
		public DataSourceManager(IMapper mapper, ISettingsUnitOfWork settingsUnitOfWork)
		{
			_mapper = mapper;
			_settingsUnitOfWork = settingsUnitOfWork;
		}

		public async Task CreateDataSourceAsync(DataSourceDto dataSourceObject)
		{
			DataSource dataSource = new()
			{
				Dbname = dataSourceObject.DbName,
				InstanceName = dataSourceObject.InstanceName,
				Password = EncryptionService.Encrypt(dataSourceObject.Password),
				Username = dataSourceObject.Username
			};
			await _settingsUnitOfWork.DataSources.AddAsync(dataSource);
			await _settingsUnitOfWork.SaveChangesAsync();
		}

		public async Task DeleteDataSourceAsync(int dataSourceId)
		{
			var dataSource = await _settingsUnitOfWork.DataSources.GetAsync(x => x.Id == dataSourceId);
			if (dataSource != null)
			{
				_settingsUnitOfWork.DataSources.Remove(dataSource);
				await _settingsUnitOfWork.SaveChangesAsync();
			}
		}

		public async Task UpdateDataSourceAsync(int dataSourceId, DataSourceDto dataSourceObject)
		{
			var dataSource = await _settingsUnitOfWork.DataSources.GetAsync(x => x.Id == dataSourceId);
			if (dataSource != null)
			{
				dataSource.Dbname = dataSourceObject.DbName;
				dataSource.InstanceName = dataSourceObject.InstanceName;
				dataSource.Password = EncryptionService.Encrypt(dataSourceObject.Password);
				dataSource.Username = dataSourceObject.Username;
				await _settingsUnitOfWork.SaveChangesAsync();
			}
		}

		public async Task<List<DataSourceListItemDto>> ListDataSourcesAsync()
		{
			var dataSources = await _settingsUnitOfWork.DataSources.ListAsync();
			return dataSources.Select(x => _mapper.Map<DataSourceListItemDto>(x)).ToList();
		}
    }
}
