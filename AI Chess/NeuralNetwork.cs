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
        public double FonctionCout(double[][] y, double[][] y_pred){
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
                this.W[i] = MatrixOperation.Diff(this.W[i], MatrixOperation.DotConstant(dw[i], this.LearningRate));
                var fakeB = new double[][] {this.B[i]};
                var fakeDB = new double[][] {db[i]};
                this.B[i] = MatrixOperation.Diff(fakeB, MatrixOperation.DotConstant(fakeDB, this.LearningRate))[0];
            }
        }

        public List<double> Train(double[][] input, double[][] output, int nbreIterations){
            for(int i = 1; i <= nbreIterations; i++){
                var y_pred = this.Forward(input);
                var loss = this.FonctionCout(output, y_pred);
                this.Loss.Add(loss);
                this.Backward(input,output);
            }
            return this.Loss;
        }

        public double[][] Predict(double[][] x){
            return this.Forward(x);
        }

        public void Test(){
            Random random = new();
            double[][] numbers = new double[300][];
            double[][] y = new double[300][];
            for (int m = 0; m < numbers.Length ; m++)
            {
                numbers[m] = new double[10];
                for (int n = 0; n < 10 ; n++)
                {
                    numbers[m][n] = random.NextDouble()*8;
                }
                var maxValue = numbers[m].Max();
                y[m] = new double[10];
                for (int n = 0; n < 10 ; n++)
                {
                    y[m][n] = numbers[m][n] == maxValue ? 1 : 0;
                }
            }
            Node[] nodes = new Node[3];
            nodes[0] = new Node(){
                Activation = new Softmax(),
                NbHiddenNode = 30
            };
            nodes[1] = new Node(){
                Activation = new Softmax(),
                NbHiddenNode = 30
            };
            nodes[2] = new Node(){
                Activation = new Softmax(),
                NbHiddenNode = 10
            };
            var nn = new NeuralNetwork(10,0.01, nodes);
            var debut = DateTime.Now;
            var loss = nn.Train(numbers,y,1000);
            var fin = DateTime.Now;
            Console.WriteLine("Dernier Loss generer: " + loss.Last());
            Console.WriteLine("Temps pour le générer: " + (fin-debut));

            var test = nn.Predict(numbers);
            var nbreReussi = 0;
            for (int m = 0; m < test.Length ; m++){
                if(Array.IndexOf(test[m], test[m].Max()) == Array.IndexOf(y[m], y[m].Max()) ) nbreReussi++;
            }
            Console.WriteLine("nbre reussi: " + nbreReussi);
        }
    }
}
