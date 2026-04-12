using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Sessions;

namespace MMS.BLL.Mapping
{
    internal class SessionMappingConfiguration : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<(Session session, LanguageDbEnum Language), SessionDto>()
                .Map(dest => dest.Id, src => src.session.Id)
                .Map(dest => dest.ReferenceNumber, src => src.session.ReferenceNumber)
                .Map(dest => dest.ExternalReferenceNumber, src => src.session.ExternalReferenceNumber)
                .Map(dest => dest.Subject, src => src.session.Subject)
                .Map(dest => dest.Note, src => src.session.Note)
                .Map(dest => dest.MeetingDate, src => src.session.MeetingDate)
                .Map(dest => dest.DueDate, src => src.session.DueDate)
                .Map(dest => dest.Tags, src => src.session.Tags)
                .Map(dest => dest.CommitteeId, src => src.session.CommitteeId)
                .Map(dest => dest.CommitteeName, src => src.Language == LanguageDbEnum.Arabic
                    ? src.session.Committee.NameAr
                    : src.session.Committee.NameEn)
                .Map(dest => dest.CreatedBy, src => src.session.CreatedBy)
                .Map(dest => dest.CreatedDate, src => src.session.CreatedDate)
                .Map(dest => dest.SessionItems, src => src.session.SessionItems
                    .OrderBy(i => i.Order)
                    .Select(i => new SessionItemDto
                    {
                        Id = i.Id,
                        ExternalId = i.ExternalId,
                        Subject = i.Subject,
                        ItemTypeId = i.ItemTypeId,
                        ItemTypeName = src.Language == LanguageDbEnum.Arabic
                            ? i.ItemType.NameAr
                            : i.ItemType.NameEn,
                        Tags = i.Tags,
                        InternalNote = i.InternalNote,
                        RelatedSessionItemId = i.RelatedSessionItemId,
                        RelatedSessionItemSubject = i.RelatedSessionItem != null
                            ? i.RelatedSessionItem.Subject
                            : null,
                        Order = i.Order
                    }).ToList());
        }
    }
}
