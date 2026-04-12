namespace MMS.DTO.Meetings
{
    public class MeetingAgendaPostDto
    {
        public MeetingAgendaPostDto()
        {
            this.AgendaTopics = new List<MeetingTopicPostDto>();
        }
        public int? Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public int? CommitteeDutyId { get; set; }
        public int VotingTypeId { get; set; }
        public string Voting { get; set; }
        public string Duty { get; set; }
        public List<MeetingTopicPostDto>? AgendaTopics { get; set; }
    }
}

