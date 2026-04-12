using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.BLL.Managers;
using MMS.DTO;
using MMS.DTO.Meetings;

namespace MMS.API.Controllers;

[Route("api/meetings/{meetingId}/transcripts")]
[ApiController]
public class MeetingTranscriptsController : IntalioBaseController
{
    private readonly MeetingTranscriptManager _transcriptManager;

    public MeetingTranscriptsController(MeetingTranscriptManager transcriptManager)
    {
        _transcriptManager = transcriptManager;
    }

    [HttpPost]
    public async Task<IActionResult> UploadAndTranscribe(
        int meetingId,
        IFormFile audioFile,
        [FromForm] int? agendaId = null,
        [FromForm] string? language = null,
        [FromForm] string? attendeeName = null)
    {
        try
        {
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest(new ApiResponseDto<string>("No audio file provided"));

            using var stream = audioFile.OpenReadStream();
            var result = await _transcriptManager.UploadAndTranscribeAsync(
                UserId, meetingId, agendaId, stream, audioFile.FileName, language,
                attendeeUserId: UserId, attendeeName: attendeeName);

            return Ok(new ApiResponseDto<MeetingTranscriptListDto>(result));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTranscripts(int meetingId)
    {
        try
        {
            var transcripts = await _transcriptManager.GetTranscriptsAsync(meetingId);
            return Ok(new ApiResponseDto<List<MeetingTranscriptListDto>>(transcripts));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    [HttpGet("combined")]
    public async Task<IActionResult> GetCombinedTranscript(int meetingId)
    {
        try
        {
            var result = await _transcriptManager.GetCombinedTranscriptAsync(meetingId);
            return Ok(new ApiResponseDto<MeetingCombinedTranscriptDto>(result));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    [HttpPost("combined/summarize")]
    public async Task<IActionResult> GenerateCombinedSummary(int meetingId)
    {
        try
        {
            var result = await _transcriptManager.GenerateCombinedSummaryAsync(meetingId);
            if (result == null)
                return NotFound(new ApiResponseDto<string>("No completed transcripts found"));

            return Ok(new ApiResponseDto<MeetingCombinedTranscriptDto>(result));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    [HttpGet("{transcriptId}")]
    public async Task<IActionResult> GetTranscript(int meetingId, int transcriptId)
    {
        try
        {
            var transcript = await _transcriptManager.GetTranscriptAsync(transcriptId);
            if (transcript == null || transcript.MeetingId != meetingId)
                return NotFound();

            return Ok(new ApiResponseDto<MeetingTranscriptListDto>(transcript));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    [HttpPost("{transcriptId}/summarize")]
    public async Task<IActionResult> GenerateSummary(int meetingId, int transcriptId)
    {
        try
        {
            var result = await _transcriptManager.GenerateSummaryAsync(transcriptId);
            if (result == null)
                return NotFound();

            return Ok(new ApiResponseDto<MeetingTranscriptListDto>(result));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    [HttpDelete("{transcriptId}")]
    public async Task<IActionResult> DeleteTranscript(int meetingId, int transcriptId)
    {
        try
        {
            var deleted = await _transcriptManager.DeleteTranscriptAsync(transcriptId);
            if (!deleted) return NotFound();

            return Ok(new ApiResponseDto<bool>(true));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
}
