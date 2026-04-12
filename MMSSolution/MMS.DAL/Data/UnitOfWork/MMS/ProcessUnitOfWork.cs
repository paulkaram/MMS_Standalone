using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Data.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.UnitOfWork.MMS
{
    internal class ProcessUnitOfWork : UnitOfWork, IProcessUnitOfWork
    {
        public IAttachmentRepository Attachments { get; private set; }
        public IAttachmentVersionRepository AttachmentVersions { get; private set; }

        public IVActiveActivityInstanceRepository VActiveActivityInstances { get; private set; }
        public IAttachmentAnnotationRepository AttachmentAnnotations { get; private set; }
        public IEmailTemplateRepository EmailTemplates { get; private set; }
        public IAttachmentsSignaturesRepository AttachmentsSignatures { get; private set; }
        public IUserSignatureRepository UserSignatures { get; private set; }

        public ProcessUnitOfWork(MmsContext context) : base(context)
        {
            Attachments = new AttachmentRepository(context);
            AttachmentVersions = new AttachmentVersionRepository(context);

            VActiveActivityInstances = new VActiveActivityInstanceRepository(context);
            AttachmentAnnotations = new AttachmentAnnotationRepository(context);
            EmailTemplates = new EmailTemplateRepository(context);
			AttachmentsSignatures=new AttachmentsSignaturesRepository(context);
			UserSignatures=new UserSignatureRepository(context);

		}
    }
}
