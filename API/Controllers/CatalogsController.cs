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
        [Route("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _listRepository.GetRoles();

            return result.Any() ? Ok(result) : BadRequest("Sin datos");
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
        [Route("Rejections")]
        public async Task<IActionResult> GetRejections()
        {
            var result = await _listRepository.GetRejections();


            return Ok(result);
        }

        [HttpGet]
        [Route("categorys")]
        public async Task<IActionResult> GetCategoryOpertars()
        {
            var result = await _listRepository.GetCategoryOperators();

            return Ok(result);
        }

        [HttpGet]
        [Route("equipments")]
        public async Task<IActionResult> GetTypeMeasuringEquipment()
        {
            var result = await _listRepository.GetTypeMeasuringEquipment();

            return Ok(result);
        }

        [HttpGet]
        [Route("pipeDiameters")]
        public async Task<IActionResult> GetPipeDiameters()
        {
            var result = await _listRepository.GetPipeDiameters();

            return Ok(result);
        }

        [HttpGet]
        [Route("wallsOfDiameters")]
        public async Task<IActionResult> GetWallsOfDiameters()
        {
            var result = await _listRepository.GetWallsOfDiameters();

            return Ok(result);
        }

        [HttpGet]
        [Route("conditions/{defectId}")]
        public async Task<IActionResult> GetConditionByDefect(int defectId)
        {
            var result = await _listRepository.GetCondition(defectId);

            return result.Any() ? Ok(result) : NotFound("Sin procesos para esta linea");
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
        [Route("machinesByLines")]
        public async Task<IActionResult> GetMachinesByLines([FromQuery] List<int> lineIds)
        {
            if (lineIds == null || !lineIds.Any())
            {
                return BadRequest(new { message = "Debe seleccionar al menos una línea." });
            }

            var result = await _listRepository.GetMachinesByLines(lineIds);

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