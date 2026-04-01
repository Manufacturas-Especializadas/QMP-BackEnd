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

        public byte[] GenerateRejectionReport(IEnumerable<RejectionResponse> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte de rechazos");
                var headers = new string[] 
                {
                    "ID", "Folio", "Fecha", "Inspector", "Nro Parte",
                    "Piezas", "Línea", "Cliente", "Defecto", "Condición", "Acción",
                    "Evidencia", "Firma"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                int row = 2;
                using var httpClient = new HttpClient();

                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.Folio;
                    worksheet.Cell(row, 3).Value = item.CreatedAt.ToString("g");
                    worksheet.Cell(row, 4).Value = item.Inspector;
                    worksheet.Cell(row, 5).Value = item.PartNumber;
                    worksheet.Cell(row, 6).Value = item.NumberOfPieces;
                    worksheet.Cell(row, 7).Value = item.LineName;
                    worksheet.Cell(row, 8).Value = item.ClientName;
                    worksheet.Cell(row, 9).Value = item.DefectName;
                    worksheet.Cell(row, 10).Value = item.ConditionName;
                    worksheet.Cell(row, 11).Value = item.ContainmentActionName;

                    worksheet.Row(row).Height = 70;

                    if (!string.IsNullOrEmpty(item.Image))
                    {
                        try
                        {
                            var allUrls = item.Image.Split(';', StringSplitOptions.RemoveEmptyEntries).Take(3);

                            int columnOffset = 0;
                            foreach (var url in allUrls)
                            {
                                byte[] imgBytes = httpClient.GetByteArrayAsync(url).Result;
                                using var ms = new MemoryStream(imgBytes);

                                var pic = worksheet.AddPicture(ms)
                                    .MoveTo(worksheet.Cell(row, 12))
                                    .WithSize(80, 80);

                                pic.Left = 5 + (columnOffset * 85);
                                pic.Top = 5;

                                columnOffset++;
                            }
                        }
                        catch {  }
                    }

                    if (!string.IsNullOrEmpty(item.InformedSignature))
                    {
                        try
                        {
                            byte[] sigBytes = httpClient.GetByteArrayAsync(item.InformedSignature).Result;
                            using var msSig = new MemoryStream(sigBytes);
                            var sig = worksheet.AddPicture(msSig)
                                .MoveTo(worksheet.Cell(row, 13))
                                .WithSize(90, 45);
                        }
                        catch { }
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();
                worksheet.Column(12).Width = 40;
                worksheet.Column(13).Width = 15;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}