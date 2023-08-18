using AI_Chess.Activation;

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
        private double[][][] W;

        private double[] B1;
        private double[] B2;
        private double[][] B;
        private double[][]? A1;
        private double[][]? A2;
        private double[][]? A3;
        private double[][][] A;
        private double[][]? Z2;
        private double[][]? Z3;
        private double[][][] Z;
        private readonly List<double> Loss;
        private readonly Node[] Nodes;

        public NeuralNetwork(int nbInputNodes, int nbHiddenNodes, int nbOutputNodes, double learningRate, Node[] nodes)
        {

            this.NbInputNodes = nbInputNodes;
            this.NbHiddenNodes = nbHiddenNodes;
            this.NbOutputNodes = nbOutputNodes;
            this.LearningRate = learningRate;
            this.Loss = new List<double>();
            this.Nodes = nodes;
            this.W1 = MatrixOperation.GenerateRandomNormal(new Random(), 0, 1, this.NbInputNodes, nbHiddenNodes);
            this.B1 = new double[this.NbHiddenNodes].Select(i => i = 1).ToArray();

            this.W2 = MatrixOperation.GenerateRandomNormal(new Random(), 0, 1, this.NbHiddenNodes, nbOutputNodes);
            this.B2 = new double[this.NbOutputNodes].Select(i => i = 1).ToArray();

            this.A = new double[this.Nodes!.Length+1][][];
            this.Z = new double[this.Nodes!.Length][][];
            this.W = new double[this.Nodes!.Length][][];
            this.B = new double[this.Nodes!.Length][];
            this.W[0] = this.W1.Clone() as double[][];
            this.W[1] = this.W2.Clone() as double[][];
            this.B[0] = this.B1.Clone() as double[];
            this.B[1] = this.B2.Clone() as double[];
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
                
                var valeurDiminuer = 0.0;
                var somme = z![i].Sum(s => s >=0 ? s : 0.0);
                var t = z![i].Sum(s => s >=0 ? 1 : 0.0);
                var secondBest = z![i].OrderDescending().Skip(1).First();
                if(somme > ChessConstant.EXP_MAX_VALUE){
                    valeurDiminuer = (somme - ChessConstant.EXP_MAX_VALUE)/t + secondBest/t;
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

        public  double[][]  DerivativeSoftmax(double[][] a3, double[][] y){
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

        public double[][] RELU(double[][] z){
            double[][] result = new double[z.Length][];
            for (int i = 0; i < z.Length; i++)
            {
                result[i] = new double[z[0].Length];
                for (int j = 0; j < z[0].Length; j++)
                {
                    result[i][j] = Math.Max(0, z[i][j]);
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
        public  double[][] DerivativeRELU(double[][] z){
            double[][] result = new double[z.Length][];
            for (int i = 0; i < z.Length; i++)
            {
                result[i] = new double[z[0].Length];
                for (int j = 0; j < z[0].Length; j++)
                {
                    result[i][j] = z[i][j] <= 0 ? 0 : 1;    
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

            this.A2 = this.RELU(this.Z2);

            dotProduct = MatrixOperation.DotProduct(this.A2, this.W2);
            this.Z3 =  MatrixOperation.Add(dotProduct, this.B2); 
            
            this.A3 = this.Softmax(this.Z3);

            return this.A3;
        }

        public double[][] ForwardTest(double[][] x){
            this.A[0] = x;
            for (int i = 0; i < this.Nodes.Length; i++){
                double[][] dotProduct = MatrixOperation.DotProduct(this.A[i], this.W[i]);
                this.Z[i] = MatrixOperation.Add(dotProduct, this.B[i]);
                this.A[i+1] = this.Nodes[i].Activation!.Activation(this.Z[i]);
            }
            return this.A.Last();
        }
        

        public void Backward(double[][] x, double[][] y){

            double[][] dz3 = this.DerivativeSoftmax(this.A3!, y);

            double [][] dw2 = MatrixOperation.DotProduct(MatrixOperation.Transpose(this.A2!), dz3);

            double[] db2 = MatrixOperation.SumColumn(dz3);


            double[][] dA2 = MatrixOperation.DotProduct(dz3, MatrixOperation.Transpose(this.W2!));

            double [][] dz2 =  MatrixOperation.DotElementWise(dA2, this.DerivativeRELU(this.Z2!));

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

        public void BackwardTest(double[][] x, double[][] y){
            double[][][] dw = new double[this.Nodes.Length][][];
            double[][] db = new double[this.Nodes.Length][];
            double[][][] dz = new double[this.Nodes.Length][][];
            for (int i = this.Nodes.Length-1; i >=0; i--){
                
                if(this.Nodes[i].Activation is Softmax){
                    dz[i] = this.Nodes[i].Activation!.Derivative(this.A[i+1], y);
                } else {
                    var dA = MatrixOperation.DotProduct(dz[i+1], MatrixOperation.Transpose(this.W[i+1]!));
                    dz[i] = MatrixOperation.DotElementWise(dA, this.Nodes[i].Activation!.Derivative(this.Z[i], y));
                }
                dw[i] = MatrixOperation.DotProduct(MatrixOperation.Transpose(this.A[i]!), dz[i]);
                db[i] = MatrixOperation.SumColumn(dz[i]);
            }

            for(int i = 0; i < this.W.Length; i++){
            this.W[i] = MatrixOperation.Diff(this.W[i], MatrixOperation.DotConstant(dw[i], this.LearningRate));
            var fakeB = new double[][] {this.B[i]};
            var fakeDB = new double[][] {db[i]};
            this.B[i] = MatrixOperation.Diff(fakeB, MatrixOperation.DotConstant(fakeDB, this.LearningRate))[0];
            }
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

        public List<double> TrainTest(double[][] x, double[][] y, int nbreIterations){
            for(int i = 1; i <= nbreIterations; i++){
                var y_pred = this.ForwardTest(x);
                var loss = this.EntropyLoss(y, y_pred);
                this.Loss.Add(loss);
                this.BackwardTest(x,y);
            }
            return this.Loss;
        }

        public double[][] Predict(double[][] x){
            return this.Forward(x);
        }
    }
}
