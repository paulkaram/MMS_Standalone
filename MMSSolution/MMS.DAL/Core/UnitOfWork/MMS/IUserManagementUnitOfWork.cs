
using MMS.DAL.Core.Repositories.MMS;

namespace MMS.DAL.Core.UnitOfWork.MMS
{
	public interface IUserManagementUnitOfWork : IUnitOfWork
	{
		IUserRepository Users { get; }
		IStructureRepository Structures { get; }
		IUserStructureRepository UserStructures { get; }
		IPermissionRepository Permissions { get; }
		IRoleRepository Roles { get; }
        IPermissionMatrixRepository PermissionMatrices { get; }
		IUserSignatureRepository UserSignature { get; }
		IRefreshTokenRepository RefreshTokens { get; }
		ICommitteeClassificationRepository CommitteeClassifications { get; }
	}
}
