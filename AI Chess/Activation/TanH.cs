using AI_Chess.Activation;

namespace AI_Chess;

public class TanH : IActivation
{
    private readonly double EXP_MAX_VALUE = 709.78271289338;
    public double[][] Activation(double[][] z)
    {
        double[][] result = new double[z.Length][];
        for (int i = 0; i < z.Length; i++)
        {
            result[i] = new double[z[0].Length];
            for (int j = 0; j < z[0].Length; j++)
            {
                var diff = 0.0;
                if (z[i][j] >= EXP_MAX_VALUE){
                    diff = z[i][j] - EXP_MAX_VALUE; 
                }
                if (z[i][j] <= -EXP_MAX_VALUE){
                    diff = -EXP_MAX_VALUE - z[i][j]; 
                }
                result[i][j] = (Math.Exp(z[i][j] - diff) - Math.Exp(-z[i][j] - diff)) / (Math.Exp(z[i][j] - diff) + Math.Exp(-z[i][j] - diff));
                if(double.IsInfinity(result[i][j]) || double.IsNaN(result[i][j]) || result[i][j] == 0)
                {
                    throw new Exception("TanH give an invalid result, " + result[i][j]);
                }
            }
        }
        return result;
    }

    public double[][] Derivative(double[][] z, double[][] y)
    {
        double[][] activation = this.Activation(z);
        double[][] result = new double[z.Length][];
        for (int i = 0; i < z.Length; i++)
        {
            result[i] = new double[z[0].Length];
            for (int j = 0; j < z[0].Length; j++)
            {
                result[i][j] = 1 - activation[i][j] * activation[i][j];
            }

        }
        return result;
    }
}
