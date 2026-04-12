using Microsoft.AspNetCore.Mvc;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.API.Common;
using Intalio.Tools.Common.FileKit;

namespace MMS.API.Controllers
{
    [Route("api/attachments")]
    public class AttachmentsController : IntalioBaseController
    {
        private readonly AttachmentManager _attachmentManager;

        public AttachmentsController(AttachmentManager attachmentManager)
        {
            _attachmentManager = attachmentManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAttachmentBytes(int att, int task, string tk, string hvd, string? act)
        {
            try
            {
                if (await _attachmentManager.ValidateToken(att, UserId, tk, hvd, act))
                {
                    (string? filename, byte[]? bytes) = await _attachmentManager.GetAttachment(att, task, "");
                    if (filename != null && bytes != null)
                    {
                        return FileBytes(filename, bytes);
                    }
                }
                return Unauthorized(new ApiResponseDto<object>());
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("actions")]
        public async Task<IActionResult> GetAttachmentActions(int att, string tk, string hvd, string? act)
        {
            try
            {
                if (await _attachmentManager.ValidateToken(att, UserId, tk, hvd, act, false))
                {
                    AttachmentActionDto attachmentActionDto = _attachmentManager.GetAttachmentActions(act);
                    return Ok(attachmentActionDto);
                }
                return Ok(new AttachmentActionDto());
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        [HttpGet("annotations")]
        public async Task<IActionResult> ListAnnotations(int att, int task, string tk, string hvd, string? act)
        {
            try
            {
                if (await _attachmentManager.ValidateToken(att, UserId, tk, hvd, act, false))
                {
                    var annotations = await _attachmentManager.ListCurrentAnnotations(att, task, UserId);
                    return Ok(new ApiResponseDto<List<StampDto>>(annotations));
                }
                return Ok(new StampDto());
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        [HttpPost("annotations")]
        public async Task<IActionResult> SaveAnnotations(int att, int task, string tk, string hvd, string? act, [FromBody] List<StampDto> stamps)
        {
            try
            {
                if (await _attachmentManager.ValidateToken(att, UserId, tk, hvd, act, false))
                {
                    bool fileSigned = await _attachmentManager.CheckFinalMeetingMinutesSigned(att, UserId);
                    if (fileSigned)
                    {
						return ConflictResponse(  "The file has already been signed." );
					}
					// Check both StampType and AnnotationType to find annotations
					var signature = stamps.FirstOrDefault(x =>
						x.StampType == (int)AnnotationTypeEnum.Signature ||
						x.AnnotationType == AnnotationTypeEnum.Signature);
                    var draws = stamps.Where(x =>
						x.StampType == (int)AnnotationTypeEnum.Draw ||
						x.AnnotationType == AnnotationTypeEnum.Draw);
                    var stampsList = stamps.Where(x =>
						x.StampType == (int)AnnotationTypeEnum.Stamp ||
						x.AnnotationType == AnnotationTypeEnum.Stamp);
                    var textLabels = stamps.Where(x =>
						x.StampType == (int)AnnotationTypeEnum.Text ||
						x.AnnotationType == AnnotationTypeEnum.Text);
                    await _attachmentManager.RemoveCurrentStamps(task, UserId);
					if (signature != null)
					{
						fileSigned = await _attachmentManager.SignFinalMeetingMinutesAttachmentAsync(att, signature, UserId, task);
					}
					

					foreach (var draw in draws)
                    {
                        await _attachmentManager.AddAnnotationAsync(att, draw, UserId, task);
                    }


                    foreach (var stamp in stampsList)
                    {
                        await _attachmentManager.AddAnnotationAsync(att, stamp, UserId, task);
                    }
                    foreach (var text in textLabels)
                    {
                        await _attachmentManager.AddAnnotationAsync(att, text, UserId, task);
                    }

                    var token = await _attachmentManager.RegenerateAttchmentQuery(att, UserId, act, fileSigned, task);
                    return Ok(new ApiResponseDto<string>(token));
                }
                return Ok(new ApiResponseDto<string>(string.Empty));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        [HttpPost("remove-signature")]
        public async Task<IActionResult> RemoveSignature(int att, int task, string tk, string hvd, string? act)
        {
            try
            {
                if (await _attachmentManager.ValidateToken(att, UserId, tk, hvd, act, false))
                {
                    bool removed = await _attachmentManager.UnsignFinalMeetingMinutesAttachmentAsync(att, UserId, task);
                    if (!removed)
                    {
                        return BadRequest(new ApiResponseDto<string>("Unable to remove signature"));
                    }
                    var token = await _attachmentManager.RegenerateAttchmentQuery(att, UserId, act ?? string.Empty, false, task);
                    return Ok(new ApiResponseDto<string>(token));
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        [HttpDelete("{attachmentId}")]
        public async Task<IActionResult> RemoveByByAttachmentId(int attachmentId)
        {
            try
            {
                await _attachmentManager.RemoveByByAttachmentIdAsync(attachmentId,UserId);
                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }



		[HttpGet("{attachmentId}")]
		public async Task<IActionResult> GetAttachment(int attachmentId)
		{
			try
			{
                bool hasAccess=await _attachmentManager.CheckUserAccess(attachmentId,UserId);
                if (hasAccess)
                {
					string? attachmentQuery = await _attachmentManager.GetAttachmentQuery(attachmentId, UserId, new AttachmentActionDto(), default(int));

					return Ok(new ApiResponseDto<string?>(
						Data: attachmentQuery,
						Success: !string.IsNullOrWhiteSpace(attachmentQuery))
					);
                }
                else
                {
                   return Unauthorized();
                }
				
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}


		[HttpGet("guide")]
        public IActionResult GetUserGuideAttachment()
        {
            try
            {
                (string? filename, byte[]? bytes) = _attachmentManager.GetUserGuideAttachment();
                if (filename != null && bytes != null)
                {
                    return FileBytes(filename, bytes);
                }
                return Ok(new ApiResponseDto<string?>( Data: null, Success: false));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Converts a PowerPoint attachment to PDF and returns the PDF bytes
        /// </summary>
        [HttpGet("{attachmentId}/as-pdf")]
        public async Task<IActionResult> GetAttachmentAsPdf(int attachmentId)
        {
            try
            {
                bool hasAccess = await _attachmentManager.CheckUserAccess(attachmentId, UserId);
                if (!hasAccess)
                {
                    return Unauthorized();
                }

                // Get the attachment bytes
                (string? filename, byte[]? bytes) = await _attachmentManager.GetAttachmentById(attachmentId);
                if (filename == null || bytes == null)
                {
                    return NotFound(new ApiResponseDto<string?>(Data: null, Success: false, Message: "Attachment not found"));
                }

                // Check if it's a PowerPoint file
                string extension = System.IO.Path.GetExtension(filename).ToLower();
                if (extension != ".pptx" && extension != ".ppt")
                {
                    return BadRequest(new ApiResponseDto<string?>(Data: null, Success: false, Message: "File is not a PowerPoint presentation"));
                }

                // Convert to PDF
                byte[] pdfBytes = FilePowerPoint.ConvertToPdf(bytes);
                string pdfFilename = System.IO.Path.GetFileNameWithoutExtension(filename) + ".pdf";

                return FileBytes(pdfFilename, pdfBytes);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
