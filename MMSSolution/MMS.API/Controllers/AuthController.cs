using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMS.BLL.Managers;
using MMS.DTO;
using MMS.DTO.Users.Auth;
using MMS.API.Common;
using MMS.BLL.Constants;

namespace MMS.API.Controllers
{
    [AllowAnonymous]
	[Route("api/auth")]
	public class AuthController : IntalioBaseController
	{
		private readonly UserManagementManager _userManager;

		public AuthController(UserManagementManager userManager)
		{
			_userManager = userManager;
		}

        [HttpPost]
        [LogUserActivity(AuditOperationConstants.Login, "User login attempt for {Username}")]
        public async Task<IActionResult> Authenticate(LoginCredentialsDto loginCredentials)
        {
            try
            {
				string decrptedPass = _userManager.DerptyUiPassword(loginCredentials.Password);
				var authenticatedUser = await _userManager.AuthenticateAsync(loginCredentials.Username, decrptedPass);
                if (authenticatedUser.locked)
                {
                    UserLockeDto userLockedDto = new UserLockeDto(StatusCode:423, "the user locked please contact the system admin");
					return Ok(new ApiResponseDto<UserLockeDto>(userLockedDto));
				}
				if (authenticatedUser.userDto != null)
                {
                    if (_userManager.TwoFactorAuthEnabled())
                    {
                        TwoFactorAuthUserDto twoFactorAuthUser = new TwoFactorAuthUserDto { StatusCode = 206, UserInfo = authenticatedUser.userDto.User };
                        return Ok(new ApiResponseDto<TwoFactorAuthUserDto>(twoFactorAuthUser));
                    }
                    else
                    {
                        return Ok(new ApiResponseDto<LoggedInUserDto>(authenticatedUser.userDto));
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

		[HttpPost("refresh-token")]
		[AllowAnonymous]
		[LogUserActivity(AuditOperationConstants.Login, "Token refresh requested")]
		public async Task<IActionResult> RefreshToken(RefreshTokenPostDto refreshTokenPostDto)
		{
			try
			{
				var tokenDto = await _userManager.RefreshToken(refreshTokenPostDto);
                if (tokenDto != null) {
					return Ok(new ApiResponseDto<RefreshTokenResponseDto>(tokenDto));
                }
                else
                {
                    return BadRequest();
                }
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPost("verify")]
        [AllowAnonymous]
        [LogUserActivity(AuditOperationConstants.TwoFactorAuth, "Verification code requested for {UserId}")]
        public async Task<IActionResult> RequestVerificationCode(UserVerificationCodePostDto requestVerificationCodeDto)
        {
            try
            {
                var result = await _userManager.RequestVerificationCodeAsync(requestVerificationCodeDto);
                return new JsonResult(new ApiResponseDto<object>(null, result.Success, result.Message));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost("2fa")]
        [AllowAnonymous]
        [LogUserActivity(AuditOperationConstants.TwoFactorAuth, "2FA verification for {UserId}")]
        public async Task<IActionResult> CheckVerificationCode(UserVerificationCodePostDto requestVerificationCodeDto)
        {
            try
            {
                var authenticatedUser = await _userManager.CheckVerificationCodeAsync(requestVerificationCodeDto);
                if (authenticatedUser != null)
                {
                    return Ok(new ApiResponseDto<LoggedInUserDto>(authenticatedUser));
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("login-options")]
		[AllowAnonymous]
		public IActionResult GetLoginOptions()
		{
			try
			{
				var loginOptions = _userManager.GetLoginOptions();
				return Ok(new ApiResponseDto<LoginOptionsDto>(loginOptions));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
	}
}
