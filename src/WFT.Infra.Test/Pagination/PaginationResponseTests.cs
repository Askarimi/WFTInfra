using Xunit;
using System.Collections.Generic;
using System.Linq;
using WFT.Infra.Application.Contracts.Models;

namespace WFT.Infra.Test.Pagination
{
    /// <summary>
    /// Tests to verify the standardized pagination response structure
    /// Ensures compatibility with frontend HttpService.getPaged() contract
    /// </summary>
    public class PaginationResponseTests
    {
        [Fact]
        public void PagedResult_Should_Have_Correct_Structure()
        {
            // Arrange
            var items = new List<string> { "Item1", "Item2", "Item3" };
            int totalCount = 25;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = new PagedResult<string>(items, totalCount, pageNumber, pageSize);

            // Assert - Verify all required properties exist
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(25, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(3, result.TotalPages); // Math.Ceiling(25 / 10.0) = 3
        }

        [Fact]
        public void PagedResult_Should_Calculate_TotalPages_Correctly()
        {
            // Test case 1: Exact division
            var result1 = new PagedResult<int>(new List<int>(), 100, 1, 10);
            Assert.Equal(10, result1.TotalPages); // 100 / 10 = 10

            // Test case 2: Requires rounding up
            var result2 = new PagedResult<int>(new List<int>(), 95, 1, 10);
            Assert.Equal(10, result2.TotalPages); // Math.Ceiling(95 / 10.0) = 10

            // Test case 3: Single page
            var result3 = new PagedResult<int>(new List<int>(), 5, 1, 10);
            Assert.Equal(1, result3.TotalPages); // Math.Ceiling(5 / 10.0) = 1

            // Test case 4: No items
            var result4 = new PagedResult<int>(new List<int>(), 0, 1, 10);
            Assert.Equal(0, result4.TotalPages); // Math.Ceiling(0 / 10.0) = 0
        }

        [Fact]
        public void PagedResult_Should_Handle_Null_Data()
        {
            // Arrange & Act
            var result = new PagedResult<string>((IEnumerable<string>)null!, 10, 1, 10);

            // Assert - Should create empty list instead of null
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public void PagedResult_Should_Convert_IEnumerable_To_List()
        {
            // Arrange
            IEnumerable<int> enumerable = Enumerable.Range(1, 5);

            // Act
            var result = new PagedResult<int>(enumerable, 50, 1, 10);

            // Assert
            Assert.NotNull(result.Data);
            Assert.IsType<List<int>>(result.Data);
            Assert.Equal(5, result.Data.Count);
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Data);
        }

        [Fact]
        public void PagedResult_Should_Use_PascalCase_Property_Names()
        {
            // Arrange
            var items = new List<string> { "test" };
            var result = new PagedResult<string>(items, 1, 1, 10);

            // Act - Use reflection to verify property names
            var properties = typeof(PagedResult<string>).GetProperties();
            var propertyNames = properties.Select(p => p.Name).ToList();

            // Assert - All properties should be PascalCase
            Assert.Contains("Data", propertyNames);
            Assert.Contains("TotalCount", propertyNames);
            Assert.Contains("PageNumber", propertyNames);
            Assert.Contains("PageSize", propertyNames);
            Assert.Contains("TotalPages", propertyNames);

            // Verify no lowercase variations exist
            Assert.DoesNotContain("data", propertyNames);
            Assert.DoesNotContain("totalCount", propertyNames);
            Assert.DoesNotContain("pageNumber", propertyNames);
            Assert.DoesNotContain("pageSize", propertyNames);
            Assert.DoesNotContain("totalPages", propertyNames);
        }

        [Theory]
        [InlineData(100, 10, 10)] // Exact division
        [InlineData(95, 10, 10)]  // Rounds up
        [InlineData(91, 10, 10)]  // Rounds up
        [InlineData(90, 10, 9)]   // Exact division
        [InlineData(1, 10, 1)]    // Single item
        [InlineData(0, 10, 0)]    // No items
        [InlineData(15, 5, 3)]    // Different page size
        [InlineData(7, 3, 3)]     // Small page size
        public void PagedResult_Should_Calculate_TotalPages_For_Various_Scenarios(int totalCount, int pageSize, int expectedTotalPages)
        {
            // Arrange & Act
            var result = new PagedResult<object>(new List<object>(), totalCount, 1, pageSize);

            // Assert
            Assert.Equal(expectedTotalPages, result.TotalPages);
        }

        [Fact]
        public void PagedResult_Should_Match_Frontend_Contract()
        {
            // Arrange - Simulate real-world scenario
            var users = new List<object>
            {
                new { Id = 1, Name = "User 1" },
                new { Id = 2, Name = "User 2" },
                new { Id = 3, Name = "User 3" }
            };

            // Act
            var result = new PagedResult<object>(users, 25, 2, 10);

            // Assert - Verify the structure matches frontend expectations
            // When serialized, this should produce:
            // {
            //   "Data": [...],
            //   "TotalCount": 25,
            //   "PageNumber": 2,
            //   "PageSize": 10,
            //   "TotalPages": 3
            // }
            
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(25, result.TotalCount);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(3, result.TotalPages);
        }

        [Fact]
        public void PagedResult_Should_Handle_Edge_Case_Zero_PageSize()
        {
            // Arrange & Act
            var result = new PagedResult<int>(new List<int>(), 100, 1, 0);

            // Assert - Should not divide by zero
            Assert.Equal(0, result.TotalPages);
        }
    }
}

