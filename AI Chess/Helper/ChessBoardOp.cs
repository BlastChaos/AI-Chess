using Chess;

namespace AI_Chess
{
    public class ChessBoardOp
    {

        public static double[][] TransformChessBoard(ChessBoard board)
        {
            double[][] pieces = new double[8][];
            for(short i = 0; i < 8; i++){
                pieces[i] = new double[8];
                for(short j = 0; j< 8; j++) {
                    var boardPosition = board[i,j];
                    if(boardPosition is not null){
                        var pieceValue = boardPosition.Color.Value == PieceColor.Black.Value ? (-boardPosition.Type.Value - 1) : (boardPosition.Type.Value + 1);
                        pieces[i][j] = 0.5-pieceValue/7.0*0.45;
                    }
                    
                }
            }
            return pieces;
        }
    }
}
