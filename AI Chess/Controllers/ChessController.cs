using Chess;
using Microsoft.AspNetCore.Mvc;

namespace AI_Chess.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChessController : ControllerBase
    {
        private readonly ILogger<ChessController> _logger;
        private readonly NeuralNetwork _neuralNetwork;

        public ChessController(ILogger<ChessController> logger, NeuralNetwork neuralNetwork)
        {
            _logger = logger;
            _neuralNetwork = neuralNetwork;
        }

        [HttpGet("move")]
        public async Task<ActionResult<string>> GetBestMove(string fen, CancellationToken stoppingToken, int opponentElo = 1000)
        {
            var chessBoard = ChessBoard.LoadFromFen(fen);

            _logger.LogInformation("Someone want a new move from this position {position}", chessBoard.ToAscii());
            var move = await Helper.GetBestMove(chessBoard, opponentElo, _neuralNetwork, stoppingToken);
            return this.Ok(move.San);
        }
    }
}