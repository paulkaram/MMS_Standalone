using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DTO.Chats
{
	public class ChatListDto
	{
		public int Id {  get; set; }
		public string Name { get; set; }
		public string LastMessage { get; set; }
		public bool IsGroup { get; set; }
		public DateTime CreatedAt { get; set; }

		public DateTime? UpdatedAt { get; set; }
		public int UnreadMessages { get; set; }

		public List<ChatMemberDto> ChatMembers { get; set; }
		public ChatListDto() {
			ChatMembers = new List<ChatMemberDto>();
		}

	}
}
