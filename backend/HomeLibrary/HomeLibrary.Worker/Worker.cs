using HomeLibrary.Worker.Configuration;
using Microsoft.Extensions.Options;
using HomeLibrary.Worker.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HomeLibrary.Worker;

public class Worker : BackgroundService
{
    private const string QueueName = "book-imports";

    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly BookMessageProcessor _bookMessageProcessor;

    public Worker(ILogger<Worker> logger, IOptions<RabbitMqOptions> rabbitMqOptions, BookMessageProcessor bookMessageProcessor)
    {
        _logger = logger;
        _rabbitMqOptions = rabbitMqOptions.Value;
        _bookMessageProcessor = bookMessageProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = CreateConnectionFactory();

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await ConfigureChannelAsync(channel, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += (_, args) => HandleMessageAsync(channel, args, stoppingToken);

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Worker is listening for book imports.");

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker is stopping.");
        }
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        return new ConnectionFactory
        {
            HostName = _rabbitMqOptions.HostName,
            UserName = _rabbitMqOptions.UserName,
            Password = _rabbitMqOptions.Password
        };
    }

    private static async Task ConfigureChannelAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: cancellationToken);
    }

    private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var message = await _bookMessageProcessor.ProcessAsync(args.Body, cancellationToken);

            await channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Imported book: {Name} by {Author}",
                message.Name,
                message.Author);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Book processing cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process book message.");

            await channel.BasicNackAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false,
                requeue: false);
        }
    }
}
