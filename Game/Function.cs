using System.Text.RegularExpressions;
using Chess;
using Game;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

public partial class Function {
    private static Move GetMove(PositionPiece[] afters, ChessBoard chessBoard) {
        var after = afters.SingleOrDefault(after =>{
            var before = chessBoard[after.Position];
            if(before == null || before.ToString() != after.Piece.ToString()) {
                return true;
            }
            return false;
        }) ?? throw new Exception("No move found");

        var befores = afters.Where(before => before != after);

        var before = chessBoard.Moves().SingleOrDefault(x => x.NewPosition.ToString() == after.Position.ToString() && !befores.Any(before => before.Position.ToString() == x.OriginalPosition.ToString())) ?? throw new Exception("No move found");
        return before;
    }

    public static Move GetOurMove(GameConfig  gameConfig){
        var allPossibleMoves = gameConfig.ChessBoard.Moves();
        var move = GetMove(allPossibleMoves);

        var pieceWebElement = gameConfig.GameWebElement.FindElement(By.ClassName($"square-{1+move.OriginalPosition.X}{1+move.OriginalPosition.Y}"));
        
        var offsetX = move.NewPosition.X - move.OriginalPosition.X;
        var offsetY = move.OriginalPosition.Y - move.NewPosition.Y;
        var stepX = gameConfig.GameWebElement.Size.Width / 8;
        var stepY = gameConfig.GameWebElement.Size.Height / 8;
        Actions builder = new(gameConfig.Driver);
        builder.DragAndDropToOffset(pieceWebElement, offsetX * stepX, offsetY * stepY).Perform();

        return move;
    }

    private static Move GetMove(Move[] moves){
        return moves[0];
    }


    public static Move GetOpponentMove(GameConfig  gameConfig){
        Move? move = null;
        while(move == null){
            Thread.Sleep(3000);
            var position = gameConfig.GameWebElement.FindElements(By.ClassName("piece")).Select(x => x.GetAttribute("class")).ToList();
            var newchessPositionsPiece = position.Select<string, PositionPiece>(x => {
                var regex= MyRegex();
                var match = regex.Match(x);
                Piece piece;
                string position;
                try {
                    piece = new(match.Groups[1].Value);
                    position = match.Groups[2].Value;
                } catch {
                    piece = new("wp");
                    position = "11";
                }
                
                short xPosition = Convert.ToInt16(int.Parse(position[0].ToString())-1);
                short yPosition = Convert.ToInt16(int.Parse(position[1].ToString())-1);
                return new(){
                    Piece = piece,
                    Position = new(xPosition, yPosition)
                };
            }).ToArray();

            try {
                move = GetMove(newchessPositionsPiece, gameConfig.ChessBoard);
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                continue;
            }
        }
        return move;
    }

    [GeneratedRegex("\\b(?:b?|w?)\\s*(\\b\\w+\\b)\\s*square-(\\d+)")]
    private static partial Regex MyRegex();
}