using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NTC.Core.Models;

namespace NTC.Core.Tests
{
    [TestFixture]
    public class GovernanceComplianceTests
    {
        [Test]
        [Description("DAMA-DMBOK: Critical Data Elements must have Audit controls (Validation Attributes).")]
        public void FamilyModel_MustHave_GovernanceAttributes()
        {
            // 1. Inspect the Type
            var modelType = typeof(FamilyModel);
            var nameProp = modelType.GetProperty(nameof(FamilyModel.Name));
            var statusProp = modelType.GetProperty(nameof(FamilyModel.Status));

            // 2. Assert 'Name' has [Required]
            var requiredAttr = nameProp.GetCustomAttribute<RequiredAttribute>();
            Assert.IsNotNull(requiredAttr, "Violation: Family 'Name' field is missing [Required] attribute. Core Data Asset integrity is at risk.");

            // 3. Assert 'Status' is an Enum (Structured Data)
            Assert.IsTrue(statusProp.PropertyType.IsEnum, "Violation: Family 'Status' must be an Enum to enforce Data Standardization.");
        }

        [Test]
        [Description("Data Quality: Naming Conventions must be enforced before Ingestion.")]
        public void NamingConvention_MustBe_Enforced()
        {
            // Arrange
            var invalidFamily = new FamilyModel
            {
                Name = "", // Empty name (Violation)
                Category = "Doors", 
                RevitVersion = 2025,
                Url = "https://valid.url"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(invalidFamily);
            bool isValid = Validator.TryValidateObject(invalidFamily, context, validationResults, true);

            // Assert
            Assert.IsFalse(isValid, "Governance Failure: System accepted an Invalid Name (Empty).");
            
            // Check specific error message (Optional)
            bool hasNameError = validationResults.Any(r => r.MemberNames.Contains("Name"));
            Assert.IsTrue(hasNameError, "Governance Failure: No validation error reported for 'Name' field.");
        }
    }
}
