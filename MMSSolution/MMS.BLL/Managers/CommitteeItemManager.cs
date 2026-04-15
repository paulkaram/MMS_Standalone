using MapsterMapper;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.CommitteeItems;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class CommitteeItemManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;
        private readonly TagManager _tagManager;

        public CommitteeItemManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork, TagManager tagManager)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
            _tagManager = tagManager;
        }

        public async Task<CommitteeItemDto> CreateAsync(CommitteeItemPostDto dto, string userId, LanguageDbEnum language)
        {
            var item = new CommitteeItem
            {
                CommitteeId = dto.CommitteeId,
                ReferenceNumber = await GenerateReferenceNumberAsync(dto.CommitteeId),
                ExternalReferenceNumber = dto.ExternalReferenceNumber,
                Content = dto.Content,
                ItemTypeId = dto.ItemTypeId,
                Tags = dto.Tags,
                InternalNote = dto.InternalNote,
                RelatedItemId = dto.RelatedItemId,
                IsPrivate = dto.IsPrivate,
                Order = dto.Order,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            await _mmsUnitOfWork.CommitteeItems.AddAsync(item);
            await _mmsUnitOfWork.SaveChangesAsync();

            if (dto.TagIds.Any())
                await _tagManager.SetTagsForEntityAsync(TagEntityTypeDbEnum.CommitteeItem, item.Id, dto.TagIds);

            return await GetAsync(item.Id, language)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
        }

        public async Task<CommitteeItemDto?> GetAsync(int itemId, LanguageDbEnum language)
        {
            var item = await _mmsUnitOfWork.CommitteeItems.GetIncludeRelationsAsync(i => i.Id == itemId);
            if (item == null) return null;

            var dto = _mapper.Map<CommitteeItemDto>((item, language));
            dto.TagList = await _tagManager.ListForEntityAsync(TagEntityTypeDbEnum.CommitteeItem, item.Id, language);
            return dto;
        }

        public async Task<List<CommitteeItemDto>> ListByCommitteeAsync(int committeeId, LanguageDbEnum language)
        {
            var items = (await _mmsUnitOfWork.CommitteeItems.ListIncludeRelationsAsync(i => i.CommitteeId == committeeId))
                .OrderBy(i => i.Order)
                .ThenBy(i => i.Id)
                .ToList();

            // Batch load tag links for all items
            var itemIds = items.Select(i => i.Id).ToList();
            var tagLinks = await _mmsUnitOfWork.TagLinks.ListForEntitiesAsync((int)TagEntityTypeDbEnum.CommitteeItem, itemIds);
            var tagsByEntity = tagLinks
                .GroupBy(tl => tl.EntityId)
                .ToDictionary(g => g.Key, g => g.Select(tl => tl.Tag).ToList());

            var result = new List<CommitteeItemDto>();
            foreach (var item in items)
            {
                var dto = _mapper.Map<CommitteeItemDto>((item, language));
                if (tagsByEntity.TryGetValue(item.Id, out var itemTags))
                {
                    dto.TagList = itemTags.Select(t => _mapper.Map<ListItemDto>((t, language))).ToList();
                }
                result.Add(dto);
            }
            return result;
        }

        public async Task<CommitteeItemDto?> UpdateAsync(int itemId, CommitteeItemPostDto dto, LanguageDbEnum language)
        {
            var item = await _mmsUnitOfWork.CommitteeItems.GetAsync(i => i.Id == itemId);
            if (item == null) return null;

            item.ExternalReferenceNumber = dto.ExternalReferenceNumber;
            item.Content = dto.Content;
            item.ItemTypeId = dto.ItemTypeId;
            item.Tags = dto.Tags;
            item.InternalNote = dto.InternalNote;
            item.RelatedItemId = dto.RelatedItemId;
            item.IsPrivate = dto.IsPrivate;
            item.Order = dto.Order;

            await _mmsUnitOfWork.SaveChangesAsync();

            await _tagManager.SetTagsForEntityAsync(TagEntityTypeDbEnum.CommitteeItem, itemId, dto.TagIds);

            return await GetAsync(itemId, language);
        }

        public async Task<bool> DeleteAsync(int itemId)
        {
            var item = await _mmsUnitOfWork.CommitteeItems.GetAsync(i => i.Id == itemId);
            if (item == null) return false;

            // Remove any tag links first
            await _mmsUnitOfWork.TagLinks.RemoveAllForEntityAsync((int)TagEntityTypeDbEnum.CommitteeItem, itemId);

            _mmsUnitOfWork.CommitteeItems.Remove(item);
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<ListItemDto>> ListItemTypesAsync(LanguageDbEnum language)
        {
            var types = await _mmsUnitOfWork.CommitteeItemTypes.ListAsync();
            return types.Select(t => _mapper.Map<ListItemDto>((t, language))).ToList();
        }

        public async Task<List<CommitteeItemTypeDto>> ListItemTypesAdminAsync()
        {
            var types = await _mmsUnitOfWork.CommitteeItemTypes.ListAsync();
            // Bid items have `ItemTypeId = null`; filter them out of the usage count
            // before building the dictionary — otherwise the null key throws.
            var usage = (await _mmsUnitOfWork.CommitteeItems.ListAsync())
                .Where(i => i.ItemTypeId.HasValue)
                .GroupBy(i => i.ItemTypeId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            return types
                .OrderBy(t => t.NameEn)
                .Select(t => new CommitteeItemTypeDto
                {
                    Id = t.Id,
                    NameAr = t.NameAr,
                    NameEn = t.NameEn,
                    UsageCount = usage.TryGetValue(t.Id, out var c) ? c : 0
                })
                .ToList();
        }

        public async Task<CommitteeItemTypeDto> CreateItemTypeAsync(CommitteeItemTypePostDto dto)
        {
            var type = new CommitteeItemType
            {
                NameAr = dto.NameAr,
                NameEn = dto.NameEn
            };
            await _mmsUnitOfWork.CommitteeItemTypes.AddAsync(type);
            await _mmsUnitOfWork.SaveChangesAsync();

            return new CommitteeItemTypeDto
            {
                Id = type.Id,
                NameAr = type.NameAr,
                NameEn = type.NameEn,
                UsageCount = 0
            };
        }

        public async Task<CommitteeItemTypeDto?> UpdateItemTypeAsync(int id, CommitteeItemTypePostDto dto)
        {
            var type = await _mmsUnitOfWork.CommitteeItemTypes.GetAsync(t => t.Id == id);
            if (type == null) return null;

            type.NameAr = dto.NameAr;
            type.NameEn = dto.NameEn;
            await _mmsUnitOfWork.SaveChangesAsync();

            var usageCount = await _mmsUnitOfWork.CommitteeItems.CountAsync(i => i.ItemTypeId == id);
            return new CommitteeItemTypeDto
            {
                Id = type.Id,
                NameAr = type.NameAr,
                NameEn = type.NameEn,
                UsageCount = usageCount
            };
        }

        public async Task<bool> DeleteItemTypeAsync(int id)
        {
            var type = await _mmsUnitOfWork.CommitteeItemTypes.GetAsync(t => t.Id == id);
            if (type == null) return false;

            // Prevent delete if used by any item
            var inUse = await _mmsUnitOfWork.CommitteeItems.AnyAsync(i => i.ItemTypeId == id);
            if (inUse) return false;

            _mmsUnitOfWork.CommitteeItemTypes.Remove(type);
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerateReferenceNumberAsync(int committeeId)
        {
            int year = DateTime.Now.Year;
            int sequence = await _mmsUnitOfWork.CommitteeItems.GetNextSequenceAsync(committeeId, year);
            return $"{FormattingConstants.CommitteeItemReferenceNumberPrefix}-{year}-{committeeId}-{sequence}";
        }
    }
}
