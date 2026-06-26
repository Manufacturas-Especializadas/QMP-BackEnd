using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.Entities;
using Core.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessesController : ControllerBase
    {
        private readonly IProcessRepository _repository;

        public ProcessesController(IProcessRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Route("GetList")]
        public async Task<IActionResult> GetAll()
        {
            var processes = await _repository.GetAllAsync();
            var dtos = processes.Select(p => new ProcessReadDto(
                p.Id,
                p.ProcessName,
                p.LineId,
                p.Line?.LineName ?? "N/A"
            ));

            return Ok(dtos);
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var process = await _repository.GetByIdAsync(id);
            if (process == null) return NotFound(new { message = "Proceso no encontrado." });

            var dto = new ProcessReadDto(
                process.Id,
                process.ProcessName,
                process.LineId,
                process.Line?.LineName ?? "N/A"
            );

            return Ok(dto);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] ProcessCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ProcessName))
                return BadRequest(new { message = "El nombre del proceso es obligatorio." });

            var newProcess = new Process
            {
                LineId = dto.LineId,
                ProcessName = dto.ProcessName.Trim().ToUpper()
            };

            await _repository.AddAsync(newProcess);
            return Ok(new { message = "Proceso creado con éxito." });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProcessUpdateDto dto)
        {
            var existingProcess = await _repository.GetByIdAsync(id);
            if (existingProcess == null) return NotFound(new { message = "Proceso no encontrado." });

            if (string.IsNullOrWhiteSpace(dto.ProcessName))
                return BadRequest(new { message = "El nombre del proceso es obligatorio." });

            existingProcess.LineId = dto.LineId;
            existingProcess.ProcessName = dto.ProcessName.Trim().ToUpper();

            await _repository.UpdateAsync(existingProcess);
            return Ok(new { message = "Proceso actualizado con éxito." });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingProcess = await _repository.GetByIdAsync(id);
            if (existingProcess == null) return NotFound(new { message = "Proceso no encontrado." });

            await _repository.DeleteAsync(existingProcess);
            return Ok(new { message = "Proceso eliminado con éxito." });
        }

    }
}