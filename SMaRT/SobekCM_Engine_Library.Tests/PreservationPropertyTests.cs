using System;
using System.IO;
using NUnit.Framework;
using SobekCM.Core.Configuration;
using SobekCM.Engine_Library.ApplicationState;
using SobekCM.Engine_Library.Database;
using SobekCM.Engine_Library.Settings;
using SobekCM_Resource_Database;

namespace SobekCM.Engine_Library.Tests
{
    /// <summary>
    /// Preservation Property Tests for the database connection string behavior.
    /// 
    /// **Validates: Requirements 3.1, 3.2, 3.3, 3.4**
    /// 
    /// These tests verify that existing correct behaviors are preserved after the fix.
    /// </summary>
    [TestFixture]
    public class PreservationPropertyTests
    {
        private string _originalEngineConnectionString;
        private string _originalItemDbConnectionString;
        
        // Expected default connection string from config file
        private const string ExpectedDefaultConnectionString = "data source=128.227.24.171,63574;initial catalog=sobekdb;integrated security=Yes;";

        [SetUp]
        public void SetUp()
        {
            _originalEngineConnectionString = Engine_Database.Connection_String;
            _originalItemDbConnectionString = SobekCM_Item_Database.Connection_String;
        }

        [TearDown]
        public void TearDown()
        {
            Engine_Database.Connection_String = _originalEngineConnectionString;
            SobekCM_Item_Database.Connection_String = _originalItemDbConnectionString;
        }

        /// <summary>
        /// Property 2a: Live Database Selection Preserved
        /// 
        /// **Validates: Requirements 3.1, 3.2**
        /// 
        /// Verifies that when the live database is selected (default),
        /// RefreshSettings() preserves that selection.
        /// </summary>
        [Test]
        public void RefreshSettings_WithLiveDatabase_ShouldPreserveLiveConnection()
        {
            // Arrange: Set connection string to the live/default database
            Engine_Database.Connection_String = ExpectedDefaultConnectionString;
            SobekCM_Item_Database.Connection_String = ExpectedDefaultConnectionString;

            // Act: Call RefreshSettings()
            try
            {
                Engine_ApplicationCache_Gateway.RefreshSettings();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"RefreshSettings threw exception (expected if DB unavailable): {ex.Message}");
            }

            // Assert: Connection strings should still be the live/default value
            Assert.AreEqual(ExpectedDefaultConnectionString, Engine_Database.Connection_String,
                $"PRESERVATION FAILURE: Engine_Database.Connection_String should remain as live database.\n" +
                $"Expected: '{ExpectedDefaultConnectionString}'\n" +
                $"Actual: '{Engine_Database.Connection_String}'");

            Assert.AreEqual(ExpectedDefaultConnectionString, SobekCM_Item_Database.Connection_String,
                $"PRESERVATION FAILURE: SobekCM_Item_Database.Connection_String should remain as live database.\n" +
                $"Expected: '{ExpectedDefaultConnectionString}'\n" +
                $"Actual: '{SobekCM_Item_Database.Connection_String}'");
        }

        /// <summary>
        /// Property 2b: Explicit Config Behavior
        /// 
        /// **Validates: Requirements 3.3**
        /// 
        /// Verifies that Build_Settings(Database_Instance_Configuration) uses the provided connection string.
        /// </summary>
        [Test]
        public void BuildSettings_WithExplicitConfig_ShouldUseProvidedConnectionString()
        {
            // Arrange: Create an explicit database configuration
            const string explicitConnectionString = "Server=ExplicitTestServer;Database=ExplicitTestDB;Integrated Security=true;";
            
            var dbConfig = new Database_Instance_Configuration
            {
                Connection_String = explicitConnectionString
            };

            Engine_Database.Connection_String = null;
            SobekCM_Item_Database.Connection_String = null;

            // Act: Call Build_Settings() with explicit database configuration
            try
            {
                InstanceWide_Settings_Builder.Build_Settings(dbConfig);
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Build_Settings threw exception (expected if DB unavailable): {ex.Message}");
            }

            // Assert: Connection string should be set to the explicit config value
            Assert.AreEqual(explicitConnectionString, Engine_Database.Connection_String,
                $"PRESERVATION FAILURE: Engine_Database.Connection_String should be set to explicit config value.\n" +
                $"Expected: '{explicitConnectionString}'\n" +
                $"Actual: '{Engine_Database.Connection_String}'");
        }
    }
}
