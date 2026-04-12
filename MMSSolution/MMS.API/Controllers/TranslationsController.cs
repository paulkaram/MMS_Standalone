using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.BLL.Managers;
using MMS.DTO;

namespace MMS.API.Controllers
{
    [Route("api/translations")]
	public class TranslationsController : IntalioBaseController
	{
		private readonly SettingManager _settingsManager;

		public TranslationsController(SettingManager settingsManager)
		{
			_settingsManager = settingsManager;
		}

        [AllowAnonymous]
        [HttpGet]
		public async Task<IActionResult> ListTranslationForTheApp()
		{
			try
			{
				var dictionary = await _settingsManager.GetApplicationDictionary();
				return Ok(new ApiResponseDto<ApplicationDictionaryDto>(dictionary));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
	}
}
