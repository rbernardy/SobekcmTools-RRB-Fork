using System;
using System.IO;
using NUnit.Framework;
using FsCheck;
using FsCheck.NUnit;
using SobekCM.Engine_Library.ApplicationState;
using SobekCM.Engine_Library.Database;
using SobekCM.Engine_Library.Settings;
using SobekCM_Resource_Database;

namespace SobekCM.Engine_Library.Tests
{
    /// <summary>
    /// Bug Condition Exploration Tests for the database connection string reversion bug.
    /// 
    /// **Validates: Requirements 1.1, 1.2, 1.3**
    /// 
    /// These tests demonstrate that when Engine_Database.Connection_String is set to a 
    /// non-default value (e.g., test database connection string) and 
    /// Engine_ApplicationCache_Gateway.RefreshSettings() is called, the connection string 
    /// reverts to the default from config file (BUG).
    /// 
    /// CRITICAL: These tests are EXPECTED TO FAIL on unfixed code - failure confirms the bug exists.
    /// DO NOT attempt to fix the test or the code when it fails.
    /// </summary>
    [TestFixture]
    public class BugConditionExplorationTests
    {
        // Store original connection strings to restore after tests
        private string _originalEngineConnectionString;
        private string _originalItemDbConnectionString;
        
        // Path to the config file - we'll find it relative to the test assembly
        private string _configFilePath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Find the config file path - it should be in the Data folder relative to the solution root
            // The test assembly is in SobekCM_Engine_Library.Tests/bin/Debug
            string assemblyLocation = Path.GetDirectoryName(typeof(BugConditionExplorationTests).Assembly.Location);
            
            // Navigate up to solution root and then to Data folder
            string solutionRoot = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", ".."));
            _configFilePath = Path.Combine(solutionRoot, "Data", "sobekcm.config");
            
            TestContext.WriteLine($"Config file path: {_configFilePath}");
            TestContext.WriteLine($"Config file exists: {File.Exists(_configFilePath)}");
        }

        [SetUp]
        public void SetUp()
        {
            // Capture original connection strings before each test
            _originalEngineConnectionString = Engine_Database.Connection_String;
            _originalItemDbConnectionString = SobekCM_Item_Database.Connection_String;
        }

        [TearDown]
        public void TearDown()
        {
            // Restore original connection strings after each test
            Engine_Database.Connection_String = _originalEngineConnectionString;
            SobekCM_Item_Database.Connection_String = _originalItemDbConnectionString;
        }

        /// <summary>
        /// Property 1: Bug Condition - Database Connection Reverts After RefreshSettings
        /// 
        /// **Validates: Requirements 1.1, 1.2, 1.3**
        /// 
        /// This property test verifies that when a non-default connection string is set
        /// and Build_Settings() is called with a config file, the connection string should be PRESERVED.
        /// 
        /// On UNFIXED code, this test will FAIL because the connection string reverts
        /// to the default from the config file. This failure confirms the bug exists.
        /// 
        /// Scoped PBT Approach: Tests the concrete failing case where:
        /// 1. Connection string is set to a non-default value
        /// 2. Build_Settings(configFilePath) is called
        /// 3. Connection string should be preserved (but isn't on unfixed code)
        /// </summary>
        [FsCheck.NUnit.Property(MaxTest = 1, Verbose = true)]
        public FsCheck.Property ConnectionString_ShouldBePreserved_AfterBuildSettings()
        {
            // Skip if config file doesn't exist
            if (!File.Exists(_configFilePath))
            {
                TestContext.WriteLine($"Skipping test - config file not found at: {_configFilePath}");
                return true.ToProperty();
            }

            // Use a single test connection string
            var connectionStringArb = Arb.From(
                Gen.Constant("Server=TestServer;Database=TestDB;Integrated Security=true;")
            );

            return Prop.ForAll(connectionStringArb, testConnectionString =>
            {
                // Arrange: Set connection strings to a non-default test value
                Engine_Database.Connection_String = testConnectionString;
                SobekCM_Item_Database.Connection_String = testConnectionString;

                // Capture the connection string before calling Build_Settings
                string connectionBeforeRefresh = Engine_Database.Connection_String;

                // Act: Call RefreshSettings() - this is where the fix is applied
                // The RefreshSettings method calls Build_Settings and should preserve the connection string
                try
                {
                    Engine_ApplicationCache_Gateway.RefreshSettings();
                }
                catch (Exception ex)
                {
                    // Log but continue - we want to check if connection string changed
                    TestContext.WriteLine($"RefreshSettings threw exception: {ex.Message}");
                }

                // Assert: Connection strings should be preserved (this will FAIL on unfixed code)
                // The bug causes the connection string to revert to the default from config file
                bool engineConnectionPreserved = Engine_Database.Connection_String == testConnectionString;
                bool itemDbConnectionPreserved = SobekCM_Item_Database.Connection_String == testConnectionString;

                return (engineConnectionPreserved && itemDbConnectionPreserved)
                    .Label($"Engine_Database.Connection_String preserved: {engineConnectionPreserved}")
                    .And(itemDbConnectionPreserved)
                    .Label($"SobekCM_Item_Database.Connection_String preserved: {itemDbConnectionPreserved}")
                    .Label($"Expected: '{testConnectionString}'")
                    .Label($"Actual Engine_Database: '{Engine_Database.Connection_String}'")
                    .Label($"Actual SobekCM_Item_Database: '{SobekCM_Item_Database.Connection_String}'");
            });
        }

        /// <summary>
        /// Unit test version of the bug condition for clearer failure messages.
        /// 
        /// **Validates: Requirements 1.1, 1.2, 1.3**
        /// 
        /// This test demonstrates the specific bug scenario:
        /// 1. User selects test database (sets connection string to test DB)
        /// 2. Build_Settings() is called (e.g., during import spreadsheet operation via RefreshSettings)
        /// 3. Connection string reverts to live database (BUG)
        /// 
        /// EXPECTED OUTCOME: Test FAILS on unfixed code (proves bug exists)
        /// </summary>
        [Test]
        public void BuildSettings_WithNonDefaultConnectionString_ShouldPreserveConnectionString()
        {
            // Skip if config file doesn't exist
            if (!File.Exists(_configFilePath))
            {
                Assert.Inconclusive($"Config file not found at: {_configFilePath}");
                return;
            }

            // Arrange: Simulate user selecting a test database
            const string testDatabaseConnectionString = "Server=TestServer;Database=TestDB;Integrated Security=true;";
            
            Engine_Database.Connection_String = testDatabaseConnectionString;
            SobekCM_Item_Database.Connection_String = testDatabaseConnectionString;

            // Verify setup
            Assert.AreEqual(testDatabaseConnectionString, Engine_Database.Connection_String,
                "Setup failed: Engine_Database.Connection_String was not set correctly");
            Assert.AreEqual(testDatabaseConnectionString, SobekCM_Item_Database.Connection_String,
                "Setup failed: SobekCM_Item_Database.Connection_String was not set correctly");

            // Act: Call RefreshSettings() - this is where the fix is applied
            try
            {
                Engine_ApplicationCache_Gateway.RefreshSettings();
            }
            catch (Exception ex)
            {
                // Log but continue - we want to check if connection string changed
                TestContext.WriteLine($"RefreshSettings threw exception: {ex.Message}");
            }

            // Assert: Connection strings should be preserved
            // On UNFIXED code, these assertions will FAIL because the connection string
            // reverts to the default from the config file
            Assert.AreEqual(testDatabaseConnectionString, Engine_Database.Connection_String,
                $"BUG CONFIRMED: Engine_Database.Connection_String was overwritten!\n" +
                $"Expected (test DB): '{testDatabaseConnectionString}'\n" +
                $"Actual (reverted to default): '{Engine_Database.Connection_String}'");

            Assert.AreEqual(testDatabaseConnectionString, SobekCM_Item_Database.Connection_String,
                $"BUG CONFIRMED: SobekCM_Item_Database.Connection_String was overwritten!\n" +
                $"Expected (test DB): '{testDatabaseConnectionString}'\n" +
                $"Actual (reverted to default): '{SobekCM_Item_Database.Connection_String}'");
        }

        /// <summary>
        /// Test that verifies the bug condition with multiple Build_Settings calls.
        /// 
        /// **Validates: Requirements 1.1, 1.2, 1.3**
        /// 
        /// This test demonstrates that the bug persists across multiple Build_Settings() calls.
        /// </summary>
        [Test]
        public void BuildSettings_CalledMultipleTimes_ShouldPreserveConnectionString()
        {
            // Skip if config file doesn't exist
            if (!File.Exists(_configFilePath))
            {
                Assert.Inconclusive($"Config file not found at: {_configFilePath}");
                return;
            }

            // Arrange
            const string testConnectionString = "Data Source=test-server;Initial Catalog=TestDB;Integrated Security=True;";
            
            Engine_Database.Connection_String = testConnectionString;
            SobekCM_Item_Database.Connection_String = testConnectionString;

            // Act: Call RefreshSettings twice (simulating multiple operations)
            try
            {
                Engine_ApplicationCache_Gateway.RefreshSettings();
                Engine_ApplicationCache_Gateway.RefreshSettings();
            }
            catch (Exception)
            {
                // Expected if database is not available
            }

            // Assert: Connection strings should still be preserved after multiple calls
            Assert.AreEqual(testConnectionString, Engine_Database.Connection_String,
                $"BUG CONFIRMED: Engine_Database.Connection_String was overwritten after multiple RefreshSettings calls!\n" +
                $"Expected: '{testConnectionString}'\n" +
                $"Actual: '{Engine_Database.Connection_String}'");

            Assert.AreEqual(testConnectionString, SobekCM_Item_Database.Connection_String,
                $"BUG CONFIRMED: SobekCM_Item_Database.Connection_String was overwritten after multiple RefreshSettings calls!\n" +
                $"Expected: '{testConnectionString}'\n" +
                $"Actual: '{SobekCM_Item_Database.Connection_String}'");
        }
    }
}
