using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
        [Route("clients")]
        public async Task<IActionResult> GetClients()
        {
            var result = await _listRepository.GetClients();

            return result.Any() ? Ok(result) : BadRequest("Sin datos");
        }

        [HttpGet]
        [Route("shifts")]
        public async Task<IActionResult> GetShifts()
        {
            var result = await _listRepository.GetShifts();

            return result.Any() ? Ok(result) : BadRequest("Sin datos");
        }

        [HttpGet]
        [Route("material")]
        public async Task<IActionResult> GetMaterial()
        {
            var result = await _listRepository.GetMaterial();

            return result.Any() ? Ok(result) : BadRequest("Sin datos");
        }

        [HttpGet]
        [Route("defects")]
        public async Task<IActionResult> GetDefects()
        {
            var result = await _listRepository.GetDefects();

            return result.Any() ? Ok(result) : BadRequest("Sin datos");
        }

        [HttpGet]
        [Route("containmentActions")]
        public async Task<IActionResult> GetContainmentAction()
        {
            var result = await _listRepository.GetContainmentActions();

            return result.Any() ? Ok(result) : BadRequest("Sin datos");            
        }

        [HttpGet]
        [Route("typeScrap")]
        public async Task<IActionResult> GetTypeScrap()
        {
            var result = await _listRepository.GetTypeScrap();

            return result.Any() ? Ok(result) : BadRequest("Sin datos");
        }

        [HttpGet]
        [Route("Scrap")]
        public async Task<IActionResult> GetScrap()
        {
            var result = await _listRepository.GetScrap();

            return Ok(result);
        }

        [HttpGet]
        [Route("process/{lineId}")]
        public async Task<IActionResult> GetProcessByLine(int lineId)
        {
            var result = await _listRepository.GetProcess(lineId);

            return result.Any() ? Ok(result) : NotFound("Sin procesos para esta linea");
        }

        [HttpGet]
        [Route("machineCodes/{processId}")]
        public async Task<IActionResult> getMachineCodes(int processId)
        {
            var result = await _listRepository.GetMachineCodes(processId);

            return Ok(result);
        }

        [HttpGet]
        [Route("defects/{typeScrapId}")]
        public async Task<IActionResult> GetDefectsByTypeScrap(int typeScrapId)
        {
            var result = await _listRepository.GetDefects(typeScrapId);

            return Ok(result);
        }
    }
}