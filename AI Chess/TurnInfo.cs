using Chess;

namespace AI_Chess;

public class TurnInfo
{
    public int Turn{ get; set; }
    public Move? Move { get; set; }
    public Piece[,] Pieces { get; set; }
    

}