using MapsterMapper;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DTO.EmailTemplates;

namespace MMS.BLL.Managers
{
    public class EmailTemplatesManager
    {
        private readonly IMapper _mapper;
        private readonly IProcessUnitOfWork _processUnitOfWork;

        public EmailTemplatesManager(IMapper mapper, IProcessUnitOfWork processUnitOfWork)
        {
            _processUnitOfWork = processUnitOfWork;
            _mapper = mapper;
        }

        public async Task<List<EmailTemplateListItemDto>> ListEmailTemplatesAsync()
        {
            var emailTemplates = await _processUnitOfWork.EmailTemplates.ListAsync();
            return emailTemplates.Select(x => _mapper.Map<EmailTemplateListItemDto>(x)).ToList();
        }

        public async Task<EmailTemplateListItemDto?> GetByIdAsync(int id)
        {
            var template = await _processUnitOfWork.EmailTemplates.Find(id);
            return template == null ? null : _mapper.Map<EmailTemplateListItemDto>(template);
        }

        public async Task<bool> UpdateAsync(int id, UpdateEmailTemplateDto dto)
        {
            var template = await _processUnitOfWork.EmailTemplates.Find(id);
            if (template == null) return false;

            template.Subject = dto.Subject;
            template.Body = dto.Body;
            template.SendTo = dto.SendTo;

            _processUnitOfWork.EmailTemplates.Update(template);
            await _processUnitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
