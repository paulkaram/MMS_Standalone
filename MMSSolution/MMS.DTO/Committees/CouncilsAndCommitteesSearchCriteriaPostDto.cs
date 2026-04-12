using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DTO.Committees
{
	public class CouncilsAndCommitteesSearchCriteriaPostDto
	{
		public string MeetingTitle { get; set; }
		public string AgendaTitle { get; set; }
		public string AgendaTopicTitle { get; set; }
		public string AgendaNote { get; set; }
		public string RecommendationTitle { get; set; }
		public string CommitteeTitle { get; set; }

	}
}
