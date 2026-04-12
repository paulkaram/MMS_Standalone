using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.DTO;


namespace MMS.API.Controllers
{
	[AllowAnonymous]
	[Route("api/about")]
	public class AboutController : IntalioBaseController
	{
		private readonly IConfiguration _configuration;

		public AboutController(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		[HttpGet]
		public IActionResult GetInformationAboutApi()
		{
			try
			{
				string? applicationName = _configuration.GetValue<string>("ApplicationName");

				return Ok(new ApiResponseDto<string?>(applicationName));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
	}
}
