using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Data.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.UnitOfWork.MMS
{
    internal class UserManagementUnitOfWork : UnitOfWork, IUserManagementUnitOfWork
    {
        public IUserRepository Users { get; private set; }
        public IStructureRepository Structures { get; private set; }
        public IPermissionRepository Permissions { get; private set; }
        public IUserSignatureRepository UserSignature { get; private set; }
        public IPermissionMatrixRepository PermissionMatrices { get; private set; }
        public IRoleRepository Roles { get; private set; }
        public IUserStructureRepository UserStructures { get; private set; }
        public IRefreshTokenRepository RefreshTokens { get; private set; }
        public ICommitteeClassificationRepository CommitteeClassifications { get; private set; }

        public UserManagementUnitOfWork(MmsContext context) : base(context)
        {
            Users = new UserRepository(_dbContext);
            Structures = new StructureRepository(_dbContext);
            Permissions = new PermissionRepository(_dbContext);
            UserSignature = new UserSignatureRepository(_dbContext);
            PermissionMatrices = new PermissionMatrixRepository(_dbContext);
            Roles = new RoleRepository(_dbContext);
            UserStructures = new UserStructureRepository(_dbContext);
            RefreshTokens = new RefreshTokenRepository(_dbContext);
            CommitteeClassifications = new CommitteeClassificationRepository(_dbContext);
        }
    }
}
