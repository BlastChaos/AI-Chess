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
        private readonly Tournament _tournement;
        public CancellationToken Token { get; set; }

        public TournamentWorker(ILogger<TournamentWorker> logger, IOptions<NeuralConfig> neuralConfig, Tournament tournement)
        {
            _logger = logger;
            _neuralConfig = neuralConfig.Value;
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
    }
}