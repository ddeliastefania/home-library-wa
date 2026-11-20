namespace HomeLibrary.Contracts.DTOs;

public record BookDto(
    int Id,
    string Name,
    string Author,
    string Genre,
    DateTime ImportDate
);
