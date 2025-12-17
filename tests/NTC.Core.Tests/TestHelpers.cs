using System;
using System.IO;
using Newtonsoft.Json;

namespace NTC.Core.Tests
{
    public class TestConfig
    {
        public string SupabaseUrl { get; set; }
        public string SupabaseKey { get; set; }
    }

    public static class TestHelpers
    {
        public static TestConfig LoadSecrets()
        {
            // Traverse up to find the src folder where secrets.json typically lives during dev
            // Or look in the test execution directory if copied there
            
            // Try explicit path first (adjust if needed based on where user put it)
            string[] possiblePaths = new[]
            {
                "secrets.json",
                "../../../secrets.json",
                "../../../../src/NTC.Core/secrets.json",
                "../../../../secrets.json" 
            };

            foreach (var path in possiblePaths)
            {
                string fullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
                if (File.Exists(fullPath))
                {
                    var content = File.ReadAllText(fullPath);
                    return JsonConvert.DeserializeObject<TestConfig>(content);
                }
            }

            throw new FileNotFoundException("Ensure 'secrets.json' exists in the project root or src/NTC.Core/ folder to run Integration Tests.");
        }
    }
}
