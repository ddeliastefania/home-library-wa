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
- The frontend uses polling to refresh the book list after an import.
- EF Core migrations are applied automatically when the Worker starts.