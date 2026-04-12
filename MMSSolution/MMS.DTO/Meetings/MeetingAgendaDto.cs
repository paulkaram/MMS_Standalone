namespace MMS.DTO.Meetings
{
	public class MeetingAgendaDto
	{
		public MeetingAgendaDto()
		{
			MeetingAgendaRecommendations = new List<MeetingAgendaRecommendationDto>();
		}
		public int? Id { get; set; }
		public int MeetingId { get; set; }
		public string? Title { get; set; }
		public int Duration { get; set; }
		public List<MeetingAgendaRecommendationDto> MeetingAgendaRecommendations { get; set; }
	}
}
