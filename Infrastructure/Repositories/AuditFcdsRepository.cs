using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Core.Interfaces;
using Core.Entities;
using Core.DTOs;

namespace Infrastructure.Repositories
{
    public class AuditFcdsRepository : IAuditFcdsRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditFcdsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAuditAsync(CreateAuditFcdsDto dto, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var inspectorUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (inspectorUser == null)
                {
                    throw new Exception("Inspector no válido");
                }

                var auditData = new AuditDataFcds
                {
                    UserId = userId,
                    ShiftId = dto.ShiftId,
                    FcdsProcessId = dto.FcdsProcessId,
                    PartNumber = dto.PartNumber,
                    IsProductConforming = dto.IsProductConforming
                };

                var selectedLines = await _context.Lines.Where(l => dto.LineIds.Contains(l.Id)).ToListAsync();

                foreach (var line in selectedLines)
                {
                    auditData.Lines.Add(line);
                }

                if (!dto.IsProductConforming)
                {
                    var newRejection = new Rejection
                    {
                        UserId = userId,
                        Inspector = inspectorUser.Username,
                        PartNumber = dto.PartNumber,
                        OperatorPayroll = (int?)Convert.ToUInt32(dto.Traceability.OperatorsPayroll),
                        LineId = dto.LineIds.FirstOrDefault(),
                        NumberOfPieces = 1,
                        Description = $"Generado automáticamente desde Auditoría FCD de {dto.PartNumber}.",
                        DefectId = await _context.DefectsRejections.Select(d => d.Id).FirstOrDefaultAsync(),
                        ClientId = await _context.Clients.Select(c => c.Id).FirstOrDefaultAsync(),
                        ContainmentActionId = await _context.ContainmentActions.Select(c => c.Id).FirstOrDefaultAsync()
                    };

                    await _context.Rejections.AddAsync(newRejection);
                    await _context.SaveChangesAsync();

                    auditData.RejectionId = newRejection.Id;
                }

                await _context.AuditDataFcds.AddAsync(auditData);
                await _context.SaveChangesAsync();

                var traceabillity = new TraceabilityElementFcds
                {
                    AuditId = auditData.Id,
                    OperatorsPayroll = dto.Traceability.OperatorsPayroll,
                    CategoryId = dto.Traceability.CategoryId,
                    TypeMeasuringEquipmentId = dto.Traceability.TypeMeasuringEquipmentId,
                    ShopOrder = dto.Traceability.ShopOrder,
                    BatchPipe = dto.Traceability.BatchPipe,
                    PipeDiameterId = dto.Traceability.PipeDiameterId,
                    PipeWallId = dto.Traceability.PipeWallId
                };

                var selectedMachines = await _context.MachineCodes
                    .Where(m => dto.Traceability.MachineCodeIds.Contains(m.Id))
                    .ToListAsync();

                foreach (var machine in selectedMachines)
                {
                    traceabillity.MachineCodes.Add(machine);
                }

                await _context.TraceabilityElementsFcds.AddAsync(traceabillity);
                await _context.SaveChangesAsync();

                if (dto.Traceability.EquipmentSerials != null)
                {
                    foreach (var serial in dto.Traceability.EquipmentSerials.Where(s => !string.IsNullOrEmpty(s)))
                    {
                        await _context.AuditEquipmentSerialsFcds.AddAsync(new AuditEquipmentSerialFcds
                        {
                            TraceabilityId = traceabillity.Id,
                            EquipmentSerial = serial
                        });
                    }
                }

                var processControl = new ProcessControlFcds
                {
                    AuditId = auditData.Id,
                    MttoValidation = dto.Controls.MttoValidation,
                    Realese1stPiece = dto.Controls.Realese1stPiece,
                    Spc = dto.Controls.Spc,
                    MaterialCorrectlyIdentified = dto.Controls.MaterialCorrectlyIdentified,
                    IdentifiedMeasuringEquipment = dto.Controls.IdentifiedMeasuringEquipment,
                    CalibratedMeasuringEquipment = dto.Controls.CalibratedMeasuringEquipment,
                    ItProcess = dto.Controls.ItProcess,
                    TypeOil = dto.Controls.TypeOil,
                    LastHourOfRelease = TimeSpan.Parse(dto.Controls.LastHourOfRelease)
                };

                await _context.ProcessControlsFcds.AddAsync(processControl);

                var physicalCondition = new ProductReleasePhysicalCondition
                {
                    AuditId = auditData.Id,
                    Brands = dto.Physicals.Brands,
                    Blows = dto.Physicals.Blows,
                    Pollution = dto.Physicals.Pollution,
                    Ovality = dto.Physicals.Ovality,
                    Burr = dto.Physicals.Burr,
                    Warped = dto.Physicals.Warped,
                    ExcessOil = dto.Physicals.ExcessOil
                };

                await _context.ProductReleasePhysicalConditions.AddAsync(physicalCondition);

                if (dto.DimensionalSpecs != null && dto.DimensionalSpecs.Any())
                {
                    foreach ( var spec in dto.DimensionalSpecs )
                    {
                        await _context.AuditDimensionalSpecsFcds.AddAsync(new AuditDimensionalSpecFcds
                        {
                            AuditId = auditData.Id,
                            SpecName = spec.SpecName,
                            ExpectedValue = spec.ExpectedValue,
                            RealValue = spec.RealValue,
                        });
                    }
                }

                if (dto.VisualChecklists != null && dto.VisualChecklists.Any())
                {
                    foreach ( var check in dto.VisualChecklists)
                    {
                        await _context.AuditVisualChecklistsFcds.AddAsync(new AuditVisualChecklistFcds
                        {
                            AuditId = auditData.Id,
                            CheckpointName = check.CheckpointName,
                            ResultValue = check.ResultValue,
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<AuditFcdsListDto>> GetListAuditsAsync()
        {
            var rawAudits = await _context.AuditDataFcds
                .AsNoTracking()
                .OrderByDescending(a => a.AuditDate)
                .Select(a => new
                {
                    a.Id,
                    a.AuditDate,
                    InspectorName = a.User.Username,
                    ProcessName = a.FcdsProcess.ProcessName,
                    a.PartNumber,
                    LineNames = a.Lines.Select(l => l.LineName).ToList(),
                    a.IsProductConforming,
                    FolioRDM = a.Rejection != null ? (int?)a.Rejection.Folio : null
                })
                .ToListAsync();

            return rawAudits.Select(a => new AuditFcdsListDto(
                a.Id,
                a.AuditDate,
                a.InspectorName,
                a.ProcessName,
                a.PartNumber,
                string.Join(", ", a.LineNames),
                a.IsProductConforming,
                a.FolioRDM
            ));
        }
    }
}