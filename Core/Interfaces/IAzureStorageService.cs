using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IAzureStorageService
    {
        Task<string> UploadFileAsync(string container, IFormFile file);

        Task<bool> DeleteFileAsync(string container, string fileUrl);

        Task<Stream> DownloadFileAsync(string container, string fileName);
    }
}