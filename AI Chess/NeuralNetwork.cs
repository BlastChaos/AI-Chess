using AI_Chess.Context;
using AI_Chess.Model;
using Microsoft.EntityFrameworkCore;

namespace AI_Chess
{
    public class NeuralNetwork
    {
        private readonly int NbInputNodes;
        private readonly double LearningRate;
        private readonly List<double> Loss;
        private readonly Node[] Nodes;
        private readonly ChessDbContext _chessDbContext;
        private readonly ILogger<NeuralNetwork> _logger;
        private readonly int NeuralNetworkId;

        public NeuralNetwork(
            int nbInputNodes,
            double learningRate,
            int neuralNetworkId,
            Node[] nodes,
            ChessDbContext chessDbContext,
            ILogger<NeuralNetwork> logger)
        {

            this.NeuralNetworkId = neuralNetworkId;
            this.NbInputNodes = nbInputNodes;
            this.LearningRate = learningRate;
            this.Loss = new List<double>();
            this.Nodes = nodes;
            _chessDbContext = chessDbContext;
            _logger = logger;
        }

        public double EntropyLoss(double[][] y, double[][] y_pred)
        {
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
        public double CostFunction(double[][] y, double[][] y_pred)
        {
            double result = 0;
            double outputLength = y_pred[0].Length;
            for (int i = 0; i < y.Length; i++)
            {
                for (int j = 0; j < outputLength; j++)
                {
                    result += Math.Pow(y_pred[i][j] - y[i][j], 2) * 1 / (y_pred[i].Length * 2);
                }
            }
            return result;
        }

        public async Task<double[][]> Forward(double[][] x, CancellationToken stoppingToken)
        {
            double[][] dotProduct;

            await UpdateContent(0, x, _chessDbContext.AContents, stoppingToken);

            double[][] a = x;

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                var w = await GetContents(i, _chessDbContext.WContents, stoppingToken);
                var b = await GetBContents(i, stoppingToken);
                dotProduct = MatrixOperation.DotProduct(a, w);
                var z = MatrixOperation.Add(dotProduct, b);
                a = this.Nodes[i].Activation!.Activation(z);
                await UpdateContent(i, z, _chessDbContext.ZContents, stoppingToken);
                await UpdateContent(i + 1, a, _chessDbContext.AContents, stoppingToken);
            }
            return a;
        }

        public async Task Backward(double[][] y, CancellationToken stoppingToken)
        {
            double[][][] dz = new double[this.Nodes.Length][][];
            double[][] dA;

            for (int i = this.Nodes.Length - 1; i >= 0; i--)
            {
                var a = await GetContents(i, _chessDbContext.AContents, stoppingToken);
                var z = await GetContents(i, _chessDbContext.ZContents, stoppingToken);

                if (i == this.Nodes.Length - 1)
                {
                    double[][] aAfter = await GetContents(i + 1, _chessDbContext.AContents, stoppingToken);
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
                }
                else
                {
                    var wAfter = await GetContents(i + 1, _chessDbContext.WContents, stoppingToken);
                    dA = MatrixOperation.DotProduct(dz[i + 1], MatrixOperation.Transpose(wAfter!));
                    dz[i] = MatrixOperation.DotElementWise(dA, this.Nodes[i].Activation!.Derivative(z, y));
                }
                var dw = MatrixOperation.DotProduct(MatrixOperation.Transpose(a), dz[i]);
                var db = MatrixOperation.SumColumn(dz[i]);

                var w = await GetContents(i, _chessDbContext.WContents, stoppingToken);
                var b = await GetBContents(i, stoppingToken);
                var newW = MatrixOperation.Diff(w, MatrixOperation.DotConstant(dw, this.LearningRate));
                var fakeB = new double[][] { b };
                var fakeDB = new double[][] { db };
                var newB = MatrixOperation.Diff(fakeB, MatrixOperation.DotConstant(fakeDB, this.LearningRate))[0];
                await UpdateDatabase(i, newW, newB, stoppingToken);
            }
        }

        public async Task<List<double>> Train(double[][] input, double[][] output, int nbreIterations, CancellationToken stoppingToken)
        {
            if (input[0].Length != this.NbInputNodes) throw new Exception("Invalid input");
            if (output[0].Length != this.Nodes.Last().NbHiddenNode) throw new Exception("Invalid output");

            var wContentLength = await _chessDbContext.WContents.Where(c => c.NeuralNetworkId == this.NeuralNetworkId).CountAsync(stoppingToken);

            if (wContentLength == 0)
            {
                _logger.LogInformation("Initialize database");
                await InitializeDatabase(stoppingToken);
            }
            _logger.LogInformation("Training start with {iterations} iterations", nbreIterations);
            for (int i = 1; i <= nbreIterations; i++)
            {
                _logger.LogInformation("Iteration {iteration}, current Loss: {loss}", i, this.Loss.LastOrDefault());
                var y_pred = await this.Forward(input, stoppingToken);
                var loss = this.CostFunction(output, y_pred);
                this.Loss.Add(loss);
                await this.Backward(output, stoppingToken);
            }
            _logger.LogInformation("Training end");
            //await _chessDbContext.BContents.AsNoTracking().ExecuteDeleteAsync();
            await _chessDbContext.AContents.AsNoTracking().ExecuteDeleteAsync();
            return this.Loss;
        }

        public Task<double[][]> Predict(double[][] x, CancellationToken stoppingToken)
        {
            return Forward(x, stoppingToken);
        }


        private async Task InitializeDatabase(CancellationToken stoppingToken)
        {
            var wContentList = new HashSet<WContent>();
            var bContentList = new HashSet<BContent>();

            var random = new Random();
            for (int i = 0; i < Nodes.Length; i++)
            {
                var w = MatrixOperation.GenerateRandomNormal(random, 0, 1, i == 0 ? this.NbInputNodes : this.Nodes[i - 1].NbHiddenNode, this.Nodes[i].NbHiddenNode);
                var wContent = new WContent() { Position = i, Value = w, NeuralNetworkId = this.NeuralNetworkId };
                wContentList.Add(wContent);

                var b = new double[this.Nodes[i].NbHiddenNode].Select(j => j = 1).ToArray();
                var bContent = new BContent() { Position = i, Value = b, NeuralNetworkId = this.NeuralNetworkId };
                bContentList.Add(bContent);
            }
            await _chessDbContext.WContents.AddRangeAsync(wContentList, stoppingToken);
            await _chessDbContext.BContents.AddRangeAsync(bContentList, stoppingToken);
            await _chessDbContext.SaveChangesAsync(stoppingToken);
            _chessDbContext.ChangeTracker.Clear();
        }

        public async Task Reset(CancellationToken stoppingToken)
        {
            await InitializeDatabase(stoppingToken);
        }

        private async Task UpdateDatabase(int position, double[][] w, double[] b, CancellationToken stoppingToken)
        {
            await UpdateContent(position, w, _chessDbContext.WContents, stoppingToken);
            await UpdateBContent(position, b, stoppingToken);
        }

        private async Task UpdateBContent(int position, double[] content, CancellationToken stoppingToken)
        {
            await _chessDbContext.BContents
                .Where(c => c.Position == position && c.NeuralNetworkId == this.NeuralNetworkId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Value, content), stoppingToken);
        }


        private async Task UpdateContent<T>(int position, double[][] content, DbSet<T> dbSet, CancellationToken stoppingToken) where T : Content, new()
        {
            var value = await dbSet.AsNoTracking().Where(c => c.Position == position && c.NeuralNetworkId == this.NeuralNetworkId).Select(c => c.Value).FirstOrDefaultAsync(stoppingToken);

            if (value == null)
            {
                var newContent = new T() { Position = position, Value = content, NeuralNetworkId = this.NeuralNetworkId };
                await dbSet.AddAsync(newContent, stoppingToken);
                await _chessDbContext.SaveChangesAsync(stoppingToken);
                _chessDbContext.ChangeTracker.Clear();
            }
            else
            {
                await dbSet
                    .Where(c => c.Position == position && c.NeuralNetworkId == this.NeuralNetworkId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(info => info.Value, content), stoppingToken);
            }
        }

        private async Task<double[][]> GetContents<T>(int postition, DbSet<T> dbSet, CancellationToken stoppingToken) where T : Content
        {
            return await dbSet.AsNoTracking().Where(c => c.Position == postition && c.NeuralNetworkId == this.NeuralNetworkId).Select(c => c.Value).FirstAsync(stoppingToken);
        }

        private async Task<double[]> GetBContents(int postition, CancellationToken stoppingToken)
        {
            return await _chessDbContext.BContents.AsNoTracking().Where(c => c.Position == postition && c.NeuralNetworkId == this.NeuralNetworkId).Select(c => c.Value).FirstAsync(stoppingToken);
        }
    }
}
