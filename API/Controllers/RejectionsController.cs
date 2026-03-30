using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RejectionsController : ControllerBase
    {
        private readonly IRejectionService _service;

        public RejectionsController(IRejectionService service)
        {
            _service = service;
        }

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
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut]
        [Route("Edit/{id}")]
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
    }
}