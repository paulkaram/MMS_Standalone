using MMS.API.Common;
using Microsoft.AspNetCore.Mvc;
using MMS.BLL.Managers;
using MMS.DTO;
using MMS.DTO.DataSources;

namespace MMS.API.Controllers
{
    [Route("api/dataSources")]
	[ApiController]
	public class DataSourcesController : IntalioBaseController
	{
		private readonly DataSourceManager _dataSourceManager;

		public DataSourcesController(DataSourceManager dataSourceManager)
		{
			_dataSourceManager = dataSourceManager;
		}

		[HttpGet]
		public async Task<IActionResult> List()
		{
			try
			{
				var dataSources = await _dataSourceManager.ListDataSourcesAsync();
				return Ok(new ApiResponseDto<List<DataSourceListItemDto>>(dataSources));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

        [HttpPost]
		public async Task<IActionResult> CreateDataSource(DataSourceDto dataSource)
		{
			try
			{
				await _dataSourceManager.CreateDataSourceAsync(dataSource);

				return Ok(new ApiResponseDto<object>(Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPut("{datasourceId}")]
		public async Task<IActionResult> UpdateDataSource(int dataSourceId, DataSourceDto dataSource)
		{
			try
			{
				await _dataSourceManager.UpdateDataSourceAsync(dataSourceId, dataSource);
				return Ok(new ApiResponseDto<object>(Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpDelete("{datasourceId}")]
		public async Task<IActionResult> DeleteDataSource(int dataSourceId)
		{
			try
			{
				await _dataSourceManager.DeleteDataSourceAsync(dataSourceId);
				return Ok(new ApiResponseDto<object>(Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
	}
}
