using Core.DTOs;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IListRepository
    {
        Task<IEnumerable<RoleReadDto>> GetRoles();

        Task<IEnumerable<LineLookupDto>> GetLines();

        Task<IEnumerable<ClientLookupDto>> GetClients();

        Task<IEnumerable<ShiftLookupDto>> GetShifts();

        Task<IEnumerable<MaterialLookupDto>> GetMaterial();

        Task<IEnumerable<DefectLookupDto>> GetDefects();

        Task<IEnumerable<ContainmentActionLookupDto>> GetContainmentActions();

        Task<IEnumerable<TypeScrapLookupDto>> GetTypeScrap();

        Task<IEnumerable<ScrapLookupDto>> GetScrap();

        Task<IEnumerable<RejectionLookupDto>> GetRejections();

        Task<IEnumerable<ConditionLookupDto>> GetCondition(int defectId);

        Task<IEnumerable<ProcessLookupDto>> GetProcess(int lineId);

        Task<IEnumerable<CategoryOperatorLookupDto>> GetCategoryOperators();

        Task<IEnumerable<TypeMeasuringEquipmentLookupDto>> GetTypeMeasuringEquipment();

        Task<IEnumerable<PipeDiametersLookupDto>> GetPipeDiameters();

        Task<IEnumerable<WallsOfDiametersLookupDto>> GetWallsOfDiameters();

        Task<IEnumerable<MachineCodeLookupDto>> GetMachineCodes(int processId);

        Task<IEnumerable<MachineByLinesLookupDto>> GetMachinesByLines(List<int> lineIds);

        Task<IEnumerable<DefectsLookupDto>> GetDefects(int typeScrapId);

        Task<IEnumerable<AuditsPointsLookDto>> GetAuditsStartPoints();

        Task<IEnumerable<AuditsPointsLookDto>> GetAuditsEndPoints();

    }
}