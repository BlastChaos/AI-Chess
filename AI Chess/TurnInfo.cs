namespace AI_Chess;

public class TurnInfo
{
    //Se référer à PieceColor.Black OU white
    public required int Turn{ get; set; }
    public required short OriginalPositionX{ get; set; }
    public required short OriginalPositionY{ get; set; }
    public required short NewPositionX{ get; set; }
    public required short NewPositionY{ get; set; }
    public required int[][] OriginalPositions { get; set; }
    public double Point {get ; set ;}
    public  required double OpponentElo {get; set;}

    public double[] GetNeuralInput()
    {
        double[] result = new double[OriginalPositions.Length * OriginalPositions[0].Length+6];
        for(int i = 0; i < OriginalPositions.Length; i++){
            for(int j = 0; j< OriginalPositions[0].Length; j++) {
                result[i*OriginalPositions[0].Length+j] = OriginalPositions[i][j];
            }
        }
        result[^6] = OpponentElo;
        result[^4] = OriginalPositionY;
        result[^3] = NewPositionX;
        result[^2] = NewPositionY;
        result[^1] = Turn;
        return result;
    }
}

