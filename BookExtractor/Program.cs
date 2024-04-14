using System.Text.Json;
using BookExtractor.Models;
using Microsoft.Extensions.Configuration;


namespace BookExtractor
{
    public class Program
    {
        // Main method to start the application asynchronously.
        public static async Task Main(string[] args)
        {
            try
            {
                await MainAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        // Asynchronous main method to perform book extraction tasks.
        public static async Task MainAsync(string[] args, HttpClient httpClient = null)
        {
            // Create a new HttpClient instance if not provided via xUnit mocking
            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }

            try
            {
                Console.WriteLine("Fetching books from the API...");
                List<Book> books = await FetchBooks(httpClient);
                Console.WriteLine($"Fetched {books.Count} books successfully.");

                Console.WriteLine("Filtering books...");
                List<Book> filteredBooks = FilterBooks(books);
                Console.WriteLine($"Filtered {filteredBooks.Count()} books.");

                Console.WriteLine("Grouping books...");
                var groupedBooks = GroupBooks(filteredBooks);
                Console.WriteLine($"{groupedBooks.Count()} groups made.");

                Console.WriteLine("Saving results to a file...");
                SaveResultToFile(groupedBooks);
                Console.WriteLine("Result has been saved to 'result.txt'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        // Extracts the data from an API into a list of Book objects
        public static async Task<List<Book>> FetchBooks(HttpClient httpClient)
        {
            // Build configuration
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Read API URL from configuration
            string apiUrl = config["ApiSettings:ApiUrl"];

            // Set the Accept header to indicate that JSON response is expected
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpResponseMessage response = null;
            try
            {
                // Send a GET request to the API URL and await the response
                response = await httpClient.GetAsync(apiUrl);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read the response content as JSON and deserialize it into a BookApiResponse object
                string json = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<BookApiResponse>(json);

                // Return the list of books from the response
                return responseObj.books;
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request exceptions
                throw new HttpRequestException($"Failed to fetch books. Status code: {(int)response.StatusCode} - {response.StatusCode}", ex);
            }
            catch (JsonException ex)
            {
                // Handle JSON deserialization exceptions
                throw new JsonException($"Failed to deserialize JSON response: {ex.Message}", ex);
            }

        }

        // Filters the list of books to include only those with states in New Jersey or Colorado, and parent_name is not null
        private static List<Book> FilterBooks(List<Book> books)
        {
            return books
                .Where(b => b.meta.states.Contains("NJ") || b.meta.states.Contains("CO"))
                .Where(b => !string.IsNullOrEmpty(b.parent_name))
                .OrderBy(b => b.parent_name)
                .ToList();
        }

        // Group the books by their parent name and create a dictionary where the key is the parent name
        // and the value is a list of books associated with that parent name.
        // Orders by parent name indirectly through the use of LINQ's GroupBy method.
        private static Dictionary<string, List<Book>> GroupBooks(List<Book> books)
        {
            return books
                .GroupBy(b => b.parent_name)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
        // Saves the result to result.txt
        private static void SaveResultToFile(Dictionary<string, List<Book>> groupedBooks)
        {
            // Open a file stream for writing the result
            using (StreamWriter writer = new StreamWriter("result.txt"))
            {
                // Iterate over each group of books
                foreach (var group in groupedBooks)
                {
                    // Write the parent name to the file
                    writer.WriteLine(group.Key);

                    // Iterate over each book in the group
                    foreach (var book in group.Value)
                    {
                        // Write the book's display name and associated states to the file
                        writer.WriteLine($"{book.display_name} {string.Join(", ", book.meta.states)}");
                    }
                    // Write a blank line to separate groups
                    writer.WriteLine();
                }
            }
        }
    }
}
