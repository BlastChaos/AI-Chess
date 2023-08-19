namespace AI_Chess.Activation
{
    public class Sigmoid : IActivation
    {
        public double[][] Activation(double[][] z)
        {
            double[][] result = new double[z.Length][];
            for (int i = 0; i < z.Length; i++)
            {
                result[i] = new double[z[0].Length];
                for (int j = 0; j < z[0].Length; j++)
                {
                    result[i][j] = 1 /(1 + Math.Exp(-z[i][j]));
                }
            }
            return result;
        }
        
        public double[][] Derivative(double[][] z, double[][] y){
            double[][] activation = this.Activation(z);
            double[][] result = new double[z.Length][];
            for (int i = 0; i < z.Length; i++)
            {
                result[i] = new double[z[0].Length];
                for (int j = 0; j < z[0].Length; j++)
                {
                    result[i][j] = activation[i][j] * (1 - activation[i][j]);
                }
            }
            return result;
        }
    }
}