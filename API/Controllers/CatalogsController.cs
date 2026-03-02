using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogsController : ControllerBase
    {
        private readonly IListRepository _listRepository;

        public CatalogsController(IListRepository listRepository)
        {
            _listRepository = listRepository;
        }

        [HttpGet]
        [Route("lines")]
        public async Task<IActionResult> GetLines()
        {
            var result = await _listRepository.GetLines();

            return result.Any() ? Ok(result) : BadRequest("Sin datos");
        }

        [HttpGet]
        [Route("shifts")]
        public async Task<IActionResult> GetShifts()
        {
            var result = await _listRepository.GetShifts();

            return result.Any() ? Ok(result) : BadRequest("Sin datos");
        }
    }
}