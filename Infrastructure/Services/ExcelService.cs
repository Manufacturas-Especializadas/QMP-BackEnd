using ClosedXML.Excel;
using Core.DTOs;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ExcelService : IExcelService
    {
        public byte[] GenerateScrapReport(IEnumerable<ScrapReadDto> data)
        {
            using(var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte de Scrap");

                var headers = new string[] { "ID", "Nómina", "Fecha", "Línea", "Proceso", "Tipo", "Peso Original", "Verificado", "Peso Verificado" };

                for(int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
                    cell.Style.Font.FontColor = XLColor.White;
                }

                int row = 2;
                foreach(var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.PayRollNumber;
                    worksheet.Cell(row, 3).Value = item.CreatedAt.ToString("g");
                    worksheet.Cell(row, 4).Value = item.LineName;
                    worksheet.Cell(row, 5).Value = item.ProcessName;
                    worksheet.Cell(row, 6).Value = item.TypeScrapName;
                    worksheet.Cell(row, 7).Value = item.Weight;
                    worksheet.Cell(row, 8).Value = item.IsVerified ? "SÍ" : "NO";
                    worksheet.Cell(row, 9).Value = item.VerifiedWeight ?? 0;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using(var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return stream.ToArray();
                }
            }
        }
    }
}