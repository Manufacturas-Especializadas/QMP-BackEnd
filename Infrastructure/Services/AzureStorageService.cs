using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Core.Interfaces;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AzureStorageService : IAzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureStorageService(IConfiguration configuration)
        {
            var conncetionString = configuration.GetConnectionString("AzureStorageConnection")
                                    ?? throw new ArgumentNullException("AzureStorageConnection no configurado");

            _blobServiceClient = new BlobServiceClient(conncetionString);
        }

        public async Task<string> UploadFileAsync(string container, IFormFile file)
        {
            if(file == null || file.Length == 0)
            {
                throw new ApplicationException("El archivo no puede estar vacio");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtension = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

            if (!allowedExtension.Contains(extension))
            {
                throw new ApplicationException("Formato de imagen no permitido");
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(container);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var blobClient = containerClient.GetBlobClient(fileName);

                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };

                var options = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                };

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, options);

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al subir a Azure Storage", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string container, string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return false;

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(container);
                var blobName = Path.GetFileName(new Uri(fileUrl).LocalPath);
                var blobClient = containerClient.GetBlobClient(blobName);

                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al eliminar: {ex.Message}");
                return false;
            }
        }

        public async Task<Stream> DownloadFileAsync(string container, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(container);
                var blobClient = containerClient.GetBlobClient(fileName);

                var response = await blobClient.DownloadStreamingAsync();
                return response.Value.Content;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new FileNotFoundException("Archivo no encontrado en Azure", ex);
            }
        }
    }
}