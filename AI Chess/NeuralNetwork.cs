﻿namespace AI_Chess
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
        private readonly List<double> Loss;

        public NeuralNetwork(int nbInputNodes, int nbHiddenNodes, int nbOutputNodes, double learningRate)
        {
            this.NbInputNodes = nbInputNodes;
            this.NbHiddenNodes = nbHiddenNodes;
            this.NbOutputNodes = nbOutputNodes;
            this.LearningRate = learningRate;
            this.Loss = new List<double>();
            this.W1 = MatrixOperation.GenerateRandomNormal(new Random(), 0, 1, this.NbInputNodes, nbHiddenNodes);
            this.B1 = new double[this.NbHiddenNodes].Select(i => i = 1).ToArray();

            this.W2 = MatrixOperation.GenerateRandomNormal(new Random(), 0, 1, this.NbHiddenNodes, nbOutputNodes);
            this.B2 = new double[this.NbOutputNodes].Select(i => i = 1).ToArray();;
        }

        public double[][] Softmax(double[][] z){
            int x = z.Length;
            int y = z[0].Length;
            double[][] numerator = new double[x][];
            double[][] softmax = new double[x][];
            for (int i = 0; i < x; i++)
            {
                //Get all numerotor before divising by the denominator
                numerator[i] = new double[y];
                
                double[][]? new_z = z.Clone() as double[][];
                var valeurDiminuer = 0.0;
                var somme = new_z![i].Sum(s => s >=0 ? s : 0.0);
                if(somme > ChessConstant.EXP_MAX_VALUE){
                    valeurDiminuer = new_z[i].Max()- (new_z[i].Max() - ChessConstant.EXP_MAX_VALUE)/new_z![i].Length;
                } 
                
/*                  if(new_z![i].Min() <=  ChessConstant.MIN_VALUE_EXP){
                    valeurDiminuer += ChessConstant.MIN_VALUE_EXP;
                }   */
                
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

        public  double[][] DerivativeSoftmax(double[][] a3, double[][] y){
            double[][] softmax = a3;
            double[][] result = new double[a3.Length][];
            for (int i = 0; i < a3.Length; i++)
            {
                result[i] = new double[a3[0].Length];
                for (int j = 0; j < a3[0].Length; j++)
                {
                    result[i][j] = softmax[i][j] - y[i][j];     
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
                    result += -y[i][j] * Math.Log(y_pred[i][j] + double.Epsilon);
                }
            }
            return result;
        }

        public double[][] Forward(double[][] x){
            this.A1 = x;
            double[][] dotProduct = MatrixOperation.DotProduct(this.A1, this.W1);
            this.Z2 =  MatrixOperation.Add(dotProduct, this.B1);

            this.A2 = this.LeakyRELU(this.Z2, ChessConstant.ALPHA);

            dotProduct = MatrixOperation.DotProduct(this.A2, this.W2);
            this.Z3 =  MatrixOperation.Add(dotProduct, this.B2); 
            
            this.A3 = this.Softmax(this.Z3);

            return this.A3;
        }

        public void Backward(double[][] x, double[][] y){
            double[][] dz3 = this.DerivativeSoftmax(this.A3!, y);

            double [][] dw2 = MatrixOperation.DotProduct(MatrixOperation.Transpose(this.A2!), dz3);

            double[] db2 = MatrixOperation.SumColumn(dz3);


            double[][] dA2 = MatrixOperation.DotProduct(dz3, MatrixOperation.Transpose(this.W2!));

            double [][] dz2 =  MatrixOperation.DotElementWise(dA2, this.DerivativeLeakyRELU(this.Z2!, ChessConstant.ALPHA));

            double[][] dw1 = MatrixOperation.DotProduct(MatrixOperation.Transpose(x!), dz2);

            double[] db1 = MatrixOperation.SumColumn(dz2);


            this.W2 = MatrixOperation.Diff(this.W2, MatrixOperation.DotConstant(dw2, this.LearningRate));
            double[][] fakeB2 = new double[][] {this.B2};
            double[][] fakedb2 = new double[][] {db2};
            this.B2 = MatrixOperation.Diff(fakeB2, MatrixOperation.DotConstant(fakedb2, this.LearningRate))[0];

            this.W1 = MatrixOperation.Diff(this.W1, MatrixOperation.DotConstant(dw1, this.LearningRate));
            var fakeB1 = new double[][] {this.B1};
            var fakedb1 = new double[][] {db1};
            this.B1 = MatrixOperation.Diff(fakeB1, MatrixOperation.DotConstant(fakedb1, this.LearningRate))[0];
        }

        public List<double> Train(double[][] x, double[][] y, int nbreIterations){
            for(int i = 1; i <= nbreIterations; i++){
                var y_pred = this.Forward(x);
                var loss = this.EntropyLoss(y, y_pred);
                this.Loss.Add(loss);
                this.Backward(x,y);
            }
            return this.Loss;
        }

        public double[][] Predict(double[][] x){
            return this.Forward(x);
        }
    }
}
