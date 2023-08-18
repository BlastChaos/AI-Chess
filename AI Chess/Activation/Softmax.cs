namespace AI_Chess.Activation
{
    public class Softmax : IActivation
    {
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
                if(somme > ChessConstant.EXP_MAX_VALUE){
                    valeurDiminuer = (somme - ChessConstant.EXP_MAX_VALUE)/t + secondBest/t;
                } 

                for (int j = 0; j < y; j++)
                {
                    
                    numerator[i][j]=Math.Exp(z[i][j] - valeurDiminuer) + double.Epsilon;  
                    if(double.IsNaN( numerator[i][j]) || double.IsInfinity(numerator[i][j]))
                    {
                        throw new Exception("Math.Exp Give Nan or Infinity. Value:" + numerator[i][j]);
                    }
                }

                double denominator = numerator[i].Sum();
                if(double.IsNaN( denominator) || double.IsInfinity(denominator))
                {
                    throw new Exception("Denominator give Infinity or Nan");
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
                    result[i][j] = softmax[i][j] - y[i][j];     
                }
            }
            return result;
        }
    }
}