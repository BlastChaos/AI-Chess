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

                    if(board[i,j] != null){
                        var pieceValue = board[i,j].Color.Value == PieceColor.Black.Value ? (-board[i,j].Type.Value-1) : (board[i,j].Type.Value+1);
                        pieces[i][j] = 0.5-pieceValue/7.0*0.45;
                    }
                    
                }
            }
            return pieces;
        }
    }
}
