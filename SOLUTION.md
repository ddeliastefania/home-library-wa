# Solution

## How to Run

**Prerequisite:** Docker Desktop with Docker Compose.

From the repository root, run:

```bash
docker compose up -d --build
```

Once the containers are ready, open **http://localhost:4200**.

The command starts PostgreSQL, RabbitMQ, the .NET API, the .NET Worker, and the Angular frontend.

To stop the application:

```bash
docker compose down
```

## Assumptions and Design Decisions

- The application uses .NET 10, Angular 22, PostgreSQL, and RabbitMQ.
- The API validates and parses uploaded CSV files, then publishes one RabbitMQ message per valid book.
- A separate Worker consumes the messages and saves books to PostgreSQL. The API does not insert books directly.
- Invalid CSV rows are skipped. Files must contain the `name,author,genre` header, and each valid row must have all three fields.
- CSV parsing supports simple comma-separated values; quoted fields containing commas are not supported.
- Book import dates are recorded in UTC.
- EF Core migrations are applied automatically when the Worker starts.
- The frontend supports CSV file selection and drag-and-drop upload, including visual drag feedback and rejection of non-CSV files.
- The frontend uses polling to refresh the book list after an import.
- The original `GET /api/books` endpoint returns books ordered by import date, newest first.
- An additional `GET /api/books/paged` endpoint provides pagination and deduplication for the frontend.
- Books are considered duplicates when their trimmed names, authors, and genres match case-insensitively. The most recently imported record is displayed.
- Deduplication applies only to the paginated view; existing database records are preserved.
- The Worker retries PostgreSQL migrations and RabbitMQ connections every 5 seconds when the services are temporarily unavailable during startup.
## Tests

The solution includes xUnit tests for CSV parsing, covering valid input, invalid rows, invalid headers, and empty files.

To run the tests:

```bash
cd backend/HomeLibrary
dotnet test HomeLibrary.Tests/HomeLibrary.Tests.csproj
```