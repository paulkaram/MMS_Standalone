namespace MMS.DTO.Meetings
{
    /// <summary>
    /// DTO for Minutes of Meeting data
    /// </summary>
    public class MeetingMinutesDto
    {
        // Meeting Header Info
        public int MeetingId { get; set; }
        public string MeetingNumber { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        // Committee/Council Info
        public string? CommitteeName { get; set; }
        public string? CouncilSessionName { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationLogo { get; set; }

        // Date & Time
        public DateTime Date { get; set; }
        public string? HijriDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? ActualDuration { get; set; }

        // Location
        public string? Location { get; set; }
        public string MeetingType { get; set; } = "physical";
        public string? MeetingUrl { get; set; }

        // Attendees
        public List<MinutesAttendeeDto> Attendees { get; set; } = new();
        public int TotalAttendees { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public bool QuorumMet { get; set; }

        // Agenda Items
        public List<MinutesAgendaItemDto> AgendaItems { get; set; } = new();

        // Summary
        public string? MeetingSummary { get; set; }

        // Signatures
        public string? ChairmanName { get; set; }
        public string ChairmanTitle { get; set; } = "Chairman";
        public string? SecretaryName { get; set; }
        public string SecretaryTitle { get; set; } = "Secretary";

        // Versioning
        public int Version { get; set; }
        public string VersionLabel { get; set; } = "1.0";
        public string Status { get; set; } = "draft";
        public DateTime GeneratedAt { get; set; }
        public string? GeneratedBy { get; set; }
        public string Language { get; set; } = "ar";
    }

    public class MinutesAttendeeDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string Role { get; set; } = "member"; // chairman, secretary, member, guest
        public bool Attended { get; set; }
        public DateTime? AttendedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class MinutesAgendaItemDto
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Time tracking
        public int? PlannedDuration { get; set; }
        public int? ActualDuration { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        // Content
        public string? Summary { get; set; }
        public List<MinutesNoteDto> DiscussionNotes { get; set; } = new();

        // Voting (if applicable)
        public bool HasVoting { get; set; }
        public MinutesVotingResultsDto? VotingResults { get; set; }

        // Recommendations
        public List<MinutesRecommendationDto> Recommendations { get; set; } = new();
    }

    public class MinutesNoteDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MinutesVotingResultsDto
    {
        public string? VotingType { get; set; }
        public int TotalVoters { get; set; }
        public List<MinutesVotingOptionDto> Options { get; set; } = new();
        public string? Outcome { get; set; }
    }

    public class MinutesVotingOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameAr { get; set; }
        public int VoteCount { get; set; }
        public int Percentage { get; set; }
        public List<string> Voters { get; set; } = new();
    }

    public class MinutesRecommendationDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? OwnerName { get; set; }
        public string? OwnerId { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Priority { get; set; }
    }

    /// <summary>
    /// Request DTO for generating meeting minutes
    /// </summary>
    public class GenerateMeetingMinutesRequestDto
    {
        public int MeetingId { get; set; }
        public bool IncludePrivateNotes { get; set; }
        public bool IncludeVoterNames { get; set; } = true;
        public string Language { get; set; } = "ar";
    }

    /// <summary>
    /// Response DTO for generated meeting minutes
    /// </summary>
    public class GenerateMeetingMinutesResponseDto
    {
        public bool Success { get; set; }
        public int? AttachmentId { get; set; }
        public string? FileName { get; set; }
        public int Version { get; set; }
        public string? PreviewUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public string? Message { get; set; }
        /// <summary>
        /// The new meeting status after generating minutes (e.g., 6 = PendingInitialMeetingMinutesApproval)
        /// </summary>
        public int? NewMeetingStatusId { get; set; }
    }

    /// <summary>
    /// DTO for minutes version history
    /// </summary>
    public class MinutesVersionDto
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public int Version { get; set; }
        public string VersionLabel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // File info
        public string? FileName { get; set; }
        public long FileSize { get; set; }

        // Metadata
        public DateTime GeneratedAt { get; set; }
        public int GeneratedBy { get; set; }
        public string? GeneratedByName { get; set; }

        // Approval workflow
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
    }
}
