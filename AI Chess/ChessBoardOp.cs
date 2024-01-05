using Chess;

namespace AI_Chess
{
    public class ChessBoardOp
    {
        public static double[] GetNeuralInput( int[][] OriginalPositions, short NewPositionX, short NewPositionY,  short OriginalPositionX, short OriginalPositionY,  int Turn)
        {
            double[] result = new double[OriginalPositions.Length * OriginalPositions[0].Length+5];
            for(int i = 0; i < OriginalPositions.Length; i++){
                for(int j = 0; j< OriginalPositions[0].Length; j++) {
                    result[i*OriginalPositions[0].Length+j] = OriginalPositions[i][j];
                }
            }
            result[^5] = OriginalPositionX;
            result[^4] = OriginalPositionY;
            result[^3] = NewPositionX;
            result[^2] = NewPositionY;
            result[^1] = Turn;
            return result;
        }

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
