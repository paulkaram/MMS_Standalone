using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MMS.BLL.Constants;
using MMS.DAL.Models.MMS;
using MMS.DTO.Meetings;

namespace MMS.BLL.Managers;

public class MeetingTranscriptManager
{
    private readonly MmsContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MeetingTranscriptManager> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MeetingTranscriptManager(
        MmsContext context,
        IConfiguration configuration,
        ILogger<MeetingTranscriptManager> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MeetingTranscriptListDto> UploadAndTranscribeAsync(
        string userId, int meetingId, int? agendaId,
        Stream audioStream, string audioFileName, string? language,
        string? attendeeUserId = null, string? attendeeName = null)
    {
        var transcript = new MeetingTranscript
        {
            MeetingId = meetingId,
            AgendaId = agendaId,
            Language = language,
            AudioFileName = audioFileName,
            AttendeeUserId = attendeeUserId ?? userId,
            AttendeeName = attendeeName,
            Status = TranscriptStatus.Transcribing,
            CreatedBy = userId,
            CreatedDate = DateTime.Now
        };

        _context.MeetingTranscripts.Add(transcript);
        await _context.SaveChangesAsync();

        try
        {
            using var client = CreateAiClient();
            using var content = new MultipartFormDataContent();

            var streamContent = new StreamContent(audioStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "file", audioFileName);
            content.Add(new StringContent(language ?? ""), "language");
            content.Add(new StringContent("json"), "response_format");

            var response = await client.PostAsync(
                GetAiUrl(AiServiceConstants.TranscribeEndpoint), content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                transcript.Status = TranscriptStatus.Failed;
                transcript.ErrorMessage = $"AI service returned {response.StatusCode}: {errorBody}";
                await _context.SaveChangesAsync();
                return MapToDto(transcript);
            }

            var json = await response.Content.ReadAsStringAsync();
            transcript.TranscriptText = ExtractText(json);
            transcript.DurationSeconds = ExtractDuration(json);
            transcript.Status = TranscriptStatus.Completed;
            await _context.SaveChangesAsync();

            return MapToDto(transcript);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to transcribe audio for meeting {MeetingId}", meetingId);
            transcript.Status = TranscriptStatus.Failed;
            transcript.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
            return MapToDto(transcript);
        }
    }

    public async Task<MeetingTranscriptListDto?> GenerateSummaryAsync(int transcriptId)
    {
        var transcript = await _context.MeetingTranscripts.FindAsync(transcriptId);
        if (transcript == null) return null;

        try
        {
            var summaryText = await SendChatRequestAsync(
                GetSummaryPrompt(transcript.Language ?? "en"),
                transcript.TranscriptText);

            if (!string.IsNullOrEmpty(summaryText))
            {
                transcript.SummaryText = summaryText;
                transcript.Status = TranscriptStatus.Summarized;
                transcript.ErrorMessage = null;
            }
            else
            {
                transcript.ErrorMessage = "AI service returned empty summary";
            }

            await _context.SaveChangesAsync();
            return MapToDto(transcript);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate summary for transcript {TranscriptId}", transcriptId);
            transcript.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
            return MapToDto(transcript);
        }
    }

    public async Task<MeetingCombinedTranscriptDto?> GenerateCombinedSummaryAsync(int meetingId)
    {
        var transcripts = await _context.MeetingTranscripts
            .Where(t => t.MeetingId == meetingId
                && (t.Status == TranscriptStatus.Completed || t.Status == TranscriptStatus.Summarized)
                && t.TranscriptText != "")
            .OrderBy(t => t.CreatedDate)
            .ToListAsync();

        if (transcripts.Count == 0) return null;

        var combinedText = string.Join("\n\n", transcripts.Select(t =>
            $"[{t.AttendeeName ?? "Attendee"}]:\n{t.TranscriptText}"));

        var lang = transcripts.FirstOrDefault()?.Language ?? "en";
        var summaryText = await SendChatRequestAsync(GetSummaryPrompt(lang), combinedText);

        if (!string.IsNullOrEmpty(summaryText))
        {
            foreach (var t in transcripts)
            {
                t.SummaryText = summaryText;
                t.Status = TranscriptStatus.Summarized;
            }
            await _context.SaveChangesAsync();
        }

        var allTranscripts = await _context.MeetingTranscripts
            .AsNoTracking()
            .Where(t => t.MeetingId == meetingId)
            .OrderBy(t => t.CreatedDate)
            .ToListAsync();

        return new MeetingCombinedTranscriptDto
        {
            MeetingId = meetingId,
            AttendeeTranscripts = allTranscripts.Select(MapToDto).ToList(),
            CombinedSummary = summaryText,
            TotalAttendees = allTranscripts.Select(t => t.AttendeeUserId).Distinct().Count(),
            CompletedTranscripts = allTranscripts.Count(t =>
                t.Status == TranscriptStatus.Completed || t.Status == TranscriptStatus.Summarized)
        };
    }

    public async Task<List<MeetingTranscriptListDto>> GetTranscriptsAsync(int meetingId)
    {
        var transcripts = await _context.MeetingTranscripts
            .AsNoTracking()
            .Where(t => t.MeetingId == meetingId)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();

        return transcripts.Select(MapToDto).ToList();
    }

    public async Task<MeetingTranscriptListDto?> GetTranscriptAsync(int transcriptId)
    {
        var transcript = await _context.MeetingTranscripts
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transcriptId);

        return transcript == null ? null : MapToDto(transcript);
    }

    public async Task<bool> DeleteTranscriptAsync(int transcriptId)
    {
        var transcript = await _context.MeetingTranscripts.FindAsync(transcriptId);
        if (transcript == null) return false;

        _context.MeetingTranscripts.Remove(transcript);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MeetingCombinedTranscriptDto> GetCombinedTranscriptAsync(int meetingId)
    {
        var transcripts = await _context.MeetingTranscripts
            .AsNoTracking()
            .Where(t => t.MeetingId == meetingId)
            .OrderBy(t => t.CreatedDate)
            .ToListAsync();

        return new MeetingCombinedTranscriptDto
        {
            MeetingId = meetingId,
            AttendeeTranscripts = transcripts.Select(MapToDto).ToList(),
            TotalAttendees = transcripts.Select(t => t.AttendeeUserId).Distinct().Count(),
            CompletedTranscripts = transcripts.Count(t =>
                t.Status == TranscriptStatus.Completed || t.Status == TranscriptStatus.Summarized)
        };
    }

    #region Private Helpers

    private HttpClient CreateAiClient()
    {
        var apiKey = _configuration[AppSettingsConstants.AiServiceApiKey];
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        if (!string.IsNullOrEmpty(apiKey))
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return client;
    }

    private string GetAiUrl(string endpoint)
    {
        var baseUrl = (_configuration[AppSettingsConstants.AiServiceBaseUrl]
            ?? "http://localhost:5175").TrimEnd('/');
        return $"{baseUrl}{endpoint}";
    }

    private string GetSummaryPrompt(string language)
    {
        return language == "ar"
            ? _configuration[AppSettingsConstants.AiTranscriptSummaryPromptAR] ?? ""
            : _configuration[AppSettingsConstants.AiTranscriptSummaryPromptEN] ?? "";
    }

    private async Task<string?> SendChatRequestAsync(string systemPrompt, string userMessage)
    {
        try
        {
            using var client = CreateAiClient();

            var maxTokens = int.TryParse(
                _configuration[AppSettingsConstants.AiSummaryMaxTokens], out var mt) ? mt : 2000;
            var temperature = double.TryParse(
                _configuration[AppSettingsConstants.AiSummaryTemperature], out var temp) ? temp : 0.3;

            var chatRequest = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                max_tokens = maxTokens,
                temperature,
                stream = false
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(chatRequest, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                GetAiUrl(AiServiceConstants.ChatEndpoint), jsonContent);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return ExtractChatResponse(json);

            _logger.LogWarning("AI chat request failed: {StatusCode} {Body}",
                response.StatusCode, json);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI chat request error");
            return null;
        }
    }

    private static string ExtractChatResponse(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.String)
                return root.GetString() ?? json;

            foreach (var prop in new[] { "response", "content", "message", "text", "result", "data" })
            {
                if (!root.TryGetProperty(prop, out var val)) continue;

                if (val.ValueKind == JsonValueKind.String)
                    return val.GetString() ?? "";

                if (val.ValueKind == JsonValueKind.Object
                    && val.TryGetProperty("content", out var nested)
                    && nested.ValueKind == JsonValueKind.String)
                    return nested.GetString() ?? "";
            }
        }
        catch { }
        return json;
    }

    private static string ExtractText(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.String)
                return root.GetString() ?? json;
            foreach (var prop in new[] { "text", "transcription", "result", "transcript", "content", "data" })
            {
                if (root.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
                    return val.GetString() ?? "";
            }
        }
        catch { }
        return json;
    }

    private static int? ExtractDuration(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            foreach (var prop in new[] { "duration", "durationSeconds", "duration_seconds", "length" })
            {
                if (root.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.Number)
                    return (int)val.GetDouble();
            }
        }
        catch { }
        return null;
    }

    private static MeetingTranscriptListDto MapToDto(MeetingTranscript t) => new()
    {
        Id = t.Id,
        MeetingId = t.MeetingId,
        AgendaId = t.AgendaId,
        TranscriptText = t.TranscriptText,
        SummaryText = t.SummaryText,
        Language = t.Language,
        AudioFileName = t.AudioFileName,
        DurationSeconds = t.DurationSeconds,
        Status = t.Status,
        ErrorMessage = t.ErrorMessage,
        AttendeeUserId = t.AttendeeUserId,
        AttendeeName = t.AttendeeName,
        CreatedBy = t.CreatedBy,
        CreatedDate = t.CreatedDate
    };

    #endregion
}
