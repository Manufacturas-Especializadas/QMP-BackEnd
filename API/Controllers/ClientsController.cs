using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsRepository _clientsRepository;

        public ClientsController(IClientsRepository clientsRepository)
        {
            _clientsRepository = clientsRepository;
        }

        [HttpGet]
        [Route("ClientById/{id}")]
        public async Task<IActionResult> GetLineById(int id)
        {
            var result = await _clientsRepository.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound("Cliente no encontrado");
            }

            var dto = new ClineReadDto(result.Id, result.ClientName);

            return Ok(dto);
        }

        [HttpPost]
        [Route("CreateClient")]
        public async Task<IActionResult> Create([FromBody] ClientCreateDto dto)
        {
            if (dto == null) return BadRequest("Datos de cliente inválidos");

            var client = new Client
            {
                ClientName = dto.ClientName,
            };

            await _clientsRepository.CreateAsync(client);

            var result = await _clientsRepository.SaveChangesAsync();

            return Ok(result);
        }

        [HttpPut]
        [Route("UpdateClient/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClientCreateDto dto)
        {
            var clientEntity = new Client { ClientName = dto.ClientName};

            var updateClient = await _clientsRepository.UpdateAsync(id, clientEntity);

            if (updateClient == null) return NotFound("El cliente no existe");

            var result = await _clientsRepository.SaveChangesAsync();

            return Ok(result);
        }

        [HttpDelete]
        [Route("DeleteClient/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _clientsRepository.DeleteAsync(id);

            if (!success) return NotFound("El cliente que intentas eliminar no existe");

            await _clientsRepository.SaveChangesAsync();

            return Ok(true);
        }
    }
}