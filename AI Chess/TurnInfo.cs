namespace AI_Chess;

public class TurnInfo
{
    public DateTime Date { get; set; }
    public string? WhiteUsername { get; set; }
    public string? BlackUsername { get; set; }
    public string? MoveString { get; set; }
    public Turn Turn{ get; set; }

}

public enum Turn {
    White,
    Black
}
