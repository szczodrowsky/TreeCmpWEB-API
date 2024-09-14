using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

public class RabbitMqWorker : BackgroundService
{
    private readonly string _hostname = "localhost";
    private readonly string _queueName = "treecmp_queue";
    private IConnection _connection;
    private IModel _channel;

    public RabbitMqWorker()
    {
        InitializeRabbitMqListener();
    }

    private void InitializeRabbitMqListener()
    {
        var factory = new ConnectionFactory() { HostName = _hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received command: {message}");

            try
            {
           
                var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles", "TreeCmp", "bin");

           
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {message}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = workingDirectory 
                    }
                };

                process.Start();
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                Console.WriteLine($"Command finished with exit code: {process.ExitCode}");
                Console.WriteLine($"Output: {output}");
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while executing command: {ex.Message}");
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
