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
                        var pieceValue = board[i,j].Color.Value == PieceColor.Black.Value ? -board[i,j].Type.Value : board[i,j].Type.Value;
                        pieces[i][j] = pieceValue/6.0;
                    }
                    
                }
            }
            return pieces;
        }

        public static int[][] ConvertToElement(int[][] board){
            int rowCount = board.Length;
            int colCount = board[0].Length;
            int[][] result = new int[colCount][];

            for (int i = 0; i < colCount; i++)
            {
                result[i] = new int[rowCount];
                for (int j = 0; j < rowCount; j++)
                {
                    result[i][j] = board[rowCount - j - 1][i];
                }
            }

            return result;
        }
    }
}
