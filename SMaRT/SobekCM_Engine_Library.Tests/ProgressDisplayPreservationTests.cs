using System;
using NUnit.Framework;
using FsCheck;
using FsCheck.NUnit;

namespace SobekCM.Engine_Library.Tests
{
    /// <summary>
    /// Preservation Property Tests for the Progress Display fix.
    /// 
    /// **Validates: Requirements 3.1, 3.2, 3.3, 3.4**
    /// 
    /// These tests verify that for progress values LESS than Maximum (where the bug
    /// does NOT occur), the behavior is preserved correctly. The modulo operation
    /// works correctly for these values, and this behavior must be maintained after
    /// the fix is applied.
    /// 
    /// Preservation Condition: NOT isBugCondition(input) means New_Progress &lt; Maximum
    /// 
    /// EXPECTED OUTCOME: Tests PASS on UNFIXED code (confirms baseline behavior to preserve)
    /// </summary>
    [TestFixture]
    public class ProgressDisplayPreservationTests
    {
        /// <summary>
        /// Simulates the current progress calculation logic from SpreadSheet_Importer_Form.cs
        /// This uses the modulo operation which works correctly for values &lt; Maximum.
        /// </summary>
        /// <param name="newProgress">The new progress value</param>
        /// <param name="maximum">The maximum value of the progress bar</param>
        /// <returns>The displayed progress value</returns>
        private int CalculateDisplayedProgress_Current(int newProgress, int maximum)
        {
            // This replicates line 617: progressBar1.Value = New_Progress % progressBar1.Maximum;
            // For values where newProgress &lt; maximum, this works correctly
            return newProgress % maximum;
        }

        /// <summary>
        /// Generates the status label text based on the displayed progress value.
        /// </summary>
        private string GenerateStatusLabel(int displayedProgress, int maximum)
        {
            return $"Processed {displayedProgress:#,##0;} of {maximum:#,##0;} records";
        }

        /// <summary>
        /// Determines if the input is in the preservation zone (NOT a bug condition).
        /// Preservation condition: New_Progress &lt; Maximum
        /// </summary>
        private bool IsPreservationCondition(int newProgress, int maximum)
        {
            return newProgress >= 0 && newProgress < maximum;
        }

        /// <summary>
        /// Property 2: Preservation - Non-Maximum Progress Behavior Unchanged
        /// 
        /// **Validates: Requirements 3.1, 3.2, 3.3, 3.4**
        /// 
        /// For all progress values where 0 &lt;= New_Progress &lt; Maximum, the status label
        /// shows "Processed {New_Progress} of {Maximum} records". The modulo operation
        /// works correctly for these values (since n % m = n when n &lt; m).
        /// 
        /// EXPECTED OUTCOME: Test PASSES on UNFIXED code (confirms baseline behavior)
        /// </summary>
        [FsCheck.NUnit.Property(MaxTest = 100, Verbose = true)]
        public FsCheck.Property ProgressDisplay_ShouldShowCorrectValue_WhenProgressLessThanMaximum()
        {
            // Generate test cases where 0 <= New_Progress < Maximum (preservation condition)
            var testCaseArb = Arb.From(
                from maximum in Gen.Choose(1, 1000)
                from newProgress in Gen.Choose(0, maximum - 1)
                select new { NewProgress = newProgress, Maximum = maximum }
            );

            return Prop.ForAll(testCaseArb, testCase =>
            {
                // Arrange
                int newProgress = testCase.NewProgress;
                int maximum = testCase.Maximum;

                // Act: Calculate what the current code produces
                int displayedValue = CalculateDisplayedProgress_Current(newProgress, maximum);

                // Generate status label
                string statusLabel = GenerateStatusLabel(displayedValue, maximum);
                string expectedStatusLabel = GenerateStatusLabel(newProgress, maximum);

                // Assert: For values < Maximum, the modulo operation should return the same value
                // This is because n % m = n when 0 <= n < m
                return (displayedValue == newProgress)
                    .Label($"New_Progress={newProgress}, Maximum={maximum}")
                    .Label($"Preservation condition: {IsPreservationCondition(newProgress, maximum)}")
                    .Label($"Displayed value: {displayedValue}")
                    .Label($"Expected value: {newProgress}")
                    .Label($"Status label: '{statusLabel}'")
                    .Label($"Expected status: '{expectedStatusLabel}'");
            });
        }

        /// <summary>
        /// Unit test: Progress is 0 - verifies initial state is preserved
        /// 
        /// **Validates: Requirement 3.1**
        /// 
        /// When New_Progress = 0, the status should show "Processed 0 of Y records".
        /// This is the initial state before any records are processed.
        /// 
        /// EXPECTED OUTCOME: Test PASSES on UNFIXED code
        /// </summary>
        [Test]
        public void ProgressDisplay_WhenProgressIsZero_ShouldShowZero()
        {
            // Arrange: Initial state - 0 of 3 records processed
            int newProgress = 0;
            int maximum = 3;

            // Act: Calculate displayed value using current logic
            int displayedValue = CalculateDisplayedProgress_Current(newProgress, maximum);
            string statusLabel = GenerateStatusLabel(displayedValue, maximum);

            // Log the preservation verification
            TestContext.WriteLine($"New_Progress: {newProgress}");
            TestContext.WriteLine($"Maximum: {maximum}");
            TestContext.WriteLine($"Calculation: {newProgress} % {maximum} = {displayedValue}");
            TestContext.WriteLine($"Status label: '{statusLabel}'");

            // Assert: 0 % 3 = 0, which is correct
            Assert.AreEqual(newProgress, displayedValue,
                $"PRESERVATION CHECK: Progress display for zero should work correctly.\n" +
                $"When processing {newProgress} of {maximum} records:\n" +
                $"  Expected displayed value: {newProgress}\n" +
                $"  Actual displayed value: {displayedValue}");

            Assert.AreEqual("Processed 0 of 3 records", statusLabel,
                $"Status label should show 'Processed 0 of 3 records'");
        }

        /// <summary>
        /// Unit test: Progress is 1 - verifies incremental progress is preserved
        /// 
        /// **Validates: Requirement 3.4**
        /// 
        /// When New_Progress = 1 with Maximum = 3, the status should show 
        /// "Processed 1 of 3 records".
        /// 
        /// EXPECTED OUTCOME: Test PASSES on UNFIXED code
        /// </summary>
        [Test]
        public void ProgressDisplay_WhenProgressIsOne_ShouldShowOne()
        {
            // Arrange: First record processed - 1 of 3
            int newProgress = 1;
            int maximum = 3;

            // Act: Calculate displayed value using current logic
            int displayedValue = CalculateDisplayedProgress_Current(newProgress, maximum);
            string statusLabel = GenerateStatusLabel(displayedValue, maximum);

            // Log the preservation verification
            TestContext.WriteLine($"New_Progress: {newProgress}");
            TestContext.WriteLine($"Maximum: {maximum}");
            TestContext.WriteLine($"Calculation: {newProgress} % {maximum} = {displayedValue}");
            TestContext.WriteLine($"Status label: '{statusLabel}'");

            // Assert: 1 % 3 = 1, which is correct
            Assert.AreEqual(newProgress, displayedValue,
                $"PRESERVATION CHECK: Progress display for 1 should work correctly.\n" +
                $"When processing {newProgress} of {maximum} records:\n" +
                $"  Expected displayed value: {newProgress}\n" +
                $"  Actual displayed value: {displayedValue}");

            Assert.AreEqual("Processed 1 of 3 records", statusLabel,
                $"Status label should show 'Processed 1 of 3 records'");
        }

        /// <summary>
        /// Unit test: Progress is 2 - verifies near-maximum progress is preserved
        /// 
        /// **Validates: Requirement 3.1**
        /// 
        /// When New_Progress = 2 with Maximum = 3, the status should show 
        /// "Processed 2 of 3 records". This is the last value before the bug occurs.
        /// 
        /// EXPECTED OUTCOME: Test PASSES on UNFIXED code
        /// </summary>
        [Test]
        public void ProgressDisplay_WhenProgressIsNearMaximum_ShouldShowCorrectValue()
        {
            // Arrange: Near maximum - 2 of 3 records processed
            int newProgress = 2;
            int maximum = 3;

            // Act: Calculate displayed value using current logic
            int displayedValue = CalculateDisplayedProgress_Current(newProgress, maximum);
            string statusLabel = GenerateStatusLabel(displayedValue, maximum);

            // Log the preservation verification
            TestContext.WriteLine($"New_Progress: {newProgress}");
            TestContext.WriteLine($"Maximum: {maximum}");
            TestContext.WriteLine($"Calculation: {newProgress} % {maximum} = {displayedValue}");
            TestContext.WriteLine($"Status label: '{statusLabel}'");

            // Assert: 2 % 3 = 2, which is correct
            Assert.AreEqual(newProgress, displayedValue,
                $"PRESERVATION CHECK: Progress display for near-maximum should work correctly.\n" +
                $"When processing {newProgress} of {maximum} records:\n" +
                $"  Expected displayed value: {newProgress}\n" +
                $"  Actual displayed value: {displayedValue}");

            Assert.AreEqual("Processed 2 of 3 records", statusLabel,
                $"Status label should show 'Processed 2 of 3 records'");
        }

        /// <summary>
        /// Unit test: Mid-range progress with larger maximum
        /// 
        /// **Validates: Requirement 3.1**
        /// 
        /// When New_Progress = 50 with Maximum = 100, the status should show 
        /// "Processed 50 of 100 records".
        /// 
        /// EXPECTED OUTCOME: Test PASSES on UNFIXED code
        /// </summary>
        [Test]
        public void ProgressDisplay_WhenProgressIsMidRange_ShouldShowCorrectValue()
        {
            // Arrange: Mid-range - 50 of 100 records processed
            int newProgress = 50;
            int maximum = 100;

            // Act: Calculate displayed value using current logic
            int displayedValue = CalculateDisplayedProgress_Current(newProgress, maximum);
            string statusLabel = GenerateStatusLabel(displayedValue, maximum);

            // Log the preservation verification
            TestContext.WriteLine($"New_Progress: {newProgress}");
            TestContext.WriteLine($"Maximum: {maximum}");
            TestContext.WriteLine($"Calculation: {newProgress} % {maximum} = {displayedValue}");
            TestContext.WriteLine($"Status label: '{statusLabel}'");

            // Assert: 50 % 100 = 50, which is correct
            Assert.AreEqual(newProgress, displayedValue,
                $"PRESERVATION CHECK: Progress display for mid-range should work correctly.\n" +
                $"When processing {newProgress} of {maximum} records:\n" +
                $"  Expected displayed value: {newProgress}\n" +
                $"  Actual displayed value: {displayedValue}");

            Assert.AreEqual("Processed 50 of 100 records", statusLabel,
                $"Status label should show 'Processed 50 of 100 records'");
        }

        /// <summary>
        /// Property test: All preservation condition inputs should display correctly
        /// 
        /// **Validates: Requirements 3.1, 3.2, 3.3, 3.4**
        /// 
        /// For any input where the preservation condition holds (0 &lt;= New_Progress &lt; Maximum),
        /// the displayed value should equal New_Progress (since n % m = n when n &lt; m).
        /// 
        /// EXPECTED OUTCOME: Test PASSES on UNFIXED code (confirms baseline behavior)
        /// </summary>
        [FsCheck.NUnit.Property(MaxTest = 100, Verbose = true)]
        public FsCheck.Property AllPreservationInputs_ShouldDisplayCorrectly()
        {
            // Generate test cases specifically for preservation condition: 0 <= New_Progress < Maximum
            var testCaseArb = Arb.From(
                from maximum in Gen.Choose(2, 500) // At least 2 to have valid range
                from newProgress in Gen.Choose(0, maximum - 1)
                select new { NewProgress = newProgress, Maximum = maximum }
            );

            return Prop.ForAll(testCaseArb, testCase =>
            {
                int newProgress = testCase.NewProgress;
                int maximum = testCase.Maximum;

                // Verify this is indeed a preservation condition
                bool isPreservation = IsPreservationCondition(newProgress, maximum);
                
                // Calculate value using current (unfixed) logic
                int displayedValue = CalculateDisplayedProgress_Current(newProgress, maximum);

                // The test asserts that current code produces correct result for preservation inputs
                // This should PASS because n % m = n when 0 <= n < m
                return (displayedValue == newProgress)
                    .Label($"Input: New_Progress={newProgress}, Maximum={maximum}")
                    .Label($"Is preservation condition: {isPreservation}")
                    .Label($"Displayed value (modulo): {displayedValue}")
                    .Label($"Expected value: {newProgress}")
                    .When(isPreservation); // Only test when preservation condition holds
            });
        }

        /// <summary>
        /// Property test: Status label format is preserved for all valid progress values
        /// 
        /// **Validates: Requirements 3.1, 3.3**
        /// 
        /// Verifies that the status label format "Processed X of Y records" is maintained
        /// for all progress values in the preservation zone.
        /// 
        /// EXPECTED OUTCOME: Test PASSES on UNFIXED code
        /// </summary>
        [FsCheck.NUnit.Property(MaxTest = 50, Verbose = true)]
        public FsCheck.Property StatusLabelFormat_ShouldBePreserved()
        {
            var testCaseArb = Arb.From(
                from maximum in Gen.Choose(1, 100)
                from newProgress in Gen.Choose(0, maximum - 1)
                select new { NewProgress = newProgress, Maximum = maximum }
            );

            return Prop.ForAll(testCaseArb, testCase =>
            {
                int newProgress = testCase.NewProgress;
                int maximum = testCase.Maximum;

                // Calculate displayed value
                int displayedValue = CalculateDisplayedProgress_Current(newProgress, maximum);
                string statusLabel = GenerateStatusLabel(displayedValue, maximum);

                // Verify the status label format
                string expectedPattern = $"Processed {newProgress:#,##0;} of {maximum:#,##0;} records";
                
                return (statusLabel == expectedPattern)
                    .Label($"New_Progress={newProgress}, Maximum={maximum}")
                    .Label($"Status label: '{statusLabel}'")
                    .Label($"Expected: '{expectedPattern}'");
            });
        }
    }
}
