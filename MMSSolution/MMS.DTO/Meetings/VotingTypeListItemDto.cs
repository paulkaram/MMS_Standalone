namespace MMS.DTO.Meetings
{
	public class VotingTypeListItemDto
	{
		public int Id { get; set; }

		public string NameAr { get; set; } = null!;

		public string NameEn { get; set; } = null!;

		public bool? Active { get; set; }

		public int? DisplayOrder { get; set; }


		public virtual List<VotingOptionsListItemDto> VotingOptions { get; set; } = new List<VotingOptionsListItemDto>();
	}
}
