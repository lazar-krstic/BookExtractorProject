namespace BookExtractor.Tests
{
    public partial class ProgramTests
    {
        // Helper class for mocking HTTP responses
        public class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            // Constructor that initializes the mock response
            public MockHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            // Override SendAsync method to return the mock response synchronously
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Return the mock response
                return Task.FromResult(_response);
            }
        }
    }
}
