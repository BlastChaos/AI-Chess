namespace AI_Chess;

public class NeuralConfig
{
    public string[] Users { get; set; }
    public string GamesOutputDirectory { get; set; }
    public int NumberData { get; set; }
    public Point GoodMatchPoint { get; set; }
    public Point BadMatchPoint { get; set; }
    public int NumberOfInputNeurons { get; set; }
    public double LearningRate {get; set;}
    public int BackgroundIterations {get; set;}
    public string BrainFileName {get; set;}
}

public class Point
{
    public double Player { get; set; }
    public double Opponent { get; set; }
}

