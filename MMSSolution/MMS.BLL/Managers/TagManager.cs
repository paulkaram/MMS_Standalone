using MapsterMapper;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Tags;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class TagManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;

        public TagManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
        }

        public async Task<List<ListItemDto>> ListAsync(LanguageDbEnum language)
        {
            var tags = await _mmsUnitOfWork.Tags.ListAsync();
            return tags
                .OrderBy(t => language == LanguageDbEnum.Arabic ? t.NameAr : t.NameEn)
                .Select(t => _mapper.Map<ListItemDto>((t, language)))
                .ToList();
        }

        /// <summary>
        /// Picker-friendly tag list — same shape as ListAsync but adds the color
        /// so the picker UI can render colored dots without needing the admin
        /// endpoint (which is permission-gated).
        /// </summary>
        public async Task<List<TagPickerDto>> ListForPickerAsync(LanguageDbEnum language)
        {
            var tags = await _mmsUnitOfWork.Tags.ListAsync();
            return tags
                .OrderBy(t => language == LanguageDbEnum.Arabic ? t.NameAr : t.NameEn)
                .Select(t => new TagPickerDto
                {
                    Id = t.Id,
                    Name = language == LanguageDbEnum.Arabic ? t.NameAr : t.NameEn,
                    Color = t.Color
                })
                .ToList();
        }

        public async Task<List<TagDto>> ListAdminAsync()
        {
            var tags = await _mmsUnitOfWork.Tags.ListAsync();
            var usage = (await _mmsUnitOfWork.TagLinks.ListAsync())
                .GroupBy(l => l.TagId)
                .ToDictionary(g => g.Key, g => g.Count());

            return tags
                .OrderBy(t => t.NameEn)
                .Select(t =>
                {
                    int count = usage.TryGetValue(t.Id, out var c) ? c : 0;
                    return _mapper.Map<TagDto>((t, count));
                })
                .ToList();
        }

        public async Task<TagDto> CreateAsync(TagPostDto dto)
        {
            var existing = await _mmsUnitOfWork.Tags.GetByNameAsync(dto.NameEn);
            if (existing != null)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var tag = new Tag
            {
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Color = string.IsNullOrWhiteSpace(dto.Color) ? FormattingConstants.DefaultTagColor : dto.Color!,
                CreatedDate = DateTime.Now
            };

            await _mmsUnitOfWork.Tags.AddAsync(tag);
            await _mmsUnitOfWork.SaveChangesAsync();

            return _mapper.Map<TagDto>((tag, 0));
        }

        public async Task<TagDto?> UpdateAsync(int id, TagPostDto dto)
        {
            var tag = await _mmsUnitOfWork.Tags.GetAsync(t => t.Id == id);
            if (tag == null) return null;

            tag.NameAr = dto.NameAr;
            tag.NameEn = dto.NameEn;
            if (!string.IsNullOrWhiteSpace(dto.Color)) tag.Color = dto.Color!;

            await _mmsUnitOfWork.SaveChangesAsync();

            int usage = await _mmsUnitOfWork.TagLinks.CountAsync(l => l.TagId == id);
            return _mapper.Map<TagDto>((tag, usage));
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tag = await _mmsUnitOfWork.Tags.GetAsync(t => t.Id == id);
            if (tag == null) return false;

            _mmsUnitOfWork.Tags.Remove(tag);
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<ListItemDto>> ListForEntityAsync(TagEntityTypeDbEnum entityType, int entityId, LanguageDbEnum language)
        {
            var tags = await _mmsUnitOfWork.TagLinks.ListTagsForEntityAsync((int)entityType, entityId);
            return tags.Select(t => _mapper.Map<ListItemDto>((t, language))).ToList();
        }

        public async Task SetTagsForEntityAsync(TagEntityTypeDbEnum entityType, int entityId, IEnumerable<int> tagIds)
        {
            await _mmsUnitOfWork.TagLinks.RemoveAllForEntityAsync((int)entityType, entityId);
            foreach (var tagId in tagIds.Distinct())
            {
                await _mmsUnitOfWork.TagLinks.AddAsync(new TagLink
                {
                    TagId = tagId,
                    EntityTypeId = (int)entityType,
                    EntityId = entityId,
                    CreatedDate = DateTime.Now
                });
            }
            await _mmsUnitOfWork.SaveChangesAsync();
        }
    }
}
