using System.Net;
using System.Net.Mail;
using AI_Chess.Controllers;
using Microsoft.Extensions.Options;

namespace AI_Chess;

public class TournementWorker : BackgroundService
{
    private readonly ILogger<TournementWorker> _logger;
    private readonly NeuralConfig _neuralConfig;
    private readonly SmtpConfig _smtpConfig;
    private readonly IServiceProvider _serviceProvider;

    public TournementWorker(ILogger<TournementWorker> logger, IServiceProvider serviceProvider, IOptions<NeuralConfig> neuralConfig, IOptions<SmtpConfig> smtpConfig)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _neuralConfig = neuralConfig.Value;
        _smtpConfig = smtpConfig.Value;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var firstTime = true;

        while (!stoppingToken.IsCancellationRequested)
        {
            var service = _serviceProvider.CreateScope().ServiceProvider;
            var chessController = service.GetRequiredService<ChessController>();
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.UtcNow);
            // if (firstTime)
            // {
            //     if(!Directory.Exists(_neuralConfig.GamesOutputDirectory)){
            //         _logger.LogInformation("Downloading games");
            //         await chessController.DownloadGames();
            //     }
            //     firstTime = false;
            // }
            await chessController.Tournement(_neuralConfig.BackgroundIterations, stoppingToken);
            _logger.LogInformation("Training end");
            SendEmail();
            await Task.Delay(TimeSpan.FromHours(2.5), stoppingToken);
        }
    }

    private void SendEmail()
    {
        var smtpClient = new SmtpClient(_smtpConfig.Host)
        {
            Port = 587,
            UseDefaultCredentials = false,
            EnableSsl = true,
            Credentials = new NetworkCredential(_smtpConfig.From, _smtpConfig.Password)
        };
        var brain = new Attachment(_neuralConfig.BrainFileName);
        var brain2 = new Attachment($"{_neuralConfig.BrainFileName}-shm");
        var brain3 = new Attachment($"{_neuralConfig.BrainFileName}-wal");
        var message = new MailMessage
        {
            Subject = "AI Chess",
            Body = "Here is the current brain",
            From = new MailAddress(_smtpConfig.From)
        };
        foreach (var to in _smtpConfig.To)
        {
            message.To.Add(to);
        }
        message.Attachments.Add(brain);
        message.Attachments.Add(brain2);
        message.Attachments.Add(brain3);
        try
        {
            smtpClient.Send(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
        finally
        {
            brain.Dispose();
            message.Dispose();
            smtpClient.Dispose();
        }
    }
}