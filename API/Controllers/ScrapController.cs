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

        public ScrapController(IScrapRepository scrapRepository)
        {
            _scrapRepository = scrapRepository;
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
                    scrap.Alloy,
                    scrap.Diameter,
                    scrap.Wall,
                    scrap.RDM,
                    scrap.Weight,
                    scrap.CreatedAt,
                    scrap.Shift?.ShiftName ?? "N/A",
                    scrap.Line?.LineName ?? "N/A",
                    scrap.Process?.ProcessName ?? "N/A",
                    scrap.MachineCode?.MachineCodeName,
                    scrap.TypeScrap?.TypeScrapName ?? "N/A",
                    scrap.Defect?.DefectName ?? "N/A",
                    scrap.IsVerified,
                    scrap.VerifiedWeight
                );

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("CreateScrap")]
        public async Task<IActionResult> Create([FromBody] CreateScrapDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("Datos de scrap inválidos");

                var scrap = new Scrap
                {
                    PayRollNumber = dto.PayRollNumber,
                    Alloy = dto.Alloy,
                    Diameter = dto.Diameter,
                    Wall = dto.Wall,
                    Weight = dto.Weight,
                    RDM = dto.RDM,
                    ShiftId = dto.ShiftId,
                    ProcessId = dto.ProcessId,
                    LineId = dto.LineId,
                    MachineCodeId = dto.MachineCodeId,
                    MaterialId = dto.MaterialId,
                    TypeScrapId = dto.TypeScrapId,
                    DefectId = dto.DefectId,
                };

                await _scrapRepository.CreateAsync(scrap);

                var result = await _scrapRepository.SaveChangesAsync();

                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message} - {ex.InnerException?.Message}");
            }
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

                if (!result) return NotFound("No se encontró el registro de scrap");

                return Ok(new { message = "Verificación actualizada correctamente" });
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}