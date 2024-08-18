using System.Net;
using System.Net.Mail;
using AI_Chess.Controllers;
using Microsoft.Extensions.Options;

namespace AI_Chess
{
    public class TournamentWorker : BackgroundService
    {
        private readonly ILogger<TournamentWorker> _logger;
        private readonly NeuralConfig _neuralConfig;
        private readonly SmtpConfig _smtpConfig;
        private readonly IServiceProvider _serviceProvider;

        public TournamentWorker(ILogger<TournamentWorker> logger, IOptions<NeuralConfig> neuralConfig, IServiceProvider serviceProvider, IOptions<SmtpConfig> smtpConfig)
        {
            _logger = logger;
            _neuralConfig = neuralConfig.Value;
            _smtpConfig = smtpConfig.Value;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // if (firstTime)
                // {
                //     if(!Directory.Exists(_neuralConfig.GamesOutputDirectory)){
                //         _logger.LogInformation("Downloading games");
                //         await chessController.DownloadGames();
                //     }
                //     firstTime = false;
                // }
                //SendEmail(); TODO: have the ability to send to database in a email
                using (var scope = _serviceProvider.CreateScope())
                {
                    var tournament = scope.ServiceProvider.GetRequiredService<Tournament>();
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.UtcNow);
                    await tournament.OpenTournament(300, stoppingToken);
                    _logger.LogInformation("End of the Tournament");
                }
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
}