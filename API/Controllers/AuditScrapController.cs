using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditScrapController : ControllerBase
    {
        private readonly IAuditScrapRepository _auditRepository;
        private readonly IAzureStorageService _storageService;
        private readonly ApplicationDbContext _context;

        private const string _container = "scrap";

        public AuditScrapController(
            IAuditScrapRepository auditRepository,
            IAzureStorageService storageService,
            ApplicationDbContext context
        )
        {
            _auditRepository = auditRepository;
            _storageService = storageService;
            _context = context;
        }

        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll()
        {
            var audits = await _auditRepository.GetAllDetailedAsync();

            return Ok(audits);
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var audit = await _auditRepository.GetByIdAsync(id);

            if (audit == null) return NotFound(new { message = "No se encontró el registro de la auditoría" });

            var dto = new AuditDataScrapReadDto(
                audit.Id,
                audit.AuditDate,
                audit.UserId,
                audit.User?.Username ?? "N/A",
                audit.ShiftId,
                audit.Shift?.ShiftName ?? "N/A",
                audit.Lines.Select(l => l.LineName).ToList(),
                audit.Findings.Select(f => new AuditFindingScrapReadDto(
                    f.Id,
                    f.TypeScrapId,
                    f.TypeScrap?.TypeScrapName ?? "N/A",
                    f.EstimatedWeight,
                    f.MaterialCorrectlyIdentified,
                    f.MaterialCorrectlySegregated,
                    f.UnreportedReason,
                    f.ImageEvidence,
                    f.SupervisorSignature
                )).ToList()
            );

            return Ok(dto);
        }

        [Authorize]
        [HttpPost]
        [Route("Create")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateAuditScrapDto dto)
        {
            var currentUsername = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(currentUsername))
            {
                return Unauthorized(new { message = "Sesión inválida. Por favor, vuelve a iniciar sesión en MesaCore." });
            }

            var inspectorUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUsername);

            if (inspectorUser == null)
            {
                return BadRequest(new { message = $"El inspector '{currentUsername}' no está dado de alta en la planta." });
            }

            int realUserId = inspectorUser.Id;

            TimeZoneInfo mexicoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            DateTime nowInMexico = TimeZoneInfo.ConvertTime(DateTime.UtcNow, mexicoTimeZone);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var audit = new AuditDataScrap
                {
                    UserId = realUserId,
                    AuditDate = nowInMexico,
                    ShiftId = dto.ShiftId
                };

                var selectedLines = await _context.Lines.Where(l => dto.LineIds.Contains(l.Id)).ToListAsync();

                foreach (var line in selectedLines)
                {
                    audit.Lines.Add(line);
                }

                await _auditRepository.CreateAsync(audit);
                await _auditRepository.SaveChangesAsync();

                foreach (var findingDto in dto.Findings)
                {
                    string? signatureUrl = null;
                    var imageUrls = new List<string>();

                    if (findingDto.ImageFiles != null && findingDto.ImageFiles.Any())
                    {
                        foreach (var file in findingDto.ImageFiles.Take(3))
                        {
                            if (file.Length > 0)
                            {
                                var url = await _storageService.UploadFileAsync(_container, file);
                                imageUrls.Add(url);
                            }
                        }
                    }

                    string? finalImageEvidence = imageUrls.Any() ? string.Join(",", imageUrls) : null;

                    if (findingDto.SignatureFile != null && findingDto.SignatureFile.Length > 0)
                    {
                        signatureUrl = await _storageService.UploadFileAsync(_container, findingDto.SignatureFile);
                    }

                    var finding = new AuditFindingScrap
                    {
                        AuditId = audit.Id,
                        TypeScrapId = findingDto.TypeScrapId,
                        EstimatedWeight = findingDto.EstimatedWeight,
                        MaterialCorrectlyIdentified = findingDto.MaterialCorrectlyIdentified,
                        MaterialCorrectlySegregated = findingDto.MaterialCorrectlySegregated,
                        UnreportedReason = findingDto.UnreportedReason,
                        ImageEvidence = finalImageEvidence,
                        SupervisorSignature = signatureUrl
                    };

                    await _context.AuditFindingsScrap.AddAsync(finding);
                }

                await _auditRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "¡Auditoría de Scrap registrada correctamente!"
                });
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = "Error interno al procesar la auditoria de scrap",
                    details = ex.Message
                });
            }
        }

        [HttpPut]
        [Route("Update/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateAuditScrapDto dto)
        {
            var audit = await _auditRepository.GetByIdAsync(id);
            if (audit == null) return NotFound(new { message = "No se encontró la auditoría a editar." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var incomingIds = dto.Findings.Where(f => f.Id > 0).Select(f => f.Id).ToList();
                var deletedFindings = audit.Findings.Where(f => !incomingIds.Contains(f.Id)).ToList();

                foreach (var oldFinding in deletedFindings)
                {
                    if (!string.IsNullOrEmpty(oldFinding.ImageEvidence))
                        await _storageService.DeleteFileAsync(_container, oldFinding.ImageEvidence);

                    if (!string.IsNullOrEmpty(oldFinding.SupervisorSignature))
                        await _storageService.DeleteFileAsync(_container, oldFinding.SupervisorSignature);
                }

                var findingsToSync = new List<AuditFindingScrap>();

                foreach (var findingDto in dto.Findings)
                {
                    string? finalImageUrl = findingDto.KeepImageUrl;
                    string? finalSignatureUrl = findingDto.KeepSignatureUrl;

                    if (findingDto.ImageFile != null && findingDto.ImageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(findingDto.KeepImageUrl))
                            await _storageService.DeleteFileAsync(_container, findingDto.KeepImageUrl);

                        finalImageUrl = await _storageService.UploadFileAsync(_container, findingDto.ImageFile);
                    }

                    if (findingDto.SignatureFile != null && findingDto.SignatureFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(findingDto.KeepSignatureUrl))
                            await _storageService.DeleteFileAsync(_container, findingDto.KeepSignatureUrl);

                        finalSignatureUrl = await _storageService.UploadFileAsync(_container, findingDto.SignatureFile);
                    }

                    findingsToSync.Add(new AuditFindingScrap
                    {
                        Id = findingDto.Id,
                        TypeScrapId = findingDto.TypeScrapId,
                        EstimatedWeight = findingDto.EstimatedWeight,
                        MaterialCorrectlyIdentified = findingDto.MaterialCorrectlyIdentified,
                        MaterialCorrectlySegregated = findingDto.MaterialCorrectlySegregated,
                        UnreportedReason = findingDto.UnreportedReason,
                        ImageEvidence = finalImageUrl,
                        SupervisorSignature = finalSignatureUrl
                    });
                }

                var success = await _auditRepository.UpdateAuditAsync(id, dto, findingsToSync);

                if (!success) return BadRequest(new { message = "No se pudieron aplicar los cambios de edición." });

                await transaction.CommitAsync();

                return Ok(new { message = "Auditoría de Scrap actualizada correctamente en MesaCore." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error al editar la auditoría.", details = ex.Message });
            }
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var audit = await _auditRepository.GetByIdAsync(id);
            if (audit == null) return NotFound(new
            {
                message = "No se encontró el registro a eliminar"
            });

            foreach (var finding in audit.Findings)
            {
                if (!string.IsNullOrEmpty(finding.ImageEvidence))
                    await _storageService.DeleteFileAsync(_container, finding.ImageEvidence);

                if (!string.IsNullOrEmpty(finding.SupervisorSignature))
                    await _storageService.DeleteFileAsync(_container, finding.SupervisorSignature);
            }

            await _auditRepository.DeleteAsync(id);

            return Ok(new
            {
                message = "Auditoría de scrap y evidencias eliminadas correctamente"
            });
        }
    }
}