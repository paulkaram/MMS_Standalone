namespace MMS.BLL.Managers;

/// <summary>
/// Centralized notification service for MMS.
/// All notifications go through IAM templates — zero hardcoded text.
/// Controllers/managers call these methods; this service handles the rest.
/// </summary>
public class MmsNotificationService
{
    private readonly IamNotificationClient _client;

    public MmsNotificationService(IamNotificationClient client)
    {
        _client = client;
    }

    public async Task MeetingInviteSent(string meetingTitle, DateTime meetingDate, string startTime,
        List<string> attendeeUserIds, string organizerUserId)
    {
        var ids = FilterIds(attendeeUserIds, organizerUserId);
        if (ids.Count == 0) return;

        await _client.SendBulkAsync("MMS.MeetingInvite", ids, new Dictionary<string, string>
        {
            ["MeetingTitle"] = meetingTitle,
            ["Date"] = meetingDate.ToString("MMM dd, yyyy"),
            ["Time"] = startTime
        });
    }

    public async Task MeetingCanceled(string meetingTitle, DateTime meetingDate,
        List<string> attendeeUserIds, string organizerUserId)
    {
        var ids = FilterIds(attendeeUserIds, organizerUserId);
        if (ids.Count == 0) return;

        await _client.SendBulkAsync("MMS.MeetingCanceled", ids, new Dictionary<string, string>
        {
            ["MeetingTitle"] = meetingTitle,
            ["Date"] = meetingDate.ToString("MMM dd, yyyy")
        });
    }

    public async Task VoteCast(string meetingOwnerUserId, string voterUserId, string? meetingTitle = null)
    {
        if (string.IsNullOrEmpty(meetingOwnerUserId) || meetingOwnerUserId == voterUserId) return;

        await _client.SendAsync("MMS.VoteCast", meetingOwnerUserId, new Dictionary<string, string>
        {
            ["MeetingTitle"] = meetingTitle ?? ""
        });
    }

    public async Task MeetingTaskAssigned(string assignedUserId, string meetingTitle, string assignerUserId)
    {
        if (string.IsNullOrEmpty(assignedUserId) || assignedUserId == assignerUserId) return;

        await _client.SendAsync("MMS.MeetingTaskAssigned", assignedUserId, new Dictionary<string, string>
        {
            ["MeetingTitle"] = meetingTitle
        });
    }

    public async Task RecommendationAssigned(string ownerUserId, string meetingTitle, string assignerUserId)
    {
        if (string.IsNullOrEmpty(ownerUserId) || ownerUserId == assignerUserId) return;

        await _client.SendAsync("MMS.RecommendationAssigned", ownerUserId, new Dictionary<string, string>
        {
            ["MeetingTitle"] = meetingTitle
        });
    }

    private static List<string> FilterIds(List<string> ids, string excludeId)
    {
        return ids.Where(id => !string.IsNullOrEmpty(id) && id != excludeId).Distinct().ToList();
    }
}
