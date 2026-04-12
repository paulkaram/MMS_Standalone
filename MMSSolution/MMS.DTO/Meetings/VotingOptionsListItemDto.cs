
namespace MMS.DTO.Meetings
{
	public class VotingOptionsListItemDto
	{
		public int Id { get; set; }

		public string NameAr { get; set; } = null!;

		public string NameEn { get; set; } = null!;

		public decimal? Weight { get; set; }

		public bool? Active { get; set; }

		public int? DisplayOrder { get; set; }

		public int? VotingTypeId { get; set; }
	}
}
