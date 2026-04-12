namespace MMS.DTO.Meetings
{
    public class MeetingAgendaListItemDto
    {
        public int? Id { get; set; }
		public int MeetingId { get; set; }
		public string? Title { get; set; }
        public int Duration { get; set; }
        public string? Voting { get; set; }
		public int? CommitteeDutyId { get; set; }
		public int VotingTypeId { get; set; }
		public List<MeetingTopicPostDto> AgendaTopics { get; set; }
		public VotingTypeListItemDto VotingType { get; set; }
		public List<MeetingAgendaRecommendationListItemDto> MeetingAgendaRecommendations { get; set; }

	}
}
