using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RejectionService : IRejectionService
    {
        private readonly IRejectionRepository _repo;
        private readonly IAzureStorageService _storage;
        private readonly string Container = "rechazos";

        public RejectionService(IRejectionRepository repo, IAzureStorageService storage)
        {
            _repo = repo;
            _storage = storage;
        }

        public async Task<IEnumerable<RejectionResponse>> GetAllAsync(string? searchTerm)
        {
            return await _repo.GetAllAsync(searchTerm);
        }
       
        public async Task<IEnumerable<string>> GetAvailableMonthsAsync()
        {
            var rejections = await _repo.GetAllAsync(null);
            return rejections
                .Select(r => r.CreatedAt.ToString("MMMM yyyy", new System.Globalization.CultureInfo("en-US")))
                .Distinct()
                .ToList();
        }

        public async Task<IEnumerable<RejectionResponse>> GetByMonthAsync(string monthYear)
        {
            var all = await _repo.GetAllAsync(null);
            return all.Where(r =>
                r.CreatedAt.ToString("MMMM yyyy", new System.Globalization.CultureInfo("en-US")) == monthYear);
        }        

        public async Task<int> CreateRejectionAsync(CreateRejectionDto dto, int userId)
        {
            if(!await _repo.UserExistsAsync(userId))
            {
                throw new UnauthorizedAccessException("Usuario no válido");
            }

            var (photoUrls, signatureUrl) = await ProcessPhotos(dto.Photos);

            TimeZoneInfo mexicoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

            DateTime nowInMexico = TimeZoneInfo.ConvertTime(DateTime.UtcNow, mexicoTimeZone);

            var entity = new Rejection
            {
                Inspector = dto.Inspector,
                PartNumber = dto.PartNumber,
                NumberOfPieces = dto.NumberOfPieces,
                DefectId = dto.IdDefect,
                ConditionId = dto.IdCondition,
                Description = dto.Description,
                LineId = dto.IdLine,
                ClientId = dto.IdClient,
                OperatorPayroll = dto.OperatorPayroll,
                ContainmentActionId = dto.IdContainmentAction,
                Folio = dto.Folio,
                Image = photoUrls,
                InformedSignature = signatureUrl,
                UserId = userId,
                CreatedAt = nowInMexico
            };

            return await _repo.CreateAsync(entity);
        }

        public async Task UpdateRejectionAsync(EditRejectionDto dto)
        {
            var existing = await _repo.GetByIdAsync(dto.Id)
                    ?? throw new KeyNotFoundException("Rechazo no encontrado");

            var originalUrls = existing.Image?.Split(";", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var remaingUrls = dto.ExistingImageUrls?.Split(";", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            foreach (var url in originalUrls.Except(remaingUrls))
            {
                await _storage.DeleteFileAsync(Container, url);
            }

            var (newPhotoUrls, newSignatureUrl) = await ProcessPhotos(dto.NewPhotos);

            if (!string.IsNullOrEmpty(newSignatureUrl))
            {
                if (!string.IsNullOrEmpty(existing.InformedSignature))
                {
                    await _storage.DeleteFileAsync(Container, existing.InformedSignature);                   
                }

                existing.InformedSignature = newSignatureUrl;
            }

            var finalPhotos = remaingUrls.Concat(newPhotoUrls?.Split(";") ?? Array.Empty<string>()).ToList();

            existing.Inspector = dto.Inspector;
            existing.PartNumber = dto.PartNumber;
            existing.NumberOfPieces = dto.NumberOfPieces;
            existing.DefectId = dto.IdDefect;
            existing.ConditionId = dto.IdCondition;
            existing.Description = dto.Description;
            existing.LineId = dto.IdLine;
            existing.ClientId = dto.IdClient;
            existing.OperatorPayroll = dto.OperatorPayroll;
            existing.ContainmentActionId = dto.IdContainmentAction;
            existing.Folio = dto.Folio;
            existing.Image = finalPhotos.Any() ? string.Join(";", finalPhotos) : null;

            await _repo.UpdateAsync(existing);
        }

        public async Task<bool> DeleteRejectionAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            var photoUrls = existing.Image?.Split(";", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            foreach (var url in photoUrls)
            {
                await _storage.DeleteFileAsync(Container, url);
            }

            if (!string.IsNullOrEmpty(existing.InformedSignature))
            {
                await _storage.DeleteFileAsync(Container, existing.InformedSignature);
            }

            await _repo.DeleteAsync(existing);

            return true;
        }

        public async Task<int> GetNextFolioAsync()
        {
            var maxFolio = await _repo.GetMaxFolioAsync();

            return maxFolio + 1;
        }

        private async Task<(string? photos, string? signature)> ProcessPhotos(List<IFormFile>? photos)
        {
            if (photos == null || !photos.Any()) return (null, null);

            var urls = new List<string>();
            string? signatureUrl = null;

            foreach (var photo in photos)
            {
                var url = await _storage.UploadFileAsync(Container, photo);
                if (photo.FileName.ToLower().Contains("signature"))
                    signatureUrl = url;
                else
                    urls.Add(url);
            }

            return (urls.Any() ? string.Join(";", urls) : null, signatureUrl);
        }
    }
}