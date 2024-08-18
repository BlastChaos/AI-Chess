using System.Net;
using System.Net.Mail;
using AI_Chess.Controllers;
using Coravel.Invocable;
using Coravel.Queuing.Interfaces;
using Microsoft.Extensions.Options;

namespace AI_Chess
{
    public class TournamentWorker : IInvocable, ICancellableTask
    {
        private readonly ILogger<TournamentWorker> _logger;
        private readonly NeuralConfig _neuralConfig;
        private readonly SmtpConfig _smtpConfig;
        private readonly Tournament _tournement;
        public CancellationToken Token { get; set; }

        public TournamentWorker(ILogger<TournamentWorker> logger, IOptions<NeuralConfig> neuralConfig, Tournament tournement, IOptions<SmtpConfig> smtpConfig)
        {
            _logger = logger;
            _neuralConfig = neuralConfig.Value;
            _smtpConfig = smtpConfig.Value;
            _tournement = tournement;
        }


        public async Task Invoke()
        {
            if (Token.IsCancellationRequested)
            {
                return;
            }
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.UtcNow);
            await _tournement.OpenTournament(_neuralConfig.TournamentLength, Token);
            _logger.LogInformation("End of the Tournament");
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