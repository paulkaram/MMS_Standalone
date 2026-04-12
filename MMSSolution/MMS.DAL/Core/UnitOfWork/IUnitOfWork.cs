namespace MMS.DAL.Core.UnitOfWork
{
	public interface IUnitOfWork
	{
		Task<int> SaveChangesAsync();
	}
}
