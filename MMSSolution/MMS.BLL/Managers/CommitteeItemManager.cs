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

        public CommitteeItemManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
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

            return await GetAsync(item.Id, language)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
        }

        public async Task<CommitteeItemDto?> GetAsync(int itemId, LanguageDbEnum language)
        {
            var item = await _mmsUnitOfWork.CommitteeItems.GetIncludeRelationsAsync(i => i.Id == itemId);
            if (item == null) return null;
            return _mapper.Map<CommitteeItemDto>((item, language));
        }

        public async Task<List<CommitteeItemDto>> ListByCommitteeAsync(int committeeId, LanguageDbEnum language)
        {
            var items = await _mmsUnitOfWork.CommitteeItems.ListIncludeRelationsAsync(i => i.CommitteeId == committeeId);
            return items
                .OrderBy(i => i.Order)
                .ThenBy(i => i.Id)
                .Select(i => _mapper.Map<CommitteeItemDto>((i, language)))
                .ToList();
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
            return await GetAsync(itemId, language);
        }

        public async Task<bool> DeleteAsync(int itemId)
        {
            var item = await _mmsUnitOfWork.CommitteeItems.GetAsync(i => i.Id == itemId);
            if (item == null) return false;

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
            var usage = (await _mmsUnitOfWork.CommitteeItems.ListAsync())
                .GroupBy(i => i.ItemTypeId)
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
