namespace AI_Chess
{
    public class NeuralNetwork
    {
        private readonly int NbInputNodes;
        private readonly int NbHiddenNodes;
        private readonly int NbOutputNodes;
        private readonly double LearningRate;
        private double[][] W1;
        private double[][] W2;
        private double[] B1;
        private double[] B2;
        private double[][]? A1;
        private double[][]? A2;
        private double[][]? A3;
        private double[][]? Z2;
        private double[][]? Z3;

        public NeuralNetwork(int nbInputNodes, int nbHiddenNodes, int nbOutputNodes, double learningRate)
        {
            this.NbInputNodes = nbInputNodes;
            this.NbHiddenNodes = nbHiddenNodes;
            this.NbOutputNodes = nbOutputNodes;
            this.LearningRate = learningRate;

            this.W1 = new double[this.NbInputNodes][];
            this.B1 = new double[this.NbHiddenNodes];
            SetupValue(this.W1, this.B1,nbHiddenNodes);

            this.W2 = new double[this.NbHiddenNodes][];
            this.B2 = new double[this.NbOutputNodes];
            SetupValue(this.W1, this.B1, nbOutputNodes);
        }

        public void SetupValue(double[][] w, double[] b, int nbOutputNodes)
        {
            var rnd = new Random();
            var oneTimeOnly = true;
            for (int i = 0; i < w.Length; i++)
            {
                w[i] = new double[nbOutputNodes];
                for (int j = 0; j < w[i].Length; j++)
                {
                    w[i][j] = rnd.NextDouble();
                    if (oneTimeOnly) this.B1[j] = 1;
                }
                oneTimeOnly = false;
            }
        }

        public double[][] Softmax(double[][] z){
            int x = z.Length;
            int y = z[0].Length;
            double[][] numerator = new double[x][];

            for (int i = 0; i < x; i++)
            {
                numerator[i] = new double[y];
                for (int j = 0; j < y; j++)
                {
                    numerator[i][j]=Math.Exp(z[i][j]);        
                }
            }
            double[][] softmax = new double[x][];
            for (int i=0;i<x;i++)
            {
                double result = 0;
                Array.ForEach(numerator[i], value => result+=Math.Exp(value));
                softmax[i] = new double[y]; 
                for (int j = 0; j < y; j++)
                {
                    softmax[i][j]= numerator[i][j] / result;    
                }   
            } 
            return softmax;
        }

        public  double[][] DerivativeSoftmax(double[][] z, double[][] y){
            double[][] softmax = this.Softmax(z);
            double[][] result = new double[z.Length][];
            for (int i = 0; i < z.Length; i++)
            {
                result[i] = new double[z[0].Length];
                for (int j = 0; j < z[0].Length; j++)
                {
                    result[i][j] = softmax[i][j] - 1*y[i][j];     
                }
            }
            return result;
        }

        public double[][] LeakyRELU(double[][] z, double alpha){
            double[][] result = new double[z.Length][];
            for (int i = 0; i < z.Length; i++)
            {
                result[i] = new double[z[0].Length];
                for (int j = 0; j < z[0].Length; j++)
                {
                    result[i][j] = Math.Max(alpha*z[i][j], z[i][j]);
                }
            }
            return result;
        }

        public  double[][] DerivativeLeakyRELU(double[][] z, double alpha){
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

        public double EntropyLoss(double[][] y, double[][] y_pred){
            
            double result = 0;
            for (int i = 0; i < y.Length; i++)
            {
                for (int j = 0; j < y[0].Length; j++)
                {
                    //Prevent taking the log of 0 with double.Epsilon
                    result += y[i][j] * Math.Log(y_pred[i][j] + double.Epsilon);
                }
            }
            return result;
        }

        public double[][] Forward(double[][] x){
            this.A1 = x;
            double[][] dotProduct = MatrixOperation.DotProduct(this.A1, this.W1);
            this.Z2 =  MatrixOperation.Add(dotProduct, this.B1);

            this.A2 = this.LeakyRELU(this.Z2, this.LearningRate);

            dotProduct = MatrixOperation.DotProduct(this.A2, this.W2);
            this.Z3 =  MatrixOperation.Add(dotProduct, this.B2); 
            
            this.A3 = this.Softmax(this.Z3);

            return this.A3;
        }

        public double[][] Backward(double[][] x, double[][] y){
            //Inutile?
            //int m = x.Length;

            double[][] dz3 = this.DerivativeSoftmax(this.A3!, y);

            double [][] dw2 = MatrixOperation.DotProduct(MatrixOperation.Transpose(this.A2!), dz3);

            double[] db2 = MatrixOperation.SumColumn(dz3);


            double[][] dA2 = MatrixOperation.DotProduct(dz3, MatrixOperation.Transpose(this.W2!));

            double [][] dz2 =  MatrixOperation.DotElementWise(dA2, this.DerivativeLeakyRELU(this.Z2!, this.LearningRate));

            double[][] dw1 = MatrixOperation.DotProduct(MatrixOperation.Transpose(x!), dz2);

            double[] db1 = MatrixOperation.SumColumn(dz2);


            this.W2 = MatrixOperation.Diff(this.W2, MatrixOperation.DotConstant(dw2, this.LearningRate));
            var fakeB2 = new double[][] {this.B2};
            var fakedb2 = new double[][] {db2};
            this.B2 = MatrixOperation.Diff(fakeB2, MatrixOperation.DotConstant(fakedb2, this.LearningRate))[0];
            
            this.W1 = MatrixOperation.Diff(this.W1, MatrixOperation.DotConstant(dw1, this.LearningRate));
            var fakeB1 = new double[][] {this.B1};
            var fakedb1 = new double[][] {db1};
            this.B1 = MatrixOperation.Diff(fakeB1, MatrixOperation.DotConstant(fakedb1, this.LearningRate))[0];
        }

    }
}
