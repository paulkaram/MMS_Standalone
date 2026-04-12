namespace MMS.DTO.Chats
{
	public class ChatMessagePostDto
	{
		public int ChatId { get; set; }
		public int? MeetingId { get; set; }
		public string MessageText { get; set; }

		//TODO add support for send image
		//public List<MessageContentDto>

	}
}
