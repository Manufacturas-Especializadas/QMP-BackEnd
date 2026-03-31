using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ListRepository : IListRepository
    {
        private readonly ApplicationDbContext _context;

        public ListRepository(ApplicationDbContext context)
        {
            _context = context;            
        }

        public async Task<IEnumerable<LineLookupDto>> GetLines()
        {
            return await _context.Lines
                    .AsNoTracking()
                    .Select(l => new LineLookupDto(l.Id, l.LineName))
                    .ToListAsync();
        }

        public async Task<IEnumerable<ClientLookupDto>> GetClients()
        {
            return await _context.Clients
                    .AsNoTracking()
                    .Select(l => new ClientLookupDto(l.Id, l.ClientName))
                    .ToListAsync();
        }

        public async Task<IEnumerable<ShiftLookupDto>> GetShifts()
        {
            return await _context.Shifts
                        .AsNoTracking()
                        .Select(s => new ShiftLookupDto(s.Id, s.ShiftName))
                        .ToListAsync();
        }

        public async Task<IEnumerable<MaterialLookupDto>> GetMaterial()
        {
            return await _context.Materials
                        .AsNoTracking()
                        .Select(m => new MaterialLookupDto(m.Id, m.MaterialName))
                        .ToListAsync();
        }

        public async Task<IEnumerable<DefectLookupDto>> GetDefects()
        {
            return await _context.DefectsRejections
                        .AsNoTracking()
                        .Select(d => new DefectLookupDto(d.Id, d.DefectName))
                        .ToListAsync();
        }

        public async Task<IEnumerable<ContainmentActionLookupDto>> GetContainmentActions()
        {
            return await _context.ContainmentActions
                            .AsNoTracking()
                            .Select(c => new ContainmentActionLookupDto(c.Id, c.ContainmentActionName))
                            .ToListAsync();
        }

        public async Task<IEnumerable<TypeScrapLookupDto>> GetTypeScrap()
        {
            return await _context.TypeScraps
                        .AsNoTracking()
                        .Select(t => new TypeScrapLookupDto(t.Id, t.TypeScrapName))
                        .ToListAsync();
        }

        public async Task<IEnumerable<RejectionLookupDto>> GetRejections()
        {
            return await _context.Rejections
                        .AsNoTracking()
                        .OrderByDescending(r => r.Id)
                        .Select(r => new RejectionLookupDto(
                            r.Id, r.Folio, r.Inspector, r.PartNumber, r.NumberOfPieces,
                            r.OperatorPayroll, r.Description, r.Image, r.InformedSignature,
                            r.CreatedAt, r.Defect!.DefectName, r.Condition!.ConditionName, r.Line!.LineName,
                            r.Client!.ClientName, r.User.Username, r.ContainmentAction!.ContainmentActionName
                        ))
                        .ToListAsync();
        }

        public async Task<IEnumerable<ScrapLookupDto>> GetScrap()
        {
            return await _context.Scraps
                        .AsNoTracking()
                        .OrderByDescending(s => s.Id)
                        .Select(s => new ScrapLookupDto(
                            s.Id, s.PayRollNumber, s.Alloy, s.Diameter, s.Wall, s.RDM,
                            s.Shift.ShiftName, s.Process!.ProcessName, s.Line.LineName, s.Material.MaterialName, s.TypeScrap.TypeScrapName,
                            s.MachineCode.MachineCodeName, s.Defect.DefectName, s.Weight, s.IsVerified, s.VerifiedWeight, s.CreatedAt))
                        .ToListAsync();
        }

        public async Task<IEnumerable<ConditionLookupDto>> GetCondition(int defectId)
        {
            return await _context.Conditions
                        .AsNoTracking()
                        .Where(c => c.DefectId == defectId)
                        .Select(c => new ConditionLookupDto(c.Id, c.ConditionName, c.DefectId))
                        .ToListAsync();
        }

        public async Task<IEnumerable<ProcessLookupDto>> GetProcess(int lineId)
        {
            return await _context.Processes
                        .AsNoTracking()
                        .Where(p => p.LineId == lineId)
                        .Select(p => new ProcessLookupDto(p.Id, p.ProcessName, p.LineId))
                        .ToListAsync();

        }

        public async Task<IEnumerable<MachineCodeLookupDto>> GetMachineCodes(int processId)
        {
            return await _context.MachineCodes
                        .AsNoTracking()
                        .Where(m => m.ProcessId == processId)
                        .Select(m => new MachineCodeLookupDto(m.Id, m.MachineCodeName, m.ProcessId))
                        .ToListAsync();
        }

        public async Task<IEnumerable<DefectsLookupDto>> GetDefects(int typeScrapId)
        {
            return await _context.Defects
                        .AsNoTracking()
                        .Where(d => d.TypeScrapId == typeScrapId)
                        .Select(d => new DefectsLookupDto(d.Id, d.DefectName, d.TypeScrapId))
                        .ToListAsync();
                        
        }
    }
}
