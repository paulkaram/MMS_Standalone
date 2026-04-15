using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Procurement;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    /// <summary>
    /// Procurement (§5.11) — manages Projects, Competitors, and their attachments.
    /// Three committee types (Opening / Examination / Qualification) are driven
    /// by separate WorkflowTemplates in the RBAC engine, seeded globally.
    ///
    /// ERP integration is stubbed: <see cref="LookupErpProjectAsync"/> returns a
    /// deterministic mock response; real integration lives with task #29.
    /// </summary>
    public class ProcurementManager
    {
        private readonly IMMSUnitOfWork _uow;

        public ProcurementManager(IMMSUnitOfWork uow)
        {
            _uow = uow;
        }

        // ─────── Project CRUD ───────

        public async Task<List<ProcurementProjectDto>> ListProjectsAsync(LanguageDbEnum language)
        {
            var projects = (await _uow.ProcurementProjects.ListAllAsync()).ToList();
            return projects.Select(p => MapProject(p, language)).ToList();
        }

        public async Task<ProcurementProjectDto?> GetProjectAsync(int id, LanguageDbEnum language)
        {
            var p = await _uow.ProcurementProjects.GetIncludeAllAsync(id);
            return p == null ? null : MapProject(p, language);
        }

        public async Task<ProcurementProjectDto> CreateProjectAsync(ProcurementProjectPostDto dto, string userId, LanguageDbEnum language)
        {
            if (string.IsNullOrWhiteSpace(dto.ProjectName) || string.IsNullOrWhiteSpace(dto.PurchaseOrderNumber))
                throw new ArgumentException(MessageConstants.ErrorOccured);

            var project = new ProcurementProject
            {
                PurchaseOrderNumber = dto.PurchaseOrderNumber,
                ProjectName = dto.ProjectName,
                ProjectManagerUserId = dto.ProjectManagerUserId,
                EstimatedValue = dto.EstimatedValue,
                AttachmentMode = dto.AttachmentMode,
                MeetingDate = dto.MeetingDate,
                MeetingLocation = dto.MeetingLocation,
                CommitteeId = dto.CommitteeId,
                StatusId = (int)ProcurementProjectStatusDbEnum.Draft,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };
            await _uow.ProcurementProjects.AddAsync(project);
            await _uow.SaveChangesAsync();

            var reloaded = await _uow.ProcurementProjects.GetIncludeAllAsync(project.Id)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            return MapProject(reloaded, language);
        }

        public async Task<ProcurementProjectDto?> UpdateProjectAsync(int id, ProcurementProjectPostDto dto, string userId, LanguageDbEnum language)
        {
            var project = await _uow.ProcurementProjects.GetAsync(p => p.Id == id);
            if (project == null) return null;

            project.ProjectName = dto.ProjectName;
            project.PurchaseOrderNumber = dto.PurchaseOrderNumber;
            project.ProjectManagerUserId = dto.ProjectManagerUserId;
            project.EstimatedValue = dto.EstimatedValue;
            project.AttachmentMode = dto.AttachmentMode;
            project.MeetingDate = dto.MeetingDate;
            project.MeetingLocation = dto.MeetingLocation;
            project.CommitteeId = dto.CommitteeId;
            project.UpdatedBy = userId;
            project.UpdatedDate = DateTime.Now;
            await _uow.SaveChangesAsync();

            var reloaded = await _uow.ProcurementProjects.GetIncludeAllAsync(id);
            return reloaded == null ? null : MapProject(reloaded, language);
        }

        public async Task<bool> DeleteProjectAsync(int id, string userId)
        {
            var project = await _uow.ProcurementProjects.GetAsync(p => p.Id == id);
            if (project == null) return false;

            // Only the creator can delete, and only while still in Draft
            if (project.CreatedBy != userId)
                throw new UnauthorizedAccessException();
            if (project.StatusId != (int)ProcurementProjectStatusDbEnum.Draft)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            _uow.ProcurementProjects.Remove(project);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ─────── Competitor CRUD ───────

        public async Task<List<CompetitorDto>> ListCompetitorsAsync(int projectId, LanguageDbEnum language)
        {
            var list = (await _uow.Competitors.ListByProjectAsync(projectId)).ToList();
            return list.Select(MapCompetitor).ToList();
        }

        public async Task<CompetitorDto> AddCompetitorAsync(int projectId, CompetitorPostDto dto, LanguageDbEnum language)
        {
            var project = await _uow.ProcurementProjects.GetAsync(p => p.Id == projectId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                throw new ArgumentException(MessageConstants.ErrorOccured);

            var competitor = new Competitor
            {
                ProjectId = projectId,
                SapCompanyId = dto.SapCompanyId,
                CompanyName = dto.CompanyName,
                CommercialRegistrationNumber = dto.CommercialRegistrationNumber,
                IsDataComplete = dto.IsDataComplete,
                FinancialValue = dto.FinancialValue,
                HasBankGuarantee = dto.HasBankGuarantee,
                HasSmeLicense = dto.HasSmeLicense,
                CreatedDate = DateTime.Now
            };
            await _uow.Competitors.AddAsync(competitor);
            await _uow.SaveChangesAsync();

            return MapCompetitor(competitor);
        }

        public async Task<CompetitorDto?> UpdateCompetitorAsync(int competitorId, CompetitorPostDto dto, LanguageDbEnum language)
        {
            var c = await _uow.Competitors.GetAsync(x => x.Id == competitorId);
            if (c == null) return null;

            c.SapCompanyId = dto.SapCompanyId;
            c.CompanyName = dto.CompanyName;
            c.CommercialRegistrationNumber = dto.CommercialRegistrationNumber;
            c.IsDataComplete = dto.IsDataComplete;
            c.FinancialValue = dto.FinancialValue;
            c.HasBankGuarantee = dto.HasBankGuarantee;
            c.HasSmeLicense = dto.HasSmeLicense;
            await _uow.SaveChangesAsync();
            return MapCompetitor(c);
        }

        public async Task<bool> RemoveCompetitorAsync(int competitorId)
        {
            var c = await _uow.Competitors.GetAsync(x => x.Id == competitorId);
            if (c == null) return false;
            _uow.Competitors.Remove(c);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ─────── ERP stub ───────

        /// <summary>
        /// Placeholder until the real ERP integration lands (task #29).
        /// Returns mock project details for a purchase-order number so the UI
        /// flow can be developed and tested end-to-end today.
        /// </summary>
        public Task<ErpProjectLookupDto> LookupErpProjectAsync(string poNumber)
        {
            var found = !string.IsNullOrWhiteSpace(poNumber);
            return Task.FromResult(new ErpProjectLookupDto
            {
                PurchaseOrderNumber = poNumber,
                ProjectName = found ? $"[ERP STUB] Project for PO {poNumber}" : string.Empty,
                ProjectManagerUserId = null,
                ProjectManagerName = found ? "Placeholder PM — fill manually" : null,
                EstimatedValue = found ? 100000m : null,
                Found = found,
                Note = "ERP integration pending (task #29). Edit fields as needed."
            });
        }

        // ─────── Mappers ───────

        private static ProcurementProjectDto MapProject(ProcurementProject p, LanguageDbEnum language)
        {
            return new ProcurementProjectDto
            {
                Id = p.Id,
                PurchaseOrderNumber = p.PurchaseOrderNumber,
                ProjectName = p.ProjectName,
                ProjectManagerUserId = p.ProjectManagerUserId,
                ProjectManagerName = p.ProjectManager == null
                    ? null
                    : (language == LanguageDbEnum.Arabic ? p.ProjectManager.FullnameAr : p.ProjectManager.FullnameEn),
                EstimatedValue = p.EstimatedValue,
                AttachmentMode = p.AttachmentMode,
                AttachmentModeName = ((ProcurementAttachmentModeDbEnum)p.AttachmentMode).ToString(),
                MeetingDate = p.MeetingDate,
                MeetingLocation = p.MeetingLocation,
                StatusId = p.StatusId,
                StatusName = ((ProcurementProjectStatusDbEnum)p.StatusId).ToString(),
                CommitteeId = p.CommitteeId,
                CommitteeName = p.Committee == null
                    ? null
                    : (language == LanguageDbEnum.Arabic ? p.Committee.NameAr : p.Committee.NameEn),
                CreatedDate = p.CreatedDate,
                CompetitorsCount = p.Competitors?.Count ?? 0,
                Competitors = (p.Competitors ?? Enumerable.Empty<Competitor>()).Select(MapCompetitor).ToList()
            };
        }

        private static CompetitorDto MapCompetitor(Competitor c)
        {
            return new CompetitorDto
            {
                Id = c.Id,
                ProjectId = c.ProjectId,
                SapCompanyId = c.SapCompanyId,
                CompanyName = c.CompanyName,
                CommercialRegistrationNumber = c.CommercialRegistrationNumber,
                IsDataComplete = c.IsDataComplete,
                FinancialValue = c.FinancialValue,
                HasBankGuarantee = c.HasBankGuarantee,
                HasSmeLicense = c.HasSmeLicense,
                CreatedDate = c.CreatedDate,
                Attachments = (c.Attachments ?? Enumerable.Empty<CompetitorAttachment>())
                    .Where(ca => ca.Attachment != null && !ca.Attachment.Deleted)
                    .Select(ca => new CompetitorAttachmentDto
                    {
                        Id = ca.Id,
                        CompetitorId = ca.CompetitorId,
                        AttachmentId = ca.AttachmentId,
                        Category = ca.Category,
                        CategoryName = ((CompetitorAttachmentCategoryDbEnum)ca.Category).ToString(),
                        FileName = ca.Attachment?.FileName ?? string.Empty,
                        FileSize = (int)(ca.Attachment?.FileSize ?? 0)
                    })
                    .ToList()
            };
        }
    }
}
