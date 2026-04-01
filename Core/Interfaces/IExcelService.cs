using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IExcelService
    {
        byte[] GenerateScrapReport(IEnumerable<ScrapReadDto> data);

        byte[] GenerateRejectionReport(IEnumerable<RejectionResponse> data);
    }
}