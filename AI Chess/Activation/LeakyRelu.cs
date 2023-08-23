  namespace AI_Chess.Activation
{
    public class LeakyRelu : IActivation
    {
        private readonly double alpha = 0.01;
        public double[][] Activation(double[][] z)
        {
            double[][] result = new double[z.Length][];
            for (int i = 0; i < z.Length; i++)
            {
                result[i] = new double[z[0].Length];
                for (int j = 0; j < z[0].Length; j++)
                {
                    result[i][j] = Math.Max(alpha * z[i][j], z[i][j]);
                }
            }
            return result;
        }

        public  double[][] Derivative(double[][] z, double[][] y){
            double[][] result = new double[z.Length][];
            for (int i = 0; i < z.Length; i++)
            {
                result[i] = new double[z[0].Length];
                for (int j = 0; j < z[0].Length; j++)
                {
                    result[i][j] = z[i][j] <= 0 ? alpha : 1;    
                }
            }
            return result;
        }
    }
}