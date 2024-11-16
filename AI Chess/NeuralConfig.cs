namespace AI_Chess;

public class NeuralConfig
{

    public required string[] Users { get; set; }
    public required string GamesOutputDirectory { get; set; }
    public int NumberData { get; set; }
    public required Point GoodMatchPoint { get; set; }
    public int MaxNumberOfMoves { get; set; }
    public required Point BadMatchPoint { get; set; }
    public double LearningRate { get; set; }
    public int BackgroundIterations { get; set; }
    public int TournamentLength { get; set; }

}

public class Point
{
    public double Player { get; set; }
    public double Opponent { get; set; }
}

