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
    }
}