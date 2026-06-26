using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.Entities;
using Core.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineCodesController : ControllerBase
    {
        private readonly IMachineCodeRepository _repository;

        public MachineCodesController(IMachineCodeRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Route("GetList")]
        public async Task<IActionResult> GetAll()
        {
            var codes = await _repository.GetAllAsync();
            var dtos = codes.Select(c => new MachineCodeReadDto(
                c.Id,
                c.MachineCodeName,
                c.ProcessId,
                c.Process?.ProcessName ?? "N/A",
                c.Process?.LineId ?? 0,
                c.Process?.Line?.LineName ?? "N/A"
            ));

            return Ok(dtos);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] MachineCodeCreateDto dto)
        {
            var newCode = new MachineCode
            {
                ProcessId = dto.ProcessId,
                MachineCodeName = dto.MachineCodeName
            };

            await _repository.AddAsync(newCode);
            return Ok(new { message = "Código de máquina creado con éxito." });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MachineCodeUpdateDto dto)
        {
            var existingCode = await _repository.GetByIdAsync(id);
            if (existingCode == null) return NotFound("Código no encontrado.");

            existingCode.ProcessId = dto.ProcessId;
            existingCode.MachineCodeName = dto.MachineCodeName;

            await _repository.UpdateAsync(existingCode);
            return Ok(new { message = "Código actualizado con éxito." });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingCode = await _repository.GetByIdAsync(id);
            if (existingCode == null) return NotFound("Código no encontrado.");

            await _repository.DeleteAsync(existingCode);
            return Ok(new { message = "Código eliminado con éxito." });
        }

    }
}