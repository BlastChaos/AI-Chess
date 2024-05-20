using AI_Chess;
using Chess;

public partial class Helper
{
    public static async Task<Move> GetBestMove(ChessBoard chessBoard, double opponentElo, NeuralNetwork neuralNetwork, CancellationToken stoppingToken)
    {
        var pieces = ChessBoardOp.TransformChessBoard(chessBoard);

        var moves = chessBoard.Moves();
        var input = moves.Select(move => new TurnInfo()
        {
            Turn = chessBoard.Turn,
            OriginalPositions = pieces,
            OpponentElo = opponentElo,
            PreviousMoves = chessBoard.ExecutedMoves.ToArray(),
            Move = move
        }.GetNeuralInput()).ToArray();

        var output = await neuralNetwork.Predict(input, stoppingToken);
        Move move = moves[0];
        double bestMove = output[0][0];
        for (var i = 0; i < output.Length; i++)
        {
            if (output[i][0] > bestMove)
            {
                //Console.WriteLine("New best move: " + moves[i] + " with " + output[i][0]);
                move = moves[i];
                bestMove = output[i][0];
            }
        }
        return move;
    }

    public static List<TurnInfo> GetTurnInfos(ChessBoard board, double opponentElo, PieceColor playerColor)
    {
        List<TurnInfo> turnInfos = new();

        var newBoard = new ChessBoard();

        foreach (Move moveMatch in board.ExecutedMoves)
        {
            if (playerColor != newBoard.Turn)
            {
                newBoard.Move(moveMatch);
                continue;
            }
            var pieces = ChessBoardOp.TransformChessBoard(newBoard);
            // for (int i = 0; i < pieces.Length; i++)
            // {
            //     for (int j = 0; j < pieces[i].Length; j++)
            //     {
            //         Console.Write(pieces[i][j] + " ");
            //     }
            //     Console.WriteLine();
            // }
            var moves = newBoard.Moves();
            for (int i = 0; i < Math.Min(moves.Length, 30); i++)
            {
                var possibleMove = moves[i];
                var isPlayerMove = moveMatch.ToString() == possibleMove.ToString();
                var point = CalculatePoint.CalculatePointOfBoard(newBoard, possibleMove, isPlayerMove);
                turnInfos.Add(new TurnInfo()
                {
                    OriginalPositions = pieces,
                    Move = moveMatch,
                    PreviousMoves = newBoard.ExecutedMoves.ToArray(),
                    Point = point,
                    Turn = newBoard.Turn,
                    OpponentElo = opponentElo,
                });

            }
            newBoard.Move(moveMatch);
        }
        return turnInfos;

    }

}