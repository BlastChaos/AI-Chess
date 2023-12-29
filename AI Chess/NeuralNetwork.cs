using AI_Chess.Context;
using AI_Chess.Model;
using Microsoft.EntityFrameworkCore;

namespace AI_Chess
{
    public class NeuralNetwork
    {
        private readonly int NbInputNodes;
        private readonly double LearningRate;
        //private double[][][] W;
        //private double[][] B;
        private readonly double[][][] A;
        private readonly double[][][] Z;
        private readonly List<double> Loss;
        private readonly Node[] Nodes;
        private readonly ChessDbContext _chessDbContext;
        private readonly ILogger<NeuralNetwork> _logger;

        public NeuralNetwork(int nbInputNodes, double learningRate, Node[] nodes, ChessDbContext chessDbContext, ILogger<NeuralNetwork> logger)
        {

            this.NbInputNodes = nbInputNodes;
            this.LearningRate = learningRate;
            this.Loss = new List<double>();
            this.Nodes = nodes;
            _chessDbContext = chessDbContext;
            _logger = logger;

            this.A = new double[this.Nodes!.Length+1][][];
            this.Z = new double[this.Nodes!.Length][][];
            /*
            this.W = new double[Nodes!.Length][][];
            this.B = new double[this.Nodes!.Length][];

            var random = new Random();
            for (int i = 0; i < this.Nodes!.Length; i++){
                this.W[i] = MatrixOperation.GenerateRandomNormal(random, 0, 1, i == 0 ? this.NbInputNodes : this.Nodes[i-1].NbHiddenNode, this.Nodes[i].NbHiddenNode);
                this.B[i] = new double[this.Nodes[i].NbHiddenNode].Select(j => j = random.NextDouble()).ToArray();
            }
            */
        }

        public double EntropyLoss(double[][] y, double[][] y_pred){
            double result = 0;
            double outputLength = y_pred[0].Length;
            for (int i = 0; i < y.Length; i++)
            {
                for (int j = 0; j < outputLength; j++)
                {
                    //Prevent taking the log of 0 with double.Epsilon
                    result += -y[i][j] * Math.Log(y_pred[i][j] + double.Epsilon);
                }
            }
            return result;
        }
        public double CostFunction(double[][] y, double[][] y_pred){
            double result = 0;
            double outputLength = y_pred[0].Length;
            for (int i = 0; i < y.Length; i++)
            {
                for (int j = 0; j < outputLength; j++)
                {
                    result += Math.Pow(y_pred[i][j] -y[i][j], 2) * 1/(y_pred[i].Length*2);
                }
            }
            return result;
        }

        public async Task<double[][]> Forward(double[][] x){
            this.A[0] = x;
            double[][] dotProduct;

            for (int i = 0; i < this.Nodes.Length; i++){
                var w = await GetWContent(i);
                var b = await GetBContent(i);
                dotProduct = MatrixOperation.DotProduct(this.A[i], w);
                this.Z[i] = MatrixOperation.Add(dotProduct, b);
                this.A[i+1] = this.Nodes[i].Activation!.Activation(this.Z[i]);
            }
            return this.A.Last();
        }

        public async Task Backward(double[][] y){
            double[][][] dw = new double[this.Nodes.Length][][];
            double[][] db = new double[this.Nodes.Length][];
            double[][][] dz = new double[this.Nodes.Length][][];
            double[][] dA;
            double[][] a;
            int aLength;
            for (int i = this.Nodes.Length-1; i >=0; i--){
                if(i == this.Nodes.Length-1){
                    a = this.A[i+1];
                    aLength = a.Length;
                    dA = new double[aLength][];
                    for (int l = 0; l < aLength; l++)
                    {
                        dA[l] = new double[a[0].Length];
                        for (int j = 0; j < a[0].Length; j++)
                        {
                            dA[l][j] = (this.A[i+1][l][j] - y[l][j]) / aLength;     
                        }
                    }
                    dz[i] = MatrixOperation.DotElementWise(dA, this.Nodes[i].Activation!.Derivative(this.Z[i], y));
                } else {
                    var w = await GetWContent(i + 1);
                    dA = MatrixOperation.DotProduct(dz[i+1], MatrixOperation.Transpose(w!));
                    dz[i] = MatrixOperation.DotElementWise(dA, this.Nodes[i].Activation!.Derivative(this.Z[i], y));
                }
                dw[i] = MatrixOperation.DotProduct(MatrixOperation.Transpose(this.A[i]!), dz[i]);
                db[i] = MatrixOperation.SumColumn(dz[i]);
            }

            for(int i = 0; i < Nodes!.Length; i++){
                var w = await GetWContent(i);
                var b = await GetBContent(i);
                var newW = MatrixOperation.Diff(w, MatrixOperation.DotConstant(dw[i], this.LearningRate));
                var fakeB = new double[][] {b};
                var fakeDB = new double[][] {db[i]};
                var newB = MatrixOperation.Diff(fakeB, MatrixOperation.DotConstant(fakeDB, this.LearningRate))[0];
                await UpdateDatabase(i, newW, newB);
            }
        }

        public async Task<List<double>> Train(double[][] input, double[][] output, int nbreIterations){
            if(input[0].Length != this.NbInputNodes) throw new Exception("Invalid input");
            if(output[0].Length != this.Nodes.Last().NbHiddenNode) throw new Exception("Invalid output");

            var wContentLength = await _chessDbContext.WContents.CountAsync();

            if(wContentLength == 0)
            {
                _logger.LogInformation("Initialize database");
                await InitializeDatabase();
            }
            _logger.LogInformation("Training start with {iterations} iterations", nbreIterations);
            for (int i = 1; i <= nbreIterations; i++){
                _logger.LogInformation("Iteration {iteration}", i);
                var y_pred = await this.Forward(input);
                var loss = this.EntropyLoss(output, y_pred);
                this.Loss.Add(loss);
                await this.Backward(output);
            }
            _logger.LogInformation("Training end");
            return this.Loss;
        }

        public Task<double[][]> Predict(double[][] x){
            return Forward(x);
        }

        private WContent[] ConvertToWContent(int position, double[][] w)
        {
            var lists = new List<WContent>();
            for(int i = 0; i < w.Length; i++)
            {
                for(int y = 0; y < w[i].Length; y++)
                {
                    lists.Add(new WContent()
                    {
                        Position = position,
                        From = i,
                        To = y,
                        Value = w[i][y]
                    });
                }
            }
            return lists.ToArray();
        }

        private BContent[] ConvertToBContent(int position, double[] b)
        {
            var lists = new List<BContent>();
            for (int i = 0; i < b.Length; i++)
            {
                    lists.Add(new BContent()
                    {
                        Position = position,
                        To = i,
                        Value = b[i]
                    });
            }
            return lists.ToArray();
        }

        private async Task InitializeDatabase()
        {
            var wContent = new List<WContent>();
            var bContent = new List<BContent>();

            var random = new Random();
            for (int i = 0; i < Nodes.Length; i++)
            {
                var w = MatrixOperation.GenerateRandomNormal(random, 0, 1, i == 0 ? this.NbInputNodes : this.Nodes[i - 1].NbHiddenNode, this.Nodes[i].NbHiddenNode);
                var wContentArray = ConvertToWContent(i, w);
                wContent.AddRange(wContentArray);

                var b = new double[this.Nodes[i].NbHiddenNode].Select(j => j = random.NextDouble()).ToArray();
                var bContentArray = ConvertToBContent(i, b);
                bContent.AddRange(bContentArray);
            }
            await _chessDbContext.WContents.AddRangeAsync(wContent);
            await _chessDbContext.BContents.AddRangeAsync(bContent);
            await _chessDbContext.SaveChangesAsync();
        }

        private async Task UpdateDatabase(int position, double[][] w, double[] b)
        {
            var wContents = ConvertToWContent(position,w);
            foreach(var wContent in wContents)
            {
                var db = _chessDbContext.WContents.Single(w => w.Position == wContent.Position && w.From == wContent.From && w.To == wContent.To);
                db.Value = wContent.Value;
            }

            var bContents = ConvertToBContent(position, b);
            foreach (var bContent in bContents)
            {
                var db = _chessDbContext.BContents.Single(w => w.Position == bContent.Position && w.To == bContent.To);
                db.Value = bContent.Value;
            }

            await _chessDbContext.SaveChangesAsync();
        }

        private async Task<double[][]> GetWContent(int postition)
        {
            var xLength = postition == 0 ? this.NbInputNodes : this.Nodes[postition - 1].NbHiddenNode;
            var yLength = this.Nodes[postition].NbHiddenNode;

            var wContents = await _chessDbContext.WContents.Where(w => w.Position == postition).ToArrayAsync();
            var w = new double[xLength][];
            for(int i = 0; i < xLength; i++)
            {
                w[i] = new double[yLength];
            }
            foreach(var wContent in wContents)
            {
                var x = wContent.From;
                var y = wContent.To;
                w[x][y] = wContent.Value;
            }
            return w;
        }

        private async Task<double[]> GetBContent(int postition)
        {
            var xLength = this.Nodes[postition].NbHiddenNode;

            var bContents = await _chessDbContext.BContents.Where(b => b.Position == postition).ToArrayAsync();
            var b = new double[xLength];

            foreach (var bContent in bContents)
            {
                var x = bContent.To;
                b[x] = bContent.Value;
            }
            return b;
        }
    }
}
