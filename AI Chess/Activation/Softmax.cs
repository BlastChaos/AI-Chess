namespace AI_Chess.Activation
{
    public class Softmax : IActivation
    {
        private readonly double EXP_MAX_VALUE = 709.78271289338;
        public double[][] Activation(double[][] z)
        {
            int x = z.Length;
            int y = z[0].Length;
            double[][] numerator = new double[x][];
            double[][] softmax = new double[x][];
            for (int i = 0; i < x; i++)
            {
                //Get all numerotor before divising by the denominator
                numerator[i] = new double[y];
                
                var valeurDiminuer = 0.0;
                var somme = z![i].Sum(s => s >=0 ? s : 0.0);
                var t = z![i].Sum(s => s >=0 ? 1 : 0.0);
                var secondBest = z![i].OrderDescending().Skip(1).First();
                if(somme > EXP_MAX_VALUE){
                    valeurDiminuer = (somme - EXP_MAX_VALUE)/t + secondBest/t*1.3;
                } 

                for (int j = 0; j < y; j++)
                {
                    
                    numerator[i][j]=Math.Exp(z[i][j] - valeurDiminuer) + double.Epsilon;  
                    if(double.IsInfinity(numerator[i][j]))
                    {
                        throw new Exception("Math.Exp gives Infinity.");
                    }
                }

                double denominator = numerator[i].Sum();
                if(double.IsNaN( denominator) || double.IsInfinity(denominator))
                {
                    throw new Exception("Denominator gives Infinity.");
                }
                softmax[i] = new double[y]; 
                for (int j = 0; j < y; j++)
                {
                    softmax[i][j] = numerator[i][j] / denominator;
                }
            }
            return softmax;
            }

        public  double[][] Derivative(double[][] z, double[][] y){
            double[][] softmax = this.Activation(z);
            double[][] result = new double[z.Length][];
            for (int i = 0; i < z.Length; i++)
            {
                result[i] = new double[z[0].Length];
                for (int j = 0; j < z[0].Length; j++)
                {
                    //result[i][j] = softmax[i][j] - y[i][j];     
                    result[i][j] = i == j ? softmax[i][j]  * (1-softmax[i][j]) : -softmax[i][j] * softmax[i][j];
                }
            }
            return result;
        }
    }
}