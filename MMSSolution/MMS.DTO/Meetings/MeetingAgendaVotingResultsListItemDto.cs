using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DTO.Meetings
{
	public record MeetingAgendaVotingResultsListItemDto(string UserFullName,string AgendaTitle,string SelectedOption ,DateTime CreatedDate);
	
}
