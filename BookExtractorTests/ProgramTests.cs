using BookExtractor;
using System.Net;
using System.Text.Json;

namespace BookExtractor.Tests
{
    public partial class ProgramTests
    {
        // Test to verify that MainAsync successfully fetches and filters books
        [Fact]
        public async Task MainAsync_SuccessfullyFetchesAndFiltersBooks()
        {
            // Arrange
            var expectedBooksCount = 2; // Assuming the filter condition matches 2 books
            var expectedGroupsCount = 2; // Assuming the filtered books result in 2 groups
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{ ""books"": [ { ""id"": 1, ""display_name"": ""Book1"", ""parent_name"": ""Parent1"", ""meta"": { ""states"": [ ""NJ"" ] } }, { ""id"": 2, ""display_name"": ""Book2"", ""parent_name"": ""Parent2"", ""meta"": { ""states"": [ ""CO"" ] } } ] }")
            };
            var mockHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHandler);

            // Act
            List<string> resultLines;
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);
                await Program.MainAsync([], httpClient);
                resultLines = new List<string>(writer.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            }

            // Assert
            Assert.Contains("Fetched 2 books successfully.", resultLines);
            Assert.Contains($"Filtered {expectedBooksCount} books.", resultLines);
            Assert.Contains($"{expectedGroupsCount} groups made.", resultLines);
            Assert.Contains("Result has been saved to 'result.txt'", resultLines);
            File.Delete("result.txt");
        }

        // Test to verify that MainAsync handles the scenario where no books match the filter condition
        [Fact]
        public async Task MainAsync_NoBooksMatchFilterCondition()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                // Only one book returned by the API, which doesn't match the filter condition
                Content = new StringContent(@"{ ""books"": [ { ""id"": 1, ""display_name"": ""Book1"", ""parent_name"": ""Parent1"", ""meta"": { ""states"": [ ""CA"" ] } } ] }") 
            };
            var mockHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHandler);

            // Act
            List<string> resultLines;
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);
                await Program.MainAsync([], httpClient);
                resultLines = new List<string>(writer.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            }

            // Assert
            Assert.Contains("Fetched 1 books successfully.", resultLines);
            Assert.Contains("Filtered 0 books.", resultLines); // Ensure that filtering message indicates no books matched the condition
            Assert.DoesNotContain("groups made", resultLines); // Ensure that grouping message is not present
            Assert.Contains("Result has been saved to 'result.txt'", resultLines);
        }

        // Test to verify that MainAsync handles the scenario where no books are returned by the API
        [Fact]
        public async Task MainAsync_NoBooksReturnedByAPI()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{ ""books"": [] }") // Simulate an empty response from the API
            };
            var mockHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHandler);

            // Act
            List<string> resultLines;
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);
                await Program.MainAsync([], httpClient);
                resultLines = new List<string>(writer.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            }

            // Assert
            Assert.Contains("Fetched 0 books successfully.", resultLines);
            Assert.DoesNotContain("Filtered", resultLines); // Ensure that filtering message is not present
            Assert.DoesNotContain("groups made", resultLines); // Ensure that grouping message is not present
            Assert.Contains("Result has been saved to 'result.txt'", resultLines);
        }

        // Test to verify that FetchBooks method throws JsonException when the API response contains invalid JSON
        [Fact]
        public async Task FetchBooks_ThrowsJsonException()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Invalid JSON")
            };
            var mockHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHandler);

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(async () =>
            {
                await Program.FetchBooks(httpClient);
            });
        }

        // Test to verify that FetchBooks method handles API error response
        [Fact]
        public async Task FetchBooks_HandlesApiErrorResponse()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError); // Simulate API error response (status code 500)
            var mockHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHandler);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Program.FetchBooks(httpClient));

            // Assert
            Assert.Equal("Failed to fetch books. Status code: 500 - InternalServerError", exception.Message);
        }
    }
}
