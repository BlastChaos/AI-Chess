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

        var indexLength = board.MoveIndex;
        board.MoveIndex = -1;
        while (board.MoveIndex != indexLength)
        {
            // for (int i = 0; i < pieces.Length; i++)
            // {
            //     for (int j = 0; j < pieces[i].Length; j++)
            //     {
            //         Console.Write(pieces[i][j] + " ");
            //     }
            //     Console.WriteLine();
            // }
            if (playerColor != board.Turn)
            {
                board.Next();
                continue;
            }
            var pieces = ChessBoardOp.TransformChessBoard(board);
            var moves = board.Moves();

            //Get executed Move
            var nextMove = board.ExecutedMoves[board.MoveIndex + 1];

            for (int i = 0; i < Math.Min(moves.Length, 30); i++)
            {
                var possibleMove = moves[i];
                var isPlayerMove = nextMove.ToString() == possibleMove.ToString();
                var point = CalculatePointOfBoard(board.ToFen(), possibleMove, isPlayerMove);
                turnInfos.Add(new TurnInfo()
                {
                    OriginalPositions = pieces,
                    Move = nextMove,
                    PreviousMoves = board.ExecutedMoves.Take(board.MoveIndex + 1).ToArray(),
                    Point = point,
                    Turn = board.Turn,
                    OpponentElo = opponentElo,
                });

            }
            board.Next();

        }

        return turnInfos;

    }

    public static double CalculatePointOfBoard(string fen, Move move, bool isPlayerMove)
    {
        var board = ChessBoard.LoadFromFen(fen);
        if (move.IsMate) return 1;

        var killPoint = 0.0;
        if (isPlayerMove) return 0.95;

        if (move.CapturedPiece != null)
            if (move.CapturedPiece.Type.Value == PieceType.Pawn.Value) killPoint = 0.1;
            else if (move.CapturedPiece.Type.Value == PieceType.Knight.Value) killPoint = 0.3;
            else if (move.CapturedPiece.Type.Value == PieceType.Bishop.Value) killPoint = 0.3;
            else if (move.CapturedPiece.Type.Value == PieceType.Rook.Value) killPoint = 0.5;
            else if (move.CapturedPiece.Type.Value == PieceType.Queen.Value) killPoint = 0.9;

        var worstScenario = 0.0;

        board.Move(move);

        foreach (var possibleMove in board.Moves())
        {
            if (possibleMove.IsMate)
            {
                worstScenario = 1;
                break;
            }
            if (possibleMove.CapturedPiece != null)
            {
                if (possibleMove.CapturedPiece.Type.Value == PieceType.Pawn.Value) worstScenario = Math.Max(worstScenario, 0.1);
                else if (possibleMove.CapturedPiece.Type.Value == PieceType.Knight.Value) worstScenario = Math.Max(worstScenario, 0.3);
                else if (possibleMove.CapturedPiece.Type.Value == PieceType.Bishop.Value) worstScenario = Math.Max(worstScenario, 0.3);
                else if (possibleMove.CapturedPiece.Type.Value == PieceType.Rook.Value) worstScenario = Math.Max(worstScenario, 0.5);
                else if (possibleMove.CapturedPiece.Type.Value == PieceType.Queen.Value) worstScenario = Math.Max(worstScenario, 0.9);
            }
        }

        return Math.Max(killPoint - worstScenario / 1.5, 0);

    }


    public static List<TurnInfo> PrepareGameInfo(NeuralConfig neuralConfig, bool randomize = false)
    {
        List<TurnInfo> turnInfos = new();
        var directory = Path.Combine(Directory.GetCurrentDirectory(), neuralConfig.GamesOutputDirectory);
        string[] filePaths = Directory.GetFiles(directory, "*.pgn",
                                        SearchOption.TopDirectoryOnly);
        if (randomize)
        {
            var random = new Random();
            filePaths = filePaths.OrderBy(x => random.Next()).ToArray();
        }
        int gameCount = 0;

        while (gameCount < neuralConfig.NumberData)
        {
            gameCount++;
            var filePath = filePaths[gameCount];
            string input = File.ReadAllText(filePath);
            if (!ChessBoard.TryLoadFromPgn(input, out ChessBoard board))
            {
                continue;
            }

            board.Headers.TryGetValue("Black", out var blackName);
            var playerBlack = neuralConfig.Users.Contains(blackName, StringComparer.OrdinalIgnoreCase);

            var opponentElo = playerBlack ? board.Headers["WhiteElo"] : board.Headers["BlackElo"];
            var playerTurn = playerBlack ? PieceColor.Black : PieceColor.White;
            var gameInfo = GetTurnInfos(board, int.Parse(opponentElo), playerTurn);

            turnInfos.AddRange(gameInfo);
        }
        return turnInfos;
    }


    public static async Task TrainAi(NeuralNetwork neural, NeuralConfig neuralConfig, CancellationToken cancellationToken, bool randomizeGame = false)
    {
        var turnInfos = PrepareGameInfo(neuralConfig, randomizeGame);
        var input = turnInfos.Select(x => x.GetNeuralInput()).ToArray();
        var output = turnInfos.Select(x => new double[] { x.Point }).ToArray();
        await neural.Train(input, output, neuralConfig.BackgroundIterations, cancellationToken);
    }

}