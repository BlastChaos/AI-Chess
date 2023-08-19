  namespace AI_Chess.Activation
{
    public class Identity : IActivation
    {
        public double[][] Activation(double[][] z) => z;
        public  double[][] Derivative(double[][] z, double[][] y) => z.Select(t => t.Select(l => 0.0).ToArray()).ToArray();
    }
}