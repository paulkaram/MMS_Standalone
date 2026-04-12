using System.Text.Json.Serialization;

namespace MMS.DTO.Meetings
{
	public class SearchRecommendationsDto
	{
		[JsonPropertyName("statusId")]
		public int? StatusId { get; set; }

		[JsonPropertyName("meetingReferenceNo")]
		public string? MeetingReferenceNo { get; set; }

		[JsonPropertyName("fromDate")]
		public DateTime? FromDate { get; set; }

		[JsonPropertyName("toDate")]
		public DateTime? ToDate { get; set; }

		[JsonPropertyName("title")]
		public string? Title { get; set; }
	}
}
