using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinesController : ControllerBase
    {
        private readonly ILinesRepository _linesRepository;

        public LinesController(ILinesRepository linesRepository)
        {
            _linesRepository = linesRepository;
        }

        [HttpGet]
        [Route("LineById/{id}")]
        public async Task<IActionResult> GetLineById(int id)
        {
            var result = await _linesRepository.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound("Línea no encontrada");
            }

            var dto = new LineReadDto(result.Id, result.LineName);

            return Ok(dto);
        }

        [HttpPost]
        [Route("CreateLine")]
        public async Task<IActionResult> Create([FromBody] LineCreateDto dto)
        {
            if (dto == null) return BadRequest("Datos de linea inválidos");

            var line = new Line
            {
                LineName = dto.LineName,
            };

            await _linesRepository.CreateAsync(line);

            var result = await _linesRepository.SaveChangesAsync();

            return Ok(result);
        }

        [HttpPut]
        [Route("UpdateLine/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LineCreateDto dto)
        {
            var lineEntity = new Line { LineName = dto.LineName };

            var updateLine = await _linesRepository.UpdateAsync(id, lineEntity);

            if (updateLine == null) return NotFound("La línea no existe");

            var result = await _linesRepository.SaveChangesAsync();

            return Ok(result);
        }

        [HttpDelete]
        [Route("DeleteLine/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _linesRepository.DeleteAsync(id);

            if (!success) return NotFound("La línea que intentas eliminar no exista");

            await _linesRepository.SaveChangesAsync();

            return Ok(true);
        }
    }
}