using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScrapController : ControllerBase
    {
        private readonly IScrapRepository _scrapRepository;
        private readonly IExcelService _excelService;


        public ScrapController(IScrapRepository scrapRepository, IExcelService excelService)
        {
            _scrapRepository = scrapRepository;
            _excelService = excelService;
        }

        [HttpGet]
        [Route("ById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var scrap = await _scrapRepository.GetByIdAsync(id);

                if (scrap == null) return NotFound();

                var dto = new ScrapReadDto(
                     scrap.Id,
                     scrap.PayRollNumber,
                     scrap.CreatedAt,
                     scrap.Shift?.ShiftName ?? "N/A",
                     scrap.Line?.LineName ?? "N/A",
                     scrap.Process?.ProcessName ?? "N/A",
                     scrap.MachineCode?.MachineCodeName,
                     scrap.ScrapDetails.Select(d => new ScrapDetailReadDto(
                         d.Id,
                         d.Alloy,
                         d.Diameter,
                         d.Wall,
                         d.RDM,
                         d.Weight,
                         d.Material?.MaterialName ?? "N/A",
                         d.TypeScrap?.TypeScrapName ?? "N/A",
                         d.Defect?.DefectName ?? "N/A",
                         d.IsVerified,
                         d.VerifiedWeight
                     )).ToList()
                 );

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetAllScrap")]
        public async Task<IActionResult> GetAll()
        {
            var scrap = await _scrapRepository.GetAllAsync();

            if(scrap == null) return NotFound();

            return Ok(scrap);
        }

        [HttpGet]
        [Route("ExportExcel")]
        public async Task<IActionResult> ExportExcel([FromQuery] int? month, [FromQuery] int? year)
        {
            try
            {
                var now = DateTime.Now;
                var scrapRecords = await _scrapRepository.GetByMonthAsync(now.Month, now.Year);
                int filterMonth = month ?? DateTime.Now.Month;
                int filterYear = year ?? DateTime.Now.Year;

                var dtos = scrapRecords.SelectMany(s => s.ScrapDetails.Select(d => new ScrapFlatExportDto(
                    s.Id,
                    s.PayRollNumber,
                    d.Alloy,
                    d.Diameter,
                    d.Wall,
                    d.RDM,
                    d.Weight,
                    s.CreatedAt,
                    s.Shift?.ShiftName ?? "N/A",
                    s.Line?.LineName ?? "N/A",
                    s.Process?.ProcessName ?? "N/A",
                    s.MachineCode?.MachineCodeName ?? "N/A",
                    d.TypeScrap?.TypeScrapName ?? "N/A",
                    d.Defect?.DefectName ?? "N/A",
                    d.IsVerified,
                    d.VerifiedWeight,
                    d.Material != null ? d.Material.MaterialName : "N/A"

                ))).ToList();

                var fileContents = _excelService.GenerateScrapReport(dtos);

                string monthName = new DateTime(filterYear, filterMonth, 1).ToString("MMMM", new System.Globalization.CultureInfo("es-ES"));
                return File(fileContents, "application/vnd...", $"Reporte_Scrap_{monthName}_{filterYear}.xlsx");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error al generar Excel: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("UpdateScrap/{id}")]
        public async Task<IActionResult> UpdateScrap(int id, [FromBody] UpdateScrapDto dto)
        {
            try
            {
                if (dto == null || dto.ScrapDetails == null || !dto.ScrapDetails.Any())
                    return BadRequest("Datos inválidos o sin detalles");

                var updatedScrap = new Scrap
                {
                    PayRollNumber = dto.PayRollNumber,
                    ShiftId = dto.ShiftId,
                    ProcessId = dto.ProcessId,
                    LineId = dto.LineId,
                    MachineCodeId = dto.MachineCodeId
                };

                var updatedDetails = dto.ScrapDetails.Select(d => new ScrapDetail
                {
                    Id = d.Id ?? 0,
                    Alloy = d.Alloy,
                    Diameter = d.Diameter,
                    Wall = d.Wall,
                    RDM = d.RDM,
                    Weight = d.Weight,
                    MaterialId = d.MaterialId,
                    TypeScrapId = d.TypeScrapId,
                    DefectId = d.DefectId
                }).ToList();

                var success = await _scrapRepository.UpdateAsync(id, updatedScrap, updatedDetails);

                if (!success) return NotFound(new { message = "No se encontró el Scrap para actualizar." });

                return Ok(new { message = "Scrap actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("CreateScrap")]
        public async Task<IActionResult> Create([FromBody] CreateScrapDto dto)
        {
            try
            {
                if (dto == null || dto.ScrapDetails == null || !dto.ScrapDetails.Any())
                    return BadRequest("Datos de scrap inválidos o sin detalles");

                TimeZoneInfo mexicoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
                DateTime nowInMexico = TimeZoneInfo.ConvertTime(DateTime.UtcNow, mexicoTimeZone);

                var scrap = new Scrap
                {
                    PayRollNumber = dto.PayRollNumber,
                    ShiftId = dto.ShiftId,
                    ProcessId = dto.ProcessId,
                    LineId = dto.LineId,
                    MachineCodeId = dto.MachineCodeId,
                    CreatedAt = nowInMexico,
                    // Mapeo automático de la lista de detalles
                    ScrapDetails = dto.ScrapDetails.Select(d => new ScrapDetail
                    {
                        Alloy = d.Alloy,
                        Diameter = d.Diameter,
                        Wall = d.Wall,
                        Weight = d.Weight,
                        RDM = d.RDM,
                        MaterialId = d.MaterialId,
                        TypeScrapId = d.TypeScrapId,
                        DefectId = d.DefectId
                    }).ToList()
                };

                await _scrapRepository.CreateAsync(scrap);
                await _scrapRepository.SaveChangesAsync();

                return Ok(new { message = "Registro de Scrap y detalles guardados correctamente." });
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpDelete]
        [Route("DeleteScrap/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _scrapRepository.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = $"No se encontró el registro de scrap"
                });
            }

            return Ok(new
            {
                message = "Registro eliminado correctamente"
            });
        }

        [HttpPatch]
        [Route("Verify")]
        public async Task<IActionResult> VerifyScrap([FromBody] VerifyScrapDto dto)
        {
            if (dto == null) return BadRequest("Datos invalidos");

            try
            {
                var result = await _scrapRepository.UpdateVerificationAsync(
                        dto.Id,
                        dto.IsVerified,
                        dto.VerifiedWeight
                    );

                if (!result) return NotFound("No se encontró el detalle de scrap");

                return Ok(new { message = "Verificación actualizada correctamente" });
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}