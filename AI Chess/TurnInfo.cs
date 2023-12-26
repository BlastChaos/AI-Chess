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
}