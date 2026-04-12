using System.Text.Json;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Settings;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class MomTemplateManager
    {
        private readonly ISettingsUnitOfWork _settingsUnitOfWork;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public MomTemplateManager(ISettingsUnitOfWork settingsUnitOfWork)
        {
            _settingsUnitOfWork = settingsUnitOfWork;
        }

        public async Task<List<MomTemplateListItemDto>> ListAsync(int? branchId = null)
        {
            var templates = branchId.HasValue
                ? await _settingsUnitOfWork.MomTemplates.ListAsync(t => t.BranchId == branchId || t.BranchId == null)
                : await _settingsUnitOfWork.MomTemplates.ListAsync();

            var branches = await _settingsUnitOfWork.Branches.ListAsync();
            var branchDict = branches.ToDictionary(b => b.Id, b => b);

            return templates.Select(t => new MomTemplateListItemDto
            {
                Id = t.Id,
                BranchId = t.BranchId,
                BranchNameAr = t.BranchId.HasValue && branchDict.ContainsKey(t.BranchId.Value) ? branchDict[t.BranchId.Value].NameAr : null,
                BranchNameEn = t.BranchId.HasValue && branchDict.ContainsKey(t.BranchId.Value) ? branchDict[t.BranchId.Value].NameEn : null,
                TemplateType = t.TemplateType,
                TemplateTypeName = GetTemplateTypeName(t.TemplateType),
                NameAr = t.NameAr,
                NameEn = t.NameEn,
                IsActive = t.IsActive,
                IsDefault = t.IsDefault,
                CreatedDate = t.CreatedDate
            }).OrderByDescending(t => t.CreatedDate).ToList();
        }

        public async Task<MomTemplateDto?> GetByIdAsync(int id)
        {
            var template = await _settingsUnitOfWork.MomTemplates.GetAsync(t => t.Id == id);
            if (template == null) return null;

            var branch = template.BranchId.HasValue
                ? await _settingsUnitOfWork.Branches.GetAsync(b => b.Id == template.BranchId)
                : null;

            return MapToDto(template, branch);
        }

        public async Task<MomTemplateDto?> GetDefaultTemplateAsync(int? branchId, MomTemplateTypeDbEnum templateType)
        {
            var typeId = (int)templateType;

            // First try to get branch-specific default template
            MomTemplate? template = null;
            if (branchId.HasValue)
            {
                template = await _settingsUnitOfWork.MomTemplates.GetAsync(
                    t => t.BranchId == branchId && t.TemplateType == typeId && t.IsDefault && t.IsActive);
            }

            // If no branch-specific default, get system default (BranchId = null)
            if (template == null)
            {
                template = await _settingsUnitOfWork.MomTemplates.GetAsync(
                    t => t.BranchId == null && t.TemplateType == typeId && t.IsDefault && t.IsActive);
            }

            // If still no default, get any active template of this type
            if (template == null)
            {
                template = await _settingsUnitOfWork.MomTemplates.GetAsync(
                    t => t.TemplateType == typeId && t.IsActive);
            }

            if (template == null) return null;

            var branch = template.BranchId.HasValue
                ? await _settingsUnitOfWork.Branches.GetAsync(b => b.Id == template.BranchId)
                : null;

            return MapToDto(template, branch);
        }

        public async Task<MomTemplateConfigDto?> GetDefaultTemplateConfigAsync(int? branchId, MomTemplateTypeDbEnum templateType)
        {
            var template = await GetDefaultTemplateAsync(branchId, templateType);
            return template?.Config;
        }

        public async Task<int> CreateAsync(MomTemplateCreateDto dto, string userId)
        {
            // If setting as default, unset other defaults for same branch and type
            if (dto.IsDefault)
            {
                await UnsetDefaultsAsync(dto.BranchId, dto.TemplateType);
            }

            var template = new MomTemplate
            {
                BranchId = dto.BranchId,
                TemplateType = dto.TemplateType,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                ConfigJson = JsonSerializer.Serialize(dto.Config, JsonOptions),
                HtmlTemplate = dto.HtmlTemplate,
                IsActive = dto.IsActive,
                IsDefault = dto.IsDefault,
                CreatedDate = DateTime.Now,
                CreatedBy = userId
            };

            await _settingsUnitOfWork.MomTemplates.AddAsync(template);
            await _settingsUnitOfWork.SaveChangesAsync();

            return template.Id;
        }

        public async Task<bool> UpdateAsync(MomTemplateUpdateDto dto, string userId)
        {
            var template = await _settingsUnitOfWork.MomTemplates.GetAsync(t => t.Id == dto.Id);
            if (template == null) return false;

            // Cannot modify system templates (BranchId = null) unless it's the only default
            // if (template.BranchId == null) return false;

            // If setting as default, unset other defaults for same branch and type
            if (dto.IsDefault && !template.IsDefault)
            {
                await UnsetDefaultsAsync(dto.BranchId, dto.TemplateType);
            }

            template.BranchId = dto.BranchId;
            template.TemplateType = dto.TemplateType;
            template.NameAr = dto.NameAr;
            template.NameEn = dto.NameEn;
            template.ConfigJson = JsonSerializer.Serialize(dto.Config, JsonOptions);
            template.HtmlTemplate = dto.HtmlTemplate;
            template.IsActive = dto.IsActive;
            template.IsDefault = dto.IsDefault;
            template.ModifiedDate = DateTime.Now;
            template.ModifiedBy = userId;

            _settingsUnitOfWork.MomTemplates.Update(template);
            return await _settingsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var template = await _settingsUnitOfWork.MomTemplates.GetAsync(t => t.Id == id);
            if (template == null) return false;

            // Cannot delete system templates (BranchId = null)
            if (template.BranchId == null) return false;

            _settingsUnitOfWork.MomTemplates.Remove(template);
            return await _settingsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<List<MomTemplateTypeDto>> GetTemplateTypesAsync()
        {
            return await Task.FromResult(new List<MomTemplateTypeDto>
            {
                new() { Id = (int)MomTemplateTypeDbEnum.Initial, NameAr = "المحضر المبدئي", NameEn = "Initial Minutes" },
                new() { Id = (int)MomTemplateTypeDbEnum.Final, NameAr = "المحضر النهائي", NameEn = "Final Minutes" }
            });
        }

        public async Task<List<BranchListItemDto>> GetBranchesAsync()
        {
            var branches = await _settingsUnitOfWork.Branches.ListAsync();
            return branches.Select(b => new BranchListItemDto(b.Id, b.NameAr, b.NameEn)).ToList();
        }

        private async Task UnsetDefaultsAsync(int? branchId, int templateType)
        {
            var existingDefaults = await _settingsUnitOfWork.MomTemplates.ListWithTrackAsync(
                t => t.BranchId == branchId && t.TemplateType == templateType && t.IsDefault);

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }
        }

        private MomTemplateDto MapToDto(MomTemplate template, Branch? branch)
        {
            MomTemplateConfigDto config;
            try
            {
                config = JsonSerializer.Deserialize<MomTemplateConfigDto>(template.ConfigJson, JsonOptions) ?? new MomTemplateConfigDto();
            }
            catch
            {
                config = new MomTemplateConfigDto();
            }

            return new MomTemplateDto
            {
                Id = template.Id,
                BranchId = template.BranchId,
                BranchNameAr = branch?.NameAr,
                BranchNameEn = branch?.NameEn,
                TemplateType = template.TemplateType,
                TemplateTypeName = GetTemplateTypeName(template.TemplateType),
                NameAr = template.NameAr,
                NameEn = template.NameEn,
                Config = config,
                HtmlTemplate = template.HtmlTemplate,
                IsActive = template.IsActive,
                IsDefault = template.IsDefault,
                CreatedDate = template.CreatedDate,
                CreatedBy = template.CreatedBy,
                ModifiedDate = template.ModifiedDate,
                ModifiedBy = template.ModifiedBy
            };
        }

        private static string GetTemplateTypeName(int templateType)
        {
            return templateType switch
            {
                1 => "Initial",
                2 => "Final",
                _ => "Unknown"
            };
        }
    }

    public record BranchListItemDto(int Id, string NameAr, string NameEn);
}
