using AI_Chess.Context;
using AI_Chess.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AI_Chess
{
    public class NeuralNetwork
    {
        private readonly int NbInputNodes;
        private readonly double LearningRate;
        //private double[][][] W;
        //private double[][] B;
        //private readonly double[][][] A;
        //private readonly double[][][] Z;
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

            /*
                    this.Z = new double[this.Nodes!.Length][][];
            this.A = new double[this.Nodes!.Length+1][][];
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
            double[][] dotProduct;

            await UpdateContent(0, x, _chessDbContext.AContents);

            double[][] a = x;

            for (int i = 0; i < this.Nodes.Length; i++){
                var w = await GetContents(i, _chessDbContext.WContents);
                var b = await GetBContents(i);
                dotProduct = MatrixOperation.DotProduct(a, w);
                var z = MatrixOperation.Add(dotProduct, b);
                a = this.Nodes[i].Activation!.Activation(z);
                await UpdateContent(i, z, _chessDbContext.ZContents);
                await UpdateContent(i+1, a, _chessDbContext.AContents);
            }
            return a;
        }

        public async Task Backward(double[][] y){
            double[][][] dz = new double[this.Nodes.Length][][];
            double[][] dA;
            
            for (int i = this.Nodes.Length-1; i >=0; i--){
                var a = await GetContents(i, _chessDbContext.AContents);
                var z = await GetContents(i, _chessDbContext.ZContents);

                if(i == this.Nodes.Length-1){
                    double[][] aAfter = await GetContents(i+1, _chessDbContext.AContents);
                    int aLength = aAfter.Length;
                    dA = new double[aLength][];
                    for (int l = 0; l < aLength; l++)
                    {
                        dA[l] = new double[aAfter[0].Length];
                        for (int j = 0; j < aAfter[0].Length; j++)
                        {
                            dA[l][j] = (aAfter[l][j] - y[l][j]) / aLength;     
                        }
                    }
                    dz[i] = MatrixOperation.DotElementWise(dA, this.Nodes[i].Activation!.Derivative(z, y));
                } else {
                    var wAfter = await GetContents(i + 1, _chessDbContext.WContents);
                    dA = MatrixOperation.DotProduct(dz[i+1], MatrixOperation.Transpose(wAfter!));
                    dz[i] = MatrixOperation.DotElementWise(dA, this.Nodes[i].Activation!.Derivative(z, y));
                }
                var dw = MatrixOperation.DotProduct(MatrixOperation.Transpose(a), dz[i]);
                var db = MatrixOperation.SumColumn(dz[i]);

                var w = await GetContents(i, _chessDbContext.WContents);
                var b = await GetBContents(i);
                var newW = MatrixOperation.Diff(w, MatrixOperation.DotConstant(dw, this.LearningRate));
                var fakeB = new double[][] {b};
                var fakeDB = new double[][] {db};
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
            var wContent = new HashSet<WContent>();
            var bContent = new HashSet<BContent>();

            var random = new Random();
            for (int i = 0; i < Nodes.Length; i++)
            {
                var w = MatrixOperation.GenerateRandomNormal(random, 0, 1, i == 0 ? this.NbInputNodes : this.Nodes[i - 1].NbHiddenNode, this.Nodes[i].NbHiddenNode);
                var wContentArray = new WContent(){Position = i, Value = w};
                wContent.Add(wContentArray);

                var b = new double[this.Nodes[i].NbHiddenNode].Select(j => j = random.NextDouble()).ToArray();
                var bContentArray = ConvertToBContent(i, b);
                bContent.UnionWith(bContentArray);
            }
            await _chessDbContext.WContents.AddRangeAsync(wContent);
            await _chessDbContext.BContents.BulkInsertAsync(bContent, options => options.AutoMapOutputDirection = false);
            
        }

        private async Task UpdateDatabase(int position, double[][] w, double[] b)
        {
            await UpdateContent(position, w, _chessDbContext.WContents);

            var bContents = ConvertToBContent(position, b);
            await _chessDbContext.BContents.AsNoTracking().Where(b => b.Position == position).ExecuteDeleteAsync();
            await _chessDbContext.BContents.BulkInsertAsync(bContents, options => options.AutoMapOutputDirection = false);
        }

        private async Task UpdateContent<T>(int position, double[][] content, DbSet<T> dbSet) where T : Content, new()
        {
            var contents = await dbSet.SingleAsync(c => c.Position == position);
            contents.Value = content;
            dbSet.Update(contents);
            await _chessDbContext.SaveChangesAsync();
        }
        
        private static async Task<double[][]> GetContents<T>(int postition, DbSet<T> dbSet) where T : Content
        {
            var contents = await dbSet.AsNoTracking().Where(c => c.Position == postition).Select(c => c.Value).SingleAsync();
            return contents;
        }

        private async Task<double[]> GetBContents(int postition)
        {
            var xLength = await _chessDbContext.BContents.AsNoTracking().CountAsync(b => b.Position == postition);

            var bContents = await _chessDbContext.BContents.AsNoTracking().Where(b => b.Position == postition).ToArrayAsync();
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
