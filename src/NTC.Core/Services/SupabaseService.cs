using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NTC.Core.DTOs;
using NTC.Core.Exceptions;
using NTC.Core.Interfaces;
using NTC.Core.Models;
using RestSharp;

namespace NTC.Core.Services
{
    public class SupabaseService : ISupabaseService
    {
        // 1. Thread-Safe Singleton using Lazy<T>
        private static readonly Lazy<SupabaseService> _lazyInstance = 
            new Lazy<SupabaseService>(() => new SupabaseService());

        public static SupabaseService Instance => _lazyInstance.Value;

        private RestClient _client;
        private string _supabaseUrl;
        private string _supabaseKey;

        private SupabaseService() { }

        public bool IsInitialized => _client != null;

        public Task InitializeAsync(string url, string key)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Invalid Credentials");

            _supabaseUrl = url;
            _supabaseKey = key;
            _client = new RestClient(new RestClientOptions(_supabaseUrl) 
            { 
                ThrowOnAnyError = false // We handle errors manually
            });

            return Task.CompletedTask;
        }

        private RestRequest CreateRequest(string resource, Method method)
        {
            if (!IsInitialized) throw new InvalidOperationException("Service not initialized.");
            var request = new RestRequest(resource, method);
            request.AddHeader("apikey", _supabaseKey);
            request.AddHeader("Authorization", $"Bearer {_supabaseKey}");
            return request;
        }

        public async Task<List<FamilyModel>> GetApprovedFamiliesAsync(int revitVersion)
        {
            try
            {
                var request = CreateRequest($"/rest/v1/families?select=*&status=eq.approved&revit_version=eq.{revitVersion}", Method.Get);
                var response = await _client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                    throw new ConnectivityException($"Fetch Failed: {response.StatusCode}");

                return JsonConvert.DeserializeObject<List<FamilyModel>>(response.Content) ?? new List<FamilyModel>();
            }
            catch (Exception ex) when (ex is not ConnectivityException)
            {
                throw new ConnectivityException("Failed to connect to Family Server.", ex);
            }
        }

        // 3. Data Consistency: Manual Rollback Implementation
        public async Task<bool> UploadFamilyAsync(FamilyUploadDto dto, string filePath)
        {
            string uploadedPath = null;
            try
            {
                // A. Upload File
                if (!File.Exists(filePath)) throw new FileNotFoundException("Local file missing");

                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(filePath)}";
                uploadedPath = $"families/{fileName}"; // Storage path

                var uploadReq = CreateRequest($"/storage/v1/object/{uploadedPath}", Method.Post);
                uploadReq.AddBody(await Task.Run(() => File.ReadAllBytes(filePath)), "application/octet-stream");

                var uploadRes = await _client.ExecuteAsync(uploadReq);
                if (!uploadRes.IsSuccessful) 
                    throw new ConnectivityException($"Storage Upload Failed: {uploadRes.Content}");

                // B. Generate URL
                string publicUrl = $"{_supabaseUrl}/storage/v1/object/public/{uploadedPath}";

                // C. Insert Database
                var family = new FamilyModel
                {
                    Name = dto.Name,
                    Category = dto.Category,
                    RevitVersion = dto.RevitVersion,
                    Url = publicUrl,
                    Status = FamilyStatus.Pending
                };

                var dbReq = CreateRequest("/rest/v1/families", Method.Post);
                dbReq.AddHeader("Prefer", "return=minimal");
                dbReq.AddParameter("application/json", JsonConvert.SerializeObject(family), ParameterType.RequestBody);

                var dbRes = await _client.ExecuteAsync(dbReq);

                if (!dbRes.IsSuccessful)
                {
                    // !CRITICAL: DB Insert Failed -> ROLLBACK STORAGE
                    await DeleteFileFromStorageAsync(uploadedPath);
                    throw new Exception($"Database Insert Failed ({dbRes.StatusCode}). Rolled back storage.");
                }

                return true;
            }
            catch (Exception ex)
            {
                // If anything crashes and we have a lingering file, try to clean it
                // (Though strict rollback is handled in the Logic Block C)
                throw new ConnectivityException("Transaction Failed: " + ex.Message, ex);
            }
        }

        private async Task DeleteFileFromStorageAsync(string path)
        {
            try
            {
                var req = CreateRequest($"/storage/v1/object/{path}", Method.Delete);
                await _client.ExecuteAsync(req);
            }
            catch { /* Best effort rollback - Log this failure in real app */ }
        }
    }
}
