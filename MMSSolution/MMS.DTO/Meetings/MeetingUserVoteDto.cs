
namespace MMS.DTO.Meetings
{
	public class MeetingUserVoteDto
	{
		public int Id { get; set; }

		public string UserId { get; set; } = null!;

		public string? UserName { get; set; }

		public int VottingOptionId { get; set; }

		public string? SelectedOptionName { get; set; }

		public DateTime CreatedDate { get; set; }

		public int MeetingAgendaId { get; set; }

	}
}
