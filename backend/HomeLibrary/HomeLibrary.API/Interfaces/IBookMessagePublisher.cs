using HomeLibrary.Contracts.Messages;

namespace HomeLibrary.API.Interfaces;

public interface IBookMessagePublisher
{
    Task PublishAsync(IEnumerable<BookImportMessage> books, CancellationToken cancellationToken);
}
