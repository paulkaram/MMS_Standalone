using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class AgendaTopicRepository : Repository<AgendaTopic>, IAgendaTopicRepository
    {
        public AgendaTopicRepository(DbContext context) : base(context)
        {
        }
    }
}
