using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DTO.Reports
{
	public record AttendanceReportDto(string UserId,int MeetingId,DateTime MeetingDate,string Title,string CommitteeName,string UserName,bool Attended);
}
