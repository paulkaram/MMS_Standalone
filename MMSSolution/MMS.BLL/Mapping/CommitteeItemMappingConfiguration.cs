using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.CommitteeItems;

namespace MMS.BLL.Mapping
{
    internal class CommitteeItemMappingConfiguration : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<(CommitteeItem item, LanguageDbEnum Language), CommitteeItemDto>()
                .Map(dest => dest.Id, src => src.item.Id)
                .Map(dest => dest.CommitteeId, src => src.item.CommitteeId)
                .Map(dest => dest.ReferenceNumber, src => src.item.ReferenceNumber)
                .Map(dest => dest.ExternalReferenceNumber, src => src.item.ExternalReferenceNumber)
                .Map(dest => dest.Content, src => src.item.Content)
                .Map(dest => dest.ItemTypeId, src => src.item.ItemTypeId)
                .Map(dest => dest.ItemTypeName, src => src.item.ItemType != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.item.ItemType.NameAr : src.item.ItemType.NameEn)
                    : null)
                .Map(dest => dest.Tags, src => src.item.Tags)
                .Map(dest => dest.InternalNote, src => src.item.InternalNote)
                .Map(dest => dest.RelatedItemId, src => src.item.RelatedItemId)
                .Map(dest => dest.RelatedItemReferenceNumber, src => src.item.RelatedItem != null
                    ? src.item.RelatedItem.ReferenceNumber
                    : null)
                .Map(dest => dest.IsPrivate, src => src.item.IsPrivate)
                .Map(dest => dest.Order, src => src.item.Order)
                .Map(dest => dest.DueDate, src => src.item.DueDate)
                .Map(dest => dest.BidItemTypeId, src => src.item.BidItemTypeId)
                .Map(dest => dest.BidItemTypeName, src => src.item.BidItemType != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.item.BidItemType.NameAr : src.item.BidItemType.NameEn)
                    : null)
                .Map(dest => dest.CreatedBy, src => src.item.CreatedBy)
                .Map(dest => dest.CreatedByName, src => src.item.CreatedByNavigation != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.item.CreatedByNavigation.FullnameAr : src.item.CreatedByNavigation.FullnameEn)
                    : null)
                .Map(dest => dest.CreatedDate, src => src.item.CreatedDate);

            config.NewConfig<(CommitteeItemType type, LanguageDbEnum Language), ListItemDto>()
                .Map(dest => dest.Id, src => src.type.Id)
                .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.type.NameAr : src.type.NameEn);
        }
    }
}
