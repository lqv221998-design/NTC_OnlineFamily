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
        /// Initialize the service with credentials from secrets.json or configuration.
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
                throw new InvalidOperationException("SupabaseService not initialized. Call InitializeAsync() first.");

            var request = new RestRequest(resource, method);
            request.AddHeader("apikey", _supabaseKey);
            request.AddHeader("Authorization", $"Bearer {_supabaseKey}");
            return request;
        }

        /// <summary>
        /// Get approved families for a specific Revit version.
        /// Implements Async-First and Resilience patterns.
        /// </summary>
        public async Task<List<FamilyModel>> GetApprovedFamiliesAsync(int revitVersion)
        {
            try
            {
                // PostgREST Query: /families?select=*&status=eq.approved&revit_version=eq.{ver}
                var resource = $"/rest/v1/families?select=*&status=eq.approved&revit_version=eq.{revitVersion}";
                var request = CreateRequest(resource, Method.Get);

                var response = await _client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                    throw new SupabaseException($"API Error ({response.StatusCode}): {response.Content}");

                return JsonConvert.DeserializeObject<List<FamilyModel>>(response.Content) ?? new List<FamilyModel>();
            }
            catch (Exception ex) when (ex is not SupabaseException)
            {
                // Wrap generic exceptions in domain-specific exception
                throw new SupabaseException("Connection Lost. Unable to retrieve families.", ex);
            }
        }

        /// <summary>
        /// Transactional Upload:
        /// 1. Upload to Storage
        /// 2. Get Public URL
        /// 3. Insert Metadata
        /// </summary>
        public async Task<bool> UploadFamilyAsync(FamilyUploadDto dto, string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Family file not found locally.", filePath);

                // 1. Upload to Storage Bucket 'families'
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(filePath)}"; // Prevent name collision
                var uploadResource = $"/storage/v1/object/families/{fileName}";
                var uploadRequest = CreateRequest(uploadResource, Method.Post);
                
                byte[] fileBytes = await Task.Run(() => File.ReadAllBytes(filePath)); // Offload IO to thread pool
                uploadRequest.AddBody(fileBytes, "application/octet-stream");

                var uploadResponse = await _client.ExecuteAsync(uploadRequest);
                if (!uploadResponse.IsSuccessful)
                    throw new SupabaseException($"Storage Upload Failed: {uploadResponse.Content}");

                // 2. Construct Public URL
                string publicUrl = $"{_supabaseUrl}/storage/v1/object/public/families/{fileName}";

                // 3. Insert Metadata into Database
                var family = new FamilyModel
                {
                    Name = dto.Name,
                    Category = dto.Category,
                    RevitVersion = dto.RevitVersion,
                    Url = publicUrl,
                    Status = FamilyStatus.Pending,
                    // Parameters would go here
                };

                var dbRequest = CreateRequest("/rest/v1/families", Method.Post);
                dbRequest.AddHeader("Prefer", "return=minimal"); // Return 201 Created if success
                dbRequest.AddParameter("application/json", JsonConvert.SerializeObject(family), ParameterType.RequestBody);

                var dbResponse = await _client.ExecuteAsync(dbRequest);
                if (!dbResponse.IsSuccessful)
                    throw new SupabaseException($"Metadata Insert Failed: {dbResponse.Content}");

                return true;
            }
            catch (Exception ex) when (ex is not SupabaseException)
            {
                throw new SupabaseException("Upload Failed. Check network connection.", ex);
            }
        }
    }
}
