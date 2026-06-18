using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.DTOs;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditsFcdsController : ControllerBase
    {
        private readonly IAuditFcdsRepository _auditFcdsRepository;

        public AuditsFcdsController(IAuditFcdsRepository auditFcdsRepository)
        {
            _auditFcdsRepository = auditFcdsRepository;
        }

        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetListAudits()
        {
            try
            {
                var audits = await _auditFcdsRepository.GetListAuditsAsync();

                return Ok(audits);
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    message = "Error al obtener el listado de auditorías FCD"
                });
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var audit = await _auditFcdsRepository.GetDetailedAuditByIdAsync(id);

            if (audit == null) return NotFound(new
            {
                message = "La auditoria no existe"
            });

            return Ok(audit);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateAudit([FromBody] CreateAuditFcdsDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new
                    {
                        message = "No se pudo determinar la identidad del inspector"
                    });
                }

                var success = await _auditFcdsRepository.CreateAuditAsync(dto, userId);

                if (!success)
                {
                    return BadRequest(new
                    {
                        message = "No se pudo registrar la auditoría FCD"
                    });
                }

                return Ok(new
                {
                    message = "¡Auditoía FCD registrada correctamente!"
                });
            }
            catch (Exception ex)
            {
                var realMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                return BadRequest(new { message = realMessage });
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateAuditFcdsDto dto)
        {
            try
            {
                var updated = await _auditFcdsRepository.UpdateAuditAsync(id, dto);
                if (!updated) return NotFound(new { message = "No se encontró la auditoría a editar." });

                return Ok(new { message = "Auditoría actualizada con éxito." });
            }
            catch (Exception ex)
            {
                var realMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { message = "Error al actualizar la auditoría", details = realMessage });
            }
        }

        [HttpDelete]
        [Route("Delte/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _auditFcdsRepository.DeleteAuditAsync(id);

            if (!deleted) return BadRequest(new
            {
                message = "No se pudo eliminar el registro"
            });

            return Ok(new
            {
                message = "Registro eliminado correctament"
            });
        }
    }
}