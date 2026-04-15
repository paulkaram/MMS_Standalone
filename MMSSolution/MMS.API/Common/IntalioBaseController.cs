using Intalio.Tools.Common.Enumerations;
using Intalio.Tools.Common.Extensions.FileExtensions;
using Intalio.Tools.Common.FileKit;
using Intalio.Tools.Common.JwtToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using System.Security.Claims;
using System.Web;

namespace MMS.API.Common
{
    [Authorize]
	[ApiController]
	public class IntalioBaseController : ControllerBase
	{
		/// <summary>
		/// Resolves user ID from either IAM token ("sub" claim) or MMS token (encrypted "account" claim).
		/// IAM tokens are checked first since they are the standard going forward.
		/// </summary>
		protected string UserId
		{
			get
			{
				// IAM token: "sub" claim contains the user GUID
				var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out _))
					return sub;

				// MMS token fallback: encrypted "account" claim
				return UserManagementManager.GetStringClaimValue(User, JwtTokenGenerator.CommonClaimNames.UserId);
			}
		}

		/// <summary>
		/// Resolves display name from either IAM token or MMS token.
		/// </summary>
		protected string UserFullName
		{
			get
			{
				// IAM token: try standard name claims
				var name = User.FindFirstValue("DisplayName")
					?? User.FindFirstValue(ClaimTypes.Name)
					?? User.FindFirstValue("name");
				if (!string.IsNullOrEmpty(name))
					return name;

				// MMS token fallback: encrypted "name" claim
				return UserManagementManager.GetStringClaimValue(User, JwtTokenGenerator.CommonClaimNames.FullName);
			}
		}

		/// <summary>
		/// Resolves structure ID. IAM tokens don't carry this, so returns 0 for IAM tokens.
		/// The structure can be resolved from the MMS user record if needed.
		/// </summary>
		protected int StructureId
		{
			get
			{
				// IAM tokens don't have structure — try MMS encrypted claim first
				var mmsValue = UserManagementManager.GetClaimValue(User, JwtTokenGenerator.CommonClaimNames.StructureId);
				if (mmsValue != 0) return mmsValue;

				// For IAM tokens, return 0 (callers should handle gracefully)
				return 0;
			}
		}

		protected LanguageDbEnum Language
		{
			get
			{
				// Check X-Language header first (set by frontend on every request)
				var xLang = HttpContext?.Request?.Headers["X-Language"].FirstOrDefault();
				if (!string.IsNullOrEmpty(xLang))
					return xLang == "ar" ? LanguageDbEnum.Arabic : LanguageDbEnum.English;

				// Check Accept-Language header
				var acceptLang = HttpContext?.Request?.Headers["Accept-Language"].FirstOrDefault() ?? "";
				if (acceptLang.Contains("ar"))
					return LanguageDbEnum.Arabic;

				// IAM token: "PreferredLanguage" claim
				var prefLang = User.FindFirstValue("PreferredLanguage");
				if (!string.IsNullOrEmpty(prefLang))
					return prefLang == "ar" ? LanguageDbEnum.Arabic : LanguageDbEnum.English;

				// MMS token fallback: "lang" claim
				return User.FindFirstValue(JwtTokenGenerator.CommonClaimNames.Language) == "ar" ? LanguageDbEnum.Arabic : LanguageDbEnum.English;
			}
		}

        protected IActionResult UnauthorizedResponse(string? message = null)
        {
            return StatusCode(401, new ApiResponseDto<object>(Success: false, Message: message));
        }
		protected IActionResult ConflictResponse(string? message = null)
		{
			return  Conflict(new { message = message?? "Item already exists." }); ;
		}
		protected IActionResult ErrorResponse(Exception ex)
		{
			// Log to stderr so the API process shows the real stack trace
			Console.Error.WriteLine($"[ErrorResponse] {ex.GetType().Name}: {ex.Message}");
			if (ex.InnerException != null)
				Console.Error.WriteLine($"  inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
			Console.Error.WriteLine(ex.StackTrace);

			// Surface the real message in the response — helpful during active dev.
			// When you want to hide details again, revert to the generic string.
			var detail = ex.InnerException?.Message ?? ex.Message;
			return StatusCode(500, new ApiResponseDto<object>(Success: false, Message: detail));
		}

		protected FileStatusEnum IsPng(IFormFile file)
		{
			return FileValidator.ValidateFile(file, new() { ".png" });
		}

		protected (FileStatusEnum status, int IndexOfCorruptedFile) AreValidAttachments(IFormFileCollection files)
		{
			return FileValidator.ValidateFiles(files, new() { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".png", ".jpg", ".jpeg", ".gif", ".txt", ".mp4", ".rar", ".zip" });
		}

        protected FileStatusEnum IsValidLetterDocument(IFormFile file)
        {
            return FileValidator.ValidateFile(file, new() { ".pdf", ".docx" });
        }

        protected IActionResult FileBytes(string filename, byte[] bytes)
        {
            Response.Headers.Add("Content-Disposition", $"attachment;filename={HttpUtility.UrlEncode(System.IO.Path.GetFileName(filename))}");
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
            return File(bytes, filename.GetFileMime());
        }

    }
}
