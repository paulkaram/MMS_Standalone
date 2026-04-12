using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.BLL.Managers;
using MMS.DTO.EmailTemplates;

namespace MMS.API.Controllers
{
    [Route("api/email-templates")]
    public class EmailTemplatesController : IntalioBaseController
    {
        private readonly EmailTemplatesManager _manager;

        public EmailTemplatesController(EmailTemplatesManager manager)
        {
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var templates = await _manager.ListEmailTemplatesAsync();
            return Ok(templates);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var template = await _manager.GetByIdAsync(id);
            if (template == null) return NotFound();
            return Ok(template);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmailTemplateDto dto)
        {
            var result = await _manager.UpdateAsync(id, dto);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }
    }
}
