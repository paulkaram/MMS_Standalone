using Microsoft.EntityFrameworkCore;
using MMS.DAL.Models.MMS;
using MMS.DAL.Core.Repositories.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class SignatureTypeRepository : Repository<SignatureType>, ISignatureTypeRepository
	{
		public SignatureTypeRepository(DbContext context) : base(context)
		{
		}

	}
}