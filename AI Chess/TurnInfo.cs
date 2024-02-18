using Chess;

namespace AI_Chess;

public class TurnInfo
{
    public required PieceColor Turn{ get; set; }
    public required Move[] PreviousMoves { get; set; }
    public required Move Move{ get; set; }
    public required double[][] OriginalPositions { get; set; }
    public double Point {get ; set ;}
    public  required double OpponentElo {get; set;}

    public double[] GetNeuralInput()
    {
        double[] result = new double[GetNumberOfInputNodes()];

        //Chessboard
        for(int i = 0; i < OriginalPositions.Length; i++){
            for(int j = 0; j< OriginalPositions[0].Length; j++) {
                result[i*OriginalPositions[0].Length+j] = OriginalPositions[i][j];
            }
        }


        // Add the last 5 moves
        for (int i = 0; i < 5; i++)
        {
            if (i < PreviousMoves.Length)
            {
                result[OriginalPositions.Length * OriginalPositions[0].Length + i * 4] = NormalizePosition(PreviousMoves[PreviousMoves.Length - i - 1].OriginalPosition.X);
                result[OriginalPositions.Length * OriginalPositions[0].Length + i * 4 + 1] = NormalizePosition(PreviousMoves[PreviousMoves.Length - i - 1].OriginalPosition.Y);
                result[OriginalPositions.Length * OriginalPositions[0].Length + i * 4 + 2] = NormalizePosition(PreviousMoves[PreviousMoves.Length - i - 1].NewPosition.X);
                result[OriginalPositions.Length * OriginalPositions[0].Length + i * 4 + 3] = NormalizePosition(PreviousMoves[PreviousMoves.Length - i - 1].NewPosition.Y);
            }
            else
            {
                // Fill with default values
                result[OriginalPositions.Length * OriginalPositions[0].Length + i * 4] = 0;
                result[OriginalPositions.Length * OriginalPositions[0].Length + i * 4 + 1] = 0;
                result[OriginalPositions.Length * OriginalPositions[0].Length + i * 4 + 2] = 0;
                result[OriginalPositions.Length * OriginalPositions[0].Length + i * 4 + 3] = 0;
            }
        }

        result[^1] = Turn.Value - 1;
        result[^2] = NormalizePosition(Move.NewPosition.Y);
        result[^3] = NormalizePosition(Move.NewPosition.X);
        result[^4] = NormalizePosition(Move.OriginalPosition.Y);
        result[^5] = NormalizePosition(Move.OriginalPosition.X);
        result[^6] = OpponentElo/2882.0;//Max elo in the world
        return result;
    }

    public static int GetNumberOfInputNodes()
    {
        return 90;
    }

    private static double NormalizePosition(int position)
    {
        return position / 8.0;

    }
}

