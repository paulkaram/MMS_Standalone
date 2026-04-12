using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Dictionary;

namespace MMS.API.Controllers
{
    [Route("api/dictionary")]
	[ApiController]
	public class DictionaryController : IntalioBaseController
	{
		private readonly DictionaryManager _dictionaryManager;

		public DictionaryController(DictionaryManager dictionaryManager)
		{
			_dictionaryManager = dictionaryManager;
		}

		[HttpGet]
		[RequiredPermission(PermissionDbEnum.Dictionary, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListDictionary()
		{
			try
			{
				var list = await _dictionaryManager.ListDictionaryAsync();
				return Ok(new ApiResponseDto<List<Dictionary>>(list));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPost]
		[RequiredPermission(PermissionDbEnum.Dictionary, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> CreateDictionary(DictionaryDto dictionary)
		{
			try
			{
				await _dictionaryManager.CreateDictionaryAsync(dictionary);

				return Ok(new ApiResponseDto<object>(Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPut("{dictionaryId}")]
		[RequiredPermission(PermissionDbEnum.Dictionary, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateDictionary(int dictionaryId, DictionaryDto dictionary)
		{
			try
			{
				await _dictionaryManager.UpdateDictionaryAsync(dictionaryId, dictionary);
				return Ok(new ApiResponseDto<object>(Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpDelete("{dictionaryId}")]
		[RequiredPermission(PermissionDbEnum.Dictionary, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> DeleteDictionary(int dictionaryId)
		{
			try
			{
				await _dictionaryManager.DeleteDictionaryAsync(dictionaryId);
				return Ok(new ApiResponseDto<object>(Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
	}
}
