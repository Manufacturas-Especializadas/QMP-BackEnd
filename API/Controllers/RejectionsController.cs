using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RejectionsController : ControllerBase
    {
        private readonly IRejectionService _service;
        private readonly IAuthRepository _authRepository;
        private readonly IExcelService _excelService;

        public RejectionsController(IRejectionService service, IAuthRepository authRepository, IExcelService excelService)
        {
            _service = service;
            _authRepository = authRepository;
            _excelService = excelService;
        }       

        [HttpGet]
        [Route("GeNextFolio")]
        public async Task<IActionResult> GetNextFolio()
        {
            try
            {
                var nextFolio = await _service.GetNextFolioAsync();

                return Ok(nextFolio);
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    message = "Error al calcular el siguiente folio",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("AvailableMonths")]
        public async Task<IActionResult> GetMonths()
        {
            return Ok(await _service.GetAvailableMonthsAsync());
        }

        [HttpGet]
        [Route("ExportByMonth")]
        public async Task<IActionResult> ExportByMonth([FromQuery] string monthYear)
        {
            var data = await _service.GetByMonthAsync(monthYear);
            var fileBytes = _excelService.GenerateRejectionReport(data);

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Reporte_Rechazos_{monthYear.Replace(" ", "_")}.xlsx"
            );
        }

        [Authorize]
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromForm] CreateRejectionDto dto)
        {
            try
            {
                if (dto.Photos?.Count > 5) return BadRequest("Máximo 5 fotos permitidas");

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var id = await _service.CreateRejectionAsync(dto, userId);

                return Ok(new
                {
                    success = true,
                    rejectionId = id
                });
            }
            catch (DbUpdateException ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Error de Base de Datos: {message}");
            }
        }

        [HttpPut]
        [Route("Edit/{id}")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Edit(int id, [FromForm] EditRejectionDto dto)
        {
            if (id != dto.Id) return BadRequest("El ID no coincide");

            try
            {
                await _service.UpdateRejectionAsync(dto);

                return Ok(new
                {
                    success = true,
                    message = "Registro actualizado correctamente"
                });
            }
            catch(KeyNotFoundException)
            {
                return NotFound("No se encontró el registro");
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteRejectionAsync(id);

                if (!deleted)
                {
                    return NotFound(new { message = "No se encontró el registro de rechazo." });
                }

                return Ok(new
                {
                    success = true,
                    message = "Registro y evidencias visuales eliminados correctamente."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Error al intentar eliminar el rechazo",
                    error = ex.Message
                });
            }
        }
    }
}