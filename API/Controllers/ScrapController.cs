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
                     scrap.InspectorPayRollNumber,
                     scrap.CreatedAt,
                     scrap.ShiftId,
     scrap.Shift?.ShiftName ?? "N/A",
     scrap.LineId,
     scrap.Line?.LineName ?? "N/A",
                     scrap.IsVerified,
                     scrap.VerifiedWeight,
                     scrap.ScrapDetails.Select(d => new ScrapDetailReadDto(
                         d.Id,
    d.PayRollNumber,
    d.ProcessId,
    d.Process?.ProcessName ?? "N/A",
    d.MachineCodeId,
    d.MachineCode?.MachineCodeName ?? "N/A",
    d.Alloy,
    d.Diameter,
    d.Wall,
    d.RDM,
    d.Weight,
    d.MaterialId,
    d.Material?.MaterialName ?? "N/A",
    d.TypeScrapId,
    d.TypeScrap?.TypeScrapName ?? "N/A",
    d.DefectId,
    d.Defect?.DefectName ?? "N/A",
    d.PartNumber
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

            if (scrap == null) return NotFound();

            return Ok(scrap);
        }

        [HttpGet]
        [Route("ExportExcel")]
        public async Task<IActionResult> ExportExcel([FromQuery] int? month, [FromQuery] int? year)
        {
            try
            {
                int filterMonth = month ?? DateTime.Now.Month;
                int filterYear = year ?? DateTime.Now.Year;

                var scrapRecords = await _scrapRepository.GetByMonthAsync(filterMonth, filterYear);


                var dtos = scrapRecords.SelectMany(s => s.ScrapDetails.Select(d => new ScrapFlatExportDto(
                    ScrapId: s.Id,
                    DetailId: d.Id,
                    InspectorPayRollNumber: s.InspectorPayRollNumber,
                    PayRollNumber: d.PayRollNumber,
                    Alloy: d.Alloy,
                    Diameter: d.Diameter,
                    Wall: d.Wall,
                    RDM: d.RDM,
                    Weight: d.Weight,
                    CreatedAt: s.CreatedAt,
                    ShiftName: s.Shift?.ShiftName ?? "N/A",
                    LineName: s.Line?.LineName ?? "N/A",
                    ProcessName: d.Process?.ProcessName ?? "N/A",
                    MachineCodeName: d.MachineCode?.MachineCodeName ?? "N/A",
                    TypeScrapName: d.TypeScrap?.TypeScrapName ?? "N/A",
                    DefectName: d.Defect?.DefectName ?? "N/A",
                    IsVerified: s.IsVerified,
                    VerifiedWeight: s.VerifiedWeight,
                    Material: d.Material?.MaterialName ?? "N/A",
                    TotalWeight: s.TotalWeight,
                    PartNumber: d.PartNumber
                ))).ToList();

                var fileContents = _excelService.GenerateScrapReport(dtos);

                string monthName = new DateTime(filterYear, filterMonth, 1).ToString("MMMM", new System.Globalization.CultureInfo("es-ES"));
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Reporte_Scrap_{monthName}_{filterYear}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar Excel: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("UpdateScrap/{id}")]
        public async Task<IActionResult> UpdateScrap(int id, [FromBody] List<ScrapDetailUpdateDto> updatedDetails)
        {
            if (updatedDetails == null || !updatedDetails.Any())
                return BadRequest("Debes enviar al menos un detalle de scrap.");

            try
            {
                var result = await _scrapRepository.UpdateDetailsOnlyAsync(id, updatedDetails);

                if (!result) return NotFound($"No se encontró el reporte de Scrap con ID {id}");

                return Ok(new { message = "Detalles actualizados correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar: {ex.Message}");
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
                    InspectorPayRollNumber = dto.InspectorPayRollNumber,
                    ShiftId = dto.ShiftId,
                    LineId = dto.LineId,
                    CreatedAt = nowInMexico,
                    ScrapDetails = dto.ScrapDetails.Select(d => new ScrapDetail
                    {
                        PayRollNumber = d.PayRollNumber,
                        ProcessId = d.ProcessId,
                        MachineCodeId = d.MachineCodeId,
                        Alloy = d.Alloy,
                        Diameter = d.Diameter,
                        Wall = d.Wall,
                        Weight = d.Weight,
                        RDM = d.RDM,
                        MaterialId = d.MaterialId,
                        TypeScrapId = d.TypeScrapId,
                        DefectId = d.DefectId,
                        PartNumber = d.PartNumber
                    }).ToList()
                };

                await _scrapRepository.CreateAsync(scrap);
                await _scrapRepository.SaveChangesAsync();

                return Ok(new { message = "Registro de Scrap y detalles guardados correctamente." });
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}