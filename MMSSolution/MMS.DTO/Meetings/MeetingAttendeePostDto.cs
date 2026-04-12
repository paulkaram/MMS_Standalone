namespace MMS.DTO.Meetings
{
    public class MeetingAttendeePostDto
    {
        public string UserId { get; set; } = null!;
        public bool NeedsApproval { get; set; }
        public string JobTitle { get; set; }
        public string Name { get; set; }
        public bool Attended { get; set; }
        public bool HasProfilePicture { get; set; }
    }
}
