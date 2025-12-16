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
        private static SupabaseService _instance;
        private static readonly object _lock = new object();
        private RestClient _client;
        private string _supabaseUrl;
        private string _supabaseKey;

        // Singleton Implementation
        public static SupabaseService Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SupabaseService();
                    }
                    return _instance;
                }
            }
        }

        private SupabaseService() { }

        public bool IsInitialized => _client != null;

        /// <summary>
        /// Initialize the service with credentials.
        /// Should be called at App Startup (e.g., loading from secrets.json).
        /// </summary>
        public Task InitializeAsync(string url, string key)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Supabase URL and Key cannot be empty.");

            _supabaseUrl = url;
            _supabaseKey = key;

            var options = new RestClientOptions(_supabaseUrl);
            _client = new RestClient(options);

            return Task.CompletedTask;
        }

        private RestRequest CreateRequest(string resource, Method method)
        {
            if (!IsInitialized) 
                throw new InvalidOperationException("SupabaseService is not initialized. Call InitializeAsync() first.");

            var request = new RestRequest(resource, method);
            request.AddHeader("apikey", _supabaseKey);
            request.AddHeader("Authorization", $"Bearer {_supabaseKey}");
            return request;
        }

        /// <summary>
        /// Get approved families for a specific Revit version data.
        /// </summary>
        public async Task<List<FamilyModel>> GetApprovedFamiliesAsync(int revitVersion)
        {
            try
            {
                // PostgREST: /families?select=*&status=eq.approved&revit_version=eq.{ver}
                var resource = $"/rest/v1/families?select=*&status=eq.approved&revit_version=eq.{revitVersion}";
                var request = CreateRequest(resource, Method.Get);

                var response = await _client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                    throw new SupabaseException($"Fetch Failed: {response.StatusCode} - {response.Content}");

                return JsonConvert.DeserializeObject<List<FamilyModel>>(response.Content) ?? new List<FamilyModel>();
            }
            catch (Exception ex) when (ex is not SupabaseException)
            {
                throw new SupabaseException("Connection error while fetching families.", ex);
            }
        }

        /// <summary>
        /// Upload transaction: Upload .rfa -> Get URL -> Insert DB
        /// </summary>
        public async Task<bool> UploadFamilyAsync(FamilyUploadDto dto, string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) 
                    throw new FileNotFoundException($"File not found: {filePath}");

                // 1. Upload File to Storage Bucket "families"
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(filePath)}";
                var uploadResource = $"/storage/v1/object/families/{fileName}";
                var uploadRequest = CreateRequest(uploadResource, Method.Post);
                
                byte[] fileBytes = File.ReadAllBytes(filePath);
                uploadRequest.AddBody(fileBytes, "application/octet-stream");

                var uploadResponse = await _client.ExecuteAsync(uploadRequest);
                if (!uploadResponse.IsSuccessful)
                    throw new SupabaseException($"Storage Upload Failed: {uploadResponse.Content}");

                // 2. Construct Public URL
                string publicUrl = $"{_supabaseUrl}/storage/v1/object/public/families/{fileName}";

                // 3. Insert Metadata
                var family = new FamilyModel
                {
                    Name = dto.Name,
                    Category = dto.Category,
                    RevitVersion = dto.RevitVersion,
                    Url = publicUrl, // Maps to 'file_url' in JSON
                    Status = FamilyStatus.Pending,
                    // Parameters would be mapped here if DTO had them
                };

                var dbRequest = CreateRequest("/rest/v1/families", Method.Post);
                // Prefer: return=minimal or representation. We just need to know if it success.
                dbRequest.AddHeader("Prefer", "return=minimal"); 
                dbRequest.AddParameter("application/json", JsonConvert.SerializeObject(family), ParameterType.RequestBody);

                var dbResponse = await _client.ExecuteAsync(dbRequest);
                if (!dbResponse.IsSuccessful)
                    throw new SupabaseException($"Database Insert Failed: {dbResponse.Content}");

                return true;
            }
            catch (Exception ex) when (ex is not SupabaseException)
            {
                throw new SupabaseException("Upload transaction failed.", ex);
            }
        }
    }
}
