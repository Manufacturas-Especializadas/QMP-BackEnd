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
        private readonly IExcelService _excelService;

        public AuditsFcdsController(IAuditFcdsRepository auditFcdsRepository, IExcelService excelService)
        {
            _auditFcdsRepository = auditFcdsRepository;
            _excelService = excelService;
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

        [HttpGet]
        [Route("AvailableMonths")]
        public async Task<IActionResult> GetAvailableMonths()
        {
            try
            {
                var months = await _auditFcdsRepository.GetAvailableMonthsAsync();
                return Ok(months);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los meses disponibles para reportes.", details = ex.Message });
            }
        }

        [HttpGet]
        [Route("DownloadExcelReport")]
        public async Task<IActionResult> DownloadExcelReport([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var data = await _auditFcdsRepository.GetAuditsByMonthAsync(year, month);

                if (data == null || !data.Any())
                {
                    return BadRequest(new { message = "No existen registros de auditorías en el mes seleccionado para generar el reporte." });
                }

                byte[] excelBytes = _excelService.GenerateAuditsFcdsReport(data);

                string fileName = $"Reporte_Auditorias_FCD_{year}_{month:D2}.xlsx";
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                var realMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = "Error interno al procesar el reporte de Excel.", details = realMessage });
            }
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