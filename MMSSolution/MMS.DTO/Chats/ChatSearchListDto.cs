using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DTO.Chats
{
	public class ChatSearchListDto
	{

		public int ChatId { get; set; }
		public string UserId { get; set; } = null!;
		public string Name { get; set; }
	}
}
