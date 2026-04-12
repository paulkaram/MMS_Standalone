using MMS.DAL.Data.Repositories;
using MMS.DAL.Models.MMS;
using MMS.DAL.Core.Repositories.MMS;
using Microsoft.EntityFrameworkCore;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class ViewerTokenRepository : Repository<ViewerToken>, IViewerTokenRepository
    {
        public ViewerTokenRepository(DbContext context) : base(context)
        {
        }
    }
}
