using Intalio.Tools.Common.Extensions.FileExtensions;
using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;

namespace MMS.BLL.Mapping
{
    internal class AttachmentMappingConfiguration : IRegister
    {
        void IRegister.Register(TypeAdapterConfig config)
        {
            config.NewConfig<Attachment, AttachmentListItemDto>()

             .Map(dest => dest.Id, src => src.Id)
             .Map(dest => dest.Name, src => src.Title)
             .Map(dest => dest.size, src => src.FileSize)
             .Map(dest => dest.Type, src => src.FileName.GetFileType().ToString());
			config.NewConfig<(Attachment attachment,LanguageDbEnum language), AttachmentListItemDto>()

			 .Map(dest => dest.Id, src => src.attachment.Id)
			 .Map(dest => dest.Name, src => src.attachment.Title)
			 .Map(dest => dest.RecordId, src => src.attachment.RecordId)
			 .Map(dest => dest.RecordTypeId, src => src.attachment.RecordTypeId)
			 .Map(dest => dest.size, src => src.attachment.FileSize)
			 .Map(dest => dest.PrivacyId, src => src.attachment.PrivacyId)
			 .Map(dest => dest.PrivacyName, src => src.language==LanguageDbEnum.Arabic? src.attachment.Privacy.NameAr:src.attachment.Privacy.Name)
			 .Map(dest => dest.RecordTypeName, src => src.language==LanguageDbEnum.Arabic? src.attachment.RecordType.DisplayNameAr:src.attachment.RecordType.DisplayNameEn)
			 .Map(dest => dest.Type, src => src.attachment.FileName.GetFileType().ToString());
		}
    }
}
