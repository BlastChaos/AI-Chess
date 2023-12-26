namespace AI_Chess;

public class NeuralConfig
{
    public string[] Users { get; set; }
    public string GamesOutputDirectory { get; set; }
    public int NumberData { get; set; }
    public double GoodMovePoint { get; set; }
    public double BadMovePoint { get; set; }
    public int MaxBadMove {get; set;}
    public double LearningRate {get; set;}
    public int BackgroundIterations {get; set;}
    public string SaveName {get; set;}
}
