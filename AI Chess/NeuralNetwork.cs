using System.Text.Json;

namespace AI_Chess
{
    public class NeuralNetwork
    {
        private readonly int NbInputNodes;
        private readonly double LearningRate;
        private double[][][] W;
        private double[][] B;
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
            this.W = new double[Nodes!.Length][][];
            this.B = new double[this.Nodes!.Length][];

            for(int i = 0; i < this.Nodes!.Length; i++){
                this.W[i] = MatrixOperation.GenerateRandomNormal(new Random(), 0, 1, i == 0 ? this.NbInputNodes : this.Nodes[i-1].NbHiddenNode, this.Nodes[i].NbHiddenNode);
                this.B[i] = new double[this.Nodes[i].NbHiddenNode].Select(j => j = new Random().NextDouble()).ToArray();
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
        public double CostFunction(double[][] y, double[][] y_pred){
            double result = 0;
            for (int i = 0; i < y.Length; i++)
            {
                for (int j = 0; j < y[0].Length; j++)
                {
                    result += Math.Pow(y_pred[i][j] -y[i][j], 2) * 1/(y_pred[i].Length*2);
                }
            }
            return result;
        }

        public double[][] Forward(double[][] x){
            this.A[0] = x;
            double[][] dotProduct;
            for (int i = 0; i < this.Nodes.Length; i++){
                dotProduct = MatrixOperation.DotProduct(this.A[i], this.W[i]);
                this.Z[i] = MatrixOperation.Add(dotProduct, this.B[i]);
                this.A[i+1] = this.Nodes[i].Activation!.Activation(this.Z[i]);
            }
            return this.A.Last();
        }

        public void Backward(double[][] y){
            double[][][] dw = new double[this.Nodes.Length][][];
            double[][] db = new double[this.Nodes.Length][];
            double[][][] dz = new double[this.Nodes.Length][][];
            for (int i = this.Nodes.Length-1; i >=0; i--){
                if(i == this.Nodes.Length-1){
                    double[][] dA = new double[this.A[i+1].Length][];
                    for (int l = 0; l < this.A[i+1].Length; l++)
                    {
                        dA[l] = new double[this.A[i+1][0].Length];
                        for (int j = 0; j < this.A[i+1][0].Length; j++)
                        {
                            dA[l][j] = (this.A[i+1][l][j] - y[l][j]) / this.A[i+1].Length;     
                        }
                    }
                    dz[i] = MatrixOperation.DotElementWise(dA, this.Nodes[i].Activation!.Derivative(this.Z[i], y));
                } else {
                    var dA = MatrixOperation.DotProduct(dz[i+1], MatrixOperation.Transpose(this.W[i+1]!));
                    dz[i] = MatrixOperation.DotElementWise(dA, this.Nodes[i].Activation!.Derivative(this.Z[i], y));
                }
                dw[i] = MatrixOperation.DotProduct(MatrixOperation.Transpose(this.A[i]!), dz[i]);
                db[i] = MatrixOperation.SumColumn(dz[i]);
            }

            for(int i = 0; i < this.W.Length; i++){
                W[i] = MatrixOperation.Diff(W[i], MatrixOperation.DotConstant(dw[i], this.LearningRate));
                var fakeB = new double[][] {B[i]};
                var fakeDB = new double[][] {db[i]};
                B[i] = MatrixOperation.Diff(fakeB, MatrixOperation.DotConstant(fakeDB, this.LearningRate))[0];
            }
        }

        public List<double> Train(double[][] input, double[][] output, int nbreIterations){
            if(input[0].Length != this.NbInputNodes) throw new Exception("Invalid input");
            if(output[0].Length != this.Nodes.Last().NbHiddenNode) throw new Exception("Invalid output");
            for(int i = 1; i <= nbreIterations; i++){
                var y_pred = this.Forward(input);
                var loss = this.EntropyLoss(output, y_pred);
                this.Loss.Add(loss);
                this.Backward(output);
            }
            return this.Loss;
        }

        public double[][] Predict(double[][] x){
            return this.Forward(x);
        }

        public void Save(string filePath){
            var saveContent = new SaveContent() {
                B = this.B,
                W = this.W
            };
            string json = JsonSerializer.Serialize(saveContent);
            File.WriteAllText(filePath, json);
            Console.WriteLine("a neural network has been saved successfully:" + filePath);
        }

        public void Load(string filePath){
            string json = File.ReadAllText(filePath);
            var loadContent = JsonSerializer.Deserialize<SaveContent>(json);
            this.W = loadContent.W;
            this.B = loadContent.B;
            Console.WriteLine("a neural network has been loaded successfully:" + filePath);
        }
    }
}
