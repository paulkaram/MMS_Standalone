
using MMS.DAL.Core.Repositories.MMS;

namespace MMS.DAL.Core.UnitOfWork.MMS
{
    public interface IProcessUnitOfWork : IUnitOfWork
	{
		IAttachmentRepository Attachments { get; }
		IAttachmentsSignaturesRepository AttachmentsSignatures { get; }
		IAttachmentAnnotationRepository AttachmentAnnotations { get; }
		IAttachmentVersionRepository AttachmentVersions { get; }
        IEmailTemplateRepository EmailTemplates { get; }

		IVActiveActivityInstanceRepository VActiveActivityInstances { get; }
		IUserSignatureRepository UserSignatures { get; }
		
	}
}
