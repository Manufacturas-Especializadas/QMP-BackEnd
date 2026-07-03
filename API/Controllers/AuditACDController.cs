using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Data;
using Core.Interfaces;
using Core.Entities;
using Core.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditACDController : ControllerBase
    {
        private readonly IAuditACDRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly IAzureStorageService _storageService;

        public AuditACDController(IAuditACDRepository repository, ApplicationDbContext context, 
            IAzureStorageService storageService)
        {
            _context = context;
            _repository = repository;
            _storageService = storageService;
        }

        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll()
        {
            var audits = await _repository.GetAllDetailedAsync();

            return Ok(audits);
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var audit = await _repository.GetByIdAsync(id);

            if (audit == null) return NotFound(new { message = "No se encontró la auditoría ACD" });

            var dto = new AuditDataACDReadDto(
                audit.Id,
                audit.AuditDate,
                audit.UserId,
                audit.User?.Username ?? "N/A",
                audit.ShiftId,
                audit.Shift?.ShiftName ?? "N/A",
                audit.RejectionId,
                audit.Rejection?.Folio,
                audit.Lines.Select(l => l.LineName).ToList(),
                audit.Lines.Select(l => l.Id).ToList(),
                audit.Findings.Select(f => new AuditFindingACDReadDto(
                    f.Id,
                    f.StartPointId,
                    f.StartPoint?.ProcessName ?? "N/A",
                    f.EndPointId,
                    f.EndPoint?.ProcessName ?? "N/A",
                    f.PartNumber,
                    f.NumberOfPieces,
                    f.SampleSize,
                    f.PackerPayroll,
                    f.ContainerIdMatch,
                    f.FrontView,
                    f.SideView,
                    f.TopView,
                    f.IsometricView,
                    f.CompleteProcess,
                    f.IsProductConforming,
                    f.ShopOrder,
                    f.WeldingDefects,
                    f.PpBom,
                    f.ImagesEvidence
                )).ToList()
            );

            return Ok(dto);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromForm] CreateAuditACDDto dto)
        {
            if (dto == null) return BadRequest("Datos inválidos");

            var currentUsername = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUsername);

            if (user == null) return BadRequest(new { message = "Usuario no válido" });

            TimeZoneInfo mexicoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

            DateTime nowInMexico = TimeZoneInfo.ConvertTime(DateTime.UtcNow, mexicoTimeZone);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var audit = new AuditDataACD
                {
                    UserId = user.Id,
                    AuditDate = nowInMexico,
                    ShiftId = dto.ShiftId,
                    RejectionId = dto.RejectionId,
                };

                var selectedLines = await _context.Lines.Where(l => dto.LineIds.Contains(l.Id)).ToListAsync();

                foreach (var line in selectedLines) audit.Lines.Add(line);

                await _repository.CreateAsync(audit);
                await _context.SaveChangesAsync();

                foreach (var fDto in dto.Findings)
                {

                    var uploadedUrls = new List<string>();

                    if (fDto.ImageFiles != null && fDto.ImageFiles.Any())
                    {
                        foreach (var file in fDto.ImageFiles.Take(3))
                        {
                            var url = await _storageService.UploadFileAsync("acd-evidences", file);
                            uploadedUrls.Add(url);
                        }
                    }

                    var finding = new AuditFindingACD
                    {
                        AuditId = audit.Id,
                        StartPointId = fDto.StartPointId,
                        EndPointId = fDto.EndPointId,
                        PartNumber = fDto.PartNumber,
                        NumberOfPieces = fDto.NumberOfPieces,
                        SampleSize = fDto.SampleSize,
                        PackerPayroll = fDto.PackerPayroll,
                        ContainerIdMatch = fDto.ContainerIdMatch,
                        FrontView = fDto.FrontView,
                        SideView = fDto.SideView,
                        TopView = fDto.TopView,
                        IsometricView = fDto.IsometricView,
                        CompleteProcess = fDto.CompleteProcess,
                        IsProductConforming = fDto.IsProductConforming,
                        ShopOrder = fDto.ShopOrder,
                        WeldingDefects = fDto.WeldingDefects,
                        PpBom = fDto.PpBom,
                        ImagesEvidence = uploadedUrls.Any() ? string.Join(",", uploadedUrls) : null
                    };

                    await _context.AuditFindingsACDs.AddAsync(finding);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Auditoría ACD registrada con éxito",
                    auditId = audit.Id,
                });
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = "Error al procesar la auditoria ACD"
                });
            }
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateAuditACDDto dto)
        {
            if (dto == null) return BadRequest("Datos de edición inválidos.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var findingsToSync = new List<AuditFindingACD>();

                foreach (var fDto in dto.Findings)
                {
                    var finalUrls = new List<string>();

                    if (!string.IsNullOrWhiteSpace(fDto.ExistingImageUrls))
                    {
                        finalUrls.AddRange(fDto.ExistingImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries));
                    }

                    if (fDto.ImageFiles != null && fDto.ImageFiles.Any())
                    {
                        int spaceLeft = 3 - finalUrls.Count;
                        if (spaceLeft > 0)
                        {
                            foreach (var file in fDto.ImageFiles.Take(spaceLeft))
                            {
                                var url = await _storageService.UploadFileAsync("acd-evidences", file);
                                finalUrls.Add(url);
                            }
                        }
                    }

                    findingsToSync.Add(new AuditFindingACD
                    {
                        Id = fDto.Id,
                        StartPointId = fDto.StartPointId,
                        EndPointId = fDto.EndPointId,
                        PartNumber = fDto.PartNumber,
                        NumberOfPieces = fDto.NumberOfPieces,
                        SampleSize = fDto.SampleSize,
                        PackerPayroll = fDto.PackerPayroll,
                        ContainerIdMatch = fDto.ContainerIdMatch,
                        FrontView = fDto.FrontView,
                        SideView = fDto.SideView,
                        TopView = fDto.TopView,
                        IsometricView = fDto.IsometricView,
                        CompleteProcess = fDto.CompleteProcess,
                        IsProductConforming = fDto.IsProductConforming,
                        ShopOrder = fDto.ShopOrder,
                        WeldingDefects = fDto.WeldingDefects,
                        PpBom = fDto.PpBom,
                        ImagesEvidence = finalUrls.Any() ? string.Join(",", finalUrls) : null
                    });
                }

                var success = await _repository.UpdateAuditAsync(id, dto, findingsToSync);
                if (!success) return NotFound(new { message = "No se pudo actualizar el registro solicitado." });

                await transaction.CommitAsync();
                return Ok(new { message = "Auditoría ACD actualizada y sincronizada." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error al editar la auditoría ACD.", details = ex.Message });
            }
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repository.DeleteAsync(id);

            if (!result) return NotFound(new
            {
                message = "No se encontró el registro ACD a eliminar"
            });

            return Ok(new
            {
                message = "Auditoría ACD eliminada correctamente"
            });
        }
    }
}