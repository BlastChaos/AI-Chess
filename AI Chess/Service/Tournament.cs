using Chess;
using Microsoft.Extensions.Options;
namespace AI_Chess.Controllers
{
    public class Tournament
    {
        private readonly ILogger<Tournament> _logger;
        private readonly NeuralConfig _neuralConfig;
        private readonly NeuralTournement _neuralTournement;
        private readonly NeuralNetwork _neuralNetwork;


        /// <summary>
        /// This class is used to train 2 <c>NeuralNetwork</c> in a battle.
        /// The loser will train, the winner will not change.
        /// </summary>
        public Tournament(
        ILogger<Tournament> logger,
        IOptions<NeuralConfig> gameConfig,
        NeuralNetwork neuralNetwork,
        NeuralTournement neuralTournement)
        {
            _logger = logger;
            _neuralConfig = gameConfig.Value;
            _neuralNetwork = neuralNetwork;
            _neuralTournement = neuralTournement;
        }
        public async Task OpenTournament(int fightNumber, CancellationToken stoppingToken)
        {

            var neuralNetwork1 = _neuralTournement.NeuralNetwork1;
            var neuralNetwork2 = _neuralTournement.NeuralNetwork2;

            await neuralNetwork1.Export(_neuralNetwork, stoppingToken);
            await neuralNetwork2.Export(_neuralNetwork, stoppingToken);
            var neuralNetwork1Victory = 0;
            var neuralNetwork2Victory = 0;
            for (int i = 1; i < fightNumber + 1; i++)
            {
                var chessGame = new ChessBoard();
                var randomNumber = new Random().Next(0, 2);
                var first = randomNumber == 0 ? neuralNetwork1 : neuralNetwork2;
                var second = randomNumber == 0 ? neuralNetwork2 : neuralNetwork1;

                var neuralNumberFirst = randomNumber == 0 ? "first" : "second";
                _logger.LogInformation("The match {fightNumber} will begin. The {neuralNumber} will begin", i, neuralNumberFirst);


                while (!chessGame.IsEndGame)
                {
                    var move = await Helper.GetBestMove(chessGame, 1000, first, stoppingToken);
                    //_logger.LogInformation("Move White: {move}", move);
                    var firstMove = chessGame.Move(move);
                    if (!firstMove)
                    {
                        throw new Exception("Move not possible");
                    }
                    if (chessGame.IsEndGame)
                    {
                        break;
                    }
                    move = await Helper.GetBestMove(chessGame, 1000, second, stoppingToken);
                    var secondMove = chessGame.Move(move);
                    // _logger.LogInformation("Move Black: {move}", move);
                    if (!secondMove)
                    {
                        throw new Exception("Move not possible");
                    }
                    if (chessGame.MoveIndex >= _neuralConfig.MaxNumberOfMoves)
                    {
                        //The game is too long. The one with the most point win
                        var firstTake = 0.0;
                        var secondTake = 0.0;

                        foreach (var piece in chessGame.CapturedBlack)
                        {
                            if (piece.Type.Value == PieceType.Pawn.Value) firstTake += 0.1;
                            else if (piece.Type.Value == PieceType.Knight.Value) firstTake += 0.3;
                            else if (piece.Type.Value == PieceType.Bishop.Value) firstTake += 0.3;
                            else if (piece.Type.Value == PieceType.Rook.Value) firstTake += 0.5;
                            else if (piece.Type.Value == PieceType.Queen.Value) firstTake += 0.9;
                        }

                        foreach (var piece in chessGame.CapturedWhite)
                        {
                            if (piece.Type.Value == PieceType.Pawn.Value) secondTake += 0.1;
                            else if (piece.Type.Value == PieceType.Knight.Value) secondTake += 0.3;
                            else if (piece.Type.Value == PieceType.Bishop.Value) secondTake += 0.3;
                            else if (piece.Type.Value == PieceType.Rook.Value) secondTake += 0.5;
                            else if (piece.Type.Value == PieceType.Queen.Value) secondTake += 0.9;
                        }
                        var colorResign = firstTake > secondTake ? PieceColor.Black : PieceColor.White;
                        chessGame.Resign(colorResign);
                    }
                }
                var wonSide = chessGame.EndGame?.WonSide ?? PieceColor.White;
                _logger.LogInformation("{wonSide} Won in {moves} moves.", wonSide, chessGame.ExecutedMoves.Count);

                if ((wonSide == PieceColor.White && first == neuralNetwork1) ||
                    (wonSide == PieceColor.Black && second == neuralNetwork1))
                {
                    neuralNetwork1Victory++;
                }
                else
                {
                    neuralNetwork2Victory++;
                }

                _logger.LogInformation("Result: first: {victoryFirst}, second: {victorySecond}, the losser will trains and the winner will remain", neuralNetwork1Victory, neuralNetwork2Victory);
                var loser = wonSide == PieceColor.White ? second : first;
                var winner = wonSide != PieceColor.White ? second : first;

                await _neuralNetwork.Export(winner, stoppingToken);
                await Helper.TrainAi(loser, _neuralConfig, stoppingToken, true);
            }

            _logger.LogInformation("The tournement is finished with {fightNumber} fights", fightNumber);
        }
    }
}
