using Chess;

namespace AI_Chess
{
    public class ChessBoardOp
    {

        public static int[][] TransformChessBoard(ChessBoard board)
        {
            int[][] pieces = new int[8][];
            for(short i = 0; i < 8; i++){
                pieces[i] = new int[8];
                for(short j = 0; j< 8; j++) {
                    if(board[i,j] != null)
                    pieces[i][j] = board[i,j].Color.Value == PieceColor.Black.Value ? -board[i,j].Type.Value : board[i,j].Type.Value;
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
