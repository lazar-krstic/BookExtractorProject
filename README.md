# Book Extractor

## Description

Book Extractor is a C# application designed to extract and process book data from a remote API. (https://api.actionnetwork.com/web/v1/books)

It fetches book information from the API, filters out irrelevant books based on certain criteria, groups the filtered books, and saves the results to a file.

## Features

- Fetches book data from a remote API.
- Filters books based on specified criteria (e.g., location, parent name).
- Groups filtered books by parent name.
- Saves the grouped book data to a text file.

## Testing

Book Extractor includes a comprehensive suite of unit tests to ensure its functionality. The tests cover various aspects of the application, including fetching data from the API, filtering books, and grouping them. To run the tests, follow these steps:

1. Open the solution file (`BookExtractor.sln`) in Visual Studio or your preferred IDE.

2. Navigate to the `BookExtractorTests` project in the Solution Explorer.

3. Build the `Tests` project to ensure all dependencies are resolved.

4. Run the tests using the test runner provided by your IDE.

5. Review the test results to ensure all tests pass successfully.
