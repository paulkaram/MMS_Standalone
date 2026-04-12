using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DTO.Chats
{
	public class ChatPostDto
	{
		public string  Name { get; set; }
		public bool IsGroup { get; set; }
		public  List<string> UsersIds { get; set; }

	}
}
