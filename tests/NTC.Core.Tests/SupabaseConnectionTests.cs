using System.Threading.Tasks;
using NUnit.Framework;
using NTC.Core.Services;
using NTC.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace NTC.Core.Tests
{
    [TestFixture]
    public class SupabaseConnectionTests
    {
        private TestConfig _config;

        [SetUp]
        public void Setup()
        {
            // 1. Load Secrets
            _config = TestHelpers.LoadSecrets();

            // 2. Initialize Service (Singleton)
            // Note: Since Instance is static lazy, InitializeAsync usually sets internal state.
            // We call it here before every test to ensure config is loaded.
            SupabaseService.Instance.InitializeAsync(_config.SupabaseUrl, _config.SupabaseKey).Wait();
        }

        [Test]
        public void Can_Connect_And_Auth()
        {
            Assert.IsTrue(SupabaseService.Instance.IsInitialized, "Supabase Service should be initialized.");
        }

        [Test]
        public async Task Can_Get_Approved_Families()
        {
            // Act
            // Attempt to get families for a common version (e.g., 2025)
            // Even if empty, it should return a list and not throw exception
            List<FamilyModel> families = await SupabaseService.Instance.GetApprovedFamiliesAsync(2025);

            // Assert
            Assert.IsNotNull(families, "Results should not be null.");
            
            // If you have seeded data, you can assert count > 0
            // Assert.Greater(families.Count, 0); 
            
            TestContext.WriteLine($"Successfully retrieved {families.Count} families.");
        }
    }
}
