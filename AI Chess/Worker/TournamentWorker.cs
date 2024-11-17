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

        private readonly HttpClient _httpClient;
        public CancellationToken Token { get; set; }

        public TournamentWorker(ILogger<TournamentWorker> logger, IOptions<NeuralConfig> neuralConfig, Tournament tournement, IHttpClientFactory factory)
        {
            _logger = logger;
            _neuralConfig = neuralConfig.Value;
            _tournement = tournement;
            _httpClient = factory.CreateClient(nameof(TournamentWorker));
        }


        public async Task Invoke()
        {
            if (Token.IsCancellationRequested)
            {
                return;
            }

            try
            {


                var directory = Path.Combine(Directory.GetCurrentDirectory(), _neuralConfig.GamesOutputDirectory);
                if (!Directory.Exists(directory))
                {
                    await Helper.DownloadGames(_neuralConfig, _httpClient, _logger);
                }

                _logger.LogInformation("Worker running at: {time} (uct time)", DateTimeOffset.UtcNow);
                await _tournement.OpenTournament(_neuralConfig.TournamentLength, Token);
                _logger.LogInformation("End of the Tournament");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in TournamentWorker");
            }


        }
    }
}