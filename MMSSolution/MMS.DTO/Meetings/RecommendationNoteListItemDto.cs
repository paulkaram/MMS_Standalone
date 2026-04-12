namespace MMS.DTO.Meetings
{
	public record RecommendationNoteListItemDto( DateTime? CreatedAt, int RecommendationId, string Text, int Id=0);

}
