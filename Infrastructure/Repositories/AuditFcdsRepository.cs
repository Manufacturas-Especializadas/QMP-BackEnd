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

                TimeZoneInfo mexicoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

                DateTime nowInMexico = TimeZoneInfo.ConvertTime(DateTime.UtcNow, mexicoTimeZone);

                var auditData = new AuditDataFcds
                {
                    UserId = userId,
                    AuditDate = nowInMexico,
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
                        CreatedAt = nowInMexico,
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
                        await _context.AuditVisualChecklistFCDS.AddAsync(new AuditVisualChecklistFcds
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

        public async Task<bool> UpdateAuditAsync(int id, CreateAuditFcdsDto dto)
        {
            var audit = await _context.AuditDataFcds
                .Include(a => a.Lines)
                .Include(a => a.TraceabilityElements).ThenInclude(t => t.MachineCodes)
                .Include(a => a.TraceabilityElements).ThenInclude(t => t.EquipmentSerials)
                .Include(a => a.ProcessControls)
                .Include(a => a.PhysicalConditions)
                .Include(a => a.DimensionalSpecs)
                .Include(a => a.VisualChecklists)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                audit.ShiftId = dto.ShiftId;
                audit.FcdsProcessId = dto.FcdsProcessId;
                audit.PartNumber = dto.PartNumber;
                audit.IsProductConforming = dto.IsProductConforming;
                audit.RejectionId = dto.RejectionId;

                audit.Lines.Clear();
                var newLines = await _context.Lines.Where(l => dto.LineIds.Contains(l.Id)).ToListAsync();
                foreach (var line in newLines) audit.Lines.Add(line);

                _context.TraceabilityElementsFcds.RemoveRange(audit.TraceabilityElements);
                _context.ProcessControlsFcds.RemoveRange(audit.ProcessControls);
                _context.ProductReleasePhysicalConditions.RemoveRange(audit.PhysicalConditions);
                _context.AuditDimensionalSpecsFcds.RemoveRange(audit.DimensionalSpecs);
                _context.AuditVisualChecklistFCDS.RemoveRange(audit.VisualChecklists);

                await _context.SaveChangesAsync();

                var traceability = new TraceabilityElementFcds
                {
                    AuditId = audit.Id,
                    OperatorsPayroll = dto.Traceability.OperatorsPayroll ?? "",
                    CategoryId = dto.Traceability.CategoryId,
                    TypeMeasuringEquipmentId = dto.Traceability.TypeMeasuringEquipmentId,
                    ShopOrder = dto.Traceability.ShopOrder,
                    BatchPipe = dto.Traceability.BatchPipe,
                    PipeDiameterId = dto.Traceability.PipeDiameterId,
                    PipeWallId = dto.Traceability.PipeWallId
                };

                if (dto.Traceability.MachineCodeIds != null && dto.Traceability.MachineCodeIds.Any())
                {
                    var machines = await _context.MachineCodes
                        .Where(m => dto.Traceability.MachineCodeIds.Contains(m.Id))
                        .ToListAsync();

                    foreach (var machine in machines)
                    {
                        traceability.MachineCodes.Add(machine);
                    }
                }

                if (dto.Traceability.EquipmentSerials != null)
                {
                    foreach (var serial in dto.Traceability.EquipmentSerials)
                    {
                        traceability.EquipmentSerials.Add(new() { EquipmentSerial = serial });
                    }
                }

                await _context.TraceabilityElementsFcds.AddAsync(traceability);


                var processControls = new ProcessControlFcds
                {
                    AuditId = audit.Id,
                    MttoValidation = dto.Controls.MttoValidation,
                    Realese1stPiece = dto.Controls.Realese1stPiece,
                    Spc = dto.Controls.Spc,
                    MaterialCorrectlyIdentified = dto.Controls.MaterialCorrectlyIdentified,
                    IdentifiedMeasuringEquipment = dto.Controls.IdentifiedMeasuringEquipment,
                    CalibratedMeasuringEquipment = dto.Controls.CalibratedMeasuringEquipment,
                    ItProcess = dto.Controls.ItProcess,
                    TypeOil = dto.Controls.TypeOil ?? "",
                    LastHourOfRelease = TimeSpan.Parse(dto.Controls.LastHourOfRelease ?? "00:00")
                };
                await _context.ProcessControlsFcds.AddAsync(processControls);


                var physicalConditions = new ProductReleasePhysicalCondition
                {
                    AuditId = audit.Id,
                    Brands = dto.Physicals.Brands,
                    Blows = dto.Physicals.Blows,
                    Pollution = dto.Physicals.Pollution,
                    Ovality = dto.Physicals.Ovality,
                    Burr = dto.Physicals.Burr,
                    Warped = dto.Physicals.Warped,
                    ExcessOil = dto.Physicals.ExcessOil
                };
                await _context.ProductReleasePhysicalConditions.AddAsync(physicalConditions);


                if (dto.DimensionalSpecs != null)
                {
                    foreach (var spec in dto.DimensionalSpecs)
                    {
                        await _context.AuditDimensionalSpecsFcds.AddAsync(new AuditDimensionalSpecFcds
                        {
                            AuditId = audit.Id,
                            SpecName = spec.SpecName,
                            ExpectedValue = spec.ExpectedValue ?? "",
                            RealValue = spec.RealValue ?? ""
                        });
                    }
                }


                if (dto.VisualChecklists != null)
                {
                    foreach (var check in dto.VisualChecklists)
                    {
                        await _context.AuditVisualChecklistFCDS.AddAsync(new AuditVisualChecklistFcds
                        {
                            AuditId = audit.Id,
                            CheckpointName = check.CheckpointName,
                            ResultValue = check.ResultValue
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

        public async Task<bool> DeleteAuditAsync(int id)
        {
            var audit = await _context.AuditDataFcds.FindAsync(id);

            if (audit == null) return false;

            _context.AuditDataFcds.Remove(audit);

            return await _context.SaveChangesAsync() > 0;
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

        public async Task<DetailedAuditFcdsDto?> GetDetailedAuditByIdAsync(int id)
        {
            var audit = await _context.AuditDataFcds
                .AsNoTracking()
                .Include(a => a.Lines)
                .Include(a => a.TraceabilityElements).ThenInclude(t => t.MachineCodes)
                .Include(a => a.TraceabilityElements).ThenInclude(t => t.EquipmentSerials)
                .Include(a => a.ProcessControls)
                .Include(a => a.PhysicalConditions)
                .Include(a => a.DimensionalSpecs)
                .Include(a => a.VisualChecklists)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null) return null;

            var traceability = audit.TraceabilityElements.FirstOrDefault();
            var controls = audit.ProcessControls.FirstOrDefault();
            var physicals = audit.PhysicalConditions.FirstOrDefault();

            return new DetailedAuditFcdsDto
            {
                Id = audit.Id,
                AuditDate = audit.AuditDate,
                ShiftId = audit.ShiftId,
                FcdsProcessId = audit.FcdsProcessId,
                PartNumber = audit.PartNumber,
                LineIds = audit.Lines.Select(l => l.Id).ToList(),
                IsProductConforming = audit.IsProductConforming,
                RejectionId = audit.RejectionId,

                Traceability = new TraceabilityFcdsDto
                {
                    MachineCodes = traceability?.MachineCodes.Select(m => m.MachineCodeName).ToList() ?? new List<string>(),
                    OperatorsPayroll = traceability?.OperatorsPayroll ?? "",
                    CategoryId = traceability?.CategoryId ?? 0,
                    TypeMeasuringEquipmentId = traceability?.TypeMeasuringEquipmentId,
                    ShopOrder = traceability?.ShopOrder,
                    BatchPipe = traceability?.BatchPipe,
                    PipeDiameterId = traceability?.PipeDiameterId,
                    PipeWallId = traceability?.PipeWallId,
                    EquipmentSerials = traceability?.EquipmentSerials.Select(s => s.EquipmentSerial).ToList() ?? new List<string>()
                },

                Controls = new ProcessControlFcdsDto
                {
                    MttoValidation = controls?.MttoValidation ?? 0,
                    Realese1stPiece = controls?.Realese1stPiece ?? 0,
                    Spc = controls?.Spc ?? 0,
                    MaterialCorrectlyIdentified = controls?.MaterialCorrectlyIdentified ?? 0,
                    IdentifiedMeasuringEquipment = controls?.IdentifiedMeasuringEquipment ?? 0,
                    CalibratedMeasuringEquipment = controls?.CalibratedMeasuringEquipment ?? 0,
                    ItProcess = controls?.ItProcess ?? 0,
                    TypeOil = controls?.TypeOil ?? "",
                    LastHourOfRelease = controls?.LastHourOfRelease.ToString(@"hh\:mm") ?? ""
                },

                Physicals = new PhysicalConditionsDto
                {
                    Brands = physicals?.Brands ?? 0,
                    Blows = physicals?.Blows ?? 0,
                    Pollution = physicals?.Pollution ?? 0,
                    Ovality = physicals?.Ovality ?? 0,
                    Burr = physicals?.Burr ?? 0,
                    Warped = physicals?.Warped ?? 0,
                    ExcessOil = physicals?.ExcessOil ?? 0
                },

                DimensionalSpecs = audit.DimensionalSpecs.Select(s => new DimensionalSpecDto(s.SpecName, s.ExpectedValue, s.RealValue)).ToList(),
                VisualChecklists = audit.VisualChecklists.Select(v => new VisualChecklistDto(v.CheckpointName, v.ResultValue)).ToList()
            };
        }
    }
}