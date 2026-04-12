using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class CommitteeStatusesRepository : Repository<CommitteeStatus>, ICommitteeStatusesRepository
    {
        public CommitteeStatusesRepository(DbContext context) : base(context)
        {
        }
    }
}
