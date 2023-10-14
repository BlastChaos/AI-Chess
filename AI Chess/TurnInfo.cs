namespace AI_Chess;

public class TurnInfo
{
    //Se référer à PieceColor.Black OU white
    public int Turn{ get; set; }
    public short OriginalPositionX{ get; set; }
    public short OriginalPositionY{ get; set; }
    public short NewPositionX{ get; set; }
    public short NewPositionY{ get; set; }
    public int[][] OriginalPositions { get; set; }
    public double Point {get; set;}
    public double[] Input(){
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
}