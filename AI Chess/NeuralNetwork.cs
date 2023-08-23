using AI_Chess.Activation;

namespace AI_Chess
{
    public class NeuralNetwork
    {
        private readonly int NbInputNodes;
        private readonly double LearningRate;
        private readonly double[][][] W;
        private readonly double[][] B;
        private readonly double[][][] A;
        private readonly double[][][] Z;
        private readonly List<double> Loss;
        private readonly Node[] Nodes;

        public NeuralNetwork(int nbInputNodes, double learningRate, Node[] nodes)
        {

            this.NbInputNodes = nbInputNodes;
            this.LearningRate = learningRate;
            this.Loss = new List<double>();
            this.Nodes = nodes;

            this.A = new double[this.Nodes!.Length+1][][];
            this.Z = new double[this.Nodes!.Length][][];
            this.W = new double[this.Nodes!.Length][][];
            this.B = new double[this.Nodes!.Length][];

            for(int i = 0; i < this.Nodes!.Length; i++){
                this.W[i] = MatrixOperation.GenerateRandomNormal(new Random(), 0, 1, i == 0 ? this.NbInputNodes : this.Nodes[i-1].NbHiddenNode, this.Nodes[i].NbHiddenNode);
                this.B[i] = new double[this.Nodes[i].NbHiddenNode].Select(j => j = 1).ToArray();
            }
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
            this.A[0] = x;
            for (int i = 0; i < this.Nodes.Length; i++){
                double[][] dotProduct = MatrixOperation.DotProduct(this.A[i], this.W[i]);
                this.Z[i] = MatrixOperation.Add(dotProduct, this.B[i]);
                this.A[i+1] = this.Nodes[i].Activation!.Activation(this.Z[i]);
            }
            return this.A.Last();
        }

        public void Backward(double[][] x, double[][] y){
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

        public double[][] Predict(double[][] x){
            return this.Forward(x);
        }
    }
}
