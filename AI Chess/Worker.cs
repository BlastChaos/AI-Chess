using AI_Chess.Controllers;
using Microsoft.Extensions.Options;

namespace AI_Chess;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ChessController _chessController;
    private readonly IOptions<NeuralConfig> _neuralConfig;

    public Worker(ILogger<Worker> logger, ChessController chessController, IOptions<NeuralConfig> neuralConfig)
    {
        _logger = logger;
        _chessController = chessController;
        _neuralConfig = neuralConfig;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var firstTime = true;
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            if (firstTime)
            {
                 _logger.LogInformation("Downloading games");
                await _chessController.DownloadGames();
                firstTime = false;
            }
            _logger.LogInformation("Training start with {iterations} iterations", _chessController);
            _chessController.Train(_neuralConfig.Value.BackgroundIterations);
            _logger.LogInformation("Training ending");
            _chessController.Save();
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}