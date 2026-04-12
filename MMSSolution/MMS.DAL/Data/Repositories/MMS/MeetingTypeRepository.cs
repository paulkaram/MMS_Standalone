using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class MeetingTypeRepository : Repository<MeetingType>, IMeetingTypeRepository
    {
        public MeetingTypeRepository(DbContext context) : base(context)
        {
        }
    }
}
