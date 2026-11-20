
using HomeLibrary.API.Configuration;
using HomeLibrary.API.Interfaces;
using HomeLibrary.Contracts.Messages;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;

namespace HomeLibrary.API.Services;

public class RabbitMqPublisher : IBookMessagePublisher
{
    private const string QueueName = "book-imports";
    private readonly RabbitMqOptions _options;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task PublishAsync(IEnumerable<BookImportMessage> books, CancellationToken cancellationToken)
    {
        var factory = CreateConnectionFactory();

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await DeclareQueueAsync(channel, cancellationToken);

        await PublishBooksAsync(channel, books, cancellationToken);
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        return new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };
    }

    private static async Task DeclareQueueAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private static async Task PublishBooksAsync(IChannel channel, IEnumerable<BookImportMessage> books, CancellationToken cancellationToken)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        foreach (var book in books)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(book);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: QueueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
