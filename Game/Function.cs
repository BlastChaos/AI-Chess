using Chess;

public class Function {
    public static Move GetMove(PositionPiece[] befores, PositionPiece[] afters, ChessBoard chessBoard) {
        var before = befores.Single(x => !afters.Any(y =>y.Position.ToString().Equals(x.Position.ToString()) && y.Piece.ToString().Equals(x.Piece.ToString())));
        
        foreach(var after in afters) {

            var move = new Move(before.Position, after.Position);
            if(chessBoard.IsValidMove(move)) {
                return move;
            }
        }
        throw new Exception("Aucun mouvement valide");
    }
}