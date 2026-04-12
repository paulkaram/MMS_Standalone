using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DTO.Chats
{
	public class ChatMemberDto
	{
		public int Id { get; set; }

		public int ChatId { get; set; }

		public string UserId { get; set; } = null!;

		public bool IsAdmin { get; set; }

		public DateTime CreatedAt { get; set; }

		public string Name { get; set; }
		public string Nickname { get; set; }
	}
}
