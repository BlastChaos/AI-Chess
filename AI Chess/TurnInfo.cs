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

    public int[] Input(){
        int[] result = new int[OriginalPositions.Length * OriginalPositions[0].Length+1];
        for(int i = 0; i < OriginalPositions.Length; i++){
            for(int j = 0; j< OriginalPositions[0].Length; j++) {
                result[i*OriginalPositions[0].Length+j] = OriginalPositions[i][j];
            }
        }
        result[^1] = Turn;
        return result;
    }

    public int[] Output(){
        int[] result = new int[4];
        return result;
    }
    

}