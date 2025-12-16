using System.Collections.Generic;
using System.Threading.Tasks;
using NTC.Core.DTOs;
using NTC.Core.Models;

namespace NTC.Core.Interfaces
{
    public interface ISupabaseService
    {
        bool IsInitialized { get; }
        Task InitializeAsync(string url, string key);
        Task<List<FamilyModel>> GetApprovedFamiliesAsync(int revitVersion);
        Task<bool> UploadFamilyAsync(FamilyUploadDto dto, string filePath);
    }
}
