using Chess;

namespace AI_Chess;

public class CalculatePoint
{

    public static double CalculatePointOfBoard(ChessBoard board, Move move, bool isPlayerMove)
    {
        if (move.IsMate) return 1;

        var killPoint = 0.0;    
        if (isPlayerMove) return  0.95;
        
        if(move.CapturedPiece != null)
            if(move.CapturedPiece.Type.Value == PieceType.Pawn.Value) killPoint = 0.1;
            else if(move.CapturedPiece.Type.Value == PieceType.Knight.Value) killPoint = 0.3;
            else if(move.CapturedPiece.Type.Value == PieceType.Bishop.Value) killPoint = 0.3;
            else if(move.CapturedPiece.Type.Value == PieceType.Rook.Value) killPoint = 0.5;
            else if(move.CapturedPiece.Type.Value == PieceType.Queen.Value) killPoint = 0.9;
            else if(move.CapturedPiece.Type.Value == PieceType.King.Value) killPoint = 1;

        var worstScenario = 0.0;

        board.Move(move);

        foreach(var possibleMove in board.Moves()) {
            if(possibleMove.CapturedPiece != null){
                if(possibleMove.CapturedPiece.Type.Value == PieceType.Pawn.Value) worstScenario = Math.Max(worstScenario, 0.1);
                else if(possibleMove.CapturedPiece.Type.Value == PieceType.Knight.Value) worstScenario = Math.Max(worstScenario, 0.3);
                else if(possibleMove.CapturedPiece.Type.Value == PieceType.Bishop.Value) worstScenario = Math.Max(worstScenario, 0.3);
                else if(possibleMove.CapturedPiece.Type.Value == PieceType.Rook.Value) worstScenario = Math.Max(worstScenario, 0.5);
                else if(possibleMove.CapturedPiece.Type.Value == PieceType.Queen.Value) worstScenario = Math.Max(worstScenario, 0.9);
                else if(possibleMove.CapturedPiece.Type.Value == PieceType.King.Value) worstScenario = Math.Max(worstScenario, 1);
            }
        }   
        board.Cancel();

        return Math.Max(killPoint - worstScenario/1.5,0);

    }
}

