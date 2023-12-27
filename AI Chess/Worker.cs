using System.Net;
using System.Net.Mail;
using AI_Chess.Controllers;
using Microsoft.Extensions.Options;

namespace AI_Chess;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ChessController _chessController;
    private readonly NeuralConfig _neuralConfig;
    private readonly SmtpConfig _smtpConfig;

    public Worker(ILogger<Worker> logger, ChessController chessController, IOptions<NeuralConfig> neuralConfig , IOptions<SmtpConfig> smtpConfig)
    {
        _logger = logger;
        _chessController = chessController;
        _neuralConfig = neuralConfig.Value;
        _smtpConfig = smtpConfig.Value;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var firstTime = true;

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.UtcNow);
            if (firstTime)
            {
                if(File.Exists(_neuralConfig.BrainFileName)){
                    _logger.LogInformation("Loading neural network");
                    _chessController.Load();
                }
                if(!Directory.Exists(_neuralConfig.GamesOutputDirectory)){
                    _logger.LogInformation("Downloading games");
                    await _chessController.DownloadGames();
                }
                firstTime = false;
            }
            _logger.LogInformation("Training start with {iterations} iterations", _neuralConfig.BackgroundIterations);
            _chessController.Train(_neuralConfig.BackgroundIterations);
            _logger.LogInformation("Training end");
            _chessController.Save();
            SendEmail();
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }

    private void SendEmail() {
        var smtpClient = new SmtpClient(_smtpConfig.Host)
        {
            Port = 587,
            UseDefaultCredentials = false,
            EnableSsl = true,
            Credentials = new NetworkCredential(_smtpConfig.From, _smtpConfig.Password)
        };
        var brain = new Attachment(_neuralConfig.BrainFileName);
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