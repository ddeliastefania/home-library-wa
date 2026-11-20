using HomeLibrary.API.Parsing;
using System.Text;

namespace HomeLibrary.Tests;

public class CsvBookParserTests
{
    private readonly CsvBookParser _parser = new();

    [Fact]
    public async Task ParseAsync_ValidCsv_ReturnsBooks()
    {
        using var stream = CreateStream(
            "name,author,genre\n" +
            "Clean Code,Robert Martin,Programming\n" +
            "Dune,Frank Herbert,Sci-Fi");

        var books = await _parser.ParseAsync(
            stream, CancellationToken.None);

        Assert.Equal(2, books.Count);
        Assert.Equal("Clean Code", books[0].Name);
        Assert.Equal("Robert Martin", books[0].Author);
        Assert.Equal("Programming", books[0].Genre);
        Assert.Equal("Dune", books[1].Name);
    }

    [Fact]
    public async Task ParseAsync_InvalidRows_SkipsThem()
    {
        using var stream = CreateStream(
            "name,author,genre\n" +
            "Clean Code,Robert Martin,Programming\n" +
            "Missing Author,,Fiction\n" +
            "Incomplete,Author\n" +
            "Dune,Frank Herbert,Sci-Fi");

        var books = await _parser.ParseAsync(stream, CancellationToken.None);

        Assert.Equal(2, books.Count);
        Assert.Equal("Clean Code", books[0].Name);
        Assert.Equal("Dune", books[1].Name);
    }

    [Fact]
    public async Task ParseAsync_InvalidHeader_ThrowsInvalidDataException()
    {
        using var stream = CreateStream("title,writer,category\n" + "Dune,Frank Herbert,Sci-Fi");

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => _parser.ParseAsync(stream, CancellationToken.None));

        Assert.Equal("Expected CSV header: name,author,genre.", exception.Message);
    }

    [Fact]
    public async Task ParseAsync_EmptyFile_ThrowsInvalidDataException()
    {
        using var stream = CreateStream("");

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => _parser.ParseAsync(stream, CancellationToken.None));

        Assert.Equal("The CSV file is empty.", exception.Message);
    }


    [Fact]
    public async Task ParseAsync_UppercaseHeader_AcceptsCsv()
    {
        using var stream = CreateStream("NAME,AUTHOR,GENRE\nDune,Frank Herbert,Sci-Fi");

        var books = await _parser.ParseAsync(stream, CancellationToken.None);

        Assert.Single(books);
        Assert.Equal("Dune", books[0].Name);
    }

    [Fact]
    public async Task ParseAsync_HeaderWithBom_AcceptsCsv()
    {
        using var stream = CreateStream("\uFEFFname,author,genre\nDune,Frank Herbert,Sci-Fi");

        var books = await _parser.ParseAsync(stream, CancellationToken.None);

        Assert.Single(books);
    }

    [Fact]
    public async Task ParseAsync_ValuesWithSpaces_TrimsFields()
    {
        using var stream = CreateStream("name,author,genre\n  Dune  , Frank Herbert , Sci-Fi ");

        var books = await _parser.ParseAsync(stream, CancellationToken.None);

        Assert.Single(books);
        Assert.Equal("Dune", books[0].Name);
        Assert.Equal("Frank Herbert", books[0].Author);
        Assert.Equal("Sci-Fi", books[0].Genre);
    }

    [Fact]
    public async Task ParseAsync_BlankRows_SkipsThem()
    {
        using var stream = CreateStream("name,author,genre\n\nDune,Frank Herbert,Sci-Fi\n   \n");

        var books = await _parser.ParseAsync(stream, CancellationToken.None);

        Assert.Single(books);
        Assert.Equal("Dune", books[0].Name);
    }

    private static MemoryStream CreateStream(string content)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }
}
