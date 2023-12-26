using System.Text.RegularExpressions;
using AI_Chess;
using Chess;
using Game;
using OpenQA.Selenium;

public partial class Function {
    private static Move GetMove(PositionPiece[] befores, PositionPiece[] afters, ChessBoard chessBoard) {
        var before = befores.Single(x => !afters.Any(y =>y.Position.ToString().Equals(x.Position.ToString()) && y.Piece.ToString().Equals(x.Piece.ToString())));
        
        foreach(var after in afters) {

            var move = new Move(before.Position, after.Position);
            if(chessBoard.IsValidMove(move)) {
                return move;
            }
        }
        throw new Exception("Aucun mouvement valide");
    }

    public static void GetOurMove(GameConfig  gameConfig){
        var allPossibleMoves = gameConfig.ChessBoard.Moves();
        var boardInt = ChessBoardOp.TransformChessBoard(gameConfig.ChessBoard);

        foreach(var move in allPossibleMoves){
            var neuralInput = ChessBoardOp.GetNeuralInput(boardInt, move.NewPosition.X, move.NewPosition.Y, move.OriginalPosition.X, move.OriginalPosition.Y, gameConfig.ChessBoard.Turn.Value);
        }
    }

    public static void GetOpponentMove(GameConfig  gameConfig){
        Move? move = null;

        while(move == null){
            Thread.Sleep(1000);
            var position = gameConfig.gameWebElement.FindElements(By.ClassName("piece")).Select(x => x.GetAttribute("class")).ToList();
            int[][] pieces = new int[8][];
            var newchessPositionsPiece = position.Select<string, PositionPiece>(x => {
                var regex= MyRegex();
                var match = regex.Match(x);
                Piece piece = new(match.Groups[1].Value);
                var position = match.Groups[2].Value;
                short xPosition = Convert.ToInt16(int.Parse(position[0].ToString())-1);
                short yPosition = Convert.ToInt16(int.Parse(position[1].ToString())-1);
                return new(){
                    Piece = piece,
                    Position = new(xPosition, yPosition)
                };
            }).ToArray();

            List<PositionPiece> chessPostionPiece = new();
            for (short i = 0; i < 8; i++)
            {
                for (short j = 0; j < 8; j++)
                {
                    var piece = gameConfig.ChessBoard[i,j];
                    if(piece != null) {
                        chessPostionPiece.Add(new PositionPiece(){
                            Piece = piece,
                            Position = new(i,j)
                        });
                    }
                }
            }
            try {
                move = GetMove(chessPostionPiece.ToArray(), newchessPositionsPiece, gameConfig.ChessBoard);
            } catch(Exception) {
                continue;
            }
        }
        gameConfig.ChessBoard.Move(move);
    }

    [GeneratedRegex("\\b(?:b?|w?)\\s*(\\b\\w+\\b)\\s*square-(\\d+)")]
    private static partial Regex MyRegex();
}