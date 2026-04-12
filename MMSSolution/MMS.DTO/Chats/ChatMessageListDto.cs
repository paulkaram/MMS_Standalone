using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DTO.Chats
{
	public class ChatMessageListDto
	{
		public int Id { get; set; }

		public string UserId { get; set; } = null!;
		public bool  Me { get; set; }

		public string MessageText { get; set; } = null!;

		public DateTime SentAt { get; set; }
		public bool HasProfilePicture { get; set; }
		public string? UserName { get; set; }
	}
}
