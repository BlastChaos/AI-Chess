
namespace AI_Chess.Activation
{
    public interface IActivation
    {
        public double[][] Activation(double[][] z);
        public double[][] Derivative(double[][] z, double[][] y);
    }
}