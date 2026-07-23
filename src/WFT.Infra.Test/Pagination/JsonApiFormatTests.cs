using Xunit;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using WFT.Infra.Application.Contracts.Models;

namespace WFT.Infra.Test.Pagination
{
    /// <summary>
    /// Tests to verify the standardized JSON:API format with camelCase
    /// Ensures responses follow: { success, data: [], meta: { page, pageSize, totalItems, totalPages } }
    /// </summary>
    public class JsonApiFormatTests
    {
        private readonly JsonSerializerOptions _camelCaseOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        [Fact]
        public void PagedResponse_Should_Serialize_To_CamelCase()
        {
            // Arrange
            var items = new List<string> { "Item1", "Item2", "Item3" };
            var response = .Ok(items, 1, 10, 25);

            // Act
            var json = JsonSerializer.Serialize(response, _camelCaseOptions);
            var deserialized = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert - Verify camelCase property names
            Assert.True(deserialized.TryGetProperty("success", out var success));
            Assert.True(success.GetBoolean());

            Assert.True(deserialized.TryGetProperty("data", out var data));
            Assert.Equal(JsonValueKind.Array, data.ValueKind);
            Assert.Equal(3, data.GetArrayLength());

            Assert.True(deserialized.TryGetProperty("meta", out var meta));
            Assert.True(meta.TryGetProperty("page", out var page));
            Assert.Equal(1, page.GetInt32());
            Assert.True(meta.TryGetProperty("pageSize", out var pageSize));
            Assert.Equal(10, pageSize.GetInt32());
            Assert.True(meta.TryGetProperty("totalItems", out var totalItems));
            Assert.Equal(25, totalItems.GetInt32());
            Assert.True(meta.TryGetProperty("totalPages", out var totalPages));
            Assert.Equal(3, totalPages.GetInt32());

            Assert.True(deserialized.TryGetProperty("message", out _));
            Assert.True(deserialized.TryGetProperty("error", out _));
        }

        [Fact]
        public void PagedResponse_Should_Have_Flat_Structure_Not_Nested()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3 };
            var response = PagedResponse<int>.Ok(items, 1, 10, 100);

            // Act
            var json = JsonSerializer.Serialize(response, _camelCaseOptions);
            var deserialized = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert - data should be array directly, not nested in another object
            Assert.True(deserialized.TryGetProperty("data", out var data));
            Assert.Equal(JsonValueKind.Array, data.ValueKind); // Should be array, not object

            // meta should be separate object
            Assert.True(deserialized.TryGetProperty("meta", out var meta));
            Assert.Equal(JsonValueKind.Object, meta.ValueKind);
        }

        [Fact]
        public void Meta_Should_Serialize_To_CamelCase()
        {
            // Arrange
            var meta = new Meta(1, 10, 100, 10);

            // Act
            var json = JsonSerializer.Serialize(meta, _camelCaseOptions);
            var deserialized = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert - All meta properties should be camelCase
            Assert.True(deserialized.TryGetProperty("page", out var page));
            Assert.Equal(1, page.GetInt32());

            Assert.True(deserialized.TryGetProperty("pageSize", out var pageSize));
            Assert.Equal(10, pageSize.GetInt32());

            Assert.True(deserialized.TryGetProperty("totalItems", out var totalItems));
            Assert.Equal(100, totalItems.GetInt32());

            Assert.True(deserialized.TryGetProperty("totalPages", out var totalPages));
            Assert.Equal(10, totalPages.GetInt32());
        }

        [Fact]
        public void PagedResponse_Should_Match_JSON_API_Convention()
        {
            // Arrange
            var users = new List<object>
            {
                new { Id = 1, Name = "User 1" },
                new { Id = 2, Name = "User 2" }
            };
            var response = PagedResponse<object>.Ok(users, 1, 10, 200, "Success");

            // Act
            var json = JsonSerializer.Serialize(response, _camelCaseOptions);

            // Assert - Should match the target format exactly
            // Expected: { "success": true, "data": [...], "meta": {...}, "message": "Success", "error": null }
            Assert.Contains("\"success\": true", json);
            Assert.Contains("\"data\":", json);
            Assert.Contains("\"meta\":", json);
            Assert.Contains("\"page\": 1", json);
            Assert.Contains("\"pageSize\": 10", json);
            Assert.Contains("\"totalItems\": 200", json);
            Assert.Contains("\"totalPages\": 20", json);
            Assert.Contains("\"message\": \"Success\"", json);
            Assert.Contains("\"error\": null", json);
        }

        [Fact]
        public void PagedResponse_Should_Calculate_TotalPages_Correctly()
        {
            // Arrange & Act
            var response1 = PagedResponse<int>.Ok(new List<int>(), 1, 10, 100);
            var response2 = PagedResponse<int>.Ok(new List<int>(), 1, 10, 95);
            var response3 = PagedResponse<int>.Ok(new List<int>(), 1, 10, 0);

            // Assert
            Assert.Equal(10, response1.Meta.TotalPages); // 100/10 = 10
            Assert.Equal(10, response2.Meta.TotalPages); // Math.Ceiling(95/10.0) = 10
            Assert.Equal(0, response3.Meta.TotalPages);  // 0/10 = 0
        }

        [Fact]
        public void PagedResponse_Error_Should_Have_Empty_Data_And_Meta()
        {
            // Arrange & Act
            var response = PagedResponse<string>.Fail("An error occurred", "Failed to fetch data");

            // Assert
            Assert.False(response.Success);
            Assert.Empty(response.Data);
            Assert.NotNull(response.Meta);
            Assert.Equal("An error occurred", response.Error);
            Assert.Equal("Failed to fetch data", response.Message);
        }

        [Fact]
        public void PagedResponse_Should_Have_Correct_Structure()
        {
            // Arrange
            var items = new List<string> { "A", "B", "C" };

            // Act
            var response = PagedResponse<string>.Ok(items, 2, 10, 25);

            // Assert - Verify structure
            Assert.True(response.Success);
            Assert.Equal(3, response.Data.Count());
            Assert.Equal(2, response.Meta.Page);
            Assert.Equal(10, response.Meta.PageSize);
            Assert.Equal(25, response.Meta.TotalItems);
            Assert.Equal(3, response.Meta.TotalPages); // Math.Ceiling(25/10.0) = 3
            Assert.Null(response.Error);
        }
    }
}



